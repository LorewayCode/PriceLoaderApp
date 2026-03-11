$ErrorActionPreference = "Continue"

$ProjectDir = "$PSScriptRoot\PriceLoaderWeb"
$DockerDir = "$PSScriptRoot\Docker\postgres"

Write-Host "Checking Docker status..." -ForegroundColor Cyan

$dockerStatus = docker info 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker is not running. Attempting to start Docker Desktop..." -ForegroundColor Yellow
    
    $dockerPath = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    if (Test-Path $dockerPath) {
        Start-Process $dockerPath
        Write-Host "Waiting for Docker to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        $maxDockerAttempts = 30
        $dockerAttempt = 0
        while ($dockerAttempt -lt $maxDockerAttempts) {
            docker info 2>$null | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Docker is now running!" -ForegroundColor Green
                break
            }
            $dockerAttempt++
            Write-Host "Waiting for Docker... ($dockerAttempt/$maxDockerAttempts)" -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        }
        
        if ($dockerAttempt -eq $maxDockerAttempts) {
            Write-Host "Error: Docker failed to start. Please start Docker Desktop manually." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "Error: Docker not found at $dockerPath. Please install Docker Desktop." -ForegroundColor Red
        exit 1
    }
}

Write-Host "Starting PostgreSQL container..." -ForegroundColor Cyan
Push-Location $DockerDir
docker-compose up -d 2>$null
Pop-Location

Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Cyan
$maxAttempts = 30
$attempt = 0
while ($attempt -lt $maxAttempts) {
    docker exec price_loader_pg pg_isready -U priceloader -d priceloader 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "PostgreSQL is ready!" -ForegroundColor Green
        break
    }
    $attempt++
    Write-Host "Waiting... ($attempt/$maxAttempts)" -ForegroundColor Yellow
    Start-Sleep -Seconds 2
}

if ($attempt -eq $maxAttempts) {
    Write-Host "Error: PostgreSQL did not start in time" -ForegroundColor Red
    exit 1
}

Write-Host "Waiting for database 'priceloader' to be created..." -ForegroundColor Cyan
$attempt = 0
while ($attempt -lt $maxAttempts) {
    docker exec price_loader_pg psql -U priceloader -d priceloader -c "SELECT 1" 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database 'priceloader' is ready!" -ForegroundColor Green
        break
    }
    $attempt++
    Write-Host "Waiting for database... ($attempt/$maxAttempts)" -ForegroundColor Yellow
    Start-Sleep -Seconds 2
}

if ($attempt -eq $maxAttempts) {
    Write-Host "Error: Database 'priceloader' was not created" -ForegroundColor Red
    exit 1
}

Write-Host "Starting PriceLoaderWeb application..." -ForegroundColor Cyan
Push-Location $ProjectDir
dotnet run
