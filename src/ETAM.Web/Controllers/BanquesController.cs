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
        compte.Solde = model.Solde;
        compte.Type = model.Type;
        compte.ChantierId = model.Type == TypeCompteBancaire.Comptes ? null : model.ChantierId;
        compte.EstActif = model.EstActif;
        _uow.ComptesBancaires.Update(compte);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Compte « {compte.Nom} » mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompteBancaire model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            return View(model);
        }
        // Un compte "Comptes" n'est pas rattaché à un chantier.
        if (model.Type == ETAM.Domain.Enums.TypeCompteBancaire.Comptes) model.ChantierId = null;
        await _uow.ComptesBancaires.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Compte « {model.Nom} » créé.";
        return RedirectToAction(nameof(Index));
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

        // Le fléchage vers le Budget Matériel ne sort pas d'argent : on peut réserver
        // au titre du marché plus que ce qui est encaissé à cet instant. La trésorerie
        // est contrôlée au vrai moment du retrait, à l'exécution d'une prévision.
        // Le transfert vers le Budget Comptes, lui, débite : la contrainte y reste.
        if (compte.Type != TypeCompteBancaire.Chantier && montant > compte.Solde)
        {
            TempData["Error"] = $"Solde insuffisant sur le compte ({compte.Solde:N0} Ar).";
            return;
        }

        var estAdmin = User.IsInRole("Administrateur");
        var cibleNom = compte.Type == TypeCompteBancaire.Comptes
            ? "Budget Comptes"
            : $"Budget Matériel ({compte.Nom.Replace("Compte ", "")})";

        await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
        {
            CompteBancaireId = compte.Id,
            Type = TypeMouvementBancaire.Virement,
            Montant = montant,
            // Pour un chantier, cette ligne trace un fléchage : elle ne bouge pas le solde.
            // Sans la mention, on lirait un virement de 400 M en face d'un solde inchangé.
            Motif = compte.Type == TypeCompteBancaire.Chantier
                ? $"Fléchage vers {cibleNom} (aucun mouvement de fonds)"
                : $"Transfert vers {cibleNom}",
            ChantierId = compte.ChantierId,
            Date = DateTime.UtcNow,
            EstValide = estAdmin,
            DemandePar = User.Identity?.Name
        }, ct);

        if (estAdmin)
        {
            await AppliquerTransfertAsync(compte, montant, ct);
            TempData["Success"] = compte.Type == TypeCompteBancaire.Chantier
                ? $"{montant:N0} Ar fléchés vers le {cibleNom}. Le budget réel a augmenté d'autant ; " +
                  "le solde bancaire ne bougera qu'aux retraits, à l'exécution des prévisions journalières."
                : $"{montant:N0} Ar transférés vers le {cibleNom}. Le budget réel a augmenté d'autant.";
        }
        else
        {
            TempData["Success"] = $"Demande de transfert de {montant:N0} Ar vers le {cibleNom} enregistrée — en attente de validation de l'Administrateur.";
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
        if (compte.Type != TypeCompteBancaire.Chantier && mvt.Montant > compte.Solde)
        { TempData["Error"] = $"Solde insuffisant ({compte.Solde:N0} Ar)."; return RedirectToAction(nameof(Index)); }

        await AppliquerTransfertAsync(compte, mvt.Montant, ct);
        mvt.EstValide = true;
        _uow.MouvementsBancaires.Update(mvt);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Transfert validé et appliqué.";
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
    /// Applique le transfert : alimente le budget réel correspondant.
    ///
    /// CAS D'UN CHANTIER — la banque n'est PAS débitée ici.
    /// Flécher de l'argent vers le Budget Matériel est une décision comptable
    /// (« sur ce que j'ai encaissé, je réserve tant pour les travaux »), pas un
    /// passage au guichet. L'argent quitte réellement le compte à l'exécution
    /// d'une prévision journalière, quand le chef va chercher les fonds
    /// (PrevisionService, « L'ARGENT SORT ICI »).
    ///
    /// Débiter aux deux endroits faisait sortir deux fois la même somme : un
    /// fléchage de 400 M suivi de 400 M de prévisions vidait le compte de 800 M,
    /// et bloquait le chantier en cours de mois sur un « solde insuffisant »
    /// alors que l'argent était bien en banque.
    ///
    /// CAS DU BUDGET COMPTES — la banque est débitée, elle.
    /// Ses dépenses ne repassent jamais par un retrait : si son transfert ne
    /// débitait pas non plus, son solde ne baisserait jamais.
    /// </summary>
    private async Task AppliquerTransfertAsync(CompteBancaire compte, decimal montant, CancellationToken ct)
    {
        if (compte.Type == TypeCompteBancaire.Comptes)
        {
            var budget = (await _uow.BudgetsComptes.ListAsync(bg => bg.EstActif, ct))
                .OrderByDescending(bg => bg.Annee).FirstOrDefault();
            if (budget is not null) { budget.MontantTransfere += montant; _uow.BudgetsComptes.Update(budget); }

            compte.Solde -= montant;
            _uow.ComptesBancaires.Update(compte);
        }
        else if (compte.ChantierId.HasValue)
        {
            var chantier = await _uow.Chantiers.GetByIdAsync(compte.ChantierId.Value, ct);
            if (chantier is not null) { chantier.MaterielTransfere += montant; _uow.Chantiers.Update(chantier); }
            // Aucun mouvement de fonds : voir le commentaire ci-dessus.
        }
    }
}
