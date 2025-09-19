@echo off
REM Database Reset Script for Development (Windows Batch)
REM Usage: scripts\reset-db.bat [migration-name]

set MIGRATION_NAME=%1
if "%MIGRATION_NAME%"=="" set MIGRATION_NAME=InitDatabase

echo.
echo ========================================
echo   Database Reset Script
echo ========================================
echo.

cd backend

echo [1/3] Dropping existing database...
dotnet ef database drop --force --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to drop database
    cd ..
    exit /b 1
)

echo [2/3] Adding migration: %MIGRATION_NAME%
dotnet ef migrations add %MIGRATION_NAME% --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to add migration
    cd ..
    exit /b 1
)

echo [3/3] Updating database...
dotnet ef database update --project TypesettingMIS.Infrastructure --startup-project TypesettingMIS.API
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to update database
    cd ..
    exit /b 1
)

cd ..

echo.
echo ========================================
echo   Database reset completed successfully!
echo ========================================
echo.
echo Usage examples:
echo   scripts\reset-db.bat
echo   scripts\reset-db.bat AddUserCompanyNullable
echo   scripts\reset-db.bat FixIndexConstraints
echo.
