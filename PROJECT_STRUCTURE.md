# V3RII Aqua Project Structure

This document explains the Aqua project structure, the main business flow, and the architectural rules used by the API and web applications.

## 1. Purpose

V3RII Aqua is an aquaculture operations platform. It manages projects, cages, fish batches, warehouses, opening imports, daily operations, ERP mirror data, reporting, and KPI/FCR calculations.

The product is designed around a step-by-step operational flow:

1. Define master data such as projects, cages, warehouses, stocks, weather definitions, wind/current direction definitions, and ERP mirror mappings.
2. Import or create the opening state of the farm.
3. Enter daily operations such as feeding, mortality, weather, weighing, transfers, stock conversion, project merge, and shipment.
4. Calculate operational balances and KPI reports from persisted movements.
5. Monitor ERP/Hangfire synchronization and administrative activities.

## 2. Repository Layout

The Aqua solution is split into separate frontend and backend projects.

| Path | Responsibility |
| --- | --- |
| `verii_aqua_api` | .NET API, persistence, business rules, reporting, integrations, Hangfire jobs, localization resources |
| `verii_aqua_web` | React frontend, routes, pages, feature modules, localization JSON files, runtime config |
| `verii_crm_api` | Reference architecture used for CRM-style module, localization, and extension conventions |

The Aqua API follows the same high-level style as CRM API: thin host startup, module-based services, module-local dependency registration, and localized API responses.

## 3. API Architecture

The API is organized around bounded modules under `Modules`.

Typical module structure:

```text
Modules/<ModuleName>/
  Api/
  Application/
    Dtos/
    Mappings/
    Services/
  Domain/
    Entities/
    Enums/
  Infrastructure/
    Persistence/
      Configurations/
  DependencyInjection/
  Localization/
```

Not every module needs every folder. Empty folders or fake boundaries should not be created just for symmetry. A folder exists when the module owns code for that layer.

## 4. Main API Modules

| Module | Responsibility |
| --- | --- |
| `Projects` | Project definitions and uniqueness rules |
| `Cages` | Cage definitions and cage/warehouse mapping |
| `FishBatches` | Fish batch definitions and batch lifecycle |
| `GoodsReceipts` | Opening and purchase receipt operations |
| `OpeningImports` | Excel-based first setup and transactional opening commit |
| `Feedings` | Daily feeding headers, lines, and distribution |
| `Mortalities` | Daily mortality/fire entries and biomass effect |
| `Weighings` | Average weight tracking |
| `Transfers` | Cage-to-cage, cage-to-warehouse, warehouse-to-cage, warehouse-to-warehouse movement logic |
| `StockConverts` | Fish growth and stock conversion operations |
| `Shipments` | Sales/shipment operations used by KPI and FCR |
| `BatchBalances` | Batch and cage balance projections |
| `AquaReports` | Legacy/compatibility report services |
| `KpiReport` | Senior-grade aggregated KPI/FCR report endpoints |
| `Stock` | Aqua stock and ERP stock mirror-facing data |
| `Warehouse` | Warehouse definitions and mirror warehouse support |
| `Integrations` | Netsis/ERP read services, mail settings, and integration facades |
| `System` | Hangfire monitoring, jobs, bootstrap, and operational system services |

## 5. Host and Extension Rules

`Program.cs` should stay thin. Application bootstrapping belongs in shared host extensions.

Current host flow:

```text
Program.cs
  AddAquaApiWebApi(...)
    AddDbContext
    AddHangfire
    AddAutoMapper
    AddAquaApplicationModules
    AddLocalization
    AddAuthentication
    AddSwagger
  UseAquaApiWebApi(...)
    UseExceptionHandler
    UseRouting
    UseCors
    UseAuthentication
    UseAuthorization
    MapControllers
    MapHangfire
```

Module registration is centralized in:

```text
Shared/Host/WebApi/Extensions/ModuleServiceCollectionExtensions.cs
```

Each business module still owns its own `DependencyInjection/ServiceCollectionExtensions.cs`. The central host extension only calls module-level registration methods.

