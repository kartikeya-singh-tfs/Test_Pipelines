joiner_html.py
================

Purpose
-------
This tool joins the canonical proto RPC list (`proto_rpcs.json`) with observed telemetry (`observed_calls.jsonl`) produced by the Observability client interceptor and emits JSON artifacts plus a small static HTML coverage report suitable for uploading as CI artifacts.

Usage
-----

1. Generate proto descriptor set and proto_rpcs.json (one-time per build):
   protoc --descriptor_set_out=services.desc --include_imports -Iproto protos/*.proto
   python proto_to_json.py services.desc > proto_rpcs.json

2. Run tests with telemetry writing enabled (example using Maven):
   mvn test -Dobserved.calls=observed_calls.jsonl

3. Run the joiner and generate HTML:
   python joiner_html.py --proto proto_rpcs.json --observed observed_calls.jsonl --out-dir artifacts

4. Upload `artifacts/coverage_report.html` (and JSON artifacts) as CI build artifacts for easy browsing.

CI snippet (GitHub Actions, conceptual)
--------------------------------------
- name: Generate proto JSON
  run: |
    protoc --descriptor_set_out=services.desc --include_imports -Iproto protos/*.proto
    python tests/API/tools/proto_to_json.py services.desc > proto_rpcs.json

- name: Run tests with telemetry
  run: mvn -Dobserved.calls=observed_calls.jsonl test

- name: Join telemetry and generate HTML
  run: python tests/API/tools/joiner_html.py --proto proto_rpcs.json --observed observed_calls.jsonl --out-dir artifacts

- name: Upload coverage HTML
  uses: actions/upload-artifact@v3
  with:
    name: grpc-coverage-report
    path: artifacts/coverage_report.html

Notes
-----
- `observed_calls.jsonl` is newline-delimited JSON produced by `ObservabilityClientInterceptor`.
- The HTML is intentionally self-contained and suitable for browser viewing as a CI artifact.
