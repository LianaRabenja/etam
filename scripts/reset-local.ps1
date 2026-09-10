# =====================================================================
#  ETAM - Remise a zero de la base LOCALE, puis demarrage.
#  Usage :  .\scripts\reset-local.ps1
#
#  Deux methodes, choisies automatiquement :
#    A. psql trouve      -> DROP DATABASE, l'application recree tout.
#    B. psql introuvable -> effacement integre a l'application
#                           (variable ETAM_TOUT_EFFACER), sans outil externe.
# =====================================================================
$ErrorActionPreference = "Stop"

$racine  = Split-Path $PSScriptRoot -Parent
$projet  = Join-Path $racine "src\ETAM.Web"

# --- Parametres de la base locale (voir appsettings.Development.json) ---
$utilisateur = "postgres"
$motDePasse  = "root"
$hote        = "localhost"
$port        = "5432"
$base        = "etam_erp"

$env:ASPNETCORE_ENVIRONMENT = "Development"

# ---------------------------------------------------------------------
#  Recherche de psql : le PATH, puis les emplacements d'installation.
# ---------------------------------------------------------------------
function Trouver-Psql {
    $cmd = Get-Command psql -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $racines = @(
        "C:\Program Files\PostgreSQL",
        "C:\Program Files (x86)\PostgreSQL",
        "$env:LOCALAPPDATA\Programs\PostgreSQL"
    )
    foreach ($r in $racines) {
        if (Test-Path $r) {
            $trouve = Get-ChildItem -Path $r -Filter psql.exe -Recurse -ErrorAction SilentlyContinue |
                      Sort-Object FullName -Descending | Select-Object -First 1
            if ($trouve) { return $trouve.FullName }
        }
    }
    return $null
}

$psql = Trouver-Psql

# ---------------------------------------------------------------------
#  METHODE A - suppression de la base avec psql
# ---------------------------------------------------------------------
if ($psql) {
    Write-Host ""
    Write-Host "psql trouve : $psql" -ForegroundColor DarkGray
    Write-Host "Suppression de la base '$base'..." -ForegroundColor Cyan

    $env:PGPASSWORD = $motDePasse
    # WITH (FORCE) ferme les connexions ouvertes (PostgreSQL 13+).
    & $psql -U $utilisateur -h $hote -p $port -d postgres `
            -c "DROP DATABASE IF EXISTS `"$base`" WITH (FORCE);"

    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Echec de la suppression." -ForegroundColor Red
        Write-Host "Verifiez que PostgreSQL tourne et que le mot de passe de '$utilisateur' est bien '$motDePasse'."
        exit 1
    }

    Write-Host "Base supprimee. L'application va la recreer." -ForegroundColor Green
}

# ---------------------------------------------------------------------
#  METHODE B - effacement integre, sans psql
# ---------------------------------------------------------------------
else {
    Write-Host ""
    Write-Host "psql introuvable : passage par l'effacement integre a l'application." -ForegroundColor Yellow
    Write-Host "Demarrage temporaire en cours, patientez..." -ForegroundColor Cyan

    $log = Join-Path $env:TEMP "etam-effacement.log"
    if (Test-Path $log) { Remove-Item $log -Force }

    $env:ETAM_TOUT_EFFACER = "OUI-TOUT-EFFACER"
    $proc = Start-Process -FilePath "dotnet" `
                          -ArgumentList "run","--project","`"$projet`"" `
                          -WorkingDirectory $racine `
                          -PassThru -NoNewWindow `
                          -RedirectStandardOutput $log `
                          -RedirectStandardError (Join-Path $env:TEMP "etam-effacement.err.log")

    $ok = $false
    $echec = $false
    $limite = (Get-Date).AddMinutes(4)

    while ((Get-Date) -lt $limite -and -not $proc.HasExited) {
        Start-Sleep -Seconds 3
        if (Test-Path $log) {
            $contenu = Get-Content $log -Raw -ErrorAction SilentlyContinue
            if ($contenu -match "EFFACEMENT TOTAL TERMIN")      { $ok = $true;    break }
            if ($contenu -match "effacement total a .{0,3}chou") { $echec = $true; break }
        }
    }

    if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
    Start-Sleep -Seconds 2
    Remove-Item Env:ETAM_TOUT_EFFACER -ErrorAction SilentlyContinue

    if ($echec) {
        Write-Host ""
        Write-Host "L'effacement a echoue. Rien n'a ete supprime." -ForegroundColor Red
        Write-Host "Journal complet : $log"
        exit 1
    }
    if (-not $ok) {
        Write-Host ""
        Write-Host "Delai depasse : impossible de confirmer l'effacement." -ForegroundColor Red
        Write-Host "Ouvrez le journal pour comprendre : $log"
        exit 1
    }

    Write-Host "Effacement termine." -ForegroundColor Green
}

# ---------------------------------------------------------------------
#  Variables parasites : elles feraient revenir les donnees fictives.
# ---------------------------------------------------------------------
Remove-Item Env:ETAM_DONNEES_EXEMPLE    -ErrorAction SilentlyContinue
Remove-Item Env:ETAM_NETTOYER_CHANTIERS -ErrorAction SilentlyContinue
Remove-Item Env:ETAM_TOUT_EFFACER       -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Demarrage. Attendez la ligne : Donnees de demonstration desactivees." -ForegroundColor Cyan
Write-Host "Puis ouvrez http://localhost:8080   (admin@etam.mg / Admin@2026)" -ForegroundColor Cyan
Write-Host ""

dotnet run --project $projet
