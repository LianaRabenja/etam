using ETAM.Application.Common.Mappings;
using ETAM.Application.DTOs;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using ETAM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize]
public class ChantiersController : Controller
{
    private readonly IUnitOfWork _uow;

    public ChantiersController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var chantiers = await _uow.Chantiers.ListAllAsync(ct);
        var model = chantiers.OrderBy(c => c.Nom).ToDto();
        return View(model);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var chantier = await _uow.Chantiers.GetByIdAsync(id, ct);
        if (chantier is null) return NotFound();

        // Compte bancaire du chantier (adossé au Budget Matériel).
        var compte = (await _uow.ComptesBancaires.ListAsync(
            c => c.ChantierId == id && c.Type == TypeCompteBancaire.Chantier, ct)).FirstOrDefault();

        var mouvements = compte is null
            ? new List<MouvementBancaire>()
            : await _uow.MouvementsBancaires.Query().AsNoTracking()
                .Where(m => m.CompteBancaireId == compte.Id)
                .OrderByDescending(m => m.Date).Take(50).ToListAsync(ct);

        var vm = new ChantierDetailsViewModel
        {
            Chantier = chantier,
            CompteBancaire = compte,
            Mouvements = mouvements,
            Materiaux = (await _uow.Materiaux.ListAsync(m => m.ChantierId == id, ct)).ToList(),
            Approvisionnements = await _uow.Approvisionnements.Query().AsNoTracking()
                .Where(a => a.ChantierId == id).Include(a => a.Lignes)
                .OrderByDescending(a => a.DateAppro).ToListAsync(ct),
            Previsions = await _uow.Previsions.Query().AsNoTracking()
                .Where(p => p.ChantierId == id).Include(p => p.Lignes)
                .OrderByDescending(p => p.DatePrevision).ToListAsync(ct),
            Depenses = await _uow.Depenses.Query().AsNoTracking()
                .Where(d => d.ChantierId == id)
                .OrderByDescending(d => d.Date).Take(100).ToListAsync(ct),
            Dettes = await _uow.DettesFournisseurs.Query().AsNoTracking()
                .Where(d => d.ChantierId == id).Include(d => d.Fournisseur)
                .OrderByDescending(d => d.CreatedAt).ToListAsync(ct),
            Alertes = await _uow.Alertes.Query().AsNoTracking()
                .Where(a => a.ChantierId == id)
                .OrderByDescending(a => a.CreatedAt).Take(50).ToListAsync(ct),
            RapportsTravail = await _uow.RapportsTravail.Query().AsNoTracking()
                .Where(r => r.ChantierId == id)
                .OrderByDescending(r => r.PeriodeFin).ToListAsync(ct)
        };
        return View(vm);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await ChargerBanquesAsync(ct);
        return View(new ChantierCreateDto());
    }

    /// <summary>Liste des banques proposées à la création. Gérée dans le menu Banques.</summary>
    private async Task ChargerBanquesAsync(CancellationToken ct)
    {
        ViewBag.Banques = (await _uow.Banques.ListAsync(b => b.EstActive, ct))
            .OrderBy(b => b.Sigle)
            .ToList();
    }

    /// <summary>
    /// Contrôles métier communs à la création et à la modification. Ils complètent
    /// les attributs du DTO, qui ne savent pas comparer deux champs entre eux.
    /// </summary>
    private void VerifierCoherence(DateTime debut, DateTime? fin, decimal marche, decimal benefice)
    {
        if (fin.HasValue && fin.Value.Date < debut.Date)
            ModelState.AddModelError(nameof(ChantierCreateDto.DateFin),
                "La date de fin ne peut pas être antérieure à la date de début.");

        if (benefice > marche)
            ModelState.AddModelError(nameof(ChantierCreateDto.Benefice),
                $"Le bénéfice ({benefice:N0} Ar) dépasse le montant du marché ({marche:N0} Ar). "
                + "Le budget du projet serait négatif.");
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChantierCreateDto dto, CancellationToken ct)
    {
        VerifierCoherence(dto.DateDebut, dto.DateFin, dto.MontantMarche, dto.Benefice);

        // Le code identifie le chantier dans toutes les références (PREV-DIEG-...).
        // Un doublon doit produire un message clair, pas une erreur 500.
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var code = dto.Code.Trim();
            if (await _uow.Chantiers.AnyAsync(c => c.Code == code, ct))
                ModelState.AddModelError(nameof(ChantierCreateDto.Code),
                    $"Le code « {code} » est déjà utilisé par un autre chantier.");
        }

        if (!ModelState.IsValid)
        {
            await ChargerBanquesAsync(ct);
            return View(dto);
        }

        // La somme perçue par défaut, c'est le montant du marché. Une saisie à zéro
        // est volontaire et doit être respectée — on ne la remplace plus en silence.
        var montantEnBanque = dto.MontantEnBanque > 0 ? dto.MontantEnBanque : dto.MontantMarche;

        // --- Chantier, compte et versement d'ouverture forment un tout ---
        // Sans transaction, un échec à mi-parcours laissait un chantier sans compte
        // bancaire : inutilisable, et irréparable depuis l'interface.
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var chantier = dto.ToEntity();
            await _uow.Chantiers.AddAsync(chantier, ct);
            await _uow.SaveChangesAsync(ct);

            // Le montant du marché entre immédiatement en banque. Bénéfice et budget
            // projet restent sur LE MÊME compte : la division est comptable, pas bancaire.
            var compte = new CompteBancaire
            {
                Nom = $"Compte {chantier.Nom}",
                Banque = string.IsNullOrWhiteSpace(dto.NomBanque) ? "BNI" : dto.NomBanque.Trim(),
                Numero = string.IsNullOrWhiteSpace(dto.NumeroCompte) ? $"CH-{chantier.Code}" : dto.NumeroCompte.Trim(),
                Type = TypeCompteBancaire.Chantier,
                ChantierId = chantier.Id,
                Solde = 0m
            };
            await _uow.ComptesBancaires.AddAsync(compte, ct);
            await _uow.SaveChangesAsync(ct);

            if (montantEnBanque > 0)
            {
                await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
                {
                    CompteBancaireId = compte.Id, ChantierId = chantier.Id, Date = DateTime.UtcNow,
                    Type = TypeMouvementBancaire.Depot, Montant = montantEnBanque,
                    Motif = $"Versement à l'ouverture — {chantier.Nom} ({compte.Banque})",
                    Reference = $"MARCHE-{chantier.Code}", EstValide = true
                }, ct);

                // Le bénéfice reste sur le même compte : aucun mouvement de sortie.
                compte.Solde += montantEnBanque;
                _uow.ComptesBancaires.Update(compte);
                await _uow.SaveChangesAsync(ct);
            }

            await _uow.CommitAsync(ct);

            TempData["Success"] = $"Chantier « {chantier.Nom} » créé. {montantEnBanque:N0} Ar perçus sur le compte {compte.Banque} " +
                                  $"({compte.Numero}). Budget projet à dépenser : {chantier.BudgetProjet:N0} Ar " +
                                  $"(marché {chantier.MontantMarche:N0} Ar − bénéfice {chantier.Benefice:N0} Ar). " +
                                  $"Saisissez maintenant la prévision globale.";

            // On enchaîne sur la prévision globale, conformément au processus métier.
            return RedirectToAction("Create", "PrevisionGlobale", new { chantierId = chantier.Id });
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            ModelState.AddModelError(string.Empty,
                "Le chantier n'a pas pu être créé, rien n'a été enregistré. Détail : " + ex.Message);
            await ChargerBanquesAsync(ct);
            return View(dto);
        }
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var chantier = await _uow.Chantiers.GetByIdAsync(id, ct);
        if (chantier is null) return NotFound();
        return View(chantier);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Chantier model, CancellationToken ct)
    {
        var chantier = await _uow.Chantiers.GetByIdAsync(model.Id, ct);
        if (chantier is null) return NotFound();

        VerifierCoherence(model.DateDebut, model.DateFin, chantier.MontantMarche, chantier.Benefice);

        if (model.PourcentageAvancement < 0 || model.PourcentageAvancement > 100)
            ModelState.AddModelError(nameof(Chantier.PourcentageAvancement),
                "L'avancement doit être compris entre 0 et 100 %.");

        if (model.BudgetMateriel < 0)
            ModelState.AddModelError(nameof(Chantier.BudgetMateriel), "Le budget matériel ne peut pas être négatif.");

        if (model.Reserve < 0)
            ModelState.AddModelError(nameof(Chantier.Reserve), "La réserve ne peut pas être négative.");

        if (!string.IsNullOrWhiteSpace(model.Code))
        {
            var code = model.Code.Trim();
            if (await _uow.Chantiers.AnyAsync(c => c.Code == code && c.Id != model.Id, ct))
                ModelState.AddModelError(nameof(Chantier.Code),
                    $"Le code « {code} » est déjà utilisé par un autre chantier.");
        }

        // SANS CE CONTRÔLE, une saisie mal formée — un montant avec une virgule,
        // une date incomplète — arrivait à zéro et écrasait la vraie valeur, le tout
        // sous un bandeau vert « mis à jour ». C'était une perte de données muette.
        if (!ModelState.IsValid) return View(model);

        // On met à jour uniquement les champs éditables (les compteurs
        // Consommation/Réserve utilisée restent pilotés par le workflow).
        chantier.Nom = model.Nom;
        chantier.Code = model.Code;
        chantier.Localisation = model.Localisation;
        chantier.Responsable = model.Responsable;
        chantier.DateDebut = model.DateDebut;
        chantier.DateFin = model.DateFin;
        chantier.Statut = model.Statut;
        chantier.BudgetMateriel = model.BudgetMateriel;
        chantier.Reserve = model.Reserve;
        chantier.PourcentageAvancement = model.PourcentageAvancement;
        chantier.Observation = model.Observation;

        _uow.Chantiers.Update(chantier);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Chantier « {chantier.Nom} » mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Suppression d'un chantier.
    ///
    /// La suppression est logique : les enfants ne partent PAS avec le parent. Un
    /// chantier supprimé sans contrôle laissait donc son compte bancaire crédité
    /// dans la trésorerie, ses prévisions actives et ses dettes dues — rattachés à
    /// un chantier devenu invisible. On refuse tant qu'il reste quelque chose.
    /// </summary>
    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var chantier = await _uow.Chantiers.GetByIdAsync(id, ct);
        if (chantier is null) return NotFound();

        var obstacles = new List<string>();

        if (await _uow.Previsions.AnyAsync(p => p.ChantierId == id, ct))
            obstacles.Add("des prévisions journalières");
        if (await _uow.PrevisionsMensuelles.AnyAsync(m => m.ChantierId == id, ct))
            obstacles.Add("des enveloppes mensuelles");
        if (await _uow.Materiaux.AnyAsync(m => m.ChantierId == id, ct))
            obstacles.Add("du stock");
        if (await _uow.Approvisionnements.AnyAsync(a => a.ChantierId == id, ct))
            obstacles.Add("des demandes de chantier");
        if (await _uow.RapportsTravail.AnyAsync(r => r.ChantierId == id, ct))
            obstacles.Add("des rapports de travail");
        if (await _uow.DettesFournisseurs.AnyAsync(d => d.ChantierId == id, ct))
            obstacles.Add("des dettes fournisseurs");

        var compte = (await _uow.ComptesBancaires.ListAsync(c => c.ChantierId == id, ct)).FirstOrDefault();
        if (compte is not null && compte.Solde != 0)
            obstacles.Add($"un compte bancaire non soldé ({compte.Solde:N0} Ar)");

        if (obstacles.Count > 0)
        {
            TempData["Error"] = $"« {chantier.Nom} » ne peut pas être supprimé : il porte encore "
                                + string.Join(", ", obstacles)
                                + ". Clôturez-le plutôt — passez son statut à « Terminé » dans Modifier.";
            return RedirectToAction(nameof(Index));
        }

        // Rien ne dépend plus du chantier : on emporte son compte vide avec lui,
        // sinon il resterait orphelin dans la liste des banques sans nom affiché.
        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (compte is not null) _uow.ComptesBancaires.Remove(compte);
            _uow.Chantiers.Remove(chantier); // soft delete via l'intercepteur
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);
            TempData["Success"] = $"Chantier « {chantier.Nom} » supprimé, ainsi que son compte bancaire vide.";
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            TempData["Error"] = "Suppression impossible : " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
