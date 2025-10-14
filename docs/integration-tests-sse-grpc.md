Integration tests: SSE + gRPC + REST (detailed reference)

Purpose
-------
This document describes the purpose, design, and implementation details of the integration tests that exercise the OPAL acquisition stack using a combination of:
- Server-Sent Events (SSE) listeners (HTTP event stream)
- gRPC client calls (unary and streaming)
- REST POST triggers (RestAssured / OkHttp)

It explains each test in the `tests/API` module, the technical building blocks used, deterministic synchronization choices, TLS/ALPN considerations, run instructions, debugging tips, and suggested CI configuration.

Tests covered
-------------
1. `testSseWithEventSourceAndRestAssured` - (OpalRestApiTest.java)
   - Open an SSE subscription using OkHttp (okhttp-sse), wait for the SSE connection to be established, then trigger an HTTP POST (RestAssured) to create a Sequence. Validate that the server emits SSE events in response to the POST.
   - Purpose: verify SSE delivery in response to REST trigger and ensure deterministic ordering (fix race conditions), plus normalize TLS/ALPN footprint between clients.

2. `replicateAcquisitionLifecycle` - (GrpcCallReplicationTest.java)
   - Reproduce the server-level acquisition lifecycle by making a sequence of gRPC calls (both streaming and unary) that reflect production client calls. At the same time, open an SSE subscription and assert that SSE lifecycle events are emitted that match the gRPC-driven transitions.
   - Purpose: validate correlation between internal gRPC state transitions and externally visible SSE lifecycle events.

3. `replicateLifecycleWithoutSse` - (MockAcquisitionServiceTest.java)
   - Execute the same gRPC call sequence against a mock acquisition service (no SSE observation). Verifies RPC handlers, `EmptyResponse` acknowledgements, and server-streaming reply behavior on the mock service.
   - Purpose: verify RPC contract and mock behavior independently of the SSE channel.

High-level contract for tests
----------------------------
Inputs
- A running server or mock that exposes the SSE endpoints and/or gRPC services on the ports expected by the tests.
- For HTTP POST tests: JSON payloads (SAMPLE_JSON) embedded in the test.
- For gRPC tests: compiled proto-generated Java classes on the classpath (protoc-generated stubs).

Outputs / success criteria
- HTTP POST returns 200 (or server-specific success response) and SSE subscription receives lifecycle events consistent with the POST.
- gRPC unary calls return `EmptyResponse` ack objects; streaming RPCs produce `SequenceReplyStream` messages as required.
- Assertions in tests confirm the presence of expected SSE types (SequenceStatus, SampleStatus, DeviceEvent etc.).

Technical building blocks and rationale
-------------------------------------
1. Server-Sent Events (SSE)
- SSE is a server -> client streaming mechanism over HTTP using the `text/event-stream` content type.
- Tests use OkHttp's `okhttp-sse` EventSource implementation because it handles event parsing and reconnection, and sits on a modern, well-supported HTTP client stack that supports HTTP/2.

2. OkHttp + okhttp-sse details
- OkHttp supports both HTTP/1.1 and HTTP/2; tests typically set explicit protocols:
  `.protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))`
- `EventSources.createFactory(okHttpClient)` provides a factory to open `EventSource`s.
- `EventSourceListener` callbacks (`onOpen`, `onEvent`, `onFailure`) are used to collect events and drive synchronization.

3. RestAssured (Apache HttpClient) vs OkHttp TLS footprint
- RestAssured uses Apache HttpClient and JSSE (the JDK SSL stack) which can negotiate ALPN and TLS differently from OkHttp.
- To eliminate TLS/ALPN differences in tests, a helper (`RestAssuredSslConfigurer`) configures RestAssured to use a `CloseableHttpClient` backed by the exact same `SSLContext` used by OkHttp in the test. This ensures consistent ALPN negotiation behavior and removes transient `SocketException` cases that were caused by TLS/ALPN mismatches.

4. SSLContext and TrustManager (test-only trust-all)
- For test environments (self-signed server certs), tests create a test `SSLContext` that trusts all certificates via a permissive `X509TrustManager`.
- Both OkHttp and RestAssured/Apache client are configured to reuse this `SSLContext` so that certificate verification is consistent across clients and machines.

