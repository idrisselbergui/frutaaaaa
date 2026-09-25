# =============================================================
# Fruta - build the Docker package for the server
# Run from the repo root:
#   powershell -ExecutionPolicy Bypass -File .\build-package.ps1
# Output: dist\fruta-deploy-<version>.zip  -> copy it to the server,
# then follow README-SERVER.txt inside the zip.
# =============================================================

$ErrorActionPreference = 'Stop'
$root   = $PSScriptRoot
$client = Join-Path $root 'fruta-client'
$out    = Join-Path $root 'dist\fruta-deploy'

function Step($msg) { Write-Host "`n== $msg" -ForegroundColor Cyan }
function Fail($msg) { Write-Host "FAILED: $msg" -ForegroundColor Red; exit 1 }

if (git -C $root status --porcelain) {
    Write-Host 'Note: there are uncommitted changes; they are included in this package.' -ForegroundColor Yellow
}

Step 'Cleaning previous output'
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
New-Item -ItemType Directory -Path $out | Out-Null

Step 'Building the web app (React, docker mode)'
Push-Location $client
try {
    if (-not (Test-Path 'node_modules')) {
        npm ci
        if ($LASTEXITCODE -ne 0) { Fail 'npm ci' }
    }
    npm run build:docker
    if ($LASTEXITCODE -ne 0) { Fail 'npm run build:docker' }
} finally { Pop-Location }

if (Select-String -Path (Join-Path $client 'dist\assets\*.js') -Pattern 'localhost:44374' -SimpleMatch -Quiet) {
    Fail 'the web app still points to localhost:44374'
}

Step 'Publishing the API (.NET)'
dotnet publish (Join-Path $root 'frutaaaaa\frutaaaaa.csproj') -c Release -o (Join-Path $out 'app') /p:UseAppHost=false
if ($LASTEXITCODE -ne 0) { Fail 'dotnet publish' }

Step 'Adding the web app to the API (wwwroot)'
$wwwroot = Join-Path $out 'app\wwwroot'
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item (Join-Path $client 'dist\*') $wwwroot -Recurse -Force

Step 'Adding Docker files and server guide'
Get-ChildItem (Join-Path $root 'deploy') -Force | Copy-Item -Destination $out -Recurse -Force

$version = "$(git -C $root rev-parse --short HEAD)-$(Get-Date -Format 'yyyyMMdd-HHmm')"
Set-Content -Path (Join-Path $out 'VERSION.txt') -Value $version -Encoding ascii

Step 'Creating the zip'
$zip = Join-Path $root "dist\fruta-deploy-$version.zip"
Compress-Archive -Path (Get-ChildItem $out -Force).FullName -DestinationPath $zip -Force

$sizeMb = [math]::Round((Get-Item $zip).Length / 1MB, 1)
Write-Host "`nDONE: $zip ($sizeMb MB)" -ForegroundColor Green
Write-Host 'Copy it to the server and follow README-SERVER.txt inside.' -ForegroundColor Green
