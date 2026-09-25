# Fruta

Fruit packing & quality management app.

| Part | Where | Stack |
|---|---|---|
| API | `frutaaaaa/` | ASP.NET Core 8, EF Core, MySQL (Pomelo) |
| Web app | `fruta-client/` (separate repo: `idrisselbergui/fruta-client`) | React 19, Vite |
| Database | MySQL server on the office network (`192.168.1.247:3306`) | `frutaaaaa_db` + `fruta_web_journal` (audit) |

## Run for development

1. **API** — open `frutaaaaa.sln` in Visual Studio and start the `frutaaaaa` profile,
   or from the repo root:
   ```bash
   dotnet run --project frutaaaaa --launch-profile frutaaaaa
   ```
   It listens on `https://localhost:44374` (Swagger at `/swagger`).

2. **Web app** — in `fruta-client/`:
   ```bash
   npm install
   npm run dev
   ```
   Open `http://localhost:5173`. It calls the API at `https://localhost:44374`
   unless `VITE_API_BASE_URL` is set.

The database connection strings are in `frutaaaaa/appsettings.json`
(`DefaultConnection`, `JournalConnection`).

## Deployment

Being reworked: Fruta will be shipped as a Docker image (API + web app in one
container) with a Cloudflare Tunnel for access from outside the office.
The old `fruta_deploy/` folder, zip archives and Windows-service scripts were
removed from the repo — build output and archives are not kept in git anymore
(see `.gitignore`).
