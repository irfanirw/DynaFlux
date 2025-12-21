$ErrorActionPreference = "Stop"

$rootDir = Split-Path -Parent $PSScriptRoot
dotnet build "$rootDir/DynaFlux.sln" -c Release
