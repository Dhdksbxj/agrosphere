@echo off
title AgroSphere ASP.NET Core Backend
cd /d "%~dp0"
echo ==============================================
echo    Starting AgroSphere Backend (.NET 8 MVC)
echo ==============================================
echo.
if exist "bin\Debug\net8.0\AgroSphere.staticwebassets.runtime.json" (
    findstr /C:"/mnt/c" "bin\Debug\net8.0\AgroSphere.staticwebassets.runtime.json" >nul 2>&1
    if not errorlevel 1 (
        echo Cleaning cross-platform build artifacts...
        if exist "bin" rd /s /q "bin"
        if exist "obj" rd /s /q "obj"
    )
)
echo Launching http://localhost:5000 ...
start http://localhost:5000
call dotnet run
pause
