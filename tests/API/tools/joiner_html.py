#!/usr/bin/env python3
"""
Join proto RPC list and observed telemetry then emit JSON artifacts and a small
self-contained HTML coverage report suitable for CI artifacts.

Usage:
  python joiner_html.py --proto proto_rpcs.json --observed observed_calls.jsonl --out-dir artifacts

Produces:
  artifacts/joined_calls.json
  artifacts/uncovered_rpcs.json
  artifacts/coverage_summary.json
  artifacts/coverage_report.html
"""
import argparse
import json
import os
from collections import defaultdict
from datetime import datetime
import math


def load_proto_rpcs(path):
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)


def stream_observed(path):
    if not os.path.exists(path):
        return
    with open(path, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                yield json.loads(line)
            except json.JSONDecodeError:
                continue


def normalize_fqname(proto_entry):
    return proto_entry.get('fullyQualifiedName') or (
        f"{proto_entry.get('package','')}.{proto_entry.get('service','')}/{proto_entry.get('method','')}"
    )


def main():
    p = argparse.ArgumentParser()
    p.add_argument('--proto', required=True)
    p.add_argument('--observed', required=True)
    p.add_argument('--status-codes', required=False,
                   help='Comma-separated list of expected status codes (gRPC names or HTTP numeric codes). Example: "200,400,401,500" or "OK,INVALID_ARGUMENT,UNAUTHENTICATED,INTERNAL"')
    p.add_argument('--out-dir', default='artifacts')
    args = p.parse_args()

    os.makedirs(args.out_dir, exist_ok=True)

    proto_list = load_proto_rpcs(args.proto)
    proto_map = {normalize_fqname(e): e for e in proto_list}

    observed_by_rpc = defaultdict(list)
    for rec in stream_observed(args.observed):
        # Build multiple normalized keys for flexible matching
        service = rec.get('service') or ''
        method = rec.get('method') or rec.get('fullMethod') and rec.get('fullMethod').split('/')[-1]
        full = rec.get('fullMethod') or (service and method and f"{service}/{method}")
        if not method:
            continue

        # full key (package.Service/Method)
        if full:
            observed_by_rpc[full].append(rec)

        # service/method key
        if service and method:
            observed_by_rpc[f"{service}/{method}"].append(rec)

        # short service (last token) e.g. Acquisition/Method
        short = service.split('.')[-1] if service else ''
        if short and method:
            observed_by_rpc[f"{short}/{method}"].append(rec)

        # method-only key (last resort)
        observed_by_rpc[f"{method}"].append(rec)

    joined = []
    uncovered = []
    total = len(proto_list)
    observed_count = 0
    # additional aggregations
    per_rpc_status = {}
    per_rpc_scenarios = {}
    per_rpc_payload_valid = {}
    per_rpc_latencies = {}

    for e in proto_list:
        fq = normalize_fqname(e)

        # Candidate observed keys to match against telemetry entries.
        candidates = [fq]
        svc = e.get('service', '')
        method = e.get('method', '')
        if svc and method:
            candidates.append(f"{svc}/{method}")
            # short service name (last token) to tolerate package differences
            short = svc.split('.')[-1]
            if short:
                candidates.append(f"{short}/{method}")

        # collect observed records from any matching candidate key
        items = []
        seen_ids = set()
        for c in candidates:
            recs = observed_by_rpc.get(c, [])
            for r in recs:
                # avoid dupes (simple dedupe by timestamp+service+method)
                key = (r.get('timestamp'), r.get('service'), r.get('method'))
                if key in seen_ids:
                    continue
                seen_ids.add(key)
                items.append(r)

        entry = {
            'proto': e,
            'observedCount': len(items),
            'observedSamples': items[:3]
        }
        # aggregate scenario and status data
        statuses = defaultdict(int)
        scen_counts = defaultdict(int)
        payload_vals = {'valid': 0, 'invalid': 0, 'unknown': 0}
        latencies = []
        for r in items:
            statuses[r.get('status','UNKNOWN')] += 1
            for s in r.get('scenarios', []) or []:
                scen_counts[s] += 1
            pv = r.get('payloadValid')
            if pv is True:
                payload_vals['valid'] += 1
            elif pv is False:
                payload_vals['invalid'] += 1
            else:
                payload_vals['unknown'] += 1
            try:
                lat = float(r.get('latencyMs', 0))
                latencies.append(lat)
            except Exception:
                pass

        per_rpc_status[fq] = dict(statuses)
        per_rpc_scenarios[fq] = dict(scen_counts)
        per_rpc_payload_valid[fq] = payload_vals
        per_rpc_latencies[fq] = latencies
        joined.append(entry)
        if len(items) == 0:
            uncovered.append(e)
        else:
            observed_count += 1

    coverage = {
        'totalRpcCount': total,
        'observedRpcCount': observed_count,
        'uncoveredRpcCount': len(uncovered),
        'coveragePercent': round((observed_count / total * 100.0) if total else 100.0, 2),
        'generatedAt': datetime.utcnow().isoformat() + 'Z'
    }

    # attach scenario/status summaries
    coverage['perRpcStatus'] = per_rpc_status
    coverage['perRpcScenarios'] = per_rpc_scenarios
    coverage['perRpcPayloadValid'] = per_rpc_payload_valid
    # add latency percentiles
    def pctile(arr, p):
        if not arr: return None
        s = sorted(arr)
        k = (len(s)-1) * (p/100.0)
        f = math.floor(k)
        c = math.ceil(k)
        if f == c: return s[int(k)]
        d0 = s[int(f)] * (c-k)
        d1 = s[int(c)] * (k-f)
        return (d0 + d1)

    latency_summary = {}
    for k, arr in per_rpc_latencies.items():
        if arr:
            latency_summary[k] = {
                'count': len(arr),
                'p50': pctile(arr, 50),
                'p90': pctile(arr, 90),
                'p99': pctile(arr, 99)
            }
    coverage['perRpcLatency'] = latency_summary

    # Handle configurable expected status codes and compute status-code coverage.
    # Accept codes as gRPC names (e.g., OK, INVALID_ARGUMENT) or HTTP numeric (e.g., 200, 400).
    def http_to_grpc_name(http_code):
        # Minimal mapping based on common conventions
        mapping = {
            200: 'OK',
            400: 'INVALID_ARGUMENT',
            401: 'UNAUTHENTICATED',
            403: 'PERMISSION_DENIED',
            404: 'NOT_FOUND',
            409: 'ALREADY_EXISTS',
            412: 'FAILED_PRECONDITION',
            429: 'RESOURCE_EXHAUSTED',
            499: 'CANCELLED',
            500: 'INTERNAL',
            501: 'UNIMPLEMENTED',
            503: 'UNAVAILABLE',
            504: 'DEADLINE_EXCEEDED'
        }
        return mapping.get(http_code)

    expected_statuses = None
    if args.status_codes:
        parts = [s.strip() for s in args.status_codes.split(',') if s.strip()]
        expected_statuses = set()
        for p in parts:
            # try numeric
            try:
                n = int(p)
                name = http_to_grpc_name(n)
                if name:
                    expected_statuses.add(name)
                else:
                    # unknown numeric mapping; keep numeric string as-is
                    expected_statuses.add(str(n))
            except ValueError:
                # treat as gRPC name
                expected_statuses.add(p.upper())

    # Compute per-rpc and global status-code coverage relative to expected_statuses
    per_rpc_status_coverage = {}
    global_observed_statuses = set()
    if expected_statuses:
        for k, stmap in per_rpc_status.items():
            observed_codes = set(stmap.keys())
            # intersection with expected (only compare names)
            matched = set()
            for code in observed_codes:
                if code in expected_statuses:
                    matched.add(code)
                else:
                    # also accept numeric matches where expected provided numeric strings
                    try:
                        # if observed code is a gRPC name, map expected numeric to name was done earlier
                        pass
                    except Exception:
                        pass
            per_rpc_status_coverage[k] = {
                'expectedCount': len(expected_statuses),
                'observedMatchedCount': len(matched),
                'observedMatched': sorted(list(matched)),
                'percent': round((len(matched) / len(expected_statuses) * 100.0) if expected_statuses else None, 2)
            }
            global_observed_statuses.update(matched)

        overall_percent = round((len(global_observed_statuses) / len(expected_statuses) * 100.0) if expected_statuses else None, 2)
        coverage['statusCoverage'] = {
            'expectedStatuses': sorted(list(expected_statuses)),
            'observedStatuses': sorted(list(global_observed_statuses)),
            'percent': overall_percent
        }
        coverage['perRpcStatusCoverage'] = per_rpc_status_coverage

    # write outputs
    joined_path = os.path.join(args.out_dir, 'joined_calls.json')
    uncovered_path = os.path.join(args.out_dir, 'uncovered_rpcs.json')
    summary_path = os.path.join(args.out_dir, 'coverage_summary.json')
    html_path = os.path.join(args.out_dir, 'coverage_report.html')

    with open(joined_path, 'w', encoding='utf-8') as f:
        json.dump(joined, f, indent=2)
    with open(uncovered_path, 'w', encoding='utf-8') as f:
        json.dump(uncovered, f, indent=2)
    with open(summary_path, 'w', encoding='utf-8') as f:
        json.dump(coverage, f, indent=2)

    # generate simple HTML
    # helper formatting routines for HTML cells
    def fmt_status(fq):
        st = coverage.get('perRpcStatus', {}).get(fq, {})
        if not st:
            return ''
        parts = [f"{k}: {v}" for k, v in sorted(st.items(), key=lambda x: (-x[1], x[0]))]
        return '<br/>'.join(parts)

    def fmt_scenarios(fq):
        sc = coverage.get('perRpcScenarios', {}).get(fq, {})
        if not sc:
            return ''
        parts = [f"{k}: {v}" for k, v in sorted(sc.items(), key=lambda x: (-x[1], x[0]))]
        return '<br/>'.join(parts)

    def fmt_payload(fq):
        pv = coverage.get('perRpcPayloadValid', {}).get(fq, {})
        if not pv:
            return ''
        parts = []
        for k in ('valid', 'invalid', 'unknown'):
            if pv.get(k, 0):
                parts.append(f"{k}: {pv.get(k)}")
        return '<br/>'.join(parts)

    def fmt_latency(fq):
        lat = coverage.get('perRpcLatency', {}).get(fq)
        if not lat:
            return ''
        return f"count: {lat.get('count')}<br/>p50: {lat.get('p50')} ms<br/>p90: {lat.get('p90')} ms<br/>p99: {lat.get('p99')} ms"

    def fmt_status_coverage(fq):
        pc = coverage.get('perRpcStatusCoverage', {}).get(fq)
        if not pc:
            # fallback to raw status counts if coverage not configured
            return fmt_status(fq)
        parts = []
        parts.append(f"expected: {pc.get('expectedCount')}")
        parts.append(f"observed: {pc.get('observedMatchedCount')}")
        pct = pc.get('percent')
        parts.append(f"percent: {pct}%")
        if pc.get('observedMatched'):
            parts.append('codes: ' + ','.join(pc.get('observedMatched')))
        return '<br/>'.join(parts)

    # build rows for table
    rows = []
    for e in joined:
        proto = e['proto']
        fq = normalize_fqname(proto)
        rows.append('<tr>' +
                    f'<td>{proto.get("package","")}</td>' +
                    f'<td>{proto.get("service","")}</td>' +
                    f'<td>{proto.get("method","")}</td>' +
                    f'<td>{fq}</td>' +
                    f'<td>{e["observedCount"]}</td>' +
                    f'<td>{fmt_status_coverage(fq)}</td>' +
                    f'<td>{fmt_scenarios(fq)}</td>' +
                    f'<td>{fmt_payload(fq)}</td>' +
                    f'<td>{fmt_latency(fq)}</td>' +
                    '</tr>')

    html = f"""
<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <title>gRPC Coverage Report</title>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial; margin: 20px; }}
    table {{ border-collapse: collapse; width: 100%; }}
    th, td {{ border: 1px solid #ddd; padding: 8px; }}
    th {{ background: #f2f2f2; text-align: left; }}
    tr:hover {{ background: #f9f9f9; }}
    .badge {{ display:inline-block; padding:4px 8px; border-radius:4px; background:#eee; margin-right:8px; }}
    .fail {{ background:#fdd; color:#900; }}
    .pass {{ background:#dfd; color:#070; }}
  </style>
</head>
<body>
  <h1>gRPC Coverage Report</h1>
  <p>Generated: {coverage['generatedAt']}</p>
    <p>
        <span class="badge">Total RPCs: {coverage['totalRpcCount']}</span>
        <span class="badge">Observed RPCs: {coverage['observedRpcCount']}</span>
        <span class="badge">Uncovered RPCs: {coverage['uncoveredRpcCount']}</span>
        <span class="badge">Coverage: {coverage['coveragePercent']}%</span>
    </p>
    {"" if not coverage.get('statusCoverage') else (
        '<div>' +
        '<h2>Status-code coverage</h2>' +
        f"<p>Expected statuses: {', '.join(coverage['statusCoverage'].get('expectedStatuses', []))} &nbsp; Observed: {', '.join(coverage['statusCoverage'].get('observedStatuses', []))} &nbsp; Coverage: {coverage['statusCoverage'].get('percent')}%</p>" +
        '</div>'
    )}
  <h2>Uncovered RPCs</h2>
  <ul>
"""
    if uncovered:
        for u in uncovered:
            html += f"    <li>{u.get('fullyQualifiedName') or (u.get('package','') + '.' + u.get('service','') + '/' + u.get('method',''))}</li>\n"
    else:
        html += "    <li><em>None — all RPCs were observed</em></li>\n"

    html += """
  </ul>

    <h2>All RPCs</h2>
    <table>
        <thead>
            <tr>
                <th>Package</th>
                <th>Service</th>
                <th>Method</th>
                <th>FQName</th>
                <th>ObservedCount</th>
                <th>Status coverage</th>
                <th>Scenarios</th>
                <th>Payload valid</th>
                <th>Latency summary</th>
            </tr>
        </thead>
        <tbody>
"""
    html += '\n'.join(rows)
    html += """
    </tbody>
  </table>

  <p>Download artifacts: <a href="./coverage_summary.json">coverage_summary.json</a> | <a href="./uncovered_rpcs.json">uncovered_rpcs.json</a> | <a href="./joined_calls.json">joined_calls.json</a></p>
</body>
</html>
"""

    with open(html_path, 'w', encoding='utf-8') as f:
        f.write(html)

    print('Wrote:', joined_path, uncovered_path, summary_path, html_path)


if __name__ == '__main__':
    main()
