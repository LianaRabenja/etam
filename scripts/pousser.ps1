# ---------------------------------------------------------------------------
#  ETAM - verifier, commiter et pousser les corrections
#
#  Ce script ne pousse RIEN si la compilation echoue : c'est le garde-fou.
#  Il n'ajoute que les six fichiers corriges, jamais le reste du dossier.
#
#  Lancement :  powershell -ExecutionPolicy Bypass -File scripts\pousser.ps1
# ---------------------------------------------------------------------------

$ErrorActionPreference = "Continue"
$projet = "C:\Users\ASUS\Documents\Etam_export"
Set-Location $projet

# --- 1) Compilation -------------------------------------------------------
Write-Host ""
Write-Host "[1/4] Compilation de la solution..." -ForegroundColor Cyan
dotnet build "$projet\ETAM.sln" -v m
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "COMPILATION EN ECHEC - rien n'a ete commite ni pousse." -ForegroundColor Red
    Write-Host "Copie les lignes d'erreur ci-dessus et envoie-les moi." -ForegroundColor Red
    exit 1
}
Write-Host "Compilation OK." -ForegroundColor Green

# --- 2) Etat du depot -----------------------------------------------------
Write-Host ""
Write-Host "[2/4] Etat du depot avant commit :" -ForegroundColor Cyan
git status --short

$fichiers = @(
    "src/ETAM.Web/Controllers/BanquesController.cs",
    "src/ETAM.Web/Controllers/PrevisionGlobaleController.cs",
    "src/ETAM.Infrastructure/Services/AlerteService.cs",
    "src/ETAM.Web/Views/Prevision/Details.cshtml",
    "src/ETAM.Web/Views/PrevisionMensuelle/Details.cshtml",
    "src/ETAM.Web/Views/PrevisionGlobale/Details.cshtml",
    "scripts/pousser.ps1"
)
git add -- $fichiers
if ($LASTEXITCODE -ne 0) {
    Write-Host "git add a echoue - on s'arrete la." -ForegroundColor Red
    exit 1
}

# --- 3) Commit ------------------------------------------------------------
Write-Host ""
Write-Host "[3/4] Commit..." -ForegroundColor Cyan
$titre = "Flechage sans double debit, plan du projet independant du solde, alertes 50/80/90"
$corps = @"
- Banques : flecher vers un chantier ne debite plus le compte. L'argent sort
  a l'execution d'une prevision journaliere, une seule fois. Le transfert vers
  le Budget Comptes continue de debiter, ses depenses ne passant pas par un retrait.
- Plan du projet : l'activation ne depend plus du solde bancaire mais du budget
  alloue au chantier. Un plan est une reference de depense, pas un depot d'argent.
- Alertes : paliers 50 / 80 / 90 pourcent sur toutes les enveloppes, niveau
  critique a 90. Le palier figure dans le titre pour ne plus creer un doublon
  a chaque heure.
- Vues : barre de boutons de Prevision/Details remise en flex, apostrophes qui
  cassaient les fenetres de confirmation, libelles Mise en banque -> Plan active.
"@
git commit -m $titre -m $corps -m "Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Rien a commiter (deja fait ?). On tente quand meme le push." -ForegroundColor Yellow
}

# --- 4) Push --------------------------------------------------------------
Write-Host ""
Write-Host "[4/4] Envoi vers GitHub..." -ForegroundColor Cyan
git push origin main
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "PUSH EN ECHEC. Si le message parle de 'schannel', relance simplement" -ForegroundColor Red
    Write-Host "le script : c'est un incident reseau, pas une erreur de code." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Termine. Va sur Render et lance Manual Deploy si le deploiement" -ForegroundColor Green
Write-Host "automatique ne demarre pas tout seul." -ForegroundColor Green
