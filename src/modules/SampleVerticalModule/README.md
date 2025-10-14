# Sample Vertical Module

## Overview

The **Sample Vertical Module** is a demonstration module that showcases how to implement the **Vertical Slice Architecture** pattern within the Opal modular monorepo framework. This module provides REST API endpoints and Middleware, demonstrating how to organize code by business features rather than technical layers, promoting high cohesion and loose coupling.

## Architecture

### Vertical Slice Architecture Pattern

This module follows the **Vertical Slice Architecture** pattern, which organizes code by business features (vertical slices) rather than technical layers. Each feature contains all the components it needs to function independently:

```plaintext
┌─────────────────────────────────────────────────────────────────┐
│                     Sample Vertical Module                      │
│                                                                 │
│  ┌────────────────────────┐    ┌──────────────────────────-─┐   │
│  │   Chromatography Slice │    │   Mass Spectrometry Slice  │   │
│  │                        │    │                            │   │
│  │  ┌─────────────────┐   │    │  ┌─────────────────────┐   │   │
│  │  │   Abstractions  │   │    │  │   Abstractions      │   │   │
│  │  │   - Interface   │   │    │  │   - Interface       │   │   │
│  │  │   - Models      │   │    │  │   - Models          │   │   │
│  │  └─────────────────┘   │    │  └─────────────────────┘   │   │
│  │  ┌─────────────────┐   │    │  ┌─────────────────────┐   │   │
│  │  │   Contracts     │   │    │  │   Contracts         │   │   │
│  │  │   - Request     │   │    │  │   - Request         │   │   │
│  │  │   - Response    │   │    │  │   - Response        │   │   │
│  │  └─────────────────┘   │    │  └─────────────────────┘   │   │
│  │  ┌─────────────────┐   │    │  ┌─────────────────────┐   │   │
│  │  │ Implementation  │   │    │  │ Implementation      │   │   │
│  │  │   - Service     │   │    │  │   - Service         │   │   │
│  │  │   - Endpoints   │   │    │  │   - Endpoints       │   │   │
│  │  │   - Extensions  │   │    │  │   - Extensions      │   │   │
│  │  │   - Middleware  │   │    │  │                     │   │   │
│  │  └─────────────────┘   │    │  └─────────────────────┘   │   │
│  └────────────────────────┘    └────────────────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                 Integration Layer                           ││
│  │            (SampleVerticalModuleRegistration)               ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

> **.Abstractions, .Contracts** are separate assemblies so they can be shared across different consumers without pulling in the entire implementation.

### Project Structure

The module is organized by business features (vertical slices) rather than technical layers:

```plaintext
SampleVerticalModule/
├── SampleVerticalModule.Abstractions/   # Feature Abstractions
│   └── Features/
│       ├── Chromatography/
│       │   ├── IChromatogramAnalysis.cs   # Business interface
│       │   └── ChromatogramAnalysis.cs    # Domain model
│       └── MassSpectrometry/
│           ├── IMassSpectrometryAnalysis.cs # Business interface
│           └── MassSpecAnalysis.cs          # Domain model
│
├── SampleVerticalModule.Contracts/      # External Contracts
│   └── Features/
│       ├── Chromatography/
│       │   ├── ChromatogramAnalysisRequest.cs   # API request
│       │   └── ChromatogramAnalysisResponse.cs  # API response
│       └── MassSpectrometry/
│           ├── MassSpecAnalysisRequest.cs       # API request
│           └── MassSpecAnalysisResponse.cs      # API response
│
├── SampleVerticalModule/                # Feature Implementations
│   └── Features/
│       ├── Chromatography/
│       │   ├── ChromatogramAnalysisService.cs   # Business logic
│       │   ├── ChromatographyEndpoints.cs       # REST endpoints
│       │   ├── ChromatographyServiceCollectionExtensions.cs
│       │   └── SampleVerticalModuleMiddleware.cs # Feature middleware
│       └── MassSpectrometry/
│           ├── MassSpecAnalysisService.cs       # Business logic
│           ├── MassSpecAnalysisEndpoints.cs     # REST endpoints
│           └── MassSpecServiceCollectionExtensions.cs
│
├── SampleVerticalModule.Opal/          # Integration Layer
|    ├── SampleVerticalModuleRegistration.cs     # Module registration
|    ├── ChromatographyApiEndpointBuilderExtensions.cs
|    └── MassSpecApiEndpointBuilderExtensions.cs
|
|── SampleVerticalModule.Test/          # Module Tests, with mocked external dependencies
```

## Key Components

### 1. Vertical Slices (Features)

Each feature is a complete vertical slice containing all necessary components:

#### Chromatography Feature

- **Interface**: `IChromatogramAnalysis` - Defines chromatography analysis operations (separated in .Abstractions assembly)
- **Model**: `ChromatogramAnalysis` - Domain model for analysis results
- **Service**: `ChromatogramAnalysisService` - Implements business logic
- **Endpoints**: `ChromatographyEndpoints` - REST API endpoints
- **Contracts**: Request/Response DTOs for external communication (separated in .Contracts assembly)
- **Middleware**: `ChromatographyMiddleware` - Feature-specific middleware
- **Extensions**: DI registration and endpoint mapping

#### Mass Spectrometry Feature

- **Interface**: `IMassSpectrometryAnalysis` - Defines mass spec analysis operations (separated in .Abstractions assembly)
- **Model**: `MassSpecAnalysis` - Domain model for analysis results
- **Service**: `MassSpecAnalysisService` - Implements business logic
- **Endpoints**: `MassSpecAnalysisEndpoints` - REST API endpoints (separated in .Contracts assembly)
- **Contracts**: Request/Response DTOs for external communication
- **Extensions**: DI registration and endpoint mapping

### 2. Integration Layer (`SampleVerticalModule.Opal`)

**Purpose**: Orchestrates the registration and integration of all features with the host application.

**Components**:

- **`SampleVerticalModuleRegistration`**: Main module registration class
- **`ChromatographyApiEndpointBuilderExtensions`**: Chromatography endpoint registration
- **`MassSpecApiEndpointBuilderExtensions`**: Mass spec endpoint registration

## Integration with Api Application Host

### Module Registration Process

The module integrates with the Opal application host via ModuleRegistry.

```csharp
// In src/Api/ModuleRegistry.cs of API application host
public static ModuleRegistryBuilder RegisterAllModules(this ModuleRegistryBuilder moduleBuilder)
{
    return moduleBuilder
        // Register with path prefix since module implements IEndpointRegistrationModule
        .RegisterModule<SampleVerticalModuleRegistration>("vertical");
}

