@echo off
setlocal

:: =================================================================================
:: ChronoTrace local publish script (Windows Batch)
:: =================================================================================
::
:: Description:
::   This script builds the solution, packs it into a NuGet package and stores locally.
::
:: Usage:
::   local-publish.bat
::
:: =================================================================================

set CONFIGURATION=Release
set PUBLISH_FOLDER=artifacts
set NUGET_CONFIG_FILE=nuget.e2e.config

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
echo Publishing NuGet package to local folder
echo Configuration: %CONFIGURATION%
echo ========================================================
echo %ColorReset%
pushd ..\..\src\..
if exist "%PUBLISH_FOLDER%" (
    rmdir /s /q "%PUBLISH_FOLDER%"
)
call dotnet pack -c %CONFIGURATION% -o %PUBLISH_FOLDER% src\ChronoTrace.sln --no-build
if errorlevel 1 (
    echo ERROR: Failed to pack & publish local package.
    goto :eof
)
popd
echo.

<NUL set /p ".=%ColorGreen%"
echo ========================================================
echo Published successfully.
echo ========================================================
echo %ColorReset%

<NUL set /p ".=%ColorPurple%"
echo ========================================================
echo Creating local NuGet config
echo ========================================================
echo %ColorReset%
if exist "%NUGET_CONFIG_FILE%" (
    del /q "%NUGET_CONFIG_FILE%"
)
call dotnet new nugetconfig
call rename nuget.config "%NUGET_CONFIG_FILE%"
call dotnet nuget add source ..\..\%PUBLISH_FOLDER% -n local-packages --configfile "%NUGET_CONFIG_FILE%"
if errorlevel 1 (
    echo ERROR: Failed to build ChronoTrace.Scratchpad solution.
    goto :eof
)
echo.

<NUL set /p ".=%ColorGreen%"
echo ========================================================
echo Created local NuGet config successfully.
echo ========================================================
echo %ColorReset%

:eof
endlocal
