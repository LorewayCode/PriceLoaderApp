$ErrorActionPreference = "Stop"

Write-Host "==========================================="
Write-Host " PriceLoaderApp - startup script (Windows) "
Write-Host "==========================================="
Write-Host ""

try {
    Write-Host "[1/4] Checking Docker..."
    docker --version
    if ($LASTEXITCODE -ne 0) { throw "Docker is not available" }

    Write-Host ""
    Write-Host "[2/4] Starting PostgreSQL and pgAdmin..."
    Set-Location "..\docker\postgres"
    docker-compose up -d

    Write-Host ""
    Write-Host "[3/4] Waiting for PostgreSQL to be ready..."
    Start-Sleep -Seconds 10

    Write-Host ""
    Write-Host "[4/4] Running PriceLoaderApp..."
    Set-Location "..\..\PriceLoaderApp"
    dotnet run

    Write-Host ""
    Write-Host "Application finished successfully."
}
catch {
    Write-Host ""
    Write-Host "ERROR:"
    Write-Host $_
}
finally {
    Write-Host ""
    Write-Host "Press ENTER to close this window..."
    Read-Host
}
