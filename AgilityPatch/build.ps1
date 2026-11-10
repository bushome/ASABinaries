# Builds AgilityPatch.exe and copies it to the repo root.
#   .\build.ps1            Native AOT build (small exe, no .NET runtime needed)
#   .\build.ps1 -NoAot     Fallback: self-contained single-file build (larger)
param([switch]$NoAot)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$out = Join-Path $PSScriptRoot 'publish'
if (Test-Path $out) { Remove-Item $out -Recurse -Force }

$args = @('publish', '-c', 'Release', '-r', 'win-x64', '-o', $out)
if ($NoAot) {
    $args += @('-p:PublishAot=false', '-p:PublishSingleFile=true', '--self-contained', 'true',
               '-p:PublishTrimmed=true', '-p:EnableCompressionInSingleFile=true')
}
& dotnet @args
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

$exe = Join-Path $out 'AgilityPatch.exe'
Copy-Item $exe (Join-Path $PSScriptRoot '..\AgilityPatch.exe') -Force
Write-Host "Built: $(Resolve-Path (Join-Path $PSScriptRoot '..\AgilityPatch.exe'))"
