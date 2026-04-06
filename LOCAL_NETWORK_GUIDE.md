# Fruta — Local Network Operation Guide

## Setup Summary

| Component   | Machine              | IP              | Port |
|-------------|----------------------|-----------------|------|
| MySQL DB    | NAS / Server Machine | 192.168.1.247   | 3306 |
| API (.NET)  | Second Laptop        | 192.168.1.200   | 5005 |
| Frontend    | Dev Laptop           | (localhost)     | 5173 |

---

## Step 1 — Start the API (Second Laptop)

Open PowerShell in the `fruta_deploy` folder and run:

```powershell
dotnet .\frutaaaaa.dll --urls "http://0.0.0.0:5005" --Logging:Console:LogLevel:Default=Information
```

### ✅ Success looks like:
```
Now listening on: http://0.0.0.0:5005
Application started. Press Ctrl+C to shut down.
```

### ❌ Common Issues:

| Error | Cause | Fix |
|-------|-------|-----|
| Closes instantly with no message | .NET 8 Runtime missing | Install .NET 8 Runtime from microsoft.com |
| `Address already in use` | Port 5005 is taken | Change port: `--urls "http://0.0.0.0:5010"` and update `start-local.ps1` |
| `Unable to connect to MySQL` | DB at 192.168.1.247 is unreachable | Check MySQL is running on that machine and port 3306 is open |
| Closes after a few seconds | Wrong `appsettings.json` copied | Make sure you copied from `fruta_deploy` not an old version |

---

## Step 2 — Start the Frontend (Dev Laptop)

Open PowerShell in `fruta-client\` and run:

```powershell
.\start-local.ps1
```

Or manually if the script is blocked:

```powershell
$env:VITE_API_BASE_URL = 'http://192.168.1.200:5005'
npm run dev
```

### ✅ Success looks like:
```
VITE v7.1.10  ready in 190 ms
➜  Local:   http://localhost:5173/
➜  Network: http://192.168.10.2:5173/
```

### ❌ Common Issues:

| Error | Cause | Fix |
|-------|-------|-----|
| `.\start-local.ps1` blocked (red error about security) | PowerShell execution policy | Run as Admin: `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` |
| `npm: command not found` | Node.js not installed | Install Node.js from nodejs.org |
| `EADDRINUSE: port 5173` | Another Vite instance is running | Stop the other instance or change port in `vite.config.js` |
| Blank page / no login form | Wrong `VITE_API_BASE_URL` set | Check the env var matches the API laptop's real IP |

---

## Step 3 — Access the App

Open a browser and go to:

```
http://localhost:5173
```

Or from any other device on the same WiFi:

```
http://192.168.10.2:5173
```

### ❌ Common Issues:

| Error | Cause | Fix |
|-------|-------|-----|
| Login fails / network error | API not reachable | Check Step 1 terminal is still open and showing "Application started" |
| `CORS error` in browser console (F12) | Your device IP not in CORS list | Add your IP to the `policy.WithOrigins(...)` list in `frutaaaaa/Program.cs`, rebuild, redeploy |
| Login page loads but API calls fail | Wrong port in `start-local.ps1` | Make sure `VITE_API_BASE_URL` port matches what the API is actually running on |
| Can't reach `192.168.10.2:5173` from phone | Firewall blocking port 5173 | Run as Admin on dev laptop: `New-NetFirewallRule -DisplayName "Fruta Frontend LAN" -Direction Inbound -Protocol TCP -LocalPort 5173 -Action Allow` |
| Can't reach `192.168.1.200:5005` from dev laptop | Firewall blocking port 5005 | Run as Admin on API laptop: `New-NetFirewallRule -DisplayName "Fruta API LAN" -Direction Inbound -Protocol TCP -LocalPort 5005 -Action Allow` |
| Two laptops can't see each other | Different subnets/routers | Both machines must be on the SAME router/switch/hotspot |

---

## Quick Network Diagnostic

Run these on the **Dev Laptop** to verify connectivity:

```powershell
# Can you reach the API laptop?
ping 192.168.1.200

# Can you reach the DB server?
ping 192.168.1.247

# Can you reach the API endpoint?
Invoke-WebRequest -Uri "http://192.168.1.200:5005/api/users/login" -Method GET
# Expected: 405 Method Not Allowed (this is GOOD — means API is alive)
```

---

## Returning to Normal (Internet Mode)

When your internet/router is back, just start the frontend normally:

```powershell
cd fruta-client
npm run dev
```

This uses the default `VITE_API_BASE_URL` (your ngrok URL) instead of the local one.

> The API on the second laptop can stay running or be shut down — it has no effect on the normal ngrok flow.

---

## File Reference

| File | Purpose |
|------|---------|
| `fruta-client/start-local.ps1` | Starts frontend pointed at local API (`192.168.1.200:5005`) |
| `fruta-client/vite.config.js` | Vite config — `host: true` exposes on all network interfaces |
| `frutaaaaa/Program.cs` | CORS policy — allowed frontend origins listed here |
| `frutaaaaa/appsettings.json` | DB connection strings — points to `192.168.1.247` |
| `frutaaaaa/Properties/launchSettings.json` | Launch profiles — `"Local Network"` profile uses `http://0.0.0.0:5000` |
| `fruta_deploy/` | Published API package — copy this to the second laptop |