## 6. Localization Rules

API localization uses `.resx` files and `LocalizationService`.

Rules:

1. API messages must not be hard-coded when they are returned to users.
2. Shared messages live under `Shared/Localization`.
3. Module-specific messages live under `Modules/<Module>/Localization`.
4. `LocalizationService` discovers `*LocalizationResource` marker classes automatically.
5. New modules that return localized messages should add a marker class and resource files.
6. Existing fallback behavior must be preserved when moving messages between modules.

Supported API cultures include:

```text
tr-TR, en-US, de-DE, fr-FR, es-ES, it-IT, ar-SA
```

## 7. Frontend Architecture

The web project is feature-oriented. Aqua pages are split into feature folders instead of one large Aqua module.

Common frontend feature structure:

```text
src/features/<feature>/
  api/
  hooks/
  types/
  config/
  localization/
  pages or components
```

Shared UI and shared localization remain under:

```text
src/shared/
src/components/shared/
```

Route-level lazy loading is used so heavy pages, report screens, and feature-specific localization are loaded only when needed.

## 8. Operational Flow

### 8.1 Opening Import

Opening Import is used to create the initial state of the farm. It can create or validate projects, cages, batches, opening stock, goods receipt lines, feeding, mortality, weather, and shipment rows depending on the template.

The commit operation must be transactional. If any critical row fails, the system should not partially apply opening data.

### 8.2 Quick Daily Entry

Quick Daily Entry is the main daily operation screen. A user selects project, cage, and date, then enters operational steps:

| Operation | Meaning |
| --- | --- |
| Feeding | Feed given to selected cage/batch for a selected slot |
| Mortality / Fire | Dead fish count; biomass should be calculated from current or known average weight |
| Weather | Daily weather type, severity, sea temperature, wind direction, and current direction |
| Net Operation | Net-related operational changes |
| Cage Change | Movement between cages |
| Cage to Warehouse | Fish moved from cage into warehouse |
| Warehouse to Warehouse | Warehouse stock transfer |
| Warehouse to Cage | Stock moved from warehouse into cage |
| Shipment | Sales/shipment movement |

### 8.3 Reports

Reports should prefer aggregated backend endpoints instead of sending many small frontend requests.

Important reports:

| Report | Purpose |
| --- | --- |
| Devir / FCR | Project-level opening, remaining stock, feed, produced biomass, mortality biomass, shipment biomass, and FCR |
| Raw KPI | Operational KPI totals |
| Business KPI | Revenue, margin, shipment, and business-facing KPI values |
| Project Detail | Project-specific movement and balance detail |
| Cage Balance | Cage/batch current balance |

## 9. ERP and Mirror Data

Netsis mirror data is read through integration modules. Stock, warehouse, branch, and customer-like mirror entities should be visible through paged frontend screens.

Feed selection should use ERP stock metadata, especially group code and group name. Feed stock filtering should use the configured group code such as `YEM`.

## 10. Coding Rules

1. Keep `Program.cs` thin.
2. Keep module registration inside module dependency injection extensions.
3. Do not add business rules directly into controllers.
4. Use transactions for commands that write to multiple tables.
5. Do not create fake empty module folders.
6. Avoid broad global usings; prefer explicit using statements where dependencies are module-specific.
7. Do not hard-code user-facing API messages.
8. Keep frontend route-level features lazy where possible.
9. Keep report screens backed by aggregated report endpoints.
10. Preserve backward-compatible endpoints unless a controlled migration is planned.

## 11. Current Alignment Status

The Aqua API is now aligned with the CRM API approach in the important areas:

| Area | Status |
| --- | --- |
| Thin host startup | Aligned |
| Central module registration map | Aligned |
| Module-local DI extensions | Aligned |
| Module localization folders | Aligned |
| Automatic localization resource discovery | Aligned |
| Reduced global namespace surface | Aligned |
| Build and test verification | Passing |

Remaining improvements should be done module-by-module, not as a large rewrite.
