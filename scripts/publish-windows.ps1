# Builds the self-contained Windows executable and a zip for release.
# Usage: powershell -ExecutionPolicy Bypass -File scripts/publish-windows.ps1 [-Runtime win-x64|win-arm64]
param(
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/AIUsageWidget/AIUsageWidget.csproj"
[xml]$csproj = Get-Content $project
$version = $csproj.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
$out = Join-Path $root "dist/$Runtime"

dotnet publish $project -c Release -r $Runtime --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $out
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$zip = Join-Path $root "dist/AIUsageWidget-$version-$Runtime.zip"
if (Test-Path $zip) { Remove-Item $zip }
Compress-Archive -Path (Join-Path $out "AIUsageWidget.exe") -DestinationPath $zip
Write-Host "Created $zip"
