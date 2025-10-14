# OPAL API: Mock Acquisition Service & gRPC Integration Documentation

## Overview
This document describes the setup and configuration for the OPAL API framework, focusing on the Mock Acquisition Service, gRPC Java client, and integration tests.

---

## 1. Mock Acquisition Service (C#)

### Purpose
Implements a mock gRPC service for testing workflows without real acquisition hardware.

### Location
- `c:/OPAL API/tests/MockAcquisitionAgent/MockAcquisitionAgent/MockAcquisitionService.cs`

### Key Steps
1. **Proto Files**
   - Ensure proto files (e.g., `rawDataAccess.proto`) are present and referenced in the C# project.
   - C# codegen generates types directly under `ThermoFisher.AcquisitionModule.Contracts`.
2. **Service Implementation**
   - Implement `MockAcquisitionService` inheriting from `RealTimePlotService.RealTimePlotServiceBase`.
   - Override methods like `UploadSparklineData` and `UploadChromatogramSvgData` to return mock responses.
3. **Enum Usage**
   - Use `UploadStatusType.Ok` for success status in responses.
4. **Service Registration**
   - Register the service in `Program.cs`:
     ```csharp
     Services = { RealTimePlotService.BindService(new MockAcquisitionService()) }
     ```
5. **Run the Service**
   - Start with:
     ```powershell
     dotnet run --project "c:/OPAL API/tests/MockAcquisitionAgent/MockAcquisitionAgent/MockAcquisitionAgent.csproj"
     ```
   - Confirm it is listening on port 50051 (`netstat -ano | findstr :50051`).

---

## 2. gRPC Java Client & Tests

### Purpose
Validates end-to-end gRPC workflow from Java client to C# mock service.

### Location
- Test file: `c:/OPAL API/tests/API/src/test/java/com/example/grpc/RawDataAccessServiceTest.java`
- Proto files: `c:/OPAL API/tests/API/src/main/proto/rawDataAccess.proto`

### Key Steps
1. **Proto Files & Codegen**
   - Use `protoc` and `protoc-gen-grpc-java` to generate Java stubs from proto files.
   - Ensure generated classes are available in the test source tree.
2. **Test Implementation**
   - Create gRPC requests (`SparklineData`, `ChromatogramSvgData`) using builder pattern.
   - Open a channel to `localhost:50051`.
   - Use `RealTimePlotServiceGrpc.RealTimePlotServiceBlockingStub` to send requests.
   - Assert and print responses.
3. **Test Execution**
   - Run tests using Maven or your IDE:
     ```shell
     mvn test -Dtest=RawDataAccessServiceTest
     ```
   - All tests should pass if the mock service is running.

---

## 3. Integration Flow

1. Java test creates and sends a gRPC request to `localhost:50051`.
2. Opal server (C#) receives and routes the request to `MockAcquisitionService`.
3. Mock service returns a canned response (`PlotDataResponse` with status `Ok`).
4. Java test receives and validates the response.

---

## Troubleshooting
- **Build Errors:**
  - Ensure proto files are correctly referenced and codegen is up-to-date.
  - Use correct enum values (`UploadStatusType.Ok`).
- **Service Not Running:**
  - Check port 50051 is listening.
  - Review service registration in `Program.cs`.
- **Test Failures:**
  - Confirm service is running before executing tests.
  - Validate gRPC stub versions match proto definitions.

---

## References
- [gRPC C# Quickstart](https://grpc.io/docs/languages/csharp/quickstart/)
- [gRPC Java Quickstart](https://grpc.io/docs/languages/java/quickstart/)
- [TestNG Documentation](https://testng.org/doc/)

---

For further details, see the source files and comments in the respective directories.
