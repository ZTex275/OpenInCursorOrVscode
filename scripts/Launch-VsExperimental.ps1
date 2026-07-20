<#
.SYNOPSIS
  Собирает OpenInCursorOrVscode (Release + Deploy) и запускает Visual Studio Experimental Instance.
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$scripts = $PSScriptRoot

function Find-DevEnv {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -find "Common7\IDE\devenv.exe" 2>$null
        if ($found) {
            return ($found | Select-Object -First 1)
        }
    }

    $fallback = "C:\Program Files\Microsoft Visual Studio\18\Insiders\Common7\IDE\devenv.exe"
    if (Test-Path $fallback) {
        return $fallback
    }

    throw "devenv.exe не найден."
}

if (-not $SkipBuild) {
    & (Join-Path $scripts "Build-OpenInCursorOrVscode.ps1") -Configuration $Configuration -Deploy
}

$devenv = Find-DevEnv
$solution = Join-Path $root "OpenInCursorOrVscode.sln"
Write-Host "Launch: $devenv /rootsuffix Exp" -ForegroundColor Cyan
Start-Process -FilePath $devenv -ArgumentList @("/rootsuffix", "Exp", "`"$solution`"")
Write-Host "OK: Experimental Instance запущен." -ForegroundColor Green
