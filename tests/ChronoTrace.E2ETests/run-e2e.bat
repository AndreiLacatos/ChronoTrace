@echo off
setlocal

:: =================================================================================
:: ChronoTrace e2e test runner script (Windows Batch)
:: =================================================================================
::
:: Description:
::   This script runs e2e tests. It relies on the locally published NuGet package.
::   It detects whether it's being run from the root or the test folder.
::
:: Usage:
::   run-e2e.bat
::
:: =================================================================================

set CONFIGURATION=Release
set NUGET_CONFIG_FILE=nuget.e2e.config
set LOCAL_PACKAGE_CACHE_FOLDER=packages
set TIMING_OUTPUT_FOLDER=timings

:: Detect if running from project root; if so, cd to tests\ChronoTrace.E2ETests
if not exist "ChronoTrace.E2ETests.csproj" (
    if exist "tests\ChronoTrace.E2ETests\ChronoTrace.E2ETests.csproj" (
        cd tests\ChronoTrace.E2ETests
    ) else (
        echo ERROR: Could not locate ChronoTrace.E2ETests.csproj.
        goto :eof
    )
)

:: Set up colors
for /F "delims=" %%a in ('echo prompt $E^|cmd') do set "ESC=%%a"
set "ColorGreen=%ESC%[92m"
set "ColorReset=%ESC%[0m"
set "ColorPurple=%ESC%[35m"
<NUL set /p ".=%ColorPurple%"

echo ========================================================
echo Restoring dependencies
echo ========================================================
echo %ColorReset%
if exist "%LOCAL_PACKAGE_CACHE_FOLDER%" (
    rmdir /s /q "%LOCAL_PACKAGE_CACHE_FOLDER%"
)
call dotnet restore ChronoTrace.E2ETests.csproj --packages "%LOCAL_PACKAGE_CACHE_FOLDER%" --configfile "%NUGET_CONFIG_FILE%" 
if errorlevel 1 (
    echo ERROR: Failed to restore dependencies.
    goto :eof
)
echo.

<NUL set /p ".=%ColorPurple%"
echo ========================================================
echo Building E2E test project
echo Configuration: %CONFIGURATION%
echo ========================================================
echo %ColorReset%
call dotnet clean ChronoTrace.E2ETests.csproj
call dotnet build ChronoTrace.E2ETests.csproj -c %CONFIGURATION% --packages "%LOCAL_PACKAGE_CACHE_FOLDER%" --no-restore
if errorlevel 1 (
    echo ERROR: Failed to build E2E test project.
    goto :eof
)
echo.

<NUL set /p ".=%ColorPurple%"
echo ========================================================
echo Running E2E tests
echo Configuration: %CONFIGURATION%
echo ========================================================
echo %ColorReset%
if exist "%TIMING_OUTPUT_FOLDER%" (
    rmdir /s /q "%TIMING_OUTPUT_FOLDER%"
)
if exist "%LOCAL_PACKAGE_CACHE_FOLDER%" (
    rmdir /s /q "%LOCAL_PACKAGE_CACHE_FOLDER%"
)
call dotnet test ChronoTrace.E2ETests.csproj -c %CONFIGURATION% --no-restore
if exist "%TIMING_OUTPUT_FOLDER%" (
    rmdir /s /q "%TIMING_OUTPUT_FOLDER%"
)
echo.

if errorlevel 1 (
    echo ERROR: Failed to run E2E tests.
    goto :eof
)
<NUL set /p ".=%ColorGreen%"
echo ========================================================
echo E2E tests run successfully.
echo ========================================================
echo %ColorReset%

:eof
endlocal
