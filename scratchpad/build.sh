#!/bin/bash

set -e

CONFIGURATION="Debug"

ColorGreen=$(printf '\033[92m')
ColorReset=$(printf '\033[0m')
ColorPurple=$(printf '\033[35m')
ColorRed=$(printf '\033[91m')

print_header() {
    local color_code="$1"
    local title_line1="$2"
    local title_line2="$3"

    printf "%s" "$color_code"
    echo "========================================================"
    echo "$title_line1"
    if [ -n "$title_line2" ]; then
        echo "$title_line2"
    fi
    echo "========================================================"
    printf "%s\n" "$ColorReset"
}

print_header "$ColorPurple" "Building ChronoTrace" "Configuration: $CONFIGURATION"

pushd ../src > /dev/null
echo "Building ChronoTrace.sln..."
dotnet build ChronoTrace.sln -c "$CONFIGURATION"
popd > /dev/null
echo

print_header "$ColorPurple" "Cleaning ChronoTrace.Scratchpad" "Configuration: $CONFIGURATION"

echo "Cleaning ChronoTrace.Scratchpad solution..."
dotnet clean -c "$CONFIGURATION"
echo

print_header "$ColorPurple" "Building ChronoTrace.Scratchpad Solution" "Configuration: $CONFIGURATION"

echo "Building ChronoTrace.Scratchpad solution..."
dotnet build -c "$CONFIGURATION"
echo

print_header "$ColorGreen" "Build process completed successfully."
