#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

cp template.xlsx report.xlsx

if reportpopulator run config.json; then
    echo "Report populated successfully: report.xlsx"
else
    echo "Report population failed."
    exit 1
fi
