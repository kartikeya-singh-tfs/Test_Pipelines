# gRPC Reporting Alternatives

This document lists alternatives for capturing and reporting gRPC requests/responses and streaming events (SSE) from automated tests and services. Each option includes a short description, pros, cons, integration notes (what you need to collect/attach), data shapes, and recommended use-cases.

## 1) Allure (current)

Description
- Allure is an open-source test reporting framework that consumes test result files (XML/JSON) and attachments produced by test code or test framework adapters and generates a browsable HTML report.

Pros
- Rich UI with attachments, steps, parameters, labels and history.
- Lightweight integration: Allure Java API allows programmatic attachments (InputStream) and steps.
- Works with TestNG, JUnit, pytest and other frameworks via adapters.
- Local CLI + CI-friendly; report artifacts are self-contained.

Cons
- Attachments should be added while test context is active (thread affinity) for best UX; this requires buffering/draining or a TestNG listener when gRPC callbacks run on other threads.
- Not a centralized real-time dashboard (reports are generated post-run).

Integration notes
- What to collect: protobuf JSON serializations (JsonFormat), streaming summaries (compact JSON), SSE event lists.
- How to attach: use `Allure.step(...)` and `Allure.addAttachment(name, mimeType, InputStream, extension)` or drain buffered captures in a `ITestListener`.

Recommended when
- You want rich per-test reports with attachments and historical comparisons, and post-run HTML reports are sufficient.

---

## 2) ReportPortal

Description
- ReportPortal is an open-source real-time test reporting server. Tests push logs, attachments, and structured events via HTTP API or client SDKs, producing a centralized dashboard with search and analytics.

Pros
- Centralized, real-time dashboards with querying and analytics.
- Attachments and logs are linked to tests immediately (no thread-local constraints; server handles association via launch/test item IDs provided by the SDK).
- Good for large-scale CI where teams need a single source of truth and failure triage.

Cons
- Requires running or hosting ReportPortal server (self-hosted or SaaS solutions exist but may cost support effort).
- More operational complexity than Allure. SDK integration required in tests.

Integration notes
- What to collect: raw protobuf bytes or JSON, streaming event streams, structured logs, screenshots, and custom attributes.
- How to attach: use ReportPortal Java client to start a launch/test item, send logs and attachments; for gRPC callbacks attach using an SDK client instance associated to the test item id (thread-safe if you keep the item id in a concurrent map keyed by test id).

Recommended when
- You need centralized, searchable real-time reporting across multiple pipelines and teams.

---

## 3) OpenTelemetry + Tracing Backends (Jaeger / Zipkin / Tempo) + Logs

Description
- Use OpenTelemetry instrumentation for gRPC (client and server) to emit spans and events to a tracing backend (Jaeger, Zipkin, Tempo). Combine with structured logs (ELK/Loki) to correlate payloads.

Pros
- Excellent for distributed tracing and performance analysis (latency, dependencies).
- Can attach span attributes containing metadata or digest of payloads; good for SLO/metric-driven analysis.

Cons
- Not optimized for human-readable per-test attachments; tracing UIs focus on spans and timing rather than payload inspection.
- Payload sizes should be kept small (sensitive data must be redacted).

Integration notes
- What to collect: span attributes (method, status, timing), optional small payload digests or truncated JSON. For full payloads, emit to a storage or attach to logs.
- How to attach: instrument gRPC client using OpenTelemetry automatic instrumentation or manual instrumentation; export to configured exporter.

Recommended when
- You primarily care about latency, call graphs, and distributed system behavior rather than detailed per-test attachments.

---

## 4) ELK Stack / Loki + Kibana / Grafana

Description
- Send structured logs and payloads (or references to payload storage) to Elasticsearch / Loki and visualize/search via Kibana or Grafana.

Pros
- Centralized logging with powerful querying and dashboards; easy to store raw JSON payloads and full text search.
- Can correlate logs across services and tests.

Cons
- Storage and operational overhead. Payloads can be large; need retention and redaction strategies.
- Not test-report specific; lacks built-in per-test grouping unless you attach test metadata to each log event.

Integration notes
- What to collect: structured logs include test id, timestamp, rpc method, direction, truncated payload or pointer to object store.
- How to attach: log from gRPC interceptor callbacks (include testId). Use connectors (Logstash, Promtail) or direct HTTP ingestion.

