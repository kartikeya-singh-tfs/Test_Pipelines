# gRPC + Allure Reporting Checklist

Purpose: concise, actionable checklist to make gRPC tests produce useful Allure reports with bounded, searchable attachments. Add this to your repo and refer tests/CI to it.

Date: 2025-10-27

---

## Prerequisites
- `allure-testng` on the test classpath (declared in `tests/API/pom.xml`).
- Allure CLI available for report generation (we keep `tests/API/tools/allure-*/bin/allure.bat` in this repo).
- Tests annotated/using TestNG and Allure (`Allure.step`, `Allure.addAttachment` usage).

---

## Quick checklist (copy into tests)
- Use intercepted channel so the interceptor attaches payloads automatically:
  - ManagedChannel raw = ManagedChannelBuilder.forAddress(host, port).usePlaintext().build();
  - Channel intercepted = ClientInterceptors.intercept(raw, new GrpcClientAllureInterceptor());
- Add test metadata parameters:
  - Allure.parameter("grpc.host", host);
  - Allure.parameter("grpc.port", String.valueOf(port));
- Wrap important actions in steps:
  - Allure.step("Call sequenceStateEvent", () -> { blocking.sequenceStateEvent(evt); });
- Attach exceptions and stack traces on catch:
  - capture stacktrace -> Allure.addAttachment("gRPC Exception: <method>", "text/plain", ..., ".txt");
- Wait for asynchronous callbacks before test ends (CountDownLatch/await).
- Attach compact stream summaries in addition to or instead of per-message attachments.
- Shutdown channels before test return and await termination so background callbacks finish.

---

## Interceptor & attachment best-practices
- Keep per-message attachments guarded by a limit (default: `maxStreamAttachments`): attach only first N or last N messages.
- Truncate attachments > `maxAttachmentSizeBytes` (default: 64KB).
- Add a redaction hook (functional callback) to scrub secrets before attaching.
- Optionally buffer per-RPC samples and emit a single per-RPC summary attachment.
- Provide a system property/env var toggle for CI vs local verbosity: `ALLURE_GRPC_CAPTURE=off|errors|sample|all`.

---

## CI-friendly guidance
- Reduce defaults in CI: smaller `maxStreamAttachments` and `maxAttachmentSizeBytes`.
- Upload `allure-report` or `allure-results` as pipeline artifacts for later inspection.
- Prefer generating report (`allure generate`) only for failed runs or nightly full-capture runs.
- If artifacts are large, offload to object storage (S3) and link in test reports.

---

## Troubleshooting (why the report looks empty)
- If Allure UI is empty when opening `index.html` locally, serve the folder via HTTP (browsers block file:// XHR):
  - `python -m http.server 8000 --directory tests/API/allure-report`
  - Open `http://localhost:8000/`
- Verify `tests/API/allure-results` contains `*-result.json` and `*-attachment.*` files.
- Use `allure generate tests/API/allure-results -o tests/API/allure-report --clean` to regenerate.

---

## Quick verification commands (PowerShell)
```powershell
# run GRPC tests (example)
& 'C:\Users\<you>\tools\apache-maven-3.9.11\bin\mvn.cmd' -B -Dgroups=GRPC test -f C:\Git\Pipeline_Github_Actions\tests\API\pom.xml

# generate Allure HTML
& 'C:\Git\Pipeline_Github_Actions\tests\API\tools\allure-2.25.0\bin\allure.bat' generate C:\Git\Pipeline_Github_Actions\tests\API\allure-results -o C:\Git\Pipeline_Github_Actions\tests\API\allure-report --clean

# serve the report folder
python -m http.server 8000 --directory C:\Git\Pipeline_Github_Actions\tests\API\allure-report
# open http://localhost:8000/
```

---

## Verification checklist (post-run)
- [ ] `tests/API/allure-results` contains at least one `*-result.json` and matching attachment files.
- [ ] `tests/API/allure-report/data/` contains JSON bundles and `data/attachments/` contains attachments.
- [ ] Allure UI shows test steps, attachments, and status.
- [ ] Streaming-heavy tests show summary attachments; per-message attachments are within size and count limits.

---

## Optional improvements (prioritized)
1. Add per-message count gating (stop adding attachments after N) in `GrpcClientAllureInterceptor`.
2. Add a redaction callback to the interceptor.
3. Add a TestNG listener to aggregate and attach a single per-test summary (reduces noise).
4. Provide a small `attachJson` / `attachText` helper in `tests/API/TestUtilities` for consistent attachments.
5. CI pipeline action to upload `allure-report` and optionally provide a simple HTML viewer link.

---

## Contact
If you want, I can add any of the optional improvements in code (small PR): just tell me which item to implement first.
