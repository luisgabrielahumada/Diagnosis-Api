Write-Host "===== Running Test Coverage ====="

# Encontrar proyecto de pruebas
$testProject = Get-ChildItem -Recurse -Filter *.csproj |
    Where-Object { $_.FullName -match "Test" -or $_.FullName -match "Tests" } |
    Select-Object -First 1

if (-not $testProject) {
    Write-Host "❌ No se encontró un proyecto de pruebas (*.Tests.csproj)"
    exit 1
}

Write-Host "Proyecto de pruebas detectado: $($testProject.FullName)"
Write-Host ""

# Crear carpeta coverage
$coverageDir = Join-Path $PSScriptRoot "../coverage"
if (!(Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir | Out-Null
}

# Ejecutar tests SIN cobertura
dotnet test $testProject.FullName --no-build

# Ejecutar coverlet.console para generar cobertura
coverlet $testProject.Directory.FullName `
    --target "dotnet" `
    --targetargs "test $($testProject.FullName) --no-build" `
    --format json `
    --output "$coverageDir/coverage.json" `
    --exclude "[xunit.*]*" `
    --exclude "[*.Tests]*"

Write-Host ""
Write-Host "===== Extracting Coverage Percentage ====="

$coverageFile = Join-Path $coverageDir "coverage.json"

if (!(Test-Path $coverageFile)) {
    Write-Host "❌ Archivo de cobertura NO encontrado: $coverageFile"
    exit 1
}

# Leer JSON
$coverageJson = Get-Content $coverageFile | ConvertFrom-Json

$covered = $coverageJson.summary.covered
$total = $coverageJson.summary.total

$coveragePercent = [math]::Round(($covered / $total) * 100, 2)

Write-Host "Cobertura total: $coveragePercent%"

if ($coveragePercent -ge 80) {
    Write-Host "✅ Cobertura OK: Por encima del 80%."
} else {
    Write-Host "❌ Cobertura insuficiente."
}

Write-Host "===== DONE ====="
