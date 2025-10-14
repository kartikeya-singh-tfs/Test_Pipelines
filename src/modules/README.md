# Modules

Folder contains modules, representing individual applications, bundle of business functionality.

- Each module contains its own implementation
  - Application logic
  - Abstractions
  - Tests
    - tests that validate behavior of single module or are focused on single module without external dependencies (or with mocked dependencies) unit and integration tests located in module's folder
  - Data access
- Modules can be structured in various ways, such as:
  - Clean Architecture style with distinct layers
    - ModuleA
    - .ModuleA.Abstractions
    - ModuleA.AspNetCore
    - .ModuleA.Tests
    - .ModuleA.Persistence
    - .ModuleA.[...]
    - .ModuleA.Opal
  - Vertical Slice style with features
    - ModuleB.Feature1
    - .ModuleB.Feature1.Abstractions
    - .ModuleB.Opal
    - ...
- Integration code to OPAL platform is located in `Module.Opal` project
  - Each module has its own Opal API host registration
    - ModuleA.Opal
    - ModuleB.Opal
- Each module should be independent (to the extent possible)
