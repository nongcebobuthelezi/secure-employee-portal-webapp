# Runs the final local verification gate for the Secure Employee Portal portfolio build.
$ErrorActionPreference = "Stop"

# Windows marks files extracted from an internet-downloaded ZIP as originating
# from another computer. The script itself is invoked with a process-scoped
# execution-policy bypass; clear that mark from the local tool manifest so
# dotnet tool restore can read it without a separate manual step.
Unblock-File -Path (Join-Path $PSScriptRoot "..\dotnet-tools.json") -ErrorAction SilentlyContinue

function Assert-NativeSuccess([string]$Step) {
    if ($LASTEXITCODE -ne 0) {
        throw "$Step failed with exit code $LASTEXITCODE."
    }
}

Write-Host "[1/6] .NET SDK" -ForegroundColor Cyan
dotnet --version
Assert-NativeSuccess ".NET SDK check"

Write-Host "[2/6] Restore local tools" -ForegroundColor Cyan
dotnet tool restore
Assert-NativeSuccess "Local tool restore"

Write-Host "[3/6] Restore dependencies" -ForegroundColor Cyan
dotnet restore .\SecureEmployeePortal.csproj
Assert-NativeSuccess "Dependency restore"

Write-Host "[4/6] Release build" -ForegroundColor Cyan
dotnet build .\SecureEmployeePortal.csproj --configuration Release --no-restore -p:TreatWarningsAsErrors=true
Assert-NativeSuccess "Release build"

Write-Host "[5/6] Automated tests" -ForegroundColor Cyan
$expectedTestExecutions = 74
$resultsDirectory = Join-Path $PSScriptRoot "..\TestResults"
$resultsFile = Join-Path $resultsDirectory "release-gate.trx"
New-Item -ItemType Directory -Force -Path $resultsDirectory | Out-Null
Remove-Item $resultsFile -ErrorAction SilentlyContinue

dotnet test .\tests\SecureEmployeePortal.Tests\SecureEmployeePortal.Tests.csproj `
    --configuration Release `
    -p:TreatWarningsAsErrors=true `
    --logger "trx;LogFileName=release-gate.trx" `
    --results-directory $resultsDirectory
Assert-NativeSuccess "Automated tests"

[xml]$trx = Get-Content -Path $resultsFile
$counters = $trx.SelectSingleNode("//*[local-name()='Counters']")
if ($null -eq $counters) {
    throw "Automated test result did not contain a Counters element."
}

$totalTests = [int]$counters.GetAttribute("total")
$passedTests = [int]$counters.GetAttribute("passed")
$failedTests = [int]$counters.GetAttribute("failed")

if ($totalTests -ne $expectedTestExecutions) {
    throw "Automated test inventory changed: expected $expectedTestExecutions executions, discovered $totalTests."
}
if ($passedTests -ne $expectedTestExecutions -or $failedTests -ne 0) {
    throw "Automated test gate failed: expected $expectedTestExecutions passed / 0 failed, got $passedTests passed / $failedTests failed."
}

Write-Host "Verified automated test inventory: $passedTests/$expectedTestExecutions passed." -ForegroundColor Green

Write-Host "[6/6] EF Core model/migration check" -ForegroundColor Cyan
# EF tooling must use the Windows/LocalDB development configuration for this local verification gate.
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = "Development"
try {
    dotnet ef migrations has-pending-model-changes --project .\SecureEmployeePortal.csproj
    Assert-NativeSuccess "EF Core model/migration check"
}
finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}

Write-Host "`nAutomated verification passed." -ForegroundColor Green
Write-Host "Next: run 'dotnet run' and complete the manual workflow checklist in docs/testing.md." -ForegroundColor Green