5. ALPN / HTTP/2
- ALPN is required to negotiate HTTP/2 over TLS. Differences in provider (JDK JSSE, Conscrypt, BoringSSL via netty-tcnative) can cause different negotiation results and intermittent failures.
- Recommended approaches:
  - Use the same `SSLContext` for both clients (as implemented).
  - Prefer modern JDKs (11/17/21) for built-in ALPN/TLS support.
  - For grpc-java in production: consider netty-tcnative (boringssl) for OpenSSL semantics and improved ALPN stability.

6. gRPC, ManagedChannel and stubs
- `ManagedChannelBuilder.forAddress(host, port)` creates the channel.
- `AcquisitionGrpc.newBlockingStub(channel)` for synchronous unary RPCs which return `EmptyResponse` ack objects.
- `AcquisitionGrpc.newStub(channel)` for asynchronous streaming RPCs with `StreamObserver` callbacks.
- Streaming RPCs are used in tests to model server->client streams (e.g., `submitSampleStream`).

7. Deterministic synchronization (avoid Thread.sleep)
- Replacing blind sleeps with synchronization primitives (e.g., `CountDownLatch`) is critical to determinism.
- Example: wait for SSE `onOpen` using a `CountDownLatch` before sending the POST. This avoids the race where the POST arrives before the server has completed subscription processing and caused intermittent connection aborts.

Test flow examples
------------------
A. `testSseWithEventSourceAndRestAssured` (flow)
1. Create a permissive `SSLContext` and OkHttp client using it.
2. Open an SSE `EventSource` to the SSE endpoint and wait for `onOpen()` via `CountDownLatch`.
3. Configure RestAssured to use the same `SSLContext` (RestAssuredSslConfigurer.configure(sc)).
4. POST the `SAMPLE_JSON` to the sequence endpoint.
5. Collect SSE events for a short period; cancel SSE; assert events contain expected lifecycle notifications.

B. `replicateAcquisitionLifecycle` (flow)
1. Create `SSLContext` and open SSE EventSource to `.../sse`.
2. Build a gRPC `ManagedChannel` to the acquisition service.
3. Start a server streaming call `submitSampleStream(EmptyRequest)` with an async `StreamObserver` to receive `SequenceReplyStream` items.
4. Use blocking stubs to send `sequenceStateEvent`, `acquisitionStateEvent`, `deviceStateEvent` messages in production-like order.
5. After sending sequence, sleep briefly to let async messages arrive, then shutdown channel and cancel SSE.
6. Assert that SSE events include SequenceStatus, SampleStatus and DeviceEvent types.

C. `replicateLifecycleWithoutSse` (flow)
1. Build gRPC channel to mock server (e.g., port 50051).
2. Start `submitSampleStream()` async to receive any replies.
3. Send the sequence of state events using blocking stubs and assert no exceptions and expected console acknowledgements.

Environment & prerequisites
---------------------------
- JDK: prefer modern LTS (17 or 21). These have built-in ALPN and TLS 1.3 support. Avoid JDK 8 unless you add Conscrypt or netty-tcnative.
- Maven installed (project uses Maven builds and Surefire)
- `protoc` and generated stubs: the repository includes gRPC-generated stubs; if you change protos, run `protoc` (a recommended protoc binary is stored under `tools/protoc` in this repo).
- Mock servers: some tests require a mock acquisition service to be running:
  - Example: dotnet mock agent in `tests/MockAcquisitionAgent/MockAcquisitionAgent` can be started with `dotnet run`.
- Ports used by tests (defaults in tests):
  - SSE: 61350 (endpoints `/api/acquisition/v1/sse`, `/api/acquisition/v1/sseSparklineData`)
  - RealTimePlot / Acquisition RPC: 51640
  - Mock acquisition default: 50051

How to run (PowerShell examples)
------------------------------
# From the repo, run a single TestNG method in the tests/API module (PowerShell quoting):
```powershell
Set-Location -Path 'C:\OPAL API\tests\API'
# run a single test method
mvn -Dtest="com.example.grpc.RawDataAccessServiceTest#testUploadSparklineData" -DfailIfNoTests=false test

# run the SSE + RestAssured test
mvn -Dtest="com.example.rest.OpalRestApiTest#testSseWithEventSourceAndRestAssured" -DfailIfNoTests=false test

# run replicateAcquisitionLifecycle
mvn -Dtest="com.exampple.grpc.GrpcCallReplicationTest#replicateAcquisitionLifecycle" -DfailIfNoTests=false test
```

