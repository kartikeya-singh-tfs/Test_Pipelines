#!/usr/bin/env python3
"""
Read a protoc descriptor_set (file produced with `protoc --descriptor_set_out=...`) and write a JSON list of RPC methods.
Output format: [{package, service, method, streamingType, requestType, responseType, protoFile}, ...]
"""
import sys
import json
from google.protobuf import descriptor_pb2


def streaming_type(m):
    if m.client_streaming and m.server_streaming:
        return "BIDI"
    if m.client_streaming:
        return "CLIENT_STREAMING"
    if m.server_streaming:
        return "SERVER_STREAMING"
    return "UNARY"


def main():
    if len(sys.argv) < 3:
        print("Usage: proto_to_json.py <descriptor_set> <out.json>")
        sys.exit(2)
    desc_path = sys.argv[1]
    out_path = sys.argv[2]

    fds = descriptor_pb2.FileDescriptorSet()
    with open(desc_path, 'rb') as f:
        fds.ParseFromString(f.read())

    services = []
    for fd in fds.file:
        pkg = fd.package
        for svc in fd.service:
            svc_name = svc.name
            for method in svc.method:
                services.append({
                    "package": pkg,
                    "service": svc_name,
                    "method": method.name,
                    "streamingType": streaming_type(method),
                    "requestType": method.input_type.lstrip('.'),
                    "responseType": method.output_type.lstrip('.'),
                    "protoFile": fd.name
                })
    with open(out_path, 'w') as o:
        json.dump(services, o, indent=2)


if __name__ == '__main__':
    main()
