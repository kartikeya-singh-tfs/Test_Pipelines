# Mock Acquisition Server & Test Command Quick Reference

This cheat sheet shows how to start the mock gRPC acquisition server and run the Java integration tests (single classes or individual methods) from PowerShell on Windows.

---
## 1. Components & Ports

| Component | Purpose | Default Port | Protocol / Security |
|-----------|---------|--------------|---------------------|
| Real Opal API (Acquisition) | Actual service under test (lifecycle + SSE) | 51640 (gRPC) / 61350 (HTTPS/SSE) | gRPC plaintext (adjust if TLS added) / HTTPS (self-signed) |
| Mock Acquisition Server | Lightweight mock for lifecycle + plot upload + sample stream | 50051 (override via `MOCK_ACQ_PORT`) | gRPC plaintext |

---
## 2. Start the Mock Acquisition Server

Default port (50051):
```powershell
# From repo root
dotnet run --project "c:/OPAL API/tests/MockAcquisitionAgent/MockAcquisitionAgent/MockAcquisitionAgent.csproj"
```
Custom port (example 50060):
```powershell
$env:MOCK_ACQ_PORT=50060
dotnet run --project "c:/OPAL API/tests/MockAcquisitionAgent/MockAcquisitionAgent/MockAcquisitionAgent.csproj"
```
(Press ENTER in the server console to shut it down.)

Verify it is listening:
```powershell
netstat -ano | findstr :50051   # or your custom port
```
You should see a LISTENING entry; absence means the server is not up.

---
## 3. Running Tests (Maven + TestNG)
All Java test commands are executed from the repository root and target the Maven POM at `tests/API/pom.xml`.

### 3.1 Run Entire Test Suite
```powershell
mvn -f tests/API/pom.xml test
```

### 3.2 Run a Single Test Class
```powershell
# gRPC lifecycle + SSE against REAL server (needs ports 51640 & 61350 up)
mvn -f tests/API/pom.xml -Dtest=GrpcCallReplicationTest test

# Mock lifecycle only (needs mock server on 50051 or overridden port)
mvn -f tests/API/pom.xml -Dtest=MockAcquisitionServiceTest test

# REST + SSE sparkline test (Opal REST/SSE must be running)
mvn -f tests/API/pom.xml -Dtest=OpalRestApiTest test

# REST submission + gRPC streaming correlation test
mvn -f tests/API/pom.xml -Dtest=RestSubmitSequenceStreamTest test
```

### 3.3 Run a Single Test Method
```powershell
# Just the SSE + gRPC lifecycle correlation
mvn -f tests/API/pom.xml -Dtest=GrpcCallReplicationTest#replicateAcquisitionLifecycle test

# Mock-only lifecycle sequence (skips SSE)
mvn -f tests/API/pom.xml -Dtest=MockAcquisitionServiceTest#replicateLifecycleWithoutSse test

# SSE sparkline test method example
mvn -f tests/API/pom.xml -Dtest=OpalRestApiTest#testSseWithEventSourceAndOkHttp test

# REST submit + stream correlation
mvn -f tests/API/pom.xml -Dtest=RestSubmitSequenceStreamTest#restSubmitTriggersStreamSnapshot test
```

### 3.4 Multiple Methods (comma-separated)
```powershell
mvn -f tests/API/pom.xml -Dtest=GrpcCallReplicationTest#replicateAcquisitionLifecycle,MockAcquisitionServiceTest#replicateLifecycleWithoutSse test
```

---
## 4. Pre-Flight Checks (Optional but Recommended)
Add a quick port probe in tests that depend on the mock server to skip instead of failing:
```java
private boolean isPortOpen(String host, int port, int timeoutMs) {
    try (java.net.Socket s = new java.net.Socket()) {
        s.connect(new java.net.InetSocketAddress(host, port), timeoutMs);
        return true;
    } catch (Exception e) { return false; }
}
```
Usage inside a TestNG method:
```java
if (!isPortOpen("localhost", 50051, 400)) {
    throw new org.testng.SkipException("Mock server not running on 50051");
}
```

---
## 5. Troubleshooting Quick Hits

| Symptom | Likely Cause | Fix |
|---------|--------------|-----|
| `StatusRuntimeException: UNAVAILABLE` on port 50051 | Mock server not started | Start server; verify with netstat |
| SSE tests receive 0 events | Lifecycle events not emitted or timing too short | Increase wait or add latch logic |
| HTTP 400 on REST sequence submission (Windows paths) | Improper JSON escaping of backslashes | Use forward slashes or double escape `\\` |
| HTTP/2 assertion fails | Server negotiated HTTP/1.1 | Allow fallback protocols list (HTTP_2, HTTP_1_1) |

---
## 6. Environment Notes
- PowerShell commands use Windows path forms; forward slashes also work with .NET & Maven.
- Self-signed HTTPS for SSE: Java test clients use trust-all SSL configuration (OkHttp) — do NOT replicate in production.
- Override mock server port via `MOCK_ACQ_PORT` before starting it.

---
## 7. Quick Copy Block (Most Common)
```powershell
# 1. Start mock server
dotnet run --project "c:/OPAL API/tests/MockAcquisitionAgent/MockAcquisitionAgent/MockAcquisitionAgent.csproj"

# 2. In another terminal run mock lifecycle test
mvn -f tests/API/pom.xml -Dtest=MockAcquisitionServiceTest#replicateLifecycleWithoutSse test

# 3. Run real lifecycle + SSE correlation (Opal API must be up)
mvn -f tests/API/pom.xml -Dtest=GrpcCallReplicationTest#replicateAcquisitionLifecycle test
```

---
## 8. Next Enhancements (Optional)
- Add Maven profile to toggle mock vs real endpoints (ports configurable via system properties).
- Introduce TestNG groups: `@Test(groups={"mock"})`, `@Test(groups={"real"})` for selective execution.
- Add automatic retries for transient SSE connection failures.

---
Feel free to extend this document as test coverage expands.
