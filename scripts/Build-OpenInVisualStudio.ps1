<#
.SYNOPSIS
  Собирает расширение Cursor/VS Code (OpenInVisualStudio → .vsix).
.PARAMETER SkipInstall
  Не выполнять npm install (если зависимости уже установлены).
#>
[CmdletBinding()]
param(
    [switch]$SkipInstall
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "OpenInVisualStudio"

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    throw "npm не найден. Установите Node.js."
}

Push-Location $project
try {
    Write-Host "Project: $project" -ForegroundColor Cyan

    if (-not $SkipInstall) {
        Write-Host "npm install..." -ForegroundColor Cyan
        npm install
        if ($LASTEXITCODE -ne 0) {
            throw "npm install завершился с ошибкой ($LASTEXITCODE)."
        }
    }

    Write-Host "npm run package..." -ForegroundColor Cyan
    npm run package
    if ($LASTEXITCODE -ne 0) {
        throw "Сборка OpenInVisualStudio завершилась с ошибкой ($LASTEXITCODE)."
    }

    $vsix = Get-ChildItem -Path $project -Filter "*.vsix" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $vsix) {
        throw "VSIX не найден в $project"
    }

    Write-Host "OK: $($vsix.FullName)" -ForegroundColor Green
}
finally {
    Pop-Location
}
