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
                .OrderByDescending(r => r.PeriodeFin).ToListAsync(ct)
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
        await _uow.Chantiers.AddAsync(chantier, ct);
        await _uow.SaveChangesAsync(ct);

        // --- Le montant du marché entre immédiatement en banque. Bénéfice et budget projet
        //     restent sur LE MÊME compte : la division est comptable, pas bancaire. ---
        var compte = new CompteBancaire
        {
            Nom = $"Compte {chantier.Nom}",
            Banque = "BNI",
            Numero = $"CH-{chantier.Code}",
            Type = TypeCompteBancaire.Chantier,
            ChantierId = chantier.Id,
            Solde = 0m
        };
        await _uow.ComptesBancaires.AddAsync(compte, ct);
        await _uow.SaveChangesAsync(ct);

        if (chantier.MontantMarche > 0)
        {
            // 1) Encaissement du marché
            await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
            {
                CompteBancaireId = compte.Id, ChantierId = chantier.Id, Date = DateTime.UtcNow,
                Type = TypeMouvementBancaire.Depot, Montant = chantier.MontantMarche,
                Motif = $"Encaissement du marché — {chantier.Nom}",
                Reference = $"MARCHE-{chantier.Code}", EstValide = true
            }, ct);
            compte.Solde += chantier.MontantMarche;

            // Le bénéfice reste sur le même compte : aucun mouvement de sortie.
            // La séparation bénéfice / budget projet est purement comptable (voir fiche chantier).
            _uow.ComptesBancaires.Update(compte);
            await _uow.SaveChangesAsync(ct);
        }

        // À la création d'un chantier, on enchaîne directement sur la saisie de sa
        // prévision globale (budget projet = marché − bénéfice), conformément au processus métier.
        TempData["Success"] = $"Chantier « {chantier.Nom} » créé. Marché de {chantier.MontantMarche:N0} Ar mis en banque " +
                              $"(dont bénéfice {chantier.Benefice:N0} Ar). Budget projet à dépenser : {chantier.BudgetProjet:N0} Ar. " +
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
        chantier.BudgetMateriel = model.BudgetMateriel;
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
