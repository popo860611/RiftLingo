[CmdletBinding()]
param(
    [string]$Destination = (Join-Path (Split-Path $PSScriptRoot -Parent) 'src\RiftLingo\tessdata')
)

$ErrorActionPreference = 'Stop'
$languages = @('eng', 'jpn', 'kor', 'vie', 'tha', 'ind')
$baseUrl = 'https://raw.githubusercontent.com/tesseract-ocr/tessdata_fast/main'
New-Item -ItemType Directory -Force -Path $Destination | Out-Null

foreach ($language in $languages) {
    $target = Join-Path $Destination "$language.traineddata"
    if ((Test-Path -LiteralPath $target) -and (Get-Item -LiteralPath $target).Length -gt 100KB) {
        Write-Host "[skip] $language already exists"
        continue
    }
    $temporary = "$target.download"
    Write-Host "[download] $language"
    Invoke-WebRequest -Uri "$baseUrl/$language.traineddata" -OutFile $temporary
    if ((Get-Item -LiteralPath $temporary).Length -lt 100KB) {
        Remove-Item -LiteralPath $temporary -Force
        throw "Downloaded model $language is unexpectedly small."
    }
    Move-Item -LiteralPath $temporary -Destination $target -Force
}

Write-Host "OCR models are ready in $Destination"