### Module Registration Implementation

The `SampleVerticalModuleRegistration` class implements multiple interfaces to provide full integration:

```csharp
public class SampleVerticalModuleRegistration :
    IServiceRegistrationModule,      // Phase 2: Service registration
    IEndpointRegistrationModule,     // Phase 3: REST API endpoints
    IOpenApiRegistrationModule,      // Phase 2: OpenAPI/Swagger
    IMiddlewareRegistrationModule    // Phase 3: Middleware registration
{
    // Module metadata
    public string Name => "SampleVerticalModule";
    public string Version => "1.0.0";
    public string Description => "A sample module demonstrating vertical slice architecture";

    // Service registration - registers all feature services
    public void RegisterServices(ModuleRegistrationContext context)
    {
        context.Services
            .AddChromatography()      // Chromatography feature services
            .AddMassSpectrometry();   // Mass spectrometry feature services
    }

    // REST API endpoints - maps all feature endpoints
    public void RegisterEndpoints(IApiEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapChromatographyEndpoints();   // Chromatography endpoints
        endpoints.MapMassSpecEndpoints();         // Mass spectrometry endpoints
    }

    // OpenAPI configuration - separate docs per feature
    public IReadOnlyCollection<string> RegisterOpenApiServices(IOpenApiServiceRegister openApiServiceRegister)
    {
        // Chromatography API documentation
        openApiServiceRegister.AddOpenApi("chromatography", options => {
            options.AddDocumentTransformer((document, context, _) => {
                document.Info.Title = "Chromatography API";
                document.Info.Version = "v1";
                document.Info.Description = "API for Chromatography Module";
                return Task.CompletedTask;
            });
        });

        // Mass Spectrometry API documentation
        openApiServiceRegister.AddOpenApi("massspec", options => {
            options.AddDocumentTransformer((document, context, _) => {
                document.Info.Title = "Mass Spectrometry API";
                document.Info.Version = "v1";
                document.Info.Description = "API for Mass Spectrometry Module";
                return Task.CompletedTask;
            });
        });

        return ["chromatography", "massspec"];
    }

    // Middleware registration
    public void RegisterMiddleware(IMiddlewareApplicationBuilder appBuilder, ModuleStartupContext context)
    {
        appBuilder.UseMiddleware<ChromatographyMiddleware>();
    }
}
```

