@echo off
cd /d "%~dp0"
dotnet restore
dotnet build
dotnet run
pause
