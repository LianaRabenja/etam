# =====================================================================
#  ETAM - Transfert de la base LOCALE vers la base Render.
#
#  Phase 1 (avant de toucher a Render) :
#      .\scripts\render-migrer.ps1
#      -> produit etam_local.sql a la racine du projet
#
#  Phase 2 (apres avoir cree la nouvelle base Render, service EN PAUSE) :
#      .\scripts\render-migrer.ps1 -RenderUrl "postgresql://user:mdp@hote/base"
#      -> refait le dump, le restaure sur Render et verifie
# =====================================================================
param(
    [string]$RenderUrl = "",
    # Dossier contenant pg_dump.exe / psql.exe, si la recherche automatique echoue.
    # Ex : -PgBin "C:\Program Files\pgAdmin 4\runtime"
    [string]$PgBin = "",
    [string]$MotDePasseLocal = "root",
    [string]$UtilisateurLocal = "postgres",
    [string]$HoteLocal = "localhost",
    [string]$PortLocal = "5432",
    [string]$BaseLocale = "etam_erp"
)

$ErrorActionPreference = "Stop"
$racine = Split-Path $PSScriptRoot -Parent

# Le dump sort du depot, volontairement : il contient les comptes utilisateurs,
# leurs empreintes de mot de passe et les cles de chiffrement des cookies. Ecrit
# dans le projet, un « git add -A » l'enverrait sur GitHub.
$dossierSauvegardes = Join-Path $env:USERPROFILE "Documents\ETAM-sauvegardes"
if (-not (Test-Path $dossierSauvegardes)) {
    New-Item -ItemType Directory -Path $dossierSauvegardes -Force | Out-Null
}
$fichier = Join-Path $dossierSauvegardes "etam_local.sql"

# ---------------------------------------------------------------------
#  Retrouver les outils PostgreSQL, meme absents du PATH.
# ---------------------------------------------------------------------
function Trouver-Outil([string]$nom) {
    # 1. Dossier impose par l'utilisateur.
    if ($PgBin -and (Test-Path (Join-Path $PgBin "$nom.exe"))) {
        return (Join-Path $PgBin "$nom.exe")
    }

    # 2. Le PATH.
    $cmd = Get-Command $nom -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    # 3. Les emplacements connus. pgAdmin embarque sa propre copie de psql et
    #    pg_dump dans son dossier "runtime" : c'est souvent la seule presente
    #    quand on n'a jamais installe le serveur PostgreSQL complet.
    $racines = @(
        "C:\Program Files\PostgreSQL",
        "C:\Program Files (x86)\PostgreSQL",
        "$env:LOCALAPPDATA\Programs\PostgreSQL",
        "C:\Program Files\pgAdmin 4",
        "C:\Program Files (x86)\pgAdmin 4",
        "$env:LOCALAPPDATA\Programs\pgAdmin 4",
        "$env:APPDATA\pgAdmin"
    )
    foreach ($r in $racines) {
        if (Test-Path $r) {
            $t = Get-ChildItem -Path $r -Filter "$nom.exe" -Recurse -ErrorAction SilentlyContinue |
                 Sort-Object FullName -Descending | Select-Object -First 1
            if ($t) { return $t.FullName }
        }
    }

    # 4. Dernier recours : balayage des disques (lent, mais evite d'etre bloque).
    Write-Host "Recherche de $nom.exe sur le disque, patientez..." -ForegroundColor DarkGray
    foreach ($d in (Get-PSDrive -PSProvider FileSystem | Where-Object { $_.Root -match '^[A-Z]:\\$' })) {
        $t = Get-ChildItem -Path $d.Root -Filter "$nom.exe" -Recurse -Force -ErrorAction SilentlyContinue |
             Select-Object -First 1
        if ($t) { return $t.FullName }
    }
    return $null
}

$pgDump = Trouver-Outil "pg_dump"
$psql   = Trouver-Outil "psql"

if (-not $pgDump -or -not $psql) {
    Write-Host ""
    Write-Host "pg_dump ou psql introuvable sur cette machine." -ForegroundColor Red
    Write-Host ""
    Write-Host "Trouvez-les vous-meme avec :"
    Write-Host '  Get-ChildItem C:\ -Filter pg_dump.exe -Recurse -Force -ErrorAction SilentlyContinue | Select -First 3 FullName' -ForegroundColor Yellow
    Write-Host ""
    Write-Host "puis relancez en indiquant le dossier :"
    Write-Host '  .\scripts\render-migrer.ps1 -PgBin "C:\Program Files\pgAdmin 4\runtime"' -ForegroundColor Yellow
    Write-Host ""
    Write-Host "A defaut, passez par l'interface pgAdmin (RENDER_REDEPLOIEMENT.md, etapes 2 et 4)."
    exit 1
}
Write-Host "pg_dump : $pgDump" -ForegroundColor DarkGray
Write-Host "psql    : $psql"   -ForegroundColor DarkGray

