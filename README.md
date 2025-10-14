# CMD  On-Prem Ardia Lightweight

## Overview

This monorepo project provides a method for developing and testing components for the
CMD SW [On-Prem Ardia Lightweight][Opal] rapid development program.

## Code Organization

The following describe the planned high-level structure of the repository:

```plaintext
├── data/                                           # PostgreSQL data directory
├── docs/                                           # Documentation for the project, including design docs, architecture, and usage guides
│
├── scripts/                                        # Scripts for automation tasks including those called from Taskfiles
|
├── src/                                            # Source code for the project
|   |
│   │── clients/                                      # Client applications - UIs,CLIs, etc.
|   │   ├── web/                                        # Web client application
|   |
│   ├── platform/                                     # Core/infrastructure modules
|   |   ├── Auth/                                       # Authentication and authorization
│   │
│   ├── modules/                                      # Application modules
│   │   ├── ModuleA/                                      # Application module A (Clean Arch. Style example)
│   │   │   ├── ModuleA/                                    # Internal module implementation
│   │   │   ├── ModuleA.Abstractions/                       # Public domain interfaces and entities
│   │   │   ├── ModuleA.AspNetCore/                         # Web API layer (endpoints, middleware...)
│   │   │   └── ModuleA.Tests/                              # Unit tests, tests where external dependencies are mocked
│   │   │   └── ModuleA.Contracts/                          # External contracts (e.g., gRPC, REST API)
│   │   │   └── ModuleA.[...]/                              # Additional module A layers
│   │   │   └── ModuleA.Opal/                               # **Integration layer** - Application module registration with Opal API host
|   |   |
│   │   ├── ModuleB/                                      # Application module B (Vertical Slice Style example)
│   │   │   ├── ModuleB.Feature1                            # Internal module implementation
│   │   │   ├── ModuleB.Feature1.Abstractions/              # Public domain interfaces and entities
│   │   │   └── ModuleB.Feature1.Tests/                     # Unit tests, tests where external dependencies are mocked
│   │   │   └── ModuleB.Contracts/                          # External contracts (e.g., gRPC, REST API)
│   │   │   └── ModuleB.[...]/                              # Additional module A packages
│   │   │   └── ModuleB.Opal/                               # **Integration layer** - Application module registration with Opal API host
│   │   │
│   │   └── [...]/
|   |
│   ├── Api/                                          # Main project, web API
│   ├── Api.Tests/                                    # Application host tests
│   │
│   └── shared/                                       # Cross-cutting concerns
│       ├── Shared.[...].Abstractions/                  # Shared DTOs, interfaces
│       ├── Shared.[...]/                               # Common utilities
|       └── shared-web-components/                      # Shared web components
│
├── tests/                                        # Integration and Architecture tests
│   ├── Integration/                                # Integration tests for cross-module interactions
│   └── Architecture/                               # Architecture tests to enforce module boundaries
│       └── ArchitectureTests/                        # Enforce module boundaries,dependencies...
│
├── Taskfile.yaml                                 # Top-level task file that references task files from sub-directories
```

### Rules and Principles

- Each application/platform module is self-contained -> contains all its code in its own directory
- Each application module is owned by a specific team
- Platform modules are owned by platform/infrastructure team
- Clear CODEOWNERS file for automated PR reviews
- No direct references between application modules, except .Abstractions
- Feature branches per module ```dev/moduleA/new-feature```
- Full System and Module-specific CI/CD pipelines
- Each module has unit and integration tests that test its functionality
- Cross-module integration tests, cross-cutting concerns, full system tests in separate project `tests/Integration/`

### Dependency Rules

```plaintext
                           ┌─────────────────────────────────────────────┐
                           │          Shared.Abstractions                │
                           │ (DTOs, interfaces, contracts, etc.)         │
                           └─────────────────────────────────────────────┘
                                          ▲         ▲
                                          │         │
                                          │         │
  ┌───────────────────────────────────────┘         └───────────────────────────────────────┐
  │                                                                                         │
```

```plaintext
┌───────────────────────┐        ┌──────────────────────────┐        ┌──────────────────────────┐
│ ModuleA.Abstractions  │        │   ModuleB.Abstractions   │        │   ModuleC.Abstractions   │
│                       │        │                          │        │ (and other modules'      │
│                       │        │                          │        │  abstractions)           │
└───────────────────────┘        └──────────────────────────┘        └──────────────────────────┘
         ▲                          ▲         ▲                      ▲         ▲
         │                          │         │                      │         │
         │                          │         │                      │         │
         └────────────────────┬─────┘         └───────────────────┬──┘         │
                              │                                   │            │
┌───────────────────┐    ┌───────────────────────┐           ┌───────────────────────┐
│  Shared.[...]     │    │        ModuleA        │    ...    │       ModuleB         │
│    (impl)         │    │        (impl)         │           │        (impl)         │
└───────────────────┘    └───────────────────────┘           └───────────────────────┘
             ▲               ▲                                ▲
             |               │                                │
             └───────----────|────────────────────────────────┘
                             │
┌────────────────────────────────────────────────────────────────────────────────┐
│                                 API Project                                    │
│                                    Api                                         │
└────────────────────────────────────────────────────────────────────────────────┘
```