### API Endpoints

#### REST API Endpoints

The module is registered so it exposes REST endpoints under the `/api/vertical/` path prefix:

## Benefits of Vertical Slice Architecture

### 1. **Feature-Oriented Organization**

- Code is organized by business features, not technical layers
- Each feature is self-contained and cohesive
- Easy to locate all code related to a specific feature

### 2. **High Cohesion, Loose Coupling**

- Everything needed for a feature is grouped together
- Features are loosely coupled to each other
- Changes to one feature rarely affect others

### 3. **Independent Development**

- Teams can work on different features without conflicts
- Features can be developed, tested, and deployed independently
- New features can be added without touching existing code

### 4. **Clear Boundaries**

- Each feature has well-defined interfaces and contracts
- Dependencies are explicit and minimal
- Easy to understand feature scope and responsibilities

### 5. **Testing**

- Each feature can be tested in isolation
- Clear test boundaries around business functionality
- Easy to write focused, feature-specific tests

## Configuration

### Module Configuration

The module can be configured through the application's configuration system, each module has its own section in the configuration file (e.g., `appsettings.json`):

```json
{
    "Modules": {
        "SampleVerticalModule": {
        "Chromatography": {
            "DefaultMethod": "HPLC",
            "AnalysisTimeout": 300
        },
        "MassSpectrometry": {
            "DefaultIonizationMode": "ESI",
            "MassRange": {
            "Min": 50,
            "Max": 2000
            }
        }
        }
    }
}
```

## Usage Examples

### API Calls

#### Chromatography Analysis

```bash
# Analyze chromatogram
curl -X 'POST' \
    'https://localhost:61350/api/vertical/v1/chromatogram-analysis' \
    -H 'accept: application/json' \
    -H 'Content-Type: application/json' \
    -d '{
    "sampleId": "
    "sampleId": "SAMPLE-001",
    "methodName": "HPLC-UV"
}'

# Response
{
    "sampleId": "SAMPLE-001",
    "methodName": "HPLC-UV",
    "peakCount": 15,
    "retentionTimeRange": 25.6,
    "analyzedAt": "2024-01-15T10:30:00Z"
}
```

#### Mass Spectrometry Analysis

```bash
# Analyze mass spectrometry data
curl -X 'POST' \
    'https://localhost:61350/api/vertical/v1/mass-spec-analysis' \
    -H 'accept: application/json' \
    -H 'Content-Type: application/json' \
    -d '{
    "sampleId": "SAMPLE-002",
    "ionizationMode": "ESI"
}'

# Response
{
    "sampleId": "SAMPLE-002",
    "ionizationMode": "ESI",
    "massRange": "50-2000",
    "spectraCount": 1250,
    "basePeakIntensity": 1.5e6,
    "analyzedAt": "2024-01-15T10:35:00Z"
}
```

## Development Guidelines

When creating similar vertical slice modules:

1. **Start with Features**: Identify distinct business features/capabilities
2. **Create Vertical Slices**: Group all code for a feature together
3. **Define Clear Interfaces**: Each feature should have well-defined interfaces
4. **Minimize Cross-Feature Dependencies**: Features should be as independent as possible
5. **Use Feature-Specific Extensions**: Each feature registers its own services
6. **Test by Feature**: Write tests that focus on complete feature functionality

## Comparison with Onion Architecture

| Aspect | Vertical Slice | Onion Architecture |
|--------|----------------|-------------------|
| **Organization** | By business features | By technical layers |
| **Coupling** | Features loosely coupled | Layers tightly coupled |
| **Change Impact** | Isolated to single feature | Can span multiple layers |

Both architectures have their place - Vertical Slice is excellent for feature-rich modules where business capabilities are distinct, while Onion Architecture works well for modules with complex domain logic and clear separation of concerns.
