# Verifica che ogni *.config locale e il corrispondente *.config.example
# elenchino le stesse chiavi. Lo scopo e' non dimenticare di aggiornare il
# template quando si introduce una nuova chiave riservata / di configurazione
# nel proprio ambiente.
#
# Per default lo script valida sia Secrets.config che Edma.config.
# Il nome del file e' rimasto "check-secrets-template.ps1" per non rompere
# il pre-commit (.githooks/pre-commit) e gli alias gia' presenti.
#
# Restituisce exit code 1 se, in uno qualsiasi dei file locali, ci sono
# chiavi non presenti nel template corrispondente. Le chiavi presenti solo
# nel template (cioe' previste ma non ancora popolate in locale) sono
# segnalate come warning ma non causano fallimento.
#
# Uso:
#   .\check-secrets-template.ps1
#   .\check-secrets-template.ps1 -Patterns 'Secrets.config.example','Edma.config.example'
#   .\check-secrets-template.ps1 -Patterns 'Edma.config.example'    # solo EDMA

[CmdletBinding()]
param(
    [string[]]$Patterns = @('Secrets.config.example', 'Edma.config.example')
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

function Get-ConfigKeys {
    param([string]$Path)
    [xml]$xml = Get-Content -Path $Path -Encoding UTF8 -Raw
    # La root del file e' <appSettings> per Secrets.config e <edmaSettings>
    # per Edma.config; in entrambi i casi le chiavi sono in <add key="...">,
    # quindi un XPath generico va bene.
    $nodes = $xml.SelectNodes('//add[@key]')
    return @($nodes | ForEach-Object { $_.GetAttribute('key') } | Where-Object { $_ })
}

$exitCode = 0

foreach ($pattern in $Patterns) {
    # Escludiamo i template che si trovano sotto cartelle di build (obj/, bin/,
    # Pubblicazione/, Package/): sono copie temporanee dell'output di MSBuild e
    # non rappresentano file sorgente da validare. Senza il filtro lo script
    # finiva per segnalare due volte la stessa cosa e sporcava l'output.
    $templates = Get-ChildItem -Path $repoRoot -Recurse -Filter $pattern -File |
        Where-Object {
            $rel = $_.FullName.Substring($repoRoot.Length + 1)
            $rel -notmatch '(^|[\\/])(obj|bin|Pubblicazione|Package)([\\/]|$)'
        }
    if ($templates.Count -eq 0) {
        Write-Host "[info] nessun template trovato per pattern '$pattern'"
        continue
    }

    foreach ($template in $templates) {
        $localName = $template.Name.Replace('.example', '')
        $localPath = Join-Path $template.DirectoryName $localName
        $rel = $template.FullName.Substring($repoRoot.Length + 1)

        if (-not (Test-Path $localPath)) {
            Write-Host "[skip] $rel ($localName non presente in locale)"
            continue
        }

        $templateKeys = Get-ConfigKeys -Path $template.FullName
        $localKeys    = Get-ConfigKeys -Path $localPath

        $missingInTemplate = @($localKeys | Where-Object { $templateKeys -notcontains $_ })
        $missingInLocal    = @($templateKeys | Where-Object { $localKeys -notcontains $_ })

        if ($missingInTemplate.Count -gt 0) {
            Write-Host "[errore] $rel : chiavi presenti in $localName ma assenti dal template:" -ForegroundColor Red
            $missingInTemplate | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
            $exitCode = 1
        }
        if ($missingInLocal.Count -gt 0) {
            Write-Host "[avviso] $rel : chiavi nel template ma assenti dal $localName locale:" -ForegroundColor Yellow
            $missingInLocal | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
        }
        if ($missingInTemplate.Count -eq 0 -and $missingInLocal.Count -eq 0) {
            Write-Host "[ok] $rel"
        }
    }
}

exit $exitCode
