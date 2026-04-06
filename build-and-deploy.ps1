# =============================================================
# Fruta - Build and Deploy Script
# Run this from the repo root after any code change.
# Produces a 'fruta_deploy' folder ready to copy to user laptops.
# Vercel deployment is NOT affected - it uses its own env vars.
# =============================================================

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Fruta - Build and Deploy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# -- Step 1: Build the React frontend --------------------------
Write-Host "[1/3] Building React frontend..." -ForegroundColor Yellow
Set-Location "$repoRoot\fruta-client"

# Set empty API base URL so all calls are relative (/api/...)
# This does NOT affect Vercel - Vercel uses its own dashboard env vars
$env:VITE_API_BASE_URL = ''
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "FAILED: Frontend build failed. Stopping." -ForegroundColor Red
    exit 1
}
Write-Host "   OK: Frontend built successfully." -ForegroundColor Green

# -- Step 2: Publish the .NET API -------------------------------
Write-Host ""
Write-Host "[2/3] Publishing .NET API..." -ForegroundColor Yellow
Set-Location $repoRoot

dotnet publish .\frutaaaaa\frutaaaaa.csproj -c Release -o .\fruta_deploy --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "FAILED: API publish failed. Stopping." -ForegroundColor Red
    exit 1
}
Write-Host "   OK: API published successfully." -ForegroundColor Green

# -- Step 3: Copy frontend into API wwwroot ---------------------
Write-Host ""
Write-Host "[3/3] Merging frontend into deploy package..." -ForegroundColor Yellow
$wwwroot = "$repoRoot\fruta_deploy\wwwroot"
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item -Recurse -Force "$repoRoot\fruta-client\dist\*" $wwwroot

# Copy the installer script into the deploy folder
$installerSrc = "$repoRoot\fruta_deploy\install-service.ps1"
if (-not (Test-Path $installerSrc)) {
    Write-Host "   Note: install-service.ps1 not found in fruta_deploy, skipping copy." -ForegroundColor DarkGray
}

Write-Host "   OK: Frontend merged into fruta_deploy\wwwroot\" -ForegroundColor Green

# -- Summary ----------------------------------------------------
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   BUILD COMPLETE" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "   Deploy folder: $repoRoot\fruta_deploy\" -ForegroundColor White
Write-Host ""
Write-Host "   Next steps:" -ForegroundColor White
Write-Host "   1. Copy 'fruta_deploy' folder to each user laptop" -ForegroundColor White
Write-Host "   2. Right-click 'install-service.ps1' inside it" -ForegroundColor White
Write-Host "   3. Select 'Run as Administrator'" -ForegroundColor White
Write-Host "   4. Browser opens automatically to http://localhost:5005" -ForegroundColor White
Write-Host ""
