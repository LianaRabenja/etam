using AutoMapper;
using ETAM.Application.DTOs;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using ETAM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur,Correspondant")]
public class ChantiersController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ChantiersController(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var chantiers = await _uow.Chantiers.ListAllAsync(ct);
        var model = _mapper.Map<List<ChantierDto>>(chantiers.OrderBy(c => c.Nom).ToList());
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
                .OrderByDescending(r => r.PeriodeFin).ToListAsync(ct),

            // Somme de TOUS les retraits du chantier — pas seulement les 50 derniers
            // mouvements affichés plus bas, sinon la caisse serait fausse dès le 51e.
            TotalRetire = await _uow.MouvementsBancaires.Query().AsNoTracking()
                .Where(m => m.ChantierId == id
                            && m.Type == TypeMouvementBancaire.Retrait
                            && m.EstValide)
                .SumAsync(m => (decimal?)m.Montant, ct) ?? 0m
        };
        return View(vm);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public IActionResult Create() => View(new ChantierCreateDto());

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChantierCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var chantier = _mapper.Map<Chantier>(dto);

        // Le Budget Matériel n'est plus saisi : c'est exactement le budget projet
        // (marché − bénéfice). Le calculer ici évite d'avoir deux montants qui
        // peuvent diverger alors qu'ils désignent la même enveloppe.
        chantier.BudgetMateriel = chantier.BudgetProjet;

        await _uow.Chantiers.AddAsync(chantier, ct);
        await _uow.SaveChangesAsync(ct);

        // --- Compte bancaire du chantier, tel qu'il a été saisi. Bénéfice et budget projet
        //     restent sur LE MÊME compte : la division est comptable, pas bancaire. ---
        var compte = new CompteBancaire
        {
            Nom = string.IsNullOrWhiteSpace(dto.NomCompte) ? $"Compte {chantier.Nom}" : dto.NomCompte.Trim(),
            Banque = string.IsNullOrWhiteSpace(dto.Banque) ? "—" : dto.Banque.Trim(),
            Numero = string.IsNullOrWhiteSpace(dto.NumeroCompte) ? $"CH-{chantier.Code}" : dto.NumeroCompte.Trim(),
            Devise = "Ar",
            Type = TypeCompteBancaire.Chantier,
            ChantierId = chantier.Id,
            Solde = 0m
        };
        await _uow.ComptesBancaires.AddAsync(compte, ct);
        await _uow.SaveChangesAsync(ct);

        // Seul l'argent RÉELLEMENT encaissé entre en banque : une avance de démarrage,
        // un premier décompte... Les encaissements suivants se saisissent au fur et à
        // mesure dans Banques › Dépôt. Créditer le marché entier d'un coup afficherait
        // un solde que l'entreprise n'a pas.
        if (dto.MontantEncaisse > 0)
        {
            await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
            {
                CompteBancaireId = compte.Id, ChantierId = chantier.Id, Date = DateTime.UtcNow,
                Type = TypeMouvementBancaire.Depot, Montant = dto.MontantEncaisse,
                Motif = string.IsNullOrWhiteSpace(dto.MotifEncaissement)
                    ? $"Premier encaissement — {chantier.Nom}"
                    : dto.MotifEncaissement.Trim(),
                Reference = $"ENC-{chantier.Code}", EstValide = true
            }, ct);
            compte.Solde += dto.MontantEncaisse;
            _uow.ComptesBancaires.Update(compte);
            await _uow.SaveChangesAsync(ct);
        }

        // À la création d'un chantier, on enchaîne directement sur la saisie de sa
        // prévision globale (budget projet = marché − bénéfice), conformément au processus métier.
        var resteAEncaisser = chantier.MontantMarche - dto.MontantEncaisse;
        TempData["Success"] = $"Chantier « {chantier.Nom} » créé. Marché de {chantier.MontantMarche:N0} Ar, " +
                              $"dont {dto.MontantEncaisse:N0} Ar encaissés sur le compte {compte.Banque} " +
                              $"(reste à encaisser : {resteAEncaisser:N0} Ar). " +
                              $"Budget projet à dépenser : {chantier.BudgetProjet:N0} Ar. " +
                              $"Saisissez maintenant la prévision globale.";
        return RedirectToAction("Create", "PrevisionGlobale", new { chantierId = chantier.Id });
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

        // On met à jour uniquement les champs éditables (les compteurs
        // Consommation/Réserve utilisée restent pilotés par le workflow).
        chantier.Nom = model.Nom;
        chantier.Code = model.Code;
        chantier.Localisation = model.Localisation;
        chantier.Responsable = model.Responsable;
        chantier.DateDebut = model.DateDebut;
        chantier.DateFin = model.DateFin;
        chantier.Statut = model.Statut;
        // Le Budget Matériel n'est pas saisi : on le réaligne sur le budget projet.
        // Cela rattrape aussi les chantiers créés avant ce changement.
        chantier.BudgetMateriel = chantier.BudgetProjet;
        chantier.Reserve = model.Reserve;
        chantier.PourcentageAvancement = model.PourcentageAvancement;
        chantier.Observation = model.Observation;

        _uow.Chantiers.Update(chantier);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Chantier « {chantier.Nom} » mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var chantier = await _uow.Chantiers.GetByIdAsync(id, ct);
        if (chantier is null) return NotFound();
        _uow.Chantiers.Remove(chantier); // soft delete via l'intercepteur
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Chantier « {chantier.Nom} » supprimé.";
        return RedirectToAction(nameof(Index));
    }
}
