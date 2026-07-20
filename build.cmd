@echo off
setlocal
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Build-All.ps1" %*
exit /b %ERRORLEVEL%
