# Attiva i git hook locali del repository (.githooks/) per lo sviluppatore
# che esegue lo script. Va lanciato una volta sola dopo il primo clone:
# da quel momento ogni commit fa girare i controlli previsti, fra cui
# check-secrets-template.ps1 che intercetta drift di chiavi fra i
# Secrets.config / Edma.config locali e i corrispondenti template
# versionati.
#
# Lo script imposta semplicemente core.hooksPath sul valore .githooks ed e'
# idempotente: rilanciarlo non causa effetti collaterali.
#
# Uso:
#   .\tools\setup-hooks.ps1

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$hooksDir = Join-Path $repoRoot '.githooks'

if (-not (Test-Path $hooksDir)) {
    Write-Host "[errore] La cartella '$hooksDir' non esiste: sicuro di essere nel repository giusto?" -ForegroundColor Red
    exit 1
}

Push-Location $repoRoot
try {
    $current = & git config --local --get core.hooksPath 2>$null
    if ($current -eq '.githooks') {
        Write-Host "[ok] core.hooksPath e' gia' impostato su .githooks per questo repository."
        exit 0
    }

    & git config --local core.hooksPath '.githooks'
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[errore] 'git config' ha restituito un codice di uscita non zero." -ForegroundColor Red
        exit 1
    }

    Write-Host "[ok] Hook abilitati per questo repository (core.hooksPath = .githooks)." -ForegroundColor Green
    Write-Host "     I controlli in .githooks/ verranno eseguiti ad ogni commit."
}
finally {
    Pop-Location
}
