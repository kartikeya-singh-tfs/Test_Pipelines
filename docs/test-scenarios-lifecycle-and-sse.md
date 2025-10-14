# Acquisition Lifecycle & SSE Test Scenarios

This document captures ONLY the scenarios and logged output patterns exercised by three specific integration tests:

1. `testSseWithEventSourceAndOkHttp` (REST -> Sparkline SSE)
2. `replicateAcquisitionLifecycle` (gRPC lifecycle -> Generic SSE)
3. `replicateLifecycleWithoutSse` (Pure gRPC lifecycle against mock, no SSE)

---
## 0. Shared Context
| Concept | Real Service | Mock Service |
|---------|--------------|--------------|
| gRPC Host/Port | localhost:51640 (plaintext in test) | localhost:50051 (plaintext) |
| HTTPS/SSE Host | https://localhost:61350 | N/A |
| Sequence Id (examples) | `001a32b5-a010-4513-a355-c0d4a21edf7c` (tests) | Same constant reused |
| Sample Id | `61f6b9b7-7149-49b9-be33-207b2d9da941` | Same constant reused |
| Channels / SSE Endpoints | `/api/acquisition/v1/sse` (generic), `/api/acquisition/v1/sseSparklineData` | Not implemented |
| Lifecycle RPCs | `submitSampleStream`, `sequenceStateEvent`, `acquisitionStateEvent`, `deviceStateEvent` | Same RPC names (mock impl returns EmptyResponse / static stream) |

---
## 1. Scenario A: `testSseWithEventSourceAndOkHttp`
**Goal**: Validate that submitting a sequence via REST triggers Sparkline (or related) SSE events on endpoint `/api/acquisition/v1/sseSparklineData`.

### Flow
1. Establish SSL-trusting OkHttp client (trust-all for self-signed).
2. Open SSE connection (OkHttp EventSource) to `.../sseSparklineData`.
3. Send REST POST to `https://localhost:61350/api/acquisition/v1/sequence` with JSON body containing the sequence definition (1 sample).
4. Collect SSE events for ~20 seconds.
5. Cancel EventSource and dump captured events.

### Key Input Payload (abbreviated)
```
{
  "id": "001a32b5-a010-4513-a355-c0d4a21edf7c",
  "samples": [ { "id": "61f6b9b7-7149-49b9-be33-207b2d9da941", ... } ]
}
```

### Representative Log Lines
```
[REST] Sending POST to trigger SSE event...
[REST] POST Response code: 200
[SSE] Event: {"Type":"SparklineData", ...}
[SSE] Event: {"Type":"SparklineData", ...}
[SSE] Recorded events:
[SSE] {"Type":"SparklineData", ...}
```
(Exact JSON varies; test currently does not assert structure.)

### Assertions
- Only HTTP 200 is asserted. No explicit assertion on SSE content (improvement opportunity).

### Purpose Summary
Ensures data path from REST sequence submission to real-time plot data SSE channel is active.

---
## 2. Scenario B: `replicateAcquisitionLifecycle`
**Goal**: Reproduce a simplified lifecycle progression via gRPC and correlate that it produces lifecycle-related SSE events on the generic SSE endpoint `/api/acquisition/v1/sse`.

### Flow
1. Create trust-all HTTPS OkHttp client & subscribe (EventSource) to generic SSE endpoint.
2. Create plaintext gRPC channel to real service on port 51640.
3. Start async server-streaming call `submitSampleStream(EmptyRequest)`; log any `SequenceReplyStream` messages.
4. Issue ordered unary RPCs:
   - Sequence states: `MethodValidationOk`, `SequenceStart`, `SampleStart`, `DataFileCreate`.
   - Acquisition states: `ReadBarcode`, `SendMethod`, (then SampleComplete as sequence state), `WaitMethodReady`, `WaitStartSlaves`, `WaitStartMaster`, `WaitContactClosure`.
   - Device states embedded in `DeviceEvent`: `ReadyForRun`, then after `Acquire` acquisition state, `Running`, `ReadyToDownload`.
   - Post run: `PostRun`, `SequenceComplete`, `SequenceCompletedFilesMoved`, `Ready`.
5. Wait ~1.5s for stream events, close gRPC channel.
6. Await latch for minimum 3 SSE events (SequenceStatus, SampleStatus, DeviceEvent) up to 5s.
7. Cancel SSE subscription; assert the presence of each expected type in captured event list.

### Representative gRPC Log Lines
```
[Client] SequenceStateEvent ack | context=MethodValidationOk sample=00000000-... | resp class=EmptyResponse
[Client] AcquisitionStateEvent ack | context=ReadBarcode | resp class=EmptyResponse
[Client] DeviceStateEvent ack | context=ReadyForRun | resp class=EmptyResponse
```

