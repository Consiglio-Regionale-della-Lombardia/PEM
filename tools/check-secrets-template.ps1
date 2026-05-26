# Verifica che ogni Secrets.config locale e il corrispondente Secrets.config.example
# elenchino le stesse chiavi. Lo scopo è non dimenticare di aggiornare il template
# quando si introduce una nuova chiave riservata nel proprio ambiente.
#
# Restituisce exit code 1 se nel Secrets.config locale ci sono chiavi non presenti
# nel template, exit code 0 altrimenti. Le chiavi presenti solo nel template (cioè
# previste ma non ancora popolate in locale) sono segnalate come warning.

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

function Get-AppSettingKeys {
    param([string]$Path)
    [xml]$xml = Get-Content -Path $Path -Encoding UTF8 -Raw
    $nodes = $xml.SelectNodes('//appSettings/add')
    return @($nodes | ForEach-Object { $_.GetAttribute('key') } | Where-Object { $_ })
}

$templates = Get-ChildItem -Path $repoRoot -Recurse -Filter 'Secrets.config.example' -File
$exitCode = 0

foreach ($template in $templates) {
    $localPath = Join-Path $template.DirectoryName 'Secrets.config'
    if (-not (Test-Path $localPath)) {
        Write-Host "[skip] $($template.FullName.Substring($repoRoot.Length + 1)) (Secrets.config non presente in locale)"
        continue
    }

    $templateKeys = Get-AppSettingKeys -Path $template.FullName
    $localKeys    = Get-AppSettingKeys -Path $localPath

    $missingInTemplate = @($localKeys | Where-Object { $templateKeys -notcontains $_ })
    $missingInLocal    = @($templateKeys | Where-Object { $localKeys -notcontains $_ })

    $rel = $template.FullName.Substring($repoRoot.Length + 1)

    if ($missingInTemplate.Count -gt 0) {
        Write-Host "[errore] $rel : chiavi presenti in Secrets.config ma assenti dal template:" -ForegroundColor Red
        $missingInTemplate | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        $exitCode = 1
    }
    if ($missingInLocal.Count -gt 0) {
        Write-Host "[avviso] $rel : chiavi nel template ma assenti dal Secrets.config locale:" -ForegroundColor Yellow
        $missingInLocal | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    }
    if ($missingInTemplate.Count -eq 0 -and $missingInLocal.Count -eq 0) {
        Write-Host "[ok] $rel"
    }
}

exit $exitCode
