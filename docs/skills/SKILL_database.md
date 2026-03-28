# Database Skill

## Purpose

Explain tenant DB routing, EF Core mappings, schema ownership, migrations, and the manual SQL artifacts in this repository. Use this before changing models, tables, queries, or anything that touches `CreateDbContext`.

## Key files and locations

- `frutaaaaa/Data/ApplicationDbContext.cs`
- `frutaaaaa/Program.cs`
- `frutaaaaa/Migrations/*`
- `frutaaaaa/Models/*.cs`
- `create_tables_prod.sql`
- `gestionavances_schema.sql`
- `frutaaaaa/Models/ShelfLifeTables.sql`
- `frutaaaaa/appsettings.json`

## The most important pattern: tenant DB routing

Most controllers do not use the injected scoped `ApplicationDbContext` registered in `Program.cs`. Instead they repeat a helper like this:

```csharp
var baseConnectionString = _configuration.GetConnectionString("DefaultConnection");
var dynamicConnectionString = baseConnectionString.Replace("frutaaaaa_db", dbName);
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseMySql(dynamicConnectionString, ServerVersion.AutoDetect(dynamicConnectionString));
return new ApplicationDbContext(optionsBuilder.Options);
```

That means:

- the selected database name must keep the same connection-string shape
- the literal `frutaaaaa_db` string is the replacement anchor
- if you rename the base DB in config, every tenant-routing helper becomes fragile

## Schema ownership by area

### EF-managed or partially EF-managed

- `ecart_direct`
- `ecart_e`
- `vente`
- `marque`
- `marque_assignment`
- `sample_test`
- `daily_check`
- `daily_check_detail`
- `gestionavance_details`
- core entities listed in `ApplicationDbContext`

### Manual SQL / not fully represented by migrations

- `gestionavances`
- `charges`
- `adherent_charges`
- some shelf-life structures
- many legacy source tables/views mapped as `HasNoKey()`

### Read-only legacy mappings

Several source tables are configured as no-key queryable entities:

- `destination`
- `partenaire`
- `grpvar`
- `tpalette`
- `verger`
- `palbrut`
- `palette`
- `palette_d`
- `variete`
- `bdq`
- `dossier`
- `view_expvervar`

Do not assume these can be updated cleanly through EF without checking the underlying table behavior first.

## Current migration reality

- Migrations exist in `frutaaaaa/Migrations`, but they cover only part of the schema.
- Example migration families:
  - `AddEcartTable`
  - `UpdateTraitAndTraitementTables`
  - `AddMarqueAssignmentTable`
- `gestionavances`, charges, and some sample-related setup also exist as SQL files outside migrations.

Golden rule: inspect both `ApplicationDbContext.cs` and the SQL files before deciding the source of truth for a table.

## Important model conventions

- Many entity/table names preserve legacy lowercase MySQL names.
- Some files have awkward names, including spaces:
  - `frutaaaaa/Models/DailyProgram .cs`
  - `frutaaaaa/Models/ecart_e .cs`
- `ApplicationDbContext` sets explicit collations/charsets for some string fields:
  - `Trait` uses `utf8mb4_unicode_ci`
  - `Marque` uses `latin1_swedish_ci`
- Composite key:
  - `UserPagePermission` key is `{ UserId, PageName }`
- Cascades:
  - `DailyCheck -> DailyCheckDetail`
  - `GestionAvance -> GestionAvanceDetail`
  - `UserPageVisit -> UserSession`

## Step by step: add a tenant-aware entity

1. Add the model under `frutaaaaa/Models`.
2. Register a `DbSet<>` in `ApplicationDbContext`.
3. Configure table name, key, types, indexes, and relationships in `OnModelCreating`.
4. Decide whether the schema should live in:
   - EF migrations
   - a manual SQL file
   - both, if legacy DB rollout requires it
5. Add controller endpoints that accept `[FromHeader(Name = "X-Database-Name")] string database`.
6. Use the existing per-controller `CreateDbContext(database)` pattern unless you are doing a deliberate refactor across the app.

## Step by step: modify an existing table safely

1. Find the model and mapping in `ApplicationDbContext.cs`.
2. Search for matching SQL in:
   - `create_tables_prod.sql`
   - `gestionavances_schema.sql`
   - `frutaaaaa/Models/ShelfLifeTables.sql`
3. Search controllers that read or write the table.
4. Search frontend pages that depend on the current response shape.
5. If the table is used by transaction-heavy code such as `GestionAvancesController` or `VenteEcartController`, update the full transaction flow, not just the model.

## Query patterns used in this project

- Heavy LINQ joins over legacy tables for analytics in `DashboardController.cs`
- Manual transaction scopes for multi-table writes:
  - `GestionAvancesController`
  - `VenteEcartController`
- Raw SQL delete for permission reset in `UsersController.UpdateUserPermissions`

Real example:

```csharp
await _context.Database.ExecuteSqlRawAsync(
    "DELETE FROM user_page_permissions WHERE user_id = ?",
    userId
);
```

## Dependencies

- MySQL connectivity through Pomelo
- connection strings in `appsettings.json`
- tenant header propagation from the frontend
- EF interceptor-based auditing for non-GET writes

## Gotchas

- Production DB credentials are committed in source.
- The runtime DI `ApplicationDbContext` registration exists, but most business controllers bypass it.
- Refactoring to a centralized tenant context factory would be worthwhile, but doing it partially is risky because every controller currently owns its DB creation.
- Some domains mix DTOs, anonymous objects, and entity returns in one controller.
- No automated DB migration/deploy pipeline exists in repo.

## Real example from this codebase

The shelf-life domain depends on manual SQL documented in `frutaaaaa/Models/ShelfLifeTables.sql`, while the live C# models add fields such as `Pdsfru`, `Couleur1`, and `Couleur2` that go beyond the bare SQL sample. Always compare both before making assumptions.
