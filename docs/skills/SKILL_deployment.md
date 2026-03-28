# Deployment Skill

## Purpose

Describe how this repo is run locally and what deployment artifacts currently exist for frontend and backend delivery.

## Key files and locations

- `fruta-client/vercel.json`
- `fruta-client/package.json`
- `fruta-client/vite.config.js`
- `frutaaaaa/Properties/launchSettings.json`
- `frutaaaaa/Properties/PublishProfiles/*.pubxml`
- `frutaaaaa/Properties/ServiceDependencies/fruta - Web Deploy/profile.arm.json`
- `frutaaaaa/appsettings.json`
- `frutaaaaa/Program.cs`

## Local development

### Backend

- Project launch URL from `launchSettings.json`: `https://localhost:44374`
- Swagger launch path: `/swagger`
- `ASPNETCORE_ENVIRONMENT` is `Development` for local profiles

### Frontend

- Vite config is minimal; no proxy is defined in `vite.config.js`
- API base URL comes from `VITE_API_BASE_URL`
- fallback API URL in `apiService.js` is `https://localhost:44374`

Example local startup:

```powershell
dotnet run --project .\frutaaaaa\frutaaaaa.csproj
cd .\fruta-client
$env:VITE_API_BASE_URL='https://localhost:44374'
npm run dev
```

## Frontend deployment

- `vercel.json` rewrites every path to `/index.html`
- This is the standard SPA fallback required for React Router on Vercel

Real file content:

```json
{
  "rewrites": [
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

## Backend deployment artifacts

### File-system publish profiles

- `\\192.168.1.154\sauvgarde\DEPLOYMENT`
- `C:\Users\info\Desktop\DEPLOYMENT`

### Azure artifacts

- Azure App Service publish profile exists
- ARM template exists for resource `fruta`
- This suggests Azure deployment was set up at some point, even if it is not the only active path

## Environment/config dependencies

- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:JournalConnection`
- `VITE_API_BASE_URL`
- CORS allowed origins hard-coded in `Program.cs`

## Step by step: prepare a deployment-safe change

1. Check whether the change affects frontend, backend, or both.
2. For backend changes, confirm the deployed environment will allow the current origin list in `Program.cs`.
3. For frontend changes, confirm the deployed API base URL is set correctly.
4. If you add new config, document where it belongs because there is no centralized env management layer in repo.
5. Build both projects locally before treating the change as deployable.

## Gotchas

- Swagger is enabled in both development and non-development branches in `Program.cs`.
- `appsettings.json` currently contains committed connection strings and credentials.
- No CI pipeline means deployment is probably manual or external to this repo.
- CORS origins include transient hosts like ngrok, so environment drift is likely.

## Real example from this codebase

The backend CORS policy is named `AllowReactApp` and includes these kinds of origins directly in source:

- `http://localhost:5173`
- `https://fruta-six.vercel.app`
- `https://fruta-api.ddnsfree.com`
- `https://fruta.accesscam.org`

If a new frontend host is introduced and not added here, the browser will fail before business logic is reached.
