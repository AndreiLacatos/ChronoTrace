@echo off
setlocal

:: =================================================================================
:: ChronoTrace Code Coverage Script (Windows Batch)
:: =================================================================================
::
:: Description:
::   This script builds the solution, runs tests, and generates an HTML code
::   coverage report for the core ChronoTrace assemblies.
::
:: Usage:
::   run-coverage.bat
::
:: =================================================================================

set CONFIGURATION=Debug
set OUTPUT_REPORT=coverage\report.html
set COVERAGE_FILTERS=^
+:assembly=ChronoTrace.SourceGenerators;^
+:assembly=ChronoTrace.ProfilingInternals;^
-:class=ChronoTrace.SourceGenerators.Logger;

for /F "delims=" %%a in ('echo prompt $E^|cmd') do set "ESC=%%a"
set "ColorGreen=%ESC%[92m"
set "ColorReset=%ESC%[0m"
set "ColorPurple=%ESC%[35m"
<NUL set /p ".=%ColorPurple%"

echo ========================================================
echo Building ChronoTrace
echo Configuration: %CONFIGURATION%
echo ========================================================
echo %ColorReset%
pushd ..\..\src
call dotnet build ChronoTrace.sln -c %CONFIGURATION%
if errorlevel 1 (
    echo ERROR: Failed to build ChronoTrace solution.
    popd
    goto :eof
)
popd
echo.

<NUL set /p ".=%ColorPurple%"

echo ========================================================
echo Running ChronoTrace.SourceGenerators.Tests
echo ========================================================
echo %ColorReset%
call dotCover cover-dotnet --output "%OUTPUT_REPORT%" --reportType HTML --filters="%COVERAGE_FILTERS%" -- test --no-build
if errorlevel 1 (
    echo ERROR: Failed to run tests.
    popd
    goto :eof
)
echo.

echo.
echo.
<NUL set /p ".=%ColorGreen%"

echo ========================================================
echo SUCCESS: Coverage report generated at "%OUTPUT_REPORT%"
echo ========================================================
echo %ColorReset%
echo.

:: Optional: Automatically open the report in the default browser.
:: The first empty quotes are a required quirk of the 'start' command.
start "" "%OUTPUT_REPORT%"

goto End

:Error
echo.
echo SCRIPT FAILED.
exit /b 1

:End
exit /b 0
