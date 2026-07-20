<#
.SYNOPSIS
  Собирает оба расширения в Release (по умолчанию).
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipNpmInstall,

    [switch]$Deploy
)

$ErrorActionPreference = "Stop"
$scripts = $PSScriptRoot

Write-Host "=== OpenInCursorOrVscode (Visual Studio) ===" -ForegroundColor Yellow
$vsArgs = @{ Configuration = $Configuration }
if ($Deploy) { $vsArgs["Deploy"] = $true }
& (Join-Path $scripts "Build-OpenInCursorOrVscode.ps1") @vsArgs

Write-Host ""
Write-Host "=== OpenInVisualStudio (Cursor / VS Code) ===" -ForegroundColor Yellow
$vsCodeArgs = @{}
if ($SkipNpmInstall) {
    $vsCodeArgs["SkipInstall"] = $true
}
& (Join-Path $scripts "Build-OpenInVisualStudio.ps1") @vsCodeArgs

Write-Host ""
Write-Host "Оба проекта собраны успешно ($Configuration)." -ForegroundColor Green
