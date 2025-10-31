# gRPC + Allure Dependencies (Maven / TestNG)

This document extracts and explains the Maven dependencies and test-related plugins used by the `tests/API` module to implement gRPC capture and Allure reporting in this repository. Copy-paste friendly coordinates (groupId:artifactId:version) are provided.

## Core test framework

- org.testng:testng:7.10.2 (scope: test)
  - Purpose: Test runner used in the project. Allure TestNG listener integrates with TestNG lifecycle.

## Allure reporting libraries

- io.qameta.allure:allure-testng:2.21.0 (scope: test)
  - Purpose: TestNG adapter for Allure (annotations, lifecycle integration, results producers that write to `allure-results`).

- io.qameta.allure:allure-rest-assured:2.21.0
  - Purpose: Convenience bindings to automatically attach RestAssured requests/responses to Allure.

- io.qameta.allure:allure-junit5:2.21.0 (scope: test)
  - Purpose: JUnit5 adapter (present in pom for multi-framework compatibility; not required for TestNG but harmless).

## gRPC & Protobuf (for message handling and serialization)

- io.grpc:grpc-netty-shaded:1.75.0
- io.grpc:grpc-protobuf:1.75.0
- io.grpc:grpc-stub:1.75.0
- com.google.protobuf:protobuf-java:4.32.1
- com.google.protobuf:protobuf-java-util:3.25.3 (scope: test)
  - Purpose: gRPC client libraries and protobuf runtime. `protobuf-java-util` provides `JsonFormat` which the tests use to convert protobuf messages to JSON for readable Allure attachments.

## HTTP / SSE support (for streaming events attached to Allure)

- com.squareup.okhttp3:okhttp:4.12.0 (scope: test)
- com.squareup.okhttp3:okhttp-sse:4.9.3
  - Purpose: OkHttp is used for HTTP/2 and SSE client support; SSE events captured by the test are serialized to JSON and attached to Allure.

## JSON / serialization

- com.fasterxml.jackson.core:jackson-databind:2.17.0
- org.json:json:20250107
  - Purpose: JSON serialization and pretty-printing used to produce readable payload attachments.

## REST & web test helpers

- io.rest-assured:rest-assured:5.5.0
- io.rest-assured:rest-assured-all:5.5.0
  - Purpose: REST testing helpers; `allure-rest-assured` works with these to attach HTTP request/response artefacts.

## Selenium / UI (present but not necessary for gRPC attachments)

- org.seleniumhq.selenium:selenium-java:4.28.1
- org.seleniumhq.selenium:selenium-api:4.28.1
  - Purpose: UI testing libraries present in the test module; not required for gRPC/Allure but used by other tests.

## Logging, AOP and utilities

- org.slf4j:slf4j-simple:2.0.9 (scope: test)
- org.apache.logging.log4j:log4j-core:2.19.0
- org.aspectj:aspectjweaver:1.9.22.1 (scope: runtime)
- org.projectlombok:lombok:1.18.32
  - Purpose: logging, aspect weaving for tests (argLine in Surefire), Lombok for POJO conveniences.

## Maven plugins and test runner configuration

- org.apache.maven.plugins:maven-surefire-plugin:2.20
  - Purpose: runs TestNG using `testng.xml`. The plugin is configured to pass an `argLine` to load the AspectJ weaver agent for runtime weaving.
- org.xolstice.maven.plugins:protobuf-maven-plugin:0.6.1
  - Purpose: Copy/generate protobuf stubs (configured to use the repo-local `tools/protoc`).

## Allure CLI (report generation)

- Allure Commandline: `allure-2.25.0` (bundled at `tests/API/tools/allure-2.25.0/` and also downloaded in GitHub Actions workflows)
  - Purpose: generate HTML report from `tests/API/allure-results` (example command used in CI):

```powershell
.\allure-2.25.0\bin\allure.bat generate tests/API/allure-results --clean -o allure-report
```

## Copy-paste dependency list (pom snippet)

Below is a compact list you can paste into a `pom.xml` dependencies block (trim scope as needed):

```xml
<!-- Test framework -->
<dependency>
  <groupId>org.testng</groupId>
  <artifactId>testng</artifactId>
  <version>7.10.2</version>
  <scope>test</scope>
</dependency>

<!-- Allure adapters -->
<dependency>
  <groupId>io.qameta.allure</groupId>
  <artifactId>allure-testng</artifactId>
  <version>2.21.0</version>
  <scope>test</scope>
</dependency>
<dependency>
  <groupId>io.qameta.allure</groupId>
  <artifactId>allure-rest-assured</artifactId>
  <version>2.21.0</version>
</dependency>

<!-- gRPC & protobuf -->
<dependency>
  <groupId>io.grpc</groupId>
  <artifactId>grpc-netty-shaded</artifactId>
  <version>1.75.0</version>
</dependency>
<dependency>
  <groupId>io.grpc</groupId>
  <artifactId>grpc-protobuf</artifactId>
  <version>1.75.0</version>
</dependency>
<dependency>
  <groupId>io.grpc</groupId>
  <artifactId>grpc-stub</artifactId>
  <version>1.75.0</version>
</dependency>
<dependency>
  <groupId>com.google.protobuf</groupId>
  <artifactId>protobuf-java</artifactId>
  <version>4.32.1</version>
</dependency>
<dependency>
  <groupId>com.google.protobuf</groupId>
  <artifactId>protobuf-java-util</artifactId>
  <version>3.25.3</version>
  <scope>test</scope>
</dependency>

<!-- OkHttp & SSE -->
<dependency>
  <groupId>com.squareup.okhttp3</groupId>
  <artifactId>okhttp</artifactId>
  <version>4.12.0</version>
  <scope>test</scope>
</dependency>
<dependency>
  <groupId>com.squareup.okhttp3</groupId>
  <artifactId>okhttp-sse</artifactId>
  <version>4.9.3</version>
</dependency>

<!-- JSON / serialization -->
<dependency>
  <groupId>com.fasterxml.jackson.core</groupId>
  <artifactId>jackson-databind</artifactId>
  <version>2.17.0</version>
</dependency>
<dependency>
  <groupId>org.json</groupId>
  <artifactId>json</artifactId>
  <version>20250107</version>
</dependency>

<!-- Optional helpers -->
<dependency>
  <groupId>io.rest-assured</groupId>
  <artifactId>rest-assured</artifactId>
  <version>5.5.0</version>
  <scope>compile</scope>
</dependency>
<dependency>
  <groupId>org.slf4j</groupId>
  <artifactId>slf4j-simple</artifactId>
  <version>2.0.9</version>
  <scope>test</scope>
</dependency>

<!-- Build / tooling -->
<plugin>
  <groupId>org.apache.maven.plugins</groupId>
  <artifactId>maven-surefire-plugin</artifactId>
  <version>2.20</version>
</plugin>

```

## Notes and recommendations

- Keep `allure-testng` as `test` scope to avoid leaking test-only runtime into production artifacts.
- Use `protobuf-java-util` (or JsonFormat) to convert protobuf messages to human-readable JSON before attaching to Allure.
- Prefer `Allure.addAttachment(name, mimeType, InputStream, ext)` inside an Allure step to ensure attachments appear under the right test/step.
- If gRPC callbacks execute on non-test threads, either (a) buffer captures and drain them in a TestNG `ITestListener` (recommended for parallel runs) or (b) use a reporting system that supports test-item ids from other threads (ReportPortal).

---

File created: `docs/grpc-allure-dependencies.md`