Start mock acquisition server (example - repo path may vary):
```powershell
Set-Location -Path 'C:\OPAL API\tests\MockAcquisitionAgent\MockAcquisitionAgent'
dotnet run
```

Troubleshooting (common failures & remedies)
-------------------------------------------
1. Connection refused (gRPC client cannot connect)
   - Confirm mock server is running and listening on the expected port (`netstat -ano | Select-String ":50051"`).
   - Ensure firewall/AV is not blocking local listening sockets.
   - Check mock startup logs for errors.

2. Intermittent SocketException during RestAssured POST
   - Often caused by a race where POST arrives before SSE subscription is fully attached on the server.
   - Fix: wait for SSE `onOpen` (CountDownLatch) before sending POST; align RestAssured SSLContext with OkHttp.

3. ALPN/TLS mismatches (SChannel errors on Windows)
   - Use JDK 11/17/21; consider adding Conscrypt as a provider for consistent OpenSSL-like behavior across platforms.
   - Alternatively, configure RestAssured to reuse the OkHttp `SSLContext` to normalize client behavior.

4. StreamResetException: "stream was reset: CANCEL"
   - Typically benign when the test actively cancels the SSE event source or shuts down the HTTP/2 connection.
   - Investigate only if it appears unexpectedly during test steady-state.

5. GroovyCastException when setting RestAssured client factory
   - Use an explicit `HttpClientConfig.HttpClientFactory` anonymous class rather than a lambda to avoid runtime cast issues when running under Surefire/Groovy.

Debugging tips
--------------
- Capture TLS/ALPN debug logs when you suspect ALPN issues:
  - Add to Surefire `argLine` or pass on the mvn command:
    `-Djavax.net.debug=ssl,handshake,verbose,alpn`
- Print the negotiated protocol (HTTP/2 vs HTTP/1.1) in OkHttp logging or examine server TLS logs.
- Reduce SSE log volume by aggregating events in a list and only printing a summary in tests (useful for CI to avoid huge logs).

CI recommendations
------------------
- Pin the JDK to a consistent LTS (JDK 17 or 21) across developer machines and CI images to avoid TLS/ALPN differences.
- Optionally add Conscrypt to the test JVM classpath in CI to normalize TLS behavior across hosts and operating systems.
- Ensure `protoc` is available (or generated sources are checked in) in CI. The repository already contains a `tools/protoc` location used by the tests' Maven config.
- Provide a step that starts the necessary mock services (or run them as separate services in CI) before running tests that require them.

Suggested follow-ups / improvements
----------------------------------
- Centralize creation of test `SSLContext` and a single `RestAssuredSslConfigurer` usage so all REST tests reuse the same TLS setup.
- Add a `tests/API/README.md` with the short runbook: how to start mocks, which ports to expect, and common `mvn` commands.
- Add lightweight acceptance markers: tests could emit summary artifacts (small JSON) indicating which SSE types were seen; CI can parse these instead of raw logs.
- Optionally wire Conscrypt into the test runs (maven surefire) to reduce ALPN/TLS platform variance if you see sporadic failures across OSes.

Appendix: key files referenced in tests
--------------------------------------
- `src/test/java/com/example/rest/OpalRestApiTest.java` - contains `testSseWithEventSourceAndRestAssured` and other REST/SSE tests.
- `src/test/java/com/exampple/grpc/GrpcCallReplicationTest.java` - contains `replicateAcquisitionLifecycle`.
- `src/test/java/com/exampple/grpc/MockAcquisitionServiceTest.java` - contains `replicateLifecycleWithoutSse`.
- `src/test/java/com/example/rest/RestAssuredSslConfigurer.java` - helper to configure RestAssured to use a `CloseableHttpClient` backed by test `SSLContext`.

If you want, I can now:
- Add a `tests/API/README.md` containing a trimmed version of this runbook, or
- Refactor tests to use a centralized test TLS/RestAssured helper across all relevant tests, or
- Add the suggested Conscrypt wiring in CI and re-run failing tests under that provider.


---
Generated on: 2025-10-09
