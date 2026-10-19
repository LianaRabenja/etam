using ETAM.Application.Common.Models;
using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur,Correspondant")]
public class BanquesController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IBanqueService _banque;
    private readonly IReferenceDataCache _referenceData;

    public BanquesController(IUnitOfWork uow, IBanqueService banque, IReferenceDataCache referenceData)
    {
        _uow = uow;
        _banque = banque;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var comptes = await _uow.ComptesBancaires.Query().AsNoTracking()
            .Include(c => c.Chantier)
            .OrderByDescending(c => c.Type).ThenBy(c => c.Nom).ToListAsync(ct);

        // Demandes de transfert en attente de validation Administrateur.
        ViewBag.TransfertsEnAttente = await _uow.MouvementsBancaires.Query().AsNoTracking()
            .Include(m => m.CompteBancaire)
            .Where(m => !m.EstValide)
            .OrderByDescending(m => m.Date).ToListAsync(ct);
        return View(comptes);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var compte = await _uow.ComptesBancaires.GetByIdAsync(id, ct);
        if (compte is null) return NotFound();

        ViewBag.Mouvements = await _uow.MouvementsBancaires.Query().AsNoTracking()
            .Where(m => m.CompteBancaireId == id)
            .OrderByDescending(m => m.Date).Take(100).ToListAsync(ct);
        return View(compte);
    }

    [Authorize(Roles = "Administrateur")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        return View(new CompteBancaire());
    }

    [Authorize(Roles = "Administrateur")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var compte = await _uow.ComptesBancaires.GetByIdAsync(id, ct);
        if (compte is null) return NotFound();
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        return View(compte);
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompteBancaire model, CancellationToken ct)
    {
        var compte = await _uow.ComptesBancaires.GetByIdAsync(model.Id, ct);
        if (compte is null) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            return View(model);
        }
        compte.Nom = model.Nom;
        compte.Banque = model.Banque;
        compte.Numero = model.Numero;
        compte.Devise = model.Devise;
        // Le solde N'EST PLUS modifiable ici. C'est un solde de grand livre, piloté
        // uniquement par les mouvements. Le rendre éditable créait une perte de données :
        // ouvrir le formulaire à 10h00 pour corriger un libellé, un mouvement arrive à
        // 10h01, on valide à 10h02 — et le solde repartait à sa valeur de 10h00.
        // Pour corriger un solde, passer par « Enregistrer un mouvement ».
        compte.Type = model.Type;
        compte.ChantierId = model.Type == TypeCompteBancaire.Comptes ? null : model.ChantierId;
        compte.EstActif = model.EstActif;
        _uow.ComptesBancaires.Update(compte);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Compte « {compte.Nom} » mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    // FAILLE CORRIGÉE : cette action n'avait aucun contrôle de rôle, alors que ses
    // trois voisines en portent un. Un Correspondant pouvait créer un compte avec
    // le solde de son choix par un envoi direct — le bouton était caché, rien d'autre.
    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompteBancaire model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Nom))
            ModelState.AddModelError(nameof(CompteBancaire.Nom), "Le nom du compte est obligatoire.");
        if (string.IsNullOrWhiteSpace(model.Banque))
            ModelState.AddModelError(nameof(CompteBancaire.Banque), "La banque est obligatoire.");
        if (model.Solde < 0)
            ModelState.AddModelError(nameof(CompteBancaire.Solde), "Le solde initial ne peut pas être négatif.");

        // Un compte de chantier doit être rattaché à un chantier, sinon il n'apparaît
        // jamais dans l'écran de fléchage et reste inutilisable sans explication.
        if (model.Type == TypeCompteBancaire.Chantier && model.ChantierId is null or 0)
            ModelState.AddModelError(nameof(CompteBancaire.ChantierId),
                "Un compte de type « Chantier » doit être rattaché à un chantier.");

        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            return View(model);
        }
        // Un compte "Comptes" n'est pas rattaché à un chantier.
        if (model.Type == TypeCompteBancaire.Comptes) model.ChantierId = null;
        await _uow.ComptesBancaires.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Compte « {model.Nom} » créé.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Supprime un compte bancaire vide — typiquement un compte devenu orphelin.
    /// Un compte avec du solde ou des mouvements est un fait comptable : on le
    /// désactive dans Modifier, on ne l'efface pas.
    /// </summary>
    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerCompte(long id, CancellationToken ct)
    {
        var compte = await _uow.ComptesBancaires.GetByIdAsync(id, ct);
        if (compte is null) return NotFound();

        if (compte.Solde != 0)
        {
            TempData["Error"] = $"« {compte.Nom} » porte encore {compte.Solde:N0} Ar. "
                                + "Videz-le par un mouvement, ou désactivez-le dans Modifier.";
            return RedirectToAction(nameof(Index));
        }

        if (await _uow.MouvementsBancaires.AnyAsync(m => m.CompteBancaireId == id, ct))
        {
            TempData["Error"] = $"« {compte.Nom} » a un historique de mouvements : il ne peut pas être effacé. "
                                + "Désactivez-le dans Modifier, son historique reste consultable.";
            return RedirectToAction(nameof(Index));
        }

        _uow.ComptesBancaires.Remove(compte);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Compte « {compte.Nom} » supprimé.";
        return RedirectToAction(nameof(Index));
    }

    // =====================================================================
    //  Les banques de la place : BNI, BOA, BFV-SG...
    //  Liste de référence proposée à la création d'un chantier.
    // =====================================================================

    public async Task<IActionResult> Banques(CancellationToken ct)
    {
        var banques = (await _uow.Banques.ListAllAsync(ct))
            .OrderByDescending(b => b.EstActive).ThenBy(b => b.Sigle).ToList();
        return View(banques);
    }

    [Authorize(Roles = "Administrateur")]
    [HttpGet]
    public IActionResult CreerBanque() => View(new Banque());

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerBanque(Banque model, CancellationToken ct)
    {
        await VerifierBanqueAsync(model, ct);
        if (!ModelState.IsValid) return View(model);

        model.Nom = model.Nom.Trim();
        model.Sigle = model.Sigle.Trim().ToUpperInvariant();
        await _uow.Banques.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Banque « {model.Sigle} » ajoutée.";
        return RedirectToAction(nameof(Banques));
    }

    [Authorize(Roles = "Administrateur")]
    [HttpGet]
    public async Task<IActionResult> ModifierBanque(long id, CancellationToken ct)
    {
        var banque = await _uow.Banques.GetByIdAsync(id, ct);
        if (banque is null) return NotFound();
        return View(banque);
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModifierBanque(Banque model, CancellationToken ct)
    {
        var banque = await _uow.Banques.GetByIdAsync(model.Id, ct);
        if (banque is null) return NotFound();

        await VerifierBanqueAsync(model, ct);
        if (!ModelState.IsValid) return View(model);

        banque.Nom = model.Nom.Trim();
        banque.Sigle = model.Sigle.Trim().ToUpperInvariant();
        banque.Agence = model.Agence;
        banque.EstActive = model.EstActive;
        banque.Observation = model.Observation;
        _uow.Banques.Update(banque);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Banque « {banque.Sigle} » mise à jour.";
        return RedirectToAction(nameof(Banques));
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerBanque(long id, CancellationToken ct)
    {
        var banque = await _uow.Banques.GetByIdAsync(id, ct);
        if (banque is null) return NotFound();

        // Les comptes gardent le nom de la banque en texte : supprimer la banque de
        // la liste ne casse donc aucun compte. Mais si elle est encore utilisée, la
        // désactiver vaut mieux que l'effacer — le nom reste cohérent dans les écrans.
        var sigle = banque.Sigle;
        if (await _uow.ComptesBancaires.AnyAsync(c => c.Banque == sigle, ct))
        {
            banque.EstActive = false;
            _uow.Banques.Update(banque);
            await _uow.SaveChangesAsync(ct);
            TempData["Success"] = $"« {sigle} » est utilisée par des comptes existants : "
                                  + "elle a été désactivée au lieu d'être supprimée. "
                                  + "Elle n'est plus proposée à la création d'un chantier.";
            return RedirectToAction(nameof(Banques));
        }

        _uow.Banques.Remove(banque);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Banque « {sigle} » supprimée.";
        return RedirectToAction(nameof(Banques));
    }

    private async Task VerifierBanqueAsync(Banque model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Nom))
            ModelState.AddModelError(nameof(Banque.Nom), "Le nom de la banque est obligatoire.");
        if (string.IsNullOrWhiteSpace(model.Sigle))
            ModelState.AddModelError(nameof(Banque.Sigle), "Le sigle est obligatoire (ex : BNI).");

        if (!string.IsNullOrWhiteSpace(model.Sigle))
        {
            var sigle = model.Sigle.Trim().ToUpperInvariant();
            if (await _uow.Banques.AnyAsync(b => b.Sigle == sigle && b.Id != model.Id, ct))
                ModelState.AddModelError(nameof(Banque.Sigle), $"Le sigle « {sigle} » existe déjà.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Mouvement(long compteId, CancellationToken ct)
    {
        var compte = await _uow.ComptesBancaires.GetByIdAsync(compteId, ct);
        if (compte is null) return NotFound();
        ViewBag.Compte = compte;
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mouvement(long compteId, TypeMouvementBancaire type, decimal montant,
        string? beneficiaire, string? motif, string? reference, long? chantierId, CancellationToken ct)
    {
        var result = await _banque.EnregistrerMouvementAsync(compteId, type, montant, beneficiaire, motif, reference, chantierId, ct);
        if (result.Succeeded) TempData["Success"] = "Mouvement enregistré.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Details), new { id = compteId });
    }

    // GET : formulaire de transfert unique et clair (destination + chantier + montant).
    // chantierId permet de pré-sélectionner un chantier (ex : depuis sa fiche).
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Transferer(long? chantierId, CancellationToken ct)
    {
        ViewBag.Chantiers = await _uow.ComptesBancaires.Query()
            .Where(c => c.Type == TypeCompteBancaire.Chantier && c.ChantierId != null)
            .Include(c => c.Chantier)
            .OrderBy(c => c.Chantier!.Nom).ToListAsync(ct);
        ViewBag.PreselectChantierId = chantierId;
        return View();
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transferer(string cible, long? chantierId, decimal montant, CancellationToken ct)
    {
        CompteBancaire? compte;
        if (cible == "Comptes")
            compte = (await _uow.ComptesBancaires.ListAsync(c => c.Type == TypeCompteBancaire.Comptes, ct)).FirstOrDefault();
        else
        {
            if (chantierId is null) { TempData["Error"] = "Choisissez le chantier."; return RedirectToAction(nameof(Transferer)); }
            compte = (await _uow.ComptesBancaires.ListAsync(c => c.Type == TypeCompteBancaire.Chantier && c.ChantierId == chantierId, ct)).FirstOrDefault();
        }
        if (compte is null) { TempData["Error"] = "Aucun compte bancaire correspondant à cette destination."; return RedirectToAction(nameof(Index)); }

        await EnregistrerDemandeTransfertAsync(compte, montant, ct);
        return RedirectToAction(nameof(Index));
    }

    // Transfert déclenché depuis la fiche chantier (le compte est déjà connu).
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemanderTransfert(long compteId, decimal montant, CancellationToken ct)
    {
        var compte = await _uow.ComptesBancaires.GetByIdAsync(compteId, ct);
        if (compte is null) return NotFound();
        await EnregistrerDemandeTransfertAsync(compte, montant, ct);
        // On revient sur la fiche du chantier pour voir tout de suite le résultat.
        if (compte.ChantierId.HasValue)
            return RedirectToAction("Details", "Chantiers", new { id = compte.ChantierId });
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Cœur du transfert : valide, enregistre la demande, et l'applique immédiatement
    /// si l'utilisateur est Administrateur (sinon elle reste en attente de sa validation).
    /// </summary>
    private async Task EnregistrerDemandeTransfertAsync(CompteBancaire compte, decimal montant, CancellationToken ct)
    {
        if (montant <= 0) { TempData["Error"] = "Le montant doit être positif."; return; }

        // On ne contrôle plus le solde brut mais ce qui n'est pas DÉJÀ fléché :
        // le fléchage ne sort pas l'argent de la banque, donc le solde seul ne protège de rien.
        var disponible = await DisponibleAFlecherAsync(compte, ct);
        if (montant > disponible)
        {
            TempData["Error"] = $"Montant trop élevé : {disponible:N0} Ar encore disponibles à flécher " +
                                $"sur ce compte (solde {compte.Solde:N0} Ar, dont une partie est déjà réservée).";
            return;
        }

        var estAdmin = User.IsInRole("Administrateur");
        var cibleNom = compte.Type == TypeCompteBancaire.Comptes
            ? "Budget Comptes"
            : $"Budget Matériel ({compte.Nom.Replace("Compte ", "")})";

        await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
        {
            CompteBancaireId = compte.Id,
            Type = TypeMouvementBancaire.Flechage,
            Montant = montant,
            Motif = $"Fléchage vers {cibleNom} (l'argent reste en banque)",
            ChantierId = compte.ChantierId,
            Date = DateTime.UtcNow,
            EstValide = estAdmin,
            DemandePar = User.Identity?.Name
        }, ct);

        if (estAdmin)
        {
            await AppliquerTransfertAsync(compte, montant, ct);
            TempData["Success"] = $"{montant:N0} Ar fléchés vers le {cibleNom}. Le solde bancaire est inchangé : l'argent ne sortira qu'à l'exécution d'une prévision réelle.";
        }
        else
        {
            TempData["Success"] = $"Demande de fléchage de {montant:N0} Ar vers le {cibleNom} enregistrée — en attente de validation de l'Administrateur.";
        }
        await _uow.SaveChangesAsync(ct);
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderTransfert(long mouvementId, CancellationToken ct)
    {
        var mvt = await _uow.MouvementsBancaires.GetByIdAsync(mouvementId, ct);
        if (mvt is null || mvt.EstValide) { TempData["Error"] = "Demande introuvable ou déjà validée."; return RedirectToAction(nameof(Index)); }
        var compte = await _uow.ComptesBancaires.GetByIdAsync(mvt.CompteBancaireId, ct);
        if (compte is null) return NotFound();
        var dispoValidation = await DisponibleAFlecherAsync(compte, ct);
        if (mvt.Montant > dispoValidation)
        {
            TempData["Error"] = $"Montant trop élevé : {dispoValidation:N0} Ar encore disponibles à flécher sur ce compte.";
            return RedirectToAction(nameof(Index));
        }

        await AppliquerTransfertAsync(compte, mvt.Montant, ct);
        mvt.EstValide = true;
        _uow.MouvementsBancaires.Update(mvt);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Fléchage validé et appliqué. Le solde bancaire reste inchangé.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefuserTransfert(long mouvementId, CancellationToken ct)
    {
        var mvt = await _uow.MouvementsBancaires.GetByIdAsync(mouvementId, ct);
        if (mvt is not null && !mvt.EstValide)
        {
            _uow.MouvementsBancaires.Remove(mvt);
            await _uow.SaveChangesAsync(ct);
            TempData["Success"] = "Demande de transfert refusée.";
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Applique le fléchage : affecte une part du solde bancaire à un budget.
    /// LA BANQUE NE BOUGE PAS. L'argent reste sur le compte ; on note simplement
    /// qu'il est réservé à ce budget. Le solde ne diminue qu'à la sortie d'argent
    /// réelle, c'est-à-dire à l'exécution d'une prévision réelle (PrevisionService).
    /// </summary>
    private async Task AppliquerTransfertAsync(CompteBancaire compte, decimal montant, CancellationToken ct)
    {
        if (compte.Type == TypeCompteBancaire.Comptes)
        {
            var budget = (await _uow.BudgetsComptes.ListAsync(bg => bg.EstActif, ct))
                .OrderByDescending(bg => bg.Annee).FirstOrDefault();
            if (budget is not null) { budget.MontantTransfere += montant; _uow.BudgetsComptes.Update(budget); }
        }
        else if (compte.ChantierId.HasValue)
        {
            var chantier = await _uow.Chantiers.GetByIdAsync(compte.ChantierId.Value, ct);
            if (chantier is not null) { chantier.MaterielTransfere += montant; _uow.Chantiers.Update(chantier); }
        }
        // Volontairement : aucun « compte.Solde -= montant ». Voir le commentaire ci-dessus.
    }

    /// <summary>
    /// Montant du solde bancaire qui n'est pas déjà fléché vers un budget.
    /// Empêche de flécher dix fois le même argent : on ne peut affecter que ce qui
    /// n'est ni déjà réservé, ni encore consommé.
    /// </summary>
    private async Task<decimal> DisponibleAFlecherAsync(CompteBancaire compte, CancellationToken ct)
    {
        // Le calcul appartient au domaine : il y est testé et ne peut pas diverger
        // entre l'écran, la validation et l'application du fléchage.
        if (compte.Type == TypeCompteBancaire.Comptes)
        {
            var budget = (await _uow.BudgetsComptes.ListAsync(bg => bg.EstActif, ct))
                .OrderByDescending(bg => bg.Annee).FirstOrDefault();
            return budget?.DisponibleAFlecher(compte.Solde) ?? compte.Solde;
        }

        if (compte.ChantierId.HasValue)
        {
            var chantier = await _uow.Chantiers.GetByIdAsync(compte.ChantierId.Value, ct);
            return chantier?.DisponibleAFlecher(compte.Solde) ?? compte.Solde;
        }

        return compte.Solde;
    }
}
