# 🚀 Modular UI – Feature-Based React Architecture

A complete **feature-driven modular React architecture** designed for scalability, maintainability, and team independence. Each business capability is a self-contained module, enabling parallel development without stepping on each other’s code.

---

## 🏗️ Project Structure

```text

src/
├── App.tsx                        # Root app component & routing
├── main.tsx                       # Application entry point
│
├── modules/                       # Feature Modules (business domains)
│   ├── acquisition/               # Acquisition Feature
│   │   ├── AcquisitionModule.tsx  # Module entry & routing
│   │   ├── components/
│   │   │   ├── Acquisition.tsx
│   │   │   ├── AcquisitionList.tsx
│   │   │   └── index.ts
│   │   └── index.ts
│   │
│   └── instruments/               # Instruments Feature
│       ├── InstrumentsModule.tsx
│       ├── components/
│       │   ├── Instruments.tsx
│       │   ├── InstrumentDetail.tsx
│       │   └── index.ts
│       └── index.ts
│
├── platform/                      # Cross-cutting platform services
│   └── auth/
│       ├── Login.tsx
│       └── index.ts
│
├── shared/                        # Shared resources
│   ├── components/                # Reusable UI
│   ├── contexts/                  # User, SSE, Notifications
│   ├── constants/                 # Global constants
│   ├── types/                     # Enums & types
│   ├── models/                    # Data models
│   └── index.ts
│
└── shell/                         # App shell & infrastructure
    ├── Home.tsx                   # Dashboard/home
    ├── MainLayout.tsx             # Global layout & nav
    └── ModuleRegistry.tsx         # Module registration
```

---

## 🎯 Why This Architecture?

### ✅ Feature-Based

- Modules grouped by **business domain**
- Teams own features end-to-end
- Easy to scale as the app grows

### ✅ Modular Components

- Each module has its own `components/`
- **Barrel exports** keep imports clean
- Shared components live in `shared/`

### ✅ Team Independence

- Clear boundaries reduce conflicts
- Shared contexts for global state
- Module registry = plug-and-play features

### ✅ Scalable & Flexible

- Domain-driven design
- TypeScript-first
- Works with Material-UI, Ant Design, or custom components
- Vite/Webpack compatible

---

## 👩‍💻 Adding a New Module (Step-by-Step)

1. **Scaffold your folder**

   ```text

   src/modules/your-feature/
   ├── YourFeatureModule.tsx
   ├── components/
   │   ├── YourFeature.tsx
   │   ├── YourFeatureDetail.tsx
   │   └── index.ts
   └── index.ts
   ```

2. **Barrel exports**

   ```ts

   // components/index.ts
   export { YourFeature } from './YourFeature';
   export { YourFeatureDetail } from './YourFeatureDetail';

   // index.ts
   export { YourFeatureModule } from './YourFeatureModule';
   export * from './components';
   ```

3. **Leverage shared utilities**

   ```tsx
   // src/modules/your-feature/YourFeatureModule.tsx
   import { UserContext, NotificationContext } from "../../shared/contexts";
   import { SAMPLE_TYPES } from "../../shared/constants/acquisition-constants";
   import { API_ENDPOINTS } from "../../shared/constants/navigation-constants";
   ```

4. **Register the module** (routes AND application cards)

   ```tsx
   // src/shell/ModuleRegistry.tsx
   import { YourFeatureModule } from "../modules/your-feature";
   import { YourIcon } from "@mui/icons-material";

   // Define module registration (both route and application card)
   const moduleRegistrations = [
     {
       // Route registration
       path: "/your-feature/*",
       element: <YourFeatureModule />,

       // Application card registration
       application: {
         id: 10,
         name: "Your Feature",
         description: "Description of your feature functionality.",
         icon: React.createElement(YourIcon),
         category: "core" as const,
         path: "/your-feature",
         showInNavigationBar: true, // optional - appears in sidebar navigation
       }
     }
   ];
   ```

## Module Registration & Extension Points

### How Modules Expose Functionality

Modules in this architecture expose functionality through several extension points:

1. **Main Menu Cards**: Modules register in the `ModuleRegistry.tsx` with application metadata to appear as cards on the home page
2. **Navigation Routes**: Routes are automatically created from module registrations in `ModuleRegistry.tsx`
3. **Left Navigation**: Modules can set `showInNavigationBar: true` in their registration to appear in the sidebar
4. **Shared Exports**: Modules export reusable components, contexts, and services via barrel exports for cross-module usage

### Current Implementation: Single Registry Pattern

**Key Principle**: All module registration happens in **one place** - the `ModuleRegistry.tsx` file. This provides a single source of truth for all module definitions, routes, and application metadata.

### Module Registration Interface

Each module follows this interface for platform integration:

```tsx
// Current ModuleRegistration interface
interface ModuleRegistration {
  path: string; // Route path (e.g., "/your-feature/*") or "" for data-only entries
  component: React.ComponentType; // Main module component
  application?: ApplicationData; // Application card for home page (optional)
}

// ApplicationData interface (for application cards)
interface ApplicationData {
  id: number;
  name: string;
  description: string;
  icon: React.ReactElement;
  category: "core" | "utility";
  path: string; // Navigation path
  showInNavigationBar?: boolean; // Optional sidebar navigation
}
```

---

## 📝 Placeholder Modules

Some modules in the `ModuleRegistry.tsx` are registered as placeholders for planned features. These entries include application card metadata (for discoverability and navigation) but do not yet have an implemented route or component. In the registry, their `path` is set to an empty string (`""`) and their `component` is a no-op (e.g., `() => null`).

**Example:**

```tsx
{
  path: "", // No route - not implemented yet
  component: () => null,
  application: {
    id: 5,
    name: "Global Settings",
    description: "Define administrative settings that are applied across the OPAL.",
    icon: React.createElement(SettingsOutlinedIcon),
    category: "utility",
    path: "/settings",
    showInNavigationBar: true
  }
}
```

This allows the UI to display upcoming features in navigation and on the home page, while making it clear that the functionality is not yet available. When development begins, simply update the `path` and `component` fields to enable the feature.

---

## 📐 Naming Conventions

- **Components** → `PascalCase.tsx`
- **Non-components** → `kebab-case.ts`
- **Constants** → `UPPER_SNAKE_CASE`
- **Variables/Functions** → `camelCase`
- **Hooks** → `useCamelCase`

---

## 🏗️ Architecture Principles

### Feature-Based Scaling

- Each module = self-contained capability
- Teams develop independently

### Shared Services

- Common utilities in `shared/`
- Cross-cutting concerns in `platform/`
- Global infra in `shell/`

### Module Communication

Modules in this architecture do not communicate with each other directly. Instead, they share data and trigger actions through shared React contexts or platform services. This approach ensures loose coupling, making it easy to add, remove, or update modules without affecting others. The diagram below illustrates this indirect communication pattern:

```text
Module A ↔ Shared Context ↔ Module B
                 │
                 ▼
          Platform Services
```
