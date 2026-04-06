# =============================================================
# Fruta - Service Installer
# Run this script ONCE on each user laptop as Administrator.
# After running, the app starts automatically on every Windows boot.
# Users just open: http://localhost:5005
# =============================================================

$ErrorActionPreference = "Stop"
$deployPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$exePath    = Join-Path $deployPath "frutaaaaa.exe"
$serviceName = "FrutaApp"
$port = 5005

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Fruta - Application Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# -- Check for Administrator rights ----------------------------
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: Please run this script as Administrator." -ForegroundColor Red
    Write-Host "   Right-click the file and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit 1
}

# -- Check for .NET 8 Runtime ----------------------------------
Write-Host "[1/4] Checking .NET 8 Runtime..." -ForegroundColor Yellow
$dotnetRuntimes = dotnet --list-runtimes 2>$null
$hasRuntime = $dotnetRuntimes | Where-Object { $_ -like "Microsoft.AspNetCore.App 8.*" }
if (-not $hasRuntime) {
    Write-Host "ERROR: .NET 8 Runtime not found on this machine." -ForegroundColor Red
    Write-Host "   Please install it from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Write-Host "   Download: ASP.NET Core Runtime 8.x - Windows x64 installer" -ForegroundColor Yellow
    pause
    exit 1
}
Write-Host "   OK: .NET 8 Runtime found." -ForegroundColor Green

# -- Remove any existing Fruta service -------------------------
Write-Host ""
Write-Host "[2/4] Removing previous installation (if any)..." -ForegroundColor Yellow
$existing = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($existing) {
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    sc.exe delete $serviceName | Out-Null
    Start-Sleep -Seconds 2
    Write-Host "   OK: Previous installation removed." -ForegroundColor Green
} else {
    Write-Host "   OK: No previous installation found." -ForegroundColor Green
}

# -- Install the Windows Service -------------------------------
Write-Host ""
Write-Host "[3/4] Installing Fruta as a Windows Service..." -ForegroundColor Yellow

$binPath = "`"$exePath`" --urls http://0.0.0.0:$port"
sc.exe create $serviceName binPath= $binPath start= auto DisplayName= "Fruta Application" | Out-Null
sc.exe description $serviceName "Fruta fruit operations - serves the app at http://localhost:$port" | Out-Null
sc.exe start $serviceName | Out-Null

# -- Add firewall rule -----------------------------------------
New-NetFirewallRule -DisplayName "Fruta App (port $port)" -Direction Inbound -Protocol TCP -LocalPort $port -Action Allow -ErrorAction SilentlyContinue | Out-Null

Write-Host "   OK: Service installed and started." -ForegroundColor Green

# -- Wait for the app to be ready ------------------------------
Write-Host ""
Write-Host "[4/4] Waiting for app to be ready..." -ForegroundColor Yellow
$ready = $false
for ($i = 0; $i -lt 15; $i++) {
    Start-Sleep -Seconds 1
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$port" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        $ready = $true
        break
    } catch { }
    Write-Host "   Waiting... ($($i+1)s)" -ForegroundColor DarkGray
}

# -- Done ------------------------------------------------------
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   FRUTA IS INSTALLED" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "   App URL: http://localhost:$port" -ForegroundColor White
Write-Host "   The app starts automatically every time this computer boots." -ForegroundColor White
Write-Host ""

# Auto-open browser
Start-Process "http://localhost:$port"

Write-Host "   Browser opened. Press any key to close this window." -ForegroundColor DarkGray
pause
