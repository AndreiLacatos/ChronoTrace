#!/bin/bash
# =================================================================================
# ChronoTrace Code Coverage Script (Bash)
# =================================================================================
#
# Description:
#   This script builds the solution, runs tests, and generates an HTML code
#   coverage report for the core ChronoTrace assemblies.
#
# Usage:
#   ./run-coverage.sh
#
# =================================================================================

# Exit immediately if a command exits with a non-zero status.
set -e

CONFIGURATION="Debug"
OUTPUT_REPORT="coverage/report.html"
COVERAGE_FILTERS="+:assembly=ChronoTrace.SourceGenerators;+:assembly=ChronoTrace.ProfilingInternals"

# --- ANSI Color Codes ---
COLOR_GREEN='\033[92m'
COLOR_RESET='\033[0m'
COLOR_PURPLE='\033[35m'

# --- Build Step ---
printf "${COLOR_PURPLE}"
echo "========================================================"
echo "Building ChronoTrace"
echo "Configuration: ${CONFIGURATION}"
echo "========================================================"
printf "${COLOR_RESET}"

pushd ../src > /dev/null
dotnet build ChronoTrace.sln -c "${CONFIGURATION}"
popd > /dev/null
echo

# --- Test & Coverage Step ---
printf "${COLOR_PURPLE}"
echo "========================================================"
echo "Running ChronoTrace.SourceGenerators.Tests"
echo "========================================================"
printf "${COLOR_RESET}"

dotnet dotcover cover-dotnet --output="${OUTPUT_REPORT}" --reportType="HTML" --filters="${COVERAGE_FILTERS}" -- test ../src/ChronoTrace.sln --no-build
echo

# --- Success Message ---
echo
printf "${COLOR_GREEN}"
echo "========================================================"
echo "SUCCESS: Coverage report generated at \"${OUTPUT_REPORT}\""
echo "========================================================"
printf "${COLOR_RESET}"
echo

# --- Open Report  ---
xdg-open "${OUTPUT_REPORT}"

exit 0
