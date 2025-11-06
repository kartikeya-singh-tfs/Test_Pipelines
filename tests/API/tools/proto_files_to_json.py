#!/usr/bin/env python3
"""
Fallback proto parser: scans .proto files under tests/API/src/main/proto and emits a proto_rpcs.json
This is a simple parser and won't resolve imports; it's intended as a pragmatic fallback when 'protoc' is not available.

Output format matches proto_to_json.py: list of {package, service, method, streamingType, requestType, responseType, protoFile, fullyQualifiedName}
"""
import re
import json
import os

ROOT = '.'
OUT = os.path.join('tests', 'API', 'tools', 'proto_rpcs.json')

rpc_re = re.compile(r'rpc\s+(\w+)\s*\(\s*(stream\s+)?([\w\.]+)\s*\)\s*returns\s*\(\s*(stream\s+)?([\w\.]+)\s*\)')
package_re = re.compile(r'^\s*package\s+([\w\.]+)\s*;')
service_re = re.compile(r'^\s*service\s+(\w+)\s*\{')

services = []

for root, dirs, files in os.walk(ROOT):
    # skip some large or irrelevant folders
    if any(part in ('target', '.git', 'node_modules', 'bin', 'obj') for part in root.split(os.sep)):
        continue
    for f in files:
        if not f.endswith('.proto'):
            continue
        path = os.path.join(root, f)
        pkg = ''
        current_service = None
        try:
            with open(path, 'r', encoding='utf-8') as fh:
                for line in fh:
                    if pkg == '':
                        m = package_re.match(line)
                        if m:
                            pkg = m.group(1)
                    msvc = service_re.match(line)
                    if msvc:
                        current_service = msvc.group(1)
                        continue
                    if current_service:
                        m = rpc_re.search(line)
                        if m:
                            method = m.group(1)
                            client_stream = bool(m.group(2))
                            req = m.group(3)
                            server_stream = bool(m.group(4))
                            resp = m.group(5)
                            if client_stream and server_stream:
                                st = 'BIDI'
                            elif client_stream:
                                st = 'CLIENT_STREAMING'
                            elif server_stream:
                                st = 'SERVER_STREAMING'
                            else:
                                st = 'UNARY'
                            fq = f"{pkg}.{current_service}/{method}" if pkg else f"{current_service}/{method}"
                            services.append({
                                'package': pkg,
                                'service': current_service,
                                'method': method,
                                'streamingType': st,
                                'requestType': req.lstrip('.'),
                                'responseType': resp.lstrip('.'),
                                'protoFile': os.path.relpath(path).replace('\\', '/'),
                                'fullyQualifiedName': fq
                            })
                    if line.strip().startswith('}'):  # naive end-of-service
                        current_service = None
        except Exception:
            # ignore unreadable files
            continue

os.makedirs(os.path.dirname(OUT), exist_ok=True)
with open(OUT, 'w', encoding='utf-8') as o:
    json.dump(services, o, indent=2)
print('Wrote', OUT)
