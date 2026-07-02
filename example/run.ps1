#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $PSCommandPath
Set-Location $scriptDir

Copy-Item -Path template.xlsx -Destination report.xlsx -Force

reportpopulator run config.json

if ($LASTEXITCODE -eq 0) {
    Write-Host "Report populated successfully: report.xlsx"
} else {
    Write-Host "Report population failed." -ForegroundColor Red
    exit 1
}
