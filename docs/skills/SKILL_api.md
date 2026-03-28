# API Skill

## Purpose

Describe how endpoints are structured, how request/response payloads are shaped, and how to add or modify controller actions without breaking the frontend.

## Key files and locations

- `frutaaaaa/Program.cs`
- `frutaaaaa/Controllers/*.cs`
- `frutaaaaa/frutaaaaa.http`
- `fruta-client/src/apiService.js`
- `fruta-client/src/pages/*.jsx`

## Endpoint structure

- Controllers use `[Route("api/[controller]")]` and `[ApiController]`.
- Most domain controllers expose REST-ish CRUD plus custom reporting routes.
- The frontend calls routes with lowercase paths matching controller names:
  - `/api/dailyprogram`
  - `/api/dashboard/data`
  - `/api/vente-ecart`
  - `/api/users/login`
  - `/api/sessions/start`

## Shared API contract rules

- Tenant DB is selected by `X-Database-Name`, not by route segment or token claim.
- Most GET requests require the tenant header even if the action signature does not strongly validate it.
- Response shapes are inconsistent by design:
  - entities
  - DTOs
  - anonymous objects
  - plain strings on error
  - `{ message: ... }` objects

Do not normalize response shapes in one endpoint unless you also update all consumers.

## Request helper pattern on the frontend

Use the existing wrappers from `apiService.js`:

```js
export const apiPost = (endpoint, body, databaseName = null) => {
  return apiFetch(endpoint, {
    method: 'POST',
    body: JSON.stringify(body),
  }, databaseName);
};
```

The wrapper automatically supplies tenant and audit headers from `sessionStorage`.

## Real endpoint families

- `UsersController`
  - login, register, users CRUD, permission CRUD
- `LookupController`
  - dropdown/reference data
- `DashboardController`
  - heavy analytics/reporting endpoints
- `DailyProgramController`
  - program CRUD
- `TraitController`, `TraitementController`, `DefautController`, `MarqueController`
  - standard CRUD-ish master data
- `SampleController`
  - shelf-life workflow and history
- `GestionAvancesController`
  - decompte entry, yearly report, wizard details
- `VenteEcartController`
  - unsold ecarts, vente creation/update/delete
- `SessionController`
  - audit/session tracking side channel

## Step by step: add a new endpoint

1. Choose the owning controller by domain.
2. Accept `[FromHeader(Name = "X-Database-Name")] string database` if the endpoint uses tenant data.
3. Open the tenant DB with the controller’s `CreateDbContext(database)`.
4. Preserve existing casing and payload style expected by the frontend.
5. If the endpoint mutates data, prefer EF writes so the audit interceptor still captures them.
6. Add a dedicated helper in `fruta-client/src/apiService.js` only if the route will be reused by pages/components.
7. Wire the consuming page to the helper and verify the exact JSON shape.

## Request/response templates from this repo

### CRUD create pattern

```csharp
[HttpPost]
public async Task<ActionResult<DailyProgram>> PostDailyProgram(
    [FromHeader(Name = "X-Database-Name")] string database,
    DailyProgram dailyProgram)
{
    using (var _context = CreateDbContext(database))
    {
        _context.DailyPrograms.Add(dailyProgram);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDailyProgram), new { id = dailyProgram.Id, database = database }, dailyProgram);
    }
}
```

### Reporting pattern

`DashboardController` builds filtered `IQueryable`s, materializes lookup dictionaries, then returns DTO/anonymous projections in one response object.

### Transactional write pattern

`GestionAvancesController` and `VenteEcartController` use `BeginTransactionAsync()` when one operation spans parent + child rows.

## Dependencies

- `apiService.js`
- tenant DB routing helper pattern
- EF Core model mappings
- frontend pages expecting specific field names

## Gotchas

- Some controllers return `NotFound()` with no body, others return `NotFound("...")`.
- Some errors are returned as plain text `StatusCode(500, "...")`, which means `apiService.js` may only surface a generic HTTP message if JSON parsing fails.
- `Program.cs` exposes Swagger in both development and non-development branches.
- Session endpoints intentionally use different minimal headers than normal business endpoints.

## Real example from this codebase

The admin permission save flow expects this exact POST body shape from `AdminPage.jsx`:

```js
const permissionsData = availablePages.map(page => ({
  PageName: page.id,
  Allowed: userPermissions[userId]?.[page.id] || false
}));
```

That is why `UsersController.UpdateUserPermissions` reads `request.Permissions` and recreates rows from `PageName`/`Allowed`.
