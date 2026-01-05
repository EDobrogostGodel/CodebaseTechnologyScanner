# Codebase Technology Scanner - Quick Start Script

Write-Host "Starting Codebase Technology Scanner..." -ForegroundColor Green
Write-Host ""

# Check if .NET is installed
Write-Host "Checking .NET installation..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK is not installed!" -ForegroundColor Red
    Write-Host "Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
Write-Host ".NET version: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Check if Node.js is installed
Write-Host "Checking Node.js installation..." -ForegroundColor Yellow
$nodeVersion = node --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Node.js is not installed!" -ForegroundColor Red
    Write-Host "Please install Node.js from https://nodejs.org/" -ForegroundColor Red
    exit 1
}
Write-Host "Node.js version: $nodeVersion" -ForegroundColor Green
Write-Host ""

# Build backend
Write-Host "Building backend API..." -ForegroundColor Yellow
Set-Location CodebaseTechnologyScanner.API
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Backend build failed!" -ForegroundColor Red
    exit 1
}
Set-Location ..
Write-Host "Backend built successfully!" -ForegroundColor Green
Write-Host ""

# Install frontend dependencies (if needed)
Write-Host "Checking frontend dependencies..." -ForegroundColor Yellow
Set-Location codebase-scanner-ui
if (!(Test-Path "node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Frontend dependency installation failed!" -ForegroundColor Red
        exit 1
    }
}
Set-Location ..
Write-Host "Frontend dependencies ready!" -ForegroundColor Green
Write-Host ""

# Start backend in background
Write-Host "Starting backend API on http://localhost:5000..." -ForegroundColor Yellow
$backendJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    Set-Location CodebaseTechnologyScanner.API
    dotnet run --urls "http://localhost:5000"
}
Write-Host "Backend started (Job ID: $($backendJob.Id))" -ForegroundColor Green
Start-Sleep -Seconds 3
Write-Host ""

# Start frontend
Write-Host "Starting frontend on http://localhost:5173..." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Application is starting!" -ForegroundColor Cyan
Write-Host "Backend API: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Frontend UI: http://localhost:5173" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop both servers" -ForegroundColor Yellow
Write-Host ""

try {
    Set-Location codebase-scanner-ui
    npm run dev
} finally {
    Write-Host ""
    Write-Host "Stopping backend server..." -ForegroundColor Yellow
    Stop-Job $backendJob
    Remove-Job $backendJob
    Write-Host "All servers stopped." -ForegroundColor Green
}