# ---------------------------------------------------------------------
#  PHASE 1 - sauvegarde de la base locale
# ---------------------------------------------------------------------
Write-Host ""
Write-Host "Sauvegarde de '$BaseLocale'..." -ForegroundColor Cyan

$env:PGPASSWORD = $MotDePasseLocal

# --no-owner / --no-privileges : sur Render l'utilisateur s'appelle 'etam' et non
#   'postgres'. Sans ces options la restauration echoue sur des roles inexistants.
# --clean --if-exists : le script commence par supprimer ce qui existe deja, donc
#   il est rejouable sans repartir d'une base neuve.
& $pgDump `
    --no-owner --no-privileges --clean --if-exists `
    --format=plain --encoding=UTF8 `
    --file="$fichier" `
    --host=$HoteLocal --port=$PortLocal --username=$UtilisateurLocal $BaseLocale

if ($LASTEXITCODE -ne 0) {
    Write-Host "Echec de la sauvegarde. PostgreSQL tourne-t-il, et le mot de passe est-il bon ?" -ForegroundColor Red
    exit 1
}

$taille = [math]::Round((Get-Item $fichier).Length / 1KB, 1)
Write-Host "OK : $fichier ($taille Ko)" -ForegroundColor Green

if (-not $RenderUrl) {
    Write-Host ""
    Write-Host "Sauvegarde terminee. Vous pouvez maintenant, sur Render :" -ForegroundColor Cyan
    Write-Host "  1. mettre le service etam-erp EN PAUSE (Settings > Suspend Web Service)"
    Write-Host "  2. supprimer l'ancienne base, en recreer une du MEME nom (etam-db)"
    Write-Host "  3. relancer ce script avec -RenderUrl `"<External Database URL>`""
    exit 0
}

# ---------------------------------------------------------------------
#  PHASE 2 - restauration sur Render
# ---------------------------------------------------------------------
# Render refuse les connexions non chiffrees : on force sslmode=require.
$url = $RenderUrl.Trim()
if ($url -notmatch "sslmode=") {
    $url += $(if ($url -match "\?") { "&sslmode=require" } else { "?sslmode=require" })
}

Write-Host ""
Write-Host "Restauration sur Render..." -ForegroundColor Cyan
Write-Host "Le service web doit etre EN PAUSE, sinon il creera le schema avant vous." -ForegroundColor Yellow

Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue   # le mot de passe est dans l'URL

# Le dump est en UTF8 : sans cette variable, psql sous Windows le lit avec la page
# de codes 1252 et massacre les accents (« Diego-Suarez », « Prévision »...).
$env:PGCLIENTENCODING = "UTF8"

# Les options passent AVANT l'URL, et sous la forme --option=valeur.
# Ecrites « -v ON_ERROR_STOP=1 -f fichier » apres l'URL, psql les prenait pour des
# arguments positionnels, les ignorait, et ouvrait son invite interactive au lieu
# d'executer le fichier.
& $psql --set=ON_ERROR_STOP=1 --file="$fichier" "$url"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "La restauration a echoue. Rien n'a ete laisse a moitie : ON_ERROR_STOP arrete au premier probleme." -ForegroundColor Red
    Write-Host "Verifiez l'URL (External Database URL, pas l'Internal) et que la base est bien vide."
    exit 1
}

# ---------------------------------------------------------------------
#  Verification
# ---------------------------------------------------------------------
Write-Host ""
Write-Host "Verification :" -ForegroundColor Cyan

$verif = @"
SELECT 'Migrations (doit etre 18)' AS controle, count(*)::text AS valeur FROM "__EFMigrationsHistory"
UNION ALL SELECT 'Chantiers',    count(*)::text FROM "Chantiers"
UNION ALL SELECT 'Comptes bancaires', count(*)::text FROM "ComptesBancaires"
UNION ALL SELECT 'Enveloppes',   count(*)::text FROM "PrevisionsMensuelles"
UNION ALL SELECT 'Utilisateurs', count(*)::text FROM "AspNetUsers"
UNION ALL SELECT 'Catalogue',    count(*)::text FROM "Catalogue";
"@

& $psql --command=$verif "$url"

Write-Host ""
Write-Host "Termine. Sur Render : Settings > Resume Web Service, puis Manual Deploy." -ForegroundColor Green
Write-Host "Dans les logs vous devez lire 'Donnees de demonstration desactivees.'"
Write-Host "et AUCUNE ligne 'Applying migration'." -ForegroundColor Green
