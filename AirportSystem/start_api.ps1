Write-Host "Starting Airport System API..." -ForegroundColor Green
Set-Location $PSScriptRoot
dotnet restore
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Starting API..." -ForegroundColor Green
    dotnet run
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}