## Development tools

OPAL uses following development tools:

- [npm](https://nodejs.org/en) for frontend development
- [.NET](https://dotnet.microsoft.com/) for backend code
- [PostgreSQL](https://www.postgresql.org/) as database
- [task](https://taskfile.dev) as task runner
- [Markdownlint](https://github.com/DavidAnson/markdownlint)
  and its [CLI](https://github.com/DavidAnson/markdownlint-cli2)
  to check the syntax and style of Markdown files
- [GitHub](https://github.com/) for source control, task tracking, and builds.

### Setting up the development environment

To install the development tools, you can use the following commands:

#### On Windows

```powershell
winget import winget.json
```

Some [tasks](#taskfile) use Powershell scripts (in the /scripts folder).
This requires the execution policy "Unrestricted".
To enable this policy, execute the following command using "Run as Administrator":

```powershell
pwsh Set-ExecutionPolicy -Scope CurrentUser -Policy Unrestricted
```

To install grpcurl (command-line utility to interact with GRPC services) on Windows, follow the instructions here [grpcurl on Windows](https://github.com/%5Bthermofisher/cmd-on-prem-platform%5D(https://github.com/thermofisher/cmd-on-prem-platform?tab=readme-ov-file#on-windows)?tab=readme-ov-file#on-windows)

#### On macOS and Linux (including WSL)

Using [Homebrew](https://brew.sh/):

```bash
brew bundle install
```

Note that the project targets only Windows for deployment.
While we try to keep the code platform-independent,
you may occasionally run into problems if you try to develop or run on macOS/Linux.

### Install dependencies

After installing the development tools,
install the dependencies:

```bash
task install
```

This will:

- Install the .NET dependencies from NuGet
- Install the Typescript dependencies from npm
- Install a development database in the data folder

You can re-run this command at any time, e.g. if dependencies have changed.
It's often a good idea to run this command after a `git pull`.

### Start OPAL

Now, you're ready to build and run OPAL using the following command:

```bash
task dev
```

This will:

- Build all code, if necessary
- Start the development database
- Start the OPAL backend server
- Start a development server to serve the frontend code and assets

Both servers are started in "watch mode".
When you make changes and save them, they will automatically rebuild as needed.

Press Ctrl+C stop the development servers.

Note that the database is not stopped automatically.
We keep it running since you often may want to inspect the database
using a tool such as pgAdmin4.

## Taskfile

This project uses a [Taskfile](https://taskfile.dev) as a task runner to automate common development and build steps.
The main taskfile provides the following tasks:

| Task              | Description                                         |
|-------------------|-----------------------------------------------------|
| `task dev`        | Start the development servers                       |
| `task build`      | Build the project or relevant binaries              |
| `task install`    | Install dependencies (NuGet and npm packages)       |
| `task update`     | Update dependencies                                 |
| `task lint`       | Run linters to check code quality                   |
| `task fix`        | Fix auto-fixable linter issues                      |
| `task test`       | Run tests                                           |
| `task fmt`        | Format the codebase                                 |
| `task check-fmt`  | Check source code formatting                        |

The following tasks are still to be implemented:

| Task              | Description                                         |
|-------------------|-----------------------------------------------------|
| `task clean`      | Clean up build artifacts                            |

<!-- link definitions -->
[Opal]: https://thermo-cmd.atlassian.net/wiki/spaces/CMDA/folder/3414098014?atlOrigin=eyJpIjoiM2FmNzljOTcxYjVkNDcxZWJlNjk2YTA1MWQ0N2ZkYzYiLCJwIjoiYyJ9

## Module Registration

### Introduction

The Opal platform uses a **three-phase module registration system** that allows modules to integrate seamlessly with the API host application. Modules register their capabilities through registration interfaces.

### Quick Start

To register a module with the API host:

1. **Create a Registration Class** implementing the required interfaces
2. **Add to ModuleRegistry.cs of Api Host** with a custom path prefix
3. **Implement Required Methods** for your module's capabilities

### Module Registration Interfaces

Implement these interfaces based on your module's capabilities:

| Interface | Purpose | Required |
|-----------|---------|----------|
| `IModule` | Basic module metadata (name, version, description) | ✅ Always |
| `IServiceRegistrationModule` | Register services with DI container | ✅ Usually |
| `IDbRegistrationModule` | Register database connections | ⚪ If using database |
| `IHttpEndpointRegistrationModule` | Register REST API endpoints | ⚪ If REST API |
| `IGrpcRegistrationModule` | Register gRPC services | ⚪ If gRPC |
| `IOpenApiRegistrationModule` | Configure Swagger documentation | ⚪ If documentation |
| `IMiddlewareRegistrationModule` | Register custom middleware | ⚪ If middleware |
| `ILifecycleModule` | Handle startup/shutdown events | ⚪ If lifecycle hooks |
| `IMessagingRegistrationModule` | Register Wolverine message handlers | ⚪ If handling Wolverine message handlers |

### Module Registration in API Host

```csharp
// In src/Api/ModuleRegistry.cs of API application host
  public static ModuleRegistryBuilder RegisterAllModules(this ModuleRegistryBuilder moduleBuilder)
  {
    return moduleBuilder
      // For modules with REST API endpoints (path prefix required)
      .RegisterModule<YourModuleRegistration>("your-path-prefix");

      // For modules without REST API endpoints (no path prefix needed)
      .RegisterModule<YourServiceOnlyModuleRegistration>();
  }
```

### Example Registration Class

```csharp
public class YourModuleRegistration :
    IServiceRegistrationModule,
    IHttpEndpointRegistrationModule,
    IOpenApiRegistrationModule
{
    public string Name => "YourModule";
    public string Version => "1.0.0";
    public string Description => "Description of your module";

    public void RegisterServices(ModuleRegistrationContext context)
    {
        // Register your services with DI
        context.Services.AddScoped<IYourService, YourService>();
    }

    public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
    {
        // Register your REST API endpoints
        endpoints.MapYourEndpoints();
    }

    public IReadOnlyCollection<string> RegisterOpenApiServices(IOpenApiServiceRegister openApiServiceRegister)
    {
        // Configure Swagger documentation
        openApiServiceRegister.AddOpenApi("your-module-spec-v1", options => {
            options.AddDocumentTransformer((document, context, _) => {
                document.Info.Title = "Your Module API";
                document.Info.Version = "v1";
                return Task.CompletedTask;
            });
        });
        return ["your-module-spec-v1"];
    }
}
```

### Database Access

Each module implementing `IDbRegistrationModule` gets its own dedicated schema.
By default, the schema name is the module name in lower case,
but you can override this using the `SchemaName` property.

#### Initializing or updating the database schema

The startup code calls the module's `InitializeOrMigrateSchema` method to perform any
initialization and migration steps needed to ensure that the schema contains the required tables.
At this point you can also create and update other resources, such as stored procedures.

For example:

```csharp
    public static void InitializeOrMigrateSchema(DbConnection dbConnection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS "sampleonionmodule"."samples" (
            "id"                UUID PRIMARY KEY,
            "name"              VARCHAR(200) NOT NULL,
            -- more fields as needed --
            );
            -- more commands as needed --
            """;
        dbConnection.Execute(sql);
    }
```

#### Accessing the module's schema

To access the data in the module's code, obtain a `DbConnection` via dependency injection with
[Keyed Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0#keyed-services):

```csharp
public class SampleOnionRepository(
    [FromKeyedServices("SampleOnionModule")] DbConnection connection
) : ISampleOnionRepository
```

The `FromKeyedServices` attribute specifies the unique *Service Key* of your module.
This is needed to distinguish your DB connection from the DB connections of the other modules.

You set the Service Key via the `ServiceKey` property.

### Wolverine Messaging

Documentation:

[Wolverine](https://wolverinefx.net/introduction/whatiswolverine.html)

[IMessageBus](https://wolverinefx.net/guide/messaging/message-bus.html)

[MessageHandlers](https://wolverinefx.net/guide/handlers/discovery.html)

`IMessageBus` provides methods to send, publish, and invoke messages.
`IMessageBus` is injected as a Scoped service into ASP.NET Core IOC/DI container.

`IMessagingRegistrationModule` is used to register message handlers with Wolverine.
On startup each registered module will receive `RegisterMessageHandlers` call if it implements `IMessagingRegistrationModule`.
Module can register message handlers by calling `RegisterMessageHandlers` on the provided `IMessagingRegistrationBuilder`.

#### Message Handler Discovery

By default, Wolverine is looking for public, concrete classes that follow any of these rules:

- Implements the `Wolverine.IWolverineHandler` interface
- Is decorated with the `[Wolverine.WolverineHandler]` attribute
- Type name ends with "Handler"
- Type name ends with "Consumer"

From the types, by default, Wolverine looks for any public instance method that is:

- Named `Handle`, `Handles`, `Consume`, `Consumes` or one of the names from Wolverine's Saga support
- Decorated by the `[WolverineHandler]` attribute if you want to use a different, descriptive name

**In all cases, Wolverine assumes that the first argument is the incoming message.**

#### Example of Wolverine message handler registration

```csharp
public class YourModuleRegistration
    : IServiceRegistrationModule,
      IMessagingRegistrationModule
{
    public void RegisterMessageHandlers(
        IMessagingRegistrationBuilder builder,
        ModuleRegistrationContext context
    )
    {
        // Register all message handlers in this assembly
        builder.RegisterMessageHandlers(this.GetType().Assembly);

        // Or register specific message handlers
        builder.RegisterMessageHandlers(typeof(YourMessageHandler));
    }
}
```

#### Example of a Wolverine message handler

```csharp
public class YourMessageHandler
{
    // Handles AddRequest messages by returning an AddResponse with the sum of X and Y.
    // AddResponse will be returned to the caller of IMessageBus.InvokeAsync<AddResponse>(AddRequest).
    // If user invoked AddRequest via SendAsync or PublishAsync, the response will be published as a message.
    public Task<AddResponse> HandleAsync(AddRequest add)
    {
        return Task.FromResult(new AddResponse(add.X + add.Y));
    }

    // Handles Message messages. This method is a placeholder and does not perform any action.
    public async Task HandleAsync(Message msg)
    {
        // Do something with the message
        await Task.CompletedTask;
    }
}
```

#### Example of sending a message

```csharp
// YourService is a Scoped service that uses the IMessageBus to send messages.
public class YourService
{
    private readonly IMessageBus _messenger;
    public YourService(IMessageBus _messenger)
    {
        _messenger = messenger;
    }

    // Sends an AddRequest message and returns the sum from the AddResponse.
    public async Task<int> AddAsync(int x, int y)
    {
        var response = await _messenger.InvokeAsync<AddResponse>(new AddRequest(x, y));
        return response.Sum;
    }

    // Sends a Message message asynchronously.
    public async Task SendMessageAsync(string content)
    {
        await _messenger.SendAsync(new Message(content));
    }

    // Publishes a Message message asynchronously.
    public async Task PublishMessageAsync(string content)
    {
        await _messenger.PublishAsync(new Message(content));
    }
}
```

### Path Prefix System

Modules implementing `IHttpEndpointRegistrationModule` **must** be registered with custom path prefixes that determine their API URLs:

- **Registration**: `RegisterModule<YourModuleRegistration>("users")` (path prefix required)
- **API URLs**: `/api/users/...` (instead of `/api/YourModule/...`)

Modules that don't implement `IHttpEndpointRegistrationModule` (such as service-only modules) are registered without a path prefix:

- **Registration**: `RegisterModule<YourServiceModuleRegistration>()` (no path prefix)
- **No API URLs**: Module provides services only, no REST endpoints

**Important**: Attempting to register a module implementing `IHttpEndpointRegistrationModule` without a path prefix will result in an `InvalidOperationException` at registration time.

### API Endpoints

#### REST API Endpoints

Modules REST endpoints are exposed under the `/api/module-path-prefix/` path prefix:

```bash
curl -X GET http://localhost:61350/api/{module-path-prefix}/{module-defined-endpoint}
```

#### gRPC Endpoints

In Development, GRPC reflection is enabled, allowing clients to discover available services and methods.

```bash
grpcurl localhost:61350 describe
```

> Path prefix does not apply to gRPC services

### Configuration

Modules can access their configuration through the registration context:

```csharp
public void RegisterServices(ModuleRegistrationContext context)
{
    var moduleConfig = context.GetModuleConfiguration();
    context.Services.Configure<YourModuleOptions>(moduleConfig);
}
```

Each module has its own configuration section in the application's configuration file (e.g., `appsettings.json`) based on `IModule.ModuleName`.

**Configuration Structure:**

```json
{
  "Modules": {
    "YourModule": {
      "Setting1": "value1",
      "Setting2": 42
    }
  }
}
```

### Examples

See the sample modules for complete implementations:

- **[SampleOnionModule](src/modules/SampleOnionModule/README.md)** - Onion Architecture pattern
- **[SampleVerticalModule](src/modules/SampleVerticalModule/README.md)** - Vertical Slice Architecture pattern