### Representative SSE Log Lines
```
[SSE] {"Type":"SequenceStatus","SequenceState":"SequenceStart", ...}
[SSE] {"Type":"SampleStatus","SequenceState":"SampleStart", ...}
[SSE] {"Type":"DeviceEvent","DeviceStates":[{"DeviceState":"Running",...}]}
```
(Order not guaranteed; detection is substring-based.)

### Assertions
- Boolean checks that at least one of each: SequenceStatus, SampleStatus, DeviceEvent is present.

### Purpose Summary
Validates end-to-end correlation: lifecycle RPC invocation triggers lifecycle SSE fan-out for sequence, sample, and device domains.

### Limitations / Gaps
- No ordering or temporal assertion.
- SSE JSON not parsed structurally (string contains only).
- Streaming `submitSampleStream` response not validated beyond logging.

---
## 3. Scenario C: `replicateLifecycleWithoutSse`
**Goal**: Exercise the same lifecycle RPC sequence against a mock Acquisition gRPC server (port 50051) without any SSE involvement, ensuring mock service contract compatibility.

### Flow
1. Open plaintext gRPC channel to mock server on port 50051.
2. Start async `submitSampleStream(EmptyRequest)`; mock emits a static `SequenceReplyStream` (logged).
3. Execute identical ordered sequence of lifecycle RPCs as Scenario B (Sequence, Acquisition, Device, Post run states).
4. Sleep briefly (800 ms) to allow stream reception.
5. Shutdown channel; assert test success (trivial assert true).

### Representative Log Lines
```
[MockLifecycle] SequenceReplyStream: id=001a32b5-a010-4513-a355-c0d4a21edf7c samples=1
[MockLifecycle] SequenceStateEvent sent state=MethodValidationOk ackClass=EmptyResponse
[MockLifecycle] AcquisitionStateEvent sent state=ReadBarcode ackClass=EmptyResponse
[MockLifecycle] DeviceStateEvent sent state=ReadyForRun ackClass=EmptyResponse
...
```
(Same pattern for remaining states.)

### Assertions
- Only a final `Assert.assertTrue(true, ...)` — effectively no validation beyond absence of exceptions.

### Purpose Summary
Confirms mock service implements the lifecycle RPC surface permitting future isolated development or faster contract tests without SSE complexity.

### Limitations / Gaps
- No validation of mock stream contents (only prints).
- Does not assert count or order of acks.
- No negative-path or error condition exercised.

---
## 4. Comparative Summary
| Aspect | Scenario A (REST→Sparkline SSE) | Scenario B (gRPC→Lifecycle SSE) | Scenario C (Mock gRPC Only) |
|--------|----------------------------------|----------------------------------|-----------------------------|
| Trigger Mechanism | REST POST (sequence submit) | gRPC unary lifecycle calls | gRPC unary lifecycle calls |
| SSE Endpoint Used | `/sseSparklineData` | `/sse` | None |
| Event Types Observed | SparklineData (assumed) | SequenceStatus, SampleStatus, DeviceEvent | N/A |
| Streaming RPC | None | `submitSampleStream` (async) | `submitSampleStream` (async) |
| Assertions Strength | HTTP 200 only | Presence of 3 event categories | Trivial always true |
| Purpose | Validate sparkline publish path | Correlate lifecycle gRPC→SSE | Contract sanity (mock) |

---
## 5. Improvement Opportunities (Across These Tests)
1. Parse SSE JSON (Jackson/Gson) for schema correctness instead of substring scanning.
2. Add ordering/time window assertions (e.g., SequenceStart must precede SampleStart).
3. Parameterize ports & base URLs via system properties to avoid hardcoding.
4. Strengthen mock test by asserting number of acks and expected stream payload.
5. Add error-path tests (e.g., invalid state transition) and confirm server returns appropriate gRPC status.
6. Capture and assert at least one sparkline payload structure (fields, numeric constraints).
7. Introduce correlation ID (if added later) to tie SSE events back to triggering RPC calls for richer diagnostics.

---
## 6. Quick Reference: State Sequence (B & C)
```
Sequence: MethodValidationOk → SequenceStart → SampleStart → DataFileCreate
Acquisition: ReadBarcode → SendMethod
Sequence: SampleComplete
Acquisition: WaitMethodReady → WaitStartSlaves → WaitStartMaster → WaitContactClosure
Device: ReadyForRun
Acquisition: Acquire
Device: Running → ReadyToDownload
Acquisition: PostRun
Sequence: SequenceComplete → SequenceCompletedFilesMoved
Acquisition: Ready
```

---
## 7. Glossary
- **EmptyResponse**: Protobuf ack message with no fields (extensible).
- **SequenceReplyStream**: Server-streamed message (initial sequence/sample list metadata).
- **SSE**: Server-Sent Events, text/event-stream channel for push JSON payloads.
- **Lifecycle RPCs**: Domain-specific transitions for acquisition orchestration (sequence, acquisition, device).

---
## 8. Status Snapshot
Current tests establish connectivity and basic signal propagation but lack deep semantic validation. This document should serve as a baseline for planning the next layer of assertion hardening.

---
*End of document.*
