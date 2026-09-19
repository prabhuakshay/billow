# Builds the Billow installer (BillowSetup.exe) into the "releases" folder.
#
# Usage:  .\build-installer.ps1             (uses the version in Billow.csproj)
#         .\build-installer.ps1 -Version 0.2.0

param(
    [string]$Version
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$project = 'src\Billow\Billow.csproj'
if (-not $Version) {
    $Version = ([xml](Get-Content $project)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
}

dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed' }

# Self-contained: bundles .NET so users don't need to install it separately.
Remove-Item -Recurse -Force publish -ErrorAction SilentlyContinue
dotnet publish $project -c Release -r win-x64 --self-contained -p:Version=$Version -o publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

dotnet vpk pack --packId Billow --packTitle Billow --packVersion $Version --packDir publish --mainExe Billow.exe --outputDir releases
if ($LASTEXITCODE -ne 0) { throw 'vpk pack failed' }

Write-Host "`nInstaller ready: releases\Billow-win-Setup.exe" -ForegroundColor Green
