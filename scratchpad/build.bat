@echo off
setlocal

set CONFIGURATION=Debug

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
pushd ..\src
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
echo Cleaning ChronoTrace.Scratchpad
echo Configuration: %CONFIGURATION%
echo ========================================================
echo %ColorReset%
call dotnet clean -c %CONFIGURATION%
if errorlevel 1 (
    echo ERROR: Failed to clean ChronoTrace.Scratchpad solution.
    goto :eof
)
echo.

<NUL set /p ".=%ColorPurple%"
echo ========================================================
echo Building ChronoTrace.Scratchpad Solution
echo Configuration: %CONFIGURATION%
echo ========================================================
echo %ColorReset%
call dotnet build -c %CONFIGURATION%
if errorlevel 1 (
    echo ERROR: Failed to build ChronoTrace.Scratchpad solution.
    goto :eof
)
echo.

<NUL set /p ".=%ColorGreen%"
echo ========================================================
echo Build process completed successfully.
echo ========================================================
echo %ColorReset%

:eof
endlocal
