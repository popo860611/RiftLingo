[CmdletBinding()]
param(
    [string]$Runtime = 'win-x64',
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$publishDirectory = Join-Path $root "artifacts\publish\$Runtime"
$outputDirectory = Join-Path $root 'outputs'
$zipPath = Join-Path $outputDirectory "RiftLingo-portable-$Runtime.zip"
$hashPath = "$zipPath.sha256"

& (Join-Path $PSScriptRoot 'download-models.ps1')
dotnet test (Join-Path $root 'RiftLingo.slnx') -c $Configuration
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
dotnet publish (Join-Path $root 'src\RiftLingo\RiftLingo.csproj') -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=false -o $publishDirectory
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }

New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $zipPath -CompressionLevel Optimal
$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath $hashPath -Value "$hash  $(Split-Path $zipPath -Leaf)" -Encoding ascii
Write-Host "Created $zipPath"
Write-Host "SHA256 $hash"
