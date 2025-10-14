# Sample Onion Module

## Overview

The **Sample Onion Module** is a demonstration module that showcases how to implement the Onion Architecture pattern within the Opal modular monorepo framework. This module provides both REST API and gRPC endpoints, demonstrating module design, dependency injection, and integration with the host application.

## Architecture

### Onion Architecture Pattern

This module follows the **Onion Architecture** (also known as Clean Architecture), which organizes code into concentric layers with dependencies flowing inward:

### Project Structure

The module is organized into several focused projects following the onion architecture:

```plaintext
SampleOnionModule/
├── SampleOnionModule.Abstractions/     # Domain Layer
│   ├── ISampleOnionModule.cs          # Core business interface
│   ├── Sample.cs                      # Domain entity
│   └── Compound.cs                    # Domain entity
│
├── SampleOnionModule/                  # Application Layer
│   ├── SampleOnionModule.cs           # Core business logic
│   ├── SampleOnionModuleConfiguration.cs
│   └── SampleOnionModuleServiceCollectionExtensions.cs
│
├── SampleOnionModule.Contracts/        # External Contracts related to transport protocols
│   ├── SampleOnionModule.proto        # gRPC contract
│   └── SampleResponse.cs              # REST API contract
│
├── SampleOnionModule.AspNetCore/       # REST API Infrastructure
│   ├── SampleOnionModuleEndpoints.cs  # HTTP endpoints
│   └── SampleOnionModuleHttpEndpointBuilderExtensions.cs
│
├── SampleOnionModule.Grpc/            # gRPC Infrastructure
│   └── SampleOnionGrpcService.cs      # gRPC service implementation
│
├── SampleOnionModule.Opal/            # Integration Layer
|    └── SampleOnionModuleRegistration.cs # Module registration with Opal API host
|
└── SampleOnionModule.Tests/           # Module Tests, with mocked external dependencies
```

## Key Components

### 1. Domain Layer (`SampleOnionModule.Abstractions`)

**Purpose**: Contains the core business entities and interfaces with no external dependencies.

**Components**:

- **`ISampleOnionModule`**: Core business interface defining operations
- **`Sample`**: Domain entity representing a chemical sample
- **`Compound`**: Domain entity representing a chemical compound

**Dependencies**: None (pure domain logic)

### 2. Application Layer (`SampleOnionModule`)

**Purpose**: Contains business logic implementation and application services.

**Components**:

- **`SampleOnionModule`**: Implements `ISampleOnionModule` with core business logic
- **`SampleOnionModuleConfiguration`**: Configuration options for the module
- **`SampleOnionModuleServiceCollectionExtensions`**: DI registration extensions

**Dependencies**: Domain abstractions only

### 3. Infrastructure Layer

#### REST API (`SampleOnionModule.AspNetCore`)

- **`SampleOnionModuleEndpoints`**: Minimal API endpoints with OpenAPI attributes
- **`SampleOnionModuleHttpEndpointBuilderExtensions`**: Extension methods for endpoint registration

#### gRPC (`SampleOnionModule.Grpc`)

- **`SampleOnionGrpcService`**: gRPC service implementation

#### Contracts (`SampleOnionModule.Contracts`)

- **`SampleOnionModule.proto`**: Protocol Buffers definition for gRPC
- **`SampleResponse.cs`**: REST API response models

#### Integration (`SampleOnionModule.Opal`)

- **`SampleOnionModuleRegistration`**: Module registration and lifecycle management

## Integration with Api Application Host

The module integrates with the Opal application host via ModuleRegistry.

```csharp
// In src/Api/ModuleRegistry.cs of API application host
public static ModuleRegistryBuilder RegisterAllModules(this ModuleRegistryBuilder moduleBuilder)
{
    return moduleBuilder
        // Register with path prefix since module implements IHttpEndpointRegistrationModule
        .RegisterModule<SampleOnionModuleRegistration>("onion");
}
```

### Module Registration Implementation

The `SampleOnionModuleRegistration` class implements multiple interfaces to provide full integration:

```csharp
public class SampleOnionModuleRegistration :
    IServiceRegistrationModule,      // Phase 2: Service registration
    IHttpEndpointRegistrationModule, // Phase 3: REST API endpoints
    IGrpcRegistrationModule,         // Phase 3: gRPC endpoints
    IOpenApiRegistrationModule,      // Phase 2: OpenAPI/Swagger
    ILifecycleModule                 // Lifecycle events
{
    // Module metadata
    public string Name => "SampleOnionModule";
    public string Version => "1.0.0";
    public string Description => "A sample module with \"onion\" architecture.";

    // Service registration
    public void RegisterServices(ModuleRegistrationContext context)
    {
        context.Services.AddSampleOnionModule(context.GetModuleConfiguration());
        context.Services.AddTransient<SampleOnionGrpcService>();
    }

    // REST API endpoints
    public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapSampleOnionModuleEndpoints();
    }

    // gRPC endpoints
    public void RegisterGrpcEndpoints(IGrpcEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapGrpcService<SampleOnionGrpcService>();
    }

    // OpenAPI configuration
    public IReadOnlyCollection<string> RegisterOpenApiServices(IOpenApiServiceRegister openApiServiceRegister)
    {
        // Configure Swagger documentation
    }

    // Lifecycle management
    public async Task OnApplicationStartedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var sampleModule = serviceProvider.GetService<ISampleOnionModule>();
        await sampleModule.InitializeAsync(cancellationToken);
    }
}
```

### API Endpoints

#### REST API Endpoints

The module exposes REST endpoints under the `/api/onion/` path prefix:

#### gRPC Endpoints

In Development, GRPC reflection is enabled, allowing clients to discover available services and methods. The gRPC service is registered under the `sampleonion` namespace.

```bash
grpcurl localhost:61350 describe
```

## Configuration

### Module Configuration

The module has a dedicated section in the application configuration file (e.g., `appsettings.json`) under `Modules`:

```json
{
    "Modules": {
        "SampleOnionModule": {
        "Option1": "value1",
        "Option2": 100
        }
    }
}
```

## Benefits of This Architecture

### 1. **Separation of Concerns**

- Domain logic is isolated in abstractions
- Infrastructure concerns are separated into focused projects
- Clear boundaries between layers

### 2. **Testability**

- Domain logic can be tested in isolation
- Infrastructure can be mocked through interfaces
- Clear dependency injection boundaries

### 3. **Maintainability**

- Changes to infrastructure don't affect domain logic
- Each project has a single responsibility
- Dependencies flow inward only

## Usage Example

### API Calls

```bash
# REST API
curl -X 'GET' \
    'https://localhost:61350/api/onion/v1/sample/123e4567-e89b-12d3-a456-426614174000' \
    -H 'accept: application/json'

# gRPC (using grpcurl)
grpcurl -d '{}' localhost:61350 sampleonion.SampleOnionService/GetCompounds
```

[grpcurl](https://github.com/fullstorydev/grpcurl)

## Development Guidelines

When creating similar modules:

1. **Start with Domain**: Define entities and interfaces in `.Abstractions`
2. **Implement Business Logic**: Create application services
3. **Add Infrastructure**: Create API endpoints and external integrations
4. **Register with Host**: Implement registration interfaces
5. **Document APIs**: Use OpenAPI attributes for REST, proto files for gRPC

This sample module demonstrates how to properly structure a module using onion architecture while integrating with the Opal modular platform.