Recommended when
- You need long-term storage of payloads and flexible search across runs and services.

---

## 5) ExtentReports / ReportNG / Custom HTML reporters

Description
- Client-side HTML reporting libraries such as ExtentReports provide more flexible per-test reporting than Allure in some cases, or you can build a small custom HTML/JSON reporter tailored to gRPC payloads.

Pros
- Highly customizable HTML outputs; you control structure and UX.
- Easier to implement as file writers from test threads; can be synchronous and thread-safe if you design a concurrency model.

Cons
- Reinventing reporting features (history, artifacts, filtering) costs time. Less community ecosystem than Allure.

Integration notes
- What to collect: same as Allure (json payloads, streaming summaries). Produce structured JSON or HTML files under per-test folders.
- How to attach: write files directly from listener or interceptor; then post-process into an index HTML.

Recommended when
- You need a custom presentation or want minimal infra (no server, no Allure dependencies). Good for teams that want a very specific report layout.

---

## 6) Test Management / Case Management Integration (e.g., TestRail, PractiTest)

Description
- Push test run results and links to artifacts into a test management tool for traceability with requirements and manual test cases.

Pros
- Good for audit, traceability, and business workflows (defect tracking / test plans).
- Centralizes run metadata and artifacts for stakeholders.

Cons
- Not designed for raw payload inspection in-line; typically stores links or small attachments.
- Usually commercial and may require paid connectors.

Integration notes
- What to collect: pass/fail, run id, links to artifact storage (S3, blob), truncated summaries.
- How to attach: call TestRail API or use CI integration once results and attachments are uploaded to a stable location.

Recommended when
- Traceability to requirements or compliance is required.

---

## 7) Custom Central Collector + Object Store

Description
- Build a small upload API (or use existing artifact storage) that tests push payload files to (S3, Azure Blob, GCS). A lightweight UI can present grouped per-test payloads.

Pros
- Maximum control over payload retention policy, redaction, and storage tiering.
- Works well with large payloads; can store full protobuf binary if needed.

Cons
- You build and maintain storage, APIs, security, and UI. More engineering effort.

Integration notes
- What to collect: raw bytes or JSON, metadata (test id, timestamp, rpc method, direction). Store as object with metadata tags.
- How to attach: from tests or interceptor push to API with testId; central UI consumes objects and groups by test metadata.

Recommended when
- You need full payload storage and want to manage lifecycle and access centrally.

---

## Comparative Summary (when to pick what)

- Allure: Best for per-test, human-friendly, post-run reports with attachments and step-level details and minimal infra. Choose when teams review test results as HTML artifacts.
- ReportPortal: Best when you need real-time centralized reporting with search and analytics across pipelines and teams.
- OpenTelemetry/Jaeger (tracing): Best for distributed tracing, latency analysis, and call-graph visualization. Not ideal as a human-readable test artifact store.
- ELK / Loki: Best for long-term searchable logs and payloads, correlation across services and runs.
- ExtentReports / Custom HTML: Best if you need custom presentation or want to avoid Allure dependency; lower ecosystem support.
- TestRail/Management: Best where formal traceability and integration with business processes is required.
- Custom collector + object store: Best if you need to retain complete payloads and control redaction/lifecycle.

## Practical integration tips (gRPC specifics)

- Attachments vs pointers: For large streaming payloads, consider storing raw payloads in object storage and attaching pointers/URLs in the report to avoid huge test artifacts.
- Redaction: Always apply redaction hooks (PII, secrets) before sending payloads to any external system.
- Thread-affinity: If interceptors attach from gRPC threads, ensure your reporting SDK is thread-safe and knows the test-item context (ReportPortal) or buffer+drain in a TestNG listener (Allure) to preserve per-test grouping.
- Sampling and truncation: For high-throughput / streaming tests, sample or truncate payloads to keep storage and reports manageable.

## Suggested next steps

1. Pick two options to prototype (Allure + ReportPortal or Allure + ELK) depending on whether you want immediate centralized dashboard capabilities.
2. Add a TestNG listener (`GrpcAllureTestListener`) and `TestContext` to reliably associate buffered captures to test runs for Allure (I can implement this for you). If you choose ReportPortal, integrate the ReportPortal Java SDK and map test ids to launch/test item ids.
3. Decide retention and redaction policy and implement a redactor hook in your gRPC interceptor.

---

Document created: docs/grpc-reporting-alternatives.md
