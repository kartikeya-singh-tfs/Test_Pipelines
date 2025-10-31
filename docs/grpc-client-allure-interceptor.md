# GrpcClientAllureInterceptor — Design, Responsibilities and Guidance

Version: 1.0
Date: 2025-10-27
Author: Automated documentation (based on latest analysis)

## Purpose
A concise reference for the test-only `GrpcClientAllureInterceptor` used in the repository. It captures client-side gRPC requests/responses and writes human-readable Allure attachments for test reporting and debugging.

## Where it's used
- Attached in tests via `ClientInterceptors.intercept(rawChannel, new GrpcClientAllureInterceptor())`.
- Stubs created from the intercepted channel automatically pass through the interceptor (no call-site changes required).

## Responsibilities
1. Intercept outgoing calls
   - Wraps the `Channel` so each RPC newCall() flows through the interceptor.
   - Overrides `sendMessage(ReqT message)` to capture outbound messages.

2. Intercept incoming responses
   - Wraps the `ClientCall.Listener` in `start(...)` to capture `onMessage(RespT)` invocations.
   - Captures `onClose(Status, Metadata)` and adds a textual error attachment when the status is not OK.

3. Serialize payloads
   - If the message is a Protobuf `Message`, serialize to JSON using `JsonFormat.printer().includingDefaultValueFields()` for readable attachments.
   - Falls back to `toString()` if serialization fails or object is not a Protobuf `Message`.

4. Create Allure attachments
   - Adds attachments with names like: `<methodFullName> - Request#N` and `<methodFullName> - Response#N`.
   - Uses `Allure.addAttachment(name, mimeType, InputStream, extension)` to produce attachments visible in the Allure UI.

5. Enforce bounds
   - Truncates attachments at `maxAttachmentSizeBytes` (default 64KB).
   - Limits per-stream attachments to `maxStreamAttachments` (default 100) to avoid unbounded output.

6. Fail-safe and non-invasive behavior
   - Attachment errors are caught and recorded via `Allure.step(...)`; interceptor must not change RPC semantics or throw to callers.

## Contract
- Inputs:
  - gRPC `Channel` to intercept
  - Outgoing/Incoming messages (ReqT/RespT), typically generated protobuf `Message` instances
  - Optional config: `maxStreamAttachments`, `maxAttachmentSizeBytes`
- Outputs:
  - Side-effect: Allure attachments written to the active test result (appear under `tests/API/allure-results/` during runs)
- Success:
  - Readable attachments appear in Allure UI and do not alter RPC behavior
- Failure modes:
  - Large streams or payloads may overflow disk/CI quotas if not sampled
  - JsonFormat failures fall back to `toString()`; unexpected message types may be unhelpful

## Edge cases & considerations
- Streaming volume: high-frequency streams will produce many attachments — use sampling or summarization.
- Sensitive data: payloads may contain secrets or PII; add masking/redaction before attaching.
- Background threads: attachments emitted after test completion may not associate with the test; ensure lifecycle alignment.
- Non-protobuf payloads: fallback to `toString()` or base64-encode raw bytes where appropriate.
- Thread-safety: counters are `AtomicInteger` but any added buffers must be thread-safe and keyed to test context.

## Recommendations & improvements (practical)
- Sampling strategy: add options FIRST_N / LAST_N / SAMPLE_RATE / SUMMARY_ONLY to reduce noise.
- Redaction hook: accept a functional callback to scrub fields before attachment.
- Aggregation: provide an option to attach a single JSON summary per RPC (first, last, count, sizes) instead of one file per message.
- Toggle via environment variable: `ALLURE_GRPC_CAPTURE` (values: `off|errors|sample|all`) so CI can limit captures.
- Companion server interceptor: add a ServerInterceptor for server-side perspective and cross-checking.

## Quick checklist for validation
- [ ] Unary request -> attachment `Method - Request#1` produced
- [ ] Unary response -> attachment `Method - Response#1` produced
- [ ] Streaming responses -> attachments up to `maxStreamAttachments` produced
- [ ] Non-OK gRPC status -> `gRPC Error: <method>` text attachment produced
- [ ] Large payloads are truncated to `maxAttachmentSizeBytes`
- [ ] JsonFormat used for protobuf messages; fallback is robust

## Example usage (test-side)
```java
ManagedChannel rawChannel = ManagedChannelBuilder.forAddress("localhost", 51640).usePlaintext().build();
Channel intercepted = ClientInterceptors.intercept(rawChannel, new GrpcClientAllureInterceptor());
AcquisitionGrpc.AcquisitionBlockingStub blocking = AcquisitionGrpc.newBlockingStub(intercepted);
// Call blocking.sequenceStateEvent(event) as usual; attachments appear in Allure
```

## Where to find artifacts
- Raw Allure results written during test run:
  - `tests/API/allure-results/`
- Generated HTML (Allure CLI):
  - `tests/API/allure-report/` (serve via HTTP to view in browser)
- Attachments in generated HTML:
  - `tests/API/allure-report/data/attachments/`

## Small follow-ups (optional)
- Add a small sampling feature and a redaction callback to the interceptor.
- Add a TestNG listener that aggregates the interceptor’s captured events into a single per-test summary attachment.

---
For edits or to include a sample attachment snippet in this doc, tell me which attachment you'd like embedded (e.g., SSE summary or specific Request# attachment) and I will paste it in.
