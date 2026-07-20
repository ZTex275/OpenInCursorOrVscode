<#
.SYNOPSIS
  Собирает расширение Visual Studio (OpenInCursorOrVscode → .vsix).
.PARAMETER Configuration
  Debug или Release. По умолчанию Release.
.PARAMETER Deploy
  Развернуть расширение в Experimental Instance (для F5).
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$Deploy
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "OpenInCursorOrVscode.sln"

function Find-MSBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" 2>$null
        if ($found) {
            return ($found | Select-Object -First 1)
        }
    }

    $fallback = "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe"
    if (Test-Path $fallback) {
        return $fallback
    }

    throw "MSBuild не найден. Установите Visual Studio с компонентом MSBuild."
}

$msbuild = Find-MSBuild
Write-Host "MSBuild: $msbuild" -ForegroundColor Cyan
Write-Host "Solution: $solution" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan

$msbuildArgs = @(
    $solution,
    "/restore",
    "/t:Build",
    "/p:Configuration=$Configuration",
    "/v:m",
    "/nologo"
)

if ($Deploy) {
    $msbuildArgs += "/p:DeployExtension=true"
    Write-Host "DeployExtension: true" -ForegroundColor Cyan
}

& $msbuild @msbuildArgs
if ($LASTEXITCODE -ne 0) {
    throw "Сборка OpenInCursorOrVscode завершилась с ошибкой ($LASTEXITCODE)."
}

$vsix = Join-Path $root "OpenInCursorOrVscode\bin\$Configuration\OpenInCursorOrVscode.vsix"
if (-not (Test-Path $vsix)) {
    throw "VSIX не найден: $vsix"
}

Write-Host "OK: $vsix" -ForegroundColor Green
