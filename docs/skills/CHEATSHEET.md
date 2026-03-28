# Fruta Cheatsheet

## Project purpose

Fruta is a tenant-selected fruit operations web app: users log into a chosen MySQL database, then manage export programs, analytics, quality sampling, ecarts, and adherent decompte workflows through a React SPA backed by an ASP.NET Core API. The backend also writes audit logs and session/page-tracking records to a separate journal database.

## Folder map

- `fruta-client/` React 19 + Vite frontend
- `fruta-client/src/pages/` route screens
- `fruta-client/src/components/` shared UI/charts/layout
- `fruta-client/src/apiService.js` frontend API wrapper and header injection
- `frutaaaaa/` ASP.NET Core backend
- `frutaaaaa/Controllers/` API endpoints by domain
- `frutaaaaa/Data/ApplicationDbContext.cs` tenant business DB mapping
- `frutaaaaa/Audit/` audit/session pipeline and journal DB mapping
- `frutaaaaa/Migrations/` partial EF migrations
- `create_tables_prod.sql`, `gestionavances_schema.sql`, `frutaaaaa/Models/ShelfLifeTables.sql` manual schema docs

## Most-used commands

```powershell
dotnet build .\frutaaaaa\frutaaaaa.csproj
dotnet run --project .\frutaaaaa\frutaaaaa.csproj
cd .\fruta-client
npm install
$env:VITE_API_BASE_URL='https://localhost:44374'
npm run dev
npm run build
```

## Most common tasks

- Change login, permissions, sessions, or audit headers: read `docs/skills/SKILL_auth.md`
- Change models, schema, queries, or migrations: read `docs/skills/SKILL_database.md`
- Add or edit endpoints: read `docs/skills/SKILL_api.md`
- Add or edit pages/components/UI flows: read `docs/skills/SKILL_frontend.md`
- Validate a change safely: read `docs/skills/SKILL_testing.md`
- Handle env/deploy/CORS/runtime setup: read `docs/skills/SKILL_deployment.md`
- Match existing error behavior: read `docs/skills/SKILL_error_handling.md`

## Critical rules: never break these

1. Preserve `X-Database-Name` flow; most business endpoints depend on it for tenant routing.
2. Do not mistake React route guards for real backend authorization; there is no server auth enforcement.
3. Keep non-GET writes compatible with audit logging by using EF save flows and existing audit headers.
4. Do not edit generated folders (`dist`, `node_modules`, `bin`, `obj`) as if they were source.
5. Check both EF mappings and manual SQL files before changing a table; schema ownership is split.
