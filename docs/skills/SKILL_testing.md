# Testing Skill

## Purpose

Document the current validation strategy for this repo, which is mostly manual because no automated test suite or CI pipeline exists.

## Key files and locations

- `frutaaaaa/frutaaaaa.csproj`
- `frutaaaaa/Properties/launchSettings.json`
- `fruta-client/package.json`
- `.github/workflows/` (empty)
- `frutaaaaa/build_log*.txt`
- `frutaaaaa/build_err.txt`

## Current reality

- No xUnit/NUnit/MSTest project found.
- No Jest/Vitest/Playwright/Cypress setup found.
- `.github/workflows` exists but contains no workflow file.

That means every non-trivial change should include a manual verification plan.

## Most-used validation commands

### Backend

```powershell
dotnet build .\frutaaaaa\frutaaaaa.csproj
dotnet run --project .\frutaaaaa\frutaaaaa.csproj
```

### Frontend

```powershell
cd .\fruta-client
npm install
npm run build
npm run dev
```

## Step by step: validate a typical feature change

1. Build the backend.
2. Build the frontend.
3. Run the backend on `https://localhost:44374`.
4. Run the frontend with `VITE_API_BASE_URL=https://localhost:44374`.
5. Log in using a real tenant DB name.
6. Exercise the changed screen end to end.
7. Verify any affected write operation also still audits correctly if relevant.

## Domain-specific manual checks

### Auth changes

- Login works with tenant DB selection.
- `sessionStorage` gets `user`, `machineName`, and `sessionId`.
- protected routes redirect correctly.

### API changes

- Request includes `X-Database-Name`.
- response shape matches the current page expectation.
- non-GET writes do not break audit logging.

### Dashboard/reporting changes

- filters update all dependent sections
- charts render
- tables still sort
- PDF export still works

### Gestion avance / vente ecart changes

- parent row saves
- child rows save
- update path replaces detail rows correctly
- delete path cleans up expected rows

## Dependencies

- live or reachable MySQL tenant DB
- local backend certificate/trust for `https://localhost:44374`
- frontend env var `VITE_API_BASE_URL`

## Gotchas

- Because production-like connection strings are committed in `appsettings.json`, local commands may hit real infrastructure if you do not override config carefully.
- Session and audit flows may fail silently, so explicit log or DB verification is sometimes necessary.
- Build artifacts are checked in; do not mistake a stale `dist/` or `bin/` output for proof that your change works.

## Real example from this codebase

The frontend build command is defined exactly as:

```json
"build": "vite build"
```

There is no test script in `fruta-client/package.json`, so `npm run build` is the closest frontend gate currently available.
