Write-Host "===== Running Test Coverage ====="

# 1. Buscar automáticamente el proyecto de pruebas
$testProject = Get-ChildItem -Recurse -Filter *.csproj |
    Where-Object { $_.FullName -match "Test" -or $_.FullName -match "Tests" } |
    Select-Object -First 1

if (-not $testProject) {
    Write-Host "❌ No se encontró un proyecto de pruebas (*.Tests.csproj)"
    exit 1
}

Write-Host "Proyecto de pruebas detectado: $($testProject.FullName)"
Write-Host ""

# 2. Crear carpeta coverage
$coverageDir = Join-Path $PSScriptRoot "../coverage"
if (!(Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir | Out-Null
}

# 3. Ejecutar dotnet test con Coverlet
dotnet test $testProject.FullName `
    /p:CollectCoverage=true `
    /p:CoverletOutput="$coverageDir/coverage.json" `
    /p:CoverletOutputFormat=\"json\" `
    /p:Exclude=\"[xunit.*]*,[*.Tests]*\"

Write-Host ""
Write-Host "===== Extracting Coverage Percentage ====="

$coverageFile = Join-Path $coverageDir "coverage.json"

if (!(Test-Path $coverageFile)) {
    Write-Host "❌ Archivo de cobertura NO encontrado: $coverageFile"
    Write-Host "No se pudo calcular el % de cobertura."
    exit 1
}

# 4. Leer JSON para calcular el porcentaje real
$coverageJson = Get-Content $coverageFile | ConvertFrom-Json

$covered = $coverageJson.summary.covered
$total = $coverageJson.summary.total

if ($total -eq 0) {
    Write-Host "❌ No se encontraron líneas analizables."
    exit 1
}

$coveragePercent = [math]::Round(($covered / $total) * 100, 2)

Write-Host "Cobertura total: $coveragePercent%"

# 5. Validar si supera el 80%
if ($coveragePercent -ge 80) {
    Write-Host "✅ Cobertura OK: Por encima del 80%."
}
else {
    Write-Host "❌ Cobertura insuficiente: Debe ser ≥ 80%."
}

Write-Host "===== DONE ====="
