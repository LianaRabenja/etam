using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Infrastructure.Identity;
using ETAM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ETAM.Infrastructure.Persistence.Seed;

/// <summary>
/// Applique les migrations et injecte des données de démonstration réalistes
/// (chantiers de forage à Madagascar, budget 2026, matériaux, utilisateurs, rôles).
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        await context.Database.MigrateAsync();

        // --- Migration douce : renomme l'ancien rôle "Responsable financier" en "Correspondant" ---
        // (si la base existait déjà avec l'ancien libellé, on met simplement à jour le nom du rôle :
        // tous les utilisateurs qui l'avaient conservent leurs droits sous le nouveau nom).
        var ancienRole = await roleManager.FindByNameAsync(RolesEtam.AncienNomResponsableFinancier);
        if (ancienRole is not null && ancienRole.Name != RolesEtam.ResponsableFinancier)
        {
            ancienRole.Name = RolesEtam.ResponsableFinancier;
            ancienRole.NormalizedName = RolesEtam.ResponsableFinancier.ToUpperInvariant();
            await roleManager.UpdateAsync(ancienRole);
        }

        // --- Rôles ---
        foreach (var role in RolesEtam.Tous)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // --- Utilisateur administrateur par défaut ---
        const string adminEmail = "admin@etam.mg";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail, Email = adminEmail, EmailConfirmed = true,
                NomComplet = "Administrateur ETAM", Fonction = "Direction", EstActif = true
            };
            await userManager.CreateAsync(admin, "Admin@2026");
            await userManager.AddToRoleAsync(admin, RolesEtam.Administrateur);
        }

        if (await userManager.FindByEmailAsync("rf@etam.mg") is null)
        {
            var rf = new ApplicationUser
            {
                UserName = "rf@etam.mg", Email = "rf@etam.mg", EmailConfirmed = true,
                NomComplet = "Correspondant ETAM", Fonction = "Finance", EstActif = true
            };
            await userManager.CreateAsync(rf, "Finance@2026");
            await userManager.AddToRoleAsync(rf, RolesEtam.ResponsableFinancier);
        }

        if (await userManager.FindByEmailAsync("chef@etam.mg") is null)
        {
            var chef = new ApplicationUser
            {
                UserName = "chef@etam.mg", Email = "chef@etam.mg", EmailConfirmed = true,
                NomComplet = "Chef de Chantier", Fonction = "Exploitation", EstActif = true
            };
            await userManager.CreateAsync(chef, "Chef@2026");
            await userManager.AddToRoleAsync(chef, RolesEtam.ChefDeChantier);
        }

        if (await userManager.FindByEmailAsync("magasinier@etam.mg") is null)
        {
            var magasinier = new ApplicationUser
            {
                UserName = "magasinier@etam.mg", Email = "magasinier@etam.mg", EmailConfirmed = true,
                NomComplet = "Magasinier ETAM", Fonction = "Logistique", EstActif = true
            };
            await userManager.CreateAsync(magasinier, "Stock@2026");
            await userManager.AddToRoleAsync(magasinier, RolesEtam.Magasinier);
        }

        // --- Budget Comptes annuel unique ---
        if (!await context.BudgetsComptes.AnyAsync(b => b.Annee == 2026))
        {
            context.BudgetsComptes.Add(new BudgetCompte
            {
                Annee = 2026, Libelle = "Budget 2026", MontantInitial = 50_000_000m,
                MontantTransfere = 20_000_000m, MontantConsomme = 12_500_000m,
                Reserve = 5_000_000m, ReserveUtilisee = 0m, EstActif = true
            });
        }

        // --- Chantiers réalistes à Madagascar ---
        if (!await context.Chantiers.AnyAsync())
        {
            var chantiers = new List<Chantier>
            {
                new() { Nom = "Forage Ampirika", Code = "AMP-01", Localisation = "Ampirika, Androy",
                        Responsable = "Rakoto Jean", DateDebut = new DateTime(2026,1,15),
                        Statut = StatutChantier.EnCours, BudgetMateriel = 4_500_000m, Reserve = 500_000m,
                        ReserveUtilisee = 0m, Consommation = 2_100_000m, MaterielTransfere = 3_000_000m, PourcentageAvancement = 46,
                        Observation = "Forage d'eau - nappe à 65m." },
                new() { Nom = "Forage Ambovombe", Code = "ABV-02", Localisation = "Ambovombe, Androy",
                        Responsable = "Rasoa Marie", DateDebut = new DateTime(2026,2,3),
                        Statut = StatutChantier.EnCours, BudgetMateriel = 6_800_000m, Reserve = 800_000m,
                        ReserveUtilisee = 150_000m, Consommation = 5_900_000m, MaterielTransfere = 6_500_000m, PourcentageAvancement = 72,
                        Observation = "Adduction d'eau potable - village." },
                new() { Nom = "Chantier Tuléar", Code = "TUL-03", Localisation = "Toliara (Tuléar), Atsimo-Andrefana",
                        Responsable = "Randria Paul", DateDebut = new DateTime(2026,3,10),
                        Statut = StatutChantier.EnCours, BudgetMateriel = 9_200_000m, Reserve = 1_000_000m,
                        ReserveUtilisee = 0m, Consommation = 3_400_000m, MaterielTransfere = 5_000_000m, PourcentageAvancement = 33,
                        Observation = "Travaux publics - voirie et forage." },
                new() { Nom = "Forage Betioky", Code = "BET-04", Localisation = "Betioky-Sud, Atsimo-Andrefana",
                        Responsable = "Rakoto Jean", DateDebut = new DateTime(2026,4,5),
                        Statut = StatutChantier.EnPreparation, BudgetMateriel = 3_000_000m, Reserve = 300_000m,
                        ReserveUtilisee = 0m, Consommation = 0m, MaterielTransfere = 500_000m, PourcentageAvancement = 5,
                        Observation = "Étude en cours." }
            };
            context.Chantiers.AddRange(chantiers);
            await context.SaveChangesAsync();

            // --- Matériaux rattachés aux chantiers ---
            var amp = chantiers[0]; var abv = chantiers[1]; var tul = chantiers[2];
            context.Materiaux.AddRange(
                new Materiau { ChantierId = amp.Id, Categorie = "Gros œuvre", Designation = "Ciment CEM II 42.5",
                    Unite = "sac", QuantiteCommandee = 200, QuantiteRecue = 180, QuantiteUtilisee = 120,
                    SeuilMinimal = 30, PrixUnitaire = 42_000m },
                new Materiau { ChantierId = amp.Id, Categorie = "Ferraillage", Designation = "Fer à béton Ø12",
                    Unite = "barre", QuantiteCommandee = 150, QuantiteRecue = 150, QuantiteUtilisee = 140,
                    SeuilMinimal = 20, PrixUnitaire = 38_000m },
                new Materiau { ChantierId = abv.Id, Categorie = "Forage", Designation = "Tube PVC forage Ø125",
                    Unite = "ml", QuantiteCommandee = 300, QuantiteRecue = 280, QuantiteUtilisee = 210,
                    SeuilMinimal = 40, PrixUnitaire = 25_000m },
                new Materiau { ChantierId = abv.Id, Categorie = "Gros œuvre", Designation = "Gravillon 5/15",
                    Unite = "m3", QuantiteCommandee = 60, QuantiteRecue = 60, QuantiteUtilisee = 58,
                    SeuilMinimal = 8, PrixUnitaire = 90_000m },
                new Materiau { ChantierId = tul.Id, Categorie = "Carburant", Designation = "Gasoil",
                    Unite = "litre", QuantiteCommandee = 2000, QuantiteRecue = 1800, QuantiteUtilisee = 900,
                    SeuilMinimal = 300, PrixUnitaire = 5_400m }
            );
            await context.SaveChangesAsync();
        }

        // --- Paramètres généraux ---
        if (!await context.Parametres.AnyAsync())
        {
            context.Parametres.AddRange(
                new Parametre { Cle = "Entreprise.Nom", Valeur = "ETAM - Forage & Travaux Publics", Groupe = "Général" },
                new Parametre { Cle = "Entreprise.Devise", Valeur = "Ar", Groupe = "Général" },
                new Parametre { Cle = "Comptabilite.Exercice", Valeur = "2026", Groupe = "Comptabilité" },
                new Parametre { Cle = "Alerte.SeuilBudgetPct", Valeur = "15", Groupe = "Alertes" },
                new Parametre { Cle = "Alerte.SeuilReceptionPct", Valeur = "90", Groupe = "Alertes" }
            );
            await context.SaveChangesAsync();
        }

        // === Données de démonstration par module ===
        var chantiersDb = await context.Chantiers.OrderBy(c => c.Id).ToListAsync();
        var materiauxDb = await context.Materiaux.OrderBy(m => m.Id).ToListAsync();

        if (chantiersDb.Count > 0)
        {
            var c1 = chantiersDb[0]; // Ampirika
            var c2 = chantiersDb.Count > 1 ? chantiersDb[1] : c1; // Ambovombe
            var c3 = chantiersDb.Count > 2 ? chantiersDb[2] : c1; // Tuléar
            var now = DateTime.UtcNow;

            // --- Prévisions journalières (un exemplaire par statut du workflow) ---
            if (!await context.Previsions.AnyAsync())
            {
                var cimentAmp = materiauxDb.FirstOrDefault(m => m.ChantierId == c1.Id && m.Designation.Contains("Ciment"));

                var previsions = new List<PrevisionJournaliere>
                {
                    new() { ChantierId = c1.Id, DatePrevision = now, Statut = StatutPrevision.Brouillon,
                        Reference = $"PREV-{c1.Code}-{now:yyyyMMdd}-0001", Observation = "Prévision du jour (brouillon).",
                        Lignes = new List<PrevisionLigne> {
                            new() { Designation = "Carburant groupe", Categorie = "Carburant", TypeBudget = TypeBudget.Compte, Quantite = 40, PrixUnitaireEstime = 5_400m },
                            new() { Designation = "Nourriture équipe", Categorie = "Nourriture", TypeBudget = TypeBudget.Compte, Quantite = 15, PrixUnitaireEstime = 8_000m } } },

                    new() { ChantierId = c2.Id, DatePrevision = now.AddDays(-1), Statut = StatutPrevision.Soumise,
                        Reference = $"PREV-{c2.Code}-{now.AddDays(-1):yyyyMMdd}-0002", SoumisePar = "chef@etam.mg", DateSoumission = now.AddDays(-1),
                        Observation = "En attente de validation RF.",
                        Lignes = new List<PrevisionLigne> {
                            new() { Designation = "Transport matériaux", Categorie = "Transport", TypeBudget = TypeBudget.Compte, Quantite = 1, PrixUnitaireEstime = 120_000m },
                            new() { Designation = "Eau chantier", Categorie = "Eau", TypeBudget = TypeBudget.Compte, Quantite = 10, PrixUnitaireEstime = 3_000m } } },

                    new() { ChantierId = c3.Id, DatePrevision = now.AddDays(-2), Statut = StatutPrevision.ValideeResponsableFinancier,
                        Reference = $"PREV-{c3.Code}-{now.AddDays(-2):yyyyMMdd}-0003", SoumisePar = "chef@etam.mg", DateSoumission = now.AddDays(-3),
                        ValideeParRfId = "rf@etam.mg", DateValidationRf = now.AddDays(-2), Observation = "Validée RF, attend Admin.",
                        Lignes = new List<PrevisionLigne> {
                            new() { Designation = "Paiement dette ciment", Categorie = "Dette", TypeBudget = TypeBudget.Compte, Quantite = 1, PrixUnitaireEstime = 450_000m } } },

                    new() { ChantierId = c1.Id, DatePrevision = now.AddDays(-3), Statut = StatutPrevision.ValideeAdministrateur,
                        Reference = $"PREV-{c1.Code}-{now.AddDays(-3):yyyyMMdd}-0004", SoumisePar = "chef@etam.mg", DateSoumission = now.AddDays(-4),
                        ValideeParRfId = "rf@etam.mg", DateValidationRf = now.AddDays(-3), ValideeParAdminId = "admin@etam.mg", DateValidationAdmin = now.AddDays(-3),
                        Observation = "Prête à exécuter.",
                        Lignes = new List<PrevisionLigne> {
                            new() { Designation = "Utilisation ciment", Categorie = "Matériel", TypeBudget = TypeBudget.Materiel, MateriauId = cimentAmp?.Id, Quantite = 20, PrixUnitaireEstime = 42_000m } } },

                    new() { ChantierId = c2.Id, DatePrevision = now.AddDays(-6), Statut = StatutPrevision.Executee,
                        Reference = $"PREV-{c2.Code}-{now.AddDays(-6):yyyyMMdd}-0005", SoumisePar = "chef@etam.mg", DateSoumission = now.AddDays(-7),
                        ValideeParRfId = "rf@etam.mg", DateValidationRf = now.AddDays(-6), ValideeParAdminId = "admin@etam.mg", DateValidationAdmin = now.AddDays(-6),
                        DateExecution = now.AddDays(-6), Observation = "Exécutée.",
                        Lignes = new List<PrevisionLigne> {
                            new() { Designation = "Carburant forage", Categorie = "Carburant", TypeBudget = TypeBudget.Compte, Quantite = 60, PrixUnitaireEstime = 5_400m } } },

                    new() { ChantierId = c3.Id, DatePrevision = now.AddDays(-5), Statut = StatutPrevision.Refusee,
                        Reference = $"PREV-{c3.Code}-{now.AddDays(-5):yyyyMMdd}-0006", SoumisePar = "chef@etam.mg", DateSoumission = now.AddDays(-5),
                        MotifRefus = "Dépense non justifiée.", Observation = "Refusée par le RF.",
                        Lignes = new List<PrevisionLigne> {
                            new() { Designation = "Hôtel mission", Categorie = "Hôtel", TypeBudget = TypeBudget.Compte, Quantite = 3, PrixUnitaireEstime = 90_000m } } }
                };
                context.Previsions.AddRange(previsions);
                await context.SaveChangesAsync();
            }

            // --- Dépenses réelles ---
            if (!await context.Depenses.AnyAsync())
            {
                context.Depenses.AddRange(
                    new Depense { Date = now.AddDays(-6), ChantierId = c2.Id, Categorie = "Carburant", Designation = "Carburant forage", Quantite = 60, PrixUnitaire = 5_400m, BudgetConcerne = TypeBudget.Compte, Justificatif = "FAC-2026-045" },
                    new Depense { Date = now.AddDays(-5), ChantierId = c1.Id, Categorie = "Nourriture", Designation = "Ravitaillement équipe", Quantite = 20, PrixUnitaire = 8_000m, BudgetConcerne = TypeBudget.Compte, Justificatif = "FAC-2026-046" },
                    new Depense { Date = now.AddDays(-4), ChantierId = c3.Id, Categorie = "Transport", Designation = "Location camion", Quantite = 1, PrixUnitaire = 250_000m, BudgetConcerne = TypeBudget.Compte, Justificatif = "FAC-2026-047" },
                    new Depense { Date = now.AddDays(-3), ChantierId = c1.Id, Categorie = "Matériel", Designation = "Achat ciment complément", Quantite = 30, PrixUnitaire = 42_000m, BudgetConcerne = TypeBudget.Materiel, Justificatif = "FAC-2026-048" },
                    new Depense { Date = now.AddDays(-2), ChantierId = c2.Id, Categorie = "Réparation", Designation = "Réparation pompe", Quantite = 1, PrixUnitaire = 180_000m, BudgetConcerne = TypeBudget.Compte, Justificatif = "FAC-2026-049" },
                    new Depense { Date = now.AddDays(-1), ChantierId = c3.Id, Categorie = "Carburant", Designation = "Gasoil groupe", Quantite = 100, PrixUnitaire = 5_400m, BudgetConcerne = TypeBudget.Materiel, Justificatif = "FAC-2026-050" }
                );
                await context.SaveChangesAsync();
            }

            // --- Alertes ---
            if (!await context.Alertes.AnyAsync())
            {
                context.Alertes.AddRange(
                    new Alerte { Type = TypeAlerte.BudgetFaible, Niveau = NiveauAlerte.Avertissement, ChantierId = c2.Id,
                        Titre = $"Budget Matériel faible - {c2.Nom}", Message = $"Il reste {c2.BudgetMaterielRestant:N0} Ar sur le budget matériel." },
                    new Alerte { Type = TypeAlerte.ValidationEnAttente, Niveau = NiveauAlerte.Info, ChantierId = c2.Id,
                        Titre = "Prévision à valider", Message = "Une prévision attend la validation du Responsable Financier." },
                    new Alerte { Type = TypeAlerte.Reception90, Niveau = NiveauAlerte.Info, ChantierId = c1.Id,
                        Titre = "Réception à 90%", Message = "Le fer à béton est reçu à 100%." },
                    new Alerte { Type = TypeAlerte.StockFaible, Niveau = NiveauAlerte.Avertissement, ChantierId = c1.Id,
                        Titre = "Stock à surveiller", Message = "Le stock de ciment approche le seuil minimal.", EstLue = true, DateLecture = now.AddDays(-1) }
                );
                await context.SaveChangesAsync();
            }

            // --- Journal d'audit (quelques entrées d'exemple) ---
            if (!await context.AuditLogs.AnyAsync())
            {
                context.AuditLogs.AddRange(
                    new AuditLog { Action = TypeActionAudit.Connexion, UtilisateurNom = "admin@etam.mg", AdresseIp = "127.0.0.1", Navigateur = "Chrome", DateAction = now.AddDays(-1) },
                    new AuditLog { Action = TypeActionAudit.Ajout, Entite = "Chantier", CleEntite = c1.Id.ToString(), UtilisateurNom = "admin@etam.mg", AdresseIp = "127.0.0.1", NouvelleValeur = c1.Nom, DateAction = now.AddDays(-1) },
                    new AuditLog { Action = TypeActionAudit.Validation, Entite = "PrevisionJournaliere", UtilisateurNom = "rf@etam.mg", AdresseIp = "127.0.0.1", NouvelleValeur = "ValideeResponsableFinancier", DateAction = now.AddHours(-5) },
                    new AuditLog { Action = TypeActionAudit.Execution, Entite = "PrevisionJournaliere", UtilisateurNom = "admin@etam.mg", AdresseIp = "127.0.0.1", DateAction = now.AddHours(-3) }
                );
                await context.SaveChangesAsync();
            }

            // --- Prévision réelle « TULEAR CE 06/07/26 » (laissée en Brouillon, non validée) ---
            const string refTulear = "PREV-TUL-03-20260706-TULEAR";
            if (!await context.Previsions.AnyAsync(p => p.Reference == refTulear))
            {
                var dateTul = new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc);
                var prevTulear = new PrevisionJournaliere
                {
                    ChantierId = c3.Id, // Tuléar
                    DatePrevision = dateTul,
                    Reference = refTulear,
                    Statut = StatutPrevision.Brouillon,
                    Observation = "TULEAR CE 06/07/26 — Sakafo + Chantier (Camion : néant). Grand total 4 149 000 Ar.",
                    Lignes = new List<PrevisionLigne>
                    {
                        // --- Sakafo (nourriture) -> Budget Comptes : 430 000 ---
                        new() { Designation = "Sakafo cadres",          Categorie = "Nourriture", TypeBudget = TypeBudget.Compte,  Quantite = 8,   PrixUnitaireEstime = 10_000m },
                        new() { Designation = "Sakafo cadres 2",        Categorie = "Nourriture", TypeBudget = TypeBudget.Compte,  Quantite = 1,   PrixUnitaireEstime = 50_000m },
                        new() { Designation = "Sakafo équipe 2",        Categorie = "Nourriture", TypeBudget = TypeBudget.Compte,  Quantite = 1,   PrixUnitaireEstime = 70_000m },
                        new() { Designation = "Sakafo Onja sy Alfa",    Categorie = "Nourriture", TypeBudget = TypeBudget.Compte,  Quantite = 2,   PrixUnitaireEstime = 10_000m },
                        new() { Designation = "Vary Eglise et centre",  Categorie = "Nourriture", TypeBudget = TypeBudget.Compte,  Quantite = 2,   PrixUnitaireEstime = 105_000m },

                        // --- Chantier -> mix Matériel / Comptes : 3 719 000 ---
                        new() { Designation = "Bois rond",                     Categorie = "Bois",        TypeBudget = TypeBudget.Materiel, Quantite = 336, PrixUnitaireEstime = 8_500m },
                        new() { Designation = "Location camion Akthar",        Categorie = "Transport",   TypeBudget = TypeBudget.Compte,   Quantite = 2,   PrixUnitaireEstime = 220_000m },
                        new() { Designation = "Lokida",                        Categorie = "Location",    TypeBudget = TypeBudget.Compte,   Quantite = 3,   PrixUnitaireEstime = 20_000m },
                        new() { Designation = "Colle silicone noir",           Categorie = "Consommable", TypeBudget = TypeBudget.Materiel, Quantite = 1,   PrixUnitaireEstime = 35_000m },
                        new() { Designation = "Papier abrasif",                Categorie = "Consommable", TypeBudget = TypeBudget.Materiel, Quantite = 2,   PrixUnitaireEstime = 1_500m },
                        new() { Designation = "Déplacement Stephan",           Categorie = "Déplacement", TypeBudget = TypeBudget.Compte,   Quantite = 1,   PrixUnitaireEstime = 40_000m },
                        new() { Designation = "Fasika (sable)",                Categorie = "Gros œuvre",  TypeBudget = TypeBudget.Materiel, Quantite = 12,  PrixUnitaireEstime = 15_000m },
                        new() { Designation = "Frais pièce tanà Tuléar",       Categorie = "Pièces",      TypeBudget = TypeBudget.Compte,   Quantite = 1,   PrixUnitaireEstime = 25_000m },
                        new() { Designation = "Frais papa Jules miakatra tanà",Categorie = "Déplacement", TypeBudget = TypeBudget.Compte,   Quantite = 1,   PrixUnitaireEstime = 80_000m }
                    }
                };
                context.Previsions.Add(prevTulear);
                await context.SaveChangesAsync();
            }

            // --- Trésorerie : un compte bancaire par chantier + un compte pour le Budget Comptes ---
            // Idempotent : crée les comptes manquants et retire les anciens comptes génériques,
            // même si la base n'a pas été réinitialisée (pas besoin de dropper la base).
            var comptesModifies = false;

            foreach (var ch in chantiersDb)
            {
                if (!await context.ComptesBancaires.AnyAsync(c => c.ChantierId == ch.Id))
                {
                    context.ComptesBancaires.Add(new CompteBancaire
                    {
                        Nom = $"Compte {ch.Nom}",
                        Banque = "BNI",
                        Numero = $"CH-{ch.Code}",
                        Type = TypeCompteBancaire.Chantier,
                        ChantierId = ch.Id,
                        Solde = ch.BudgetMateriel - ch.MaterielTransfere
                    });
                    comptesModifies = true;
                }
            }

            if (!await context.ComptesBancaires.AnyAsync(c => c.Type == TypeCompteBancaire.Comptes))
            {
                var bcActif = await context.BudgetsComptes.Where(b => b.EstActif)
                    .OrderByDescending(b => b.Annee).FirstOrDefaultAsync();
                context.ComptesBancaires.Add(new CompteBancaire
                {
                    Nom = "Compte Budget Comptes",
                    Banque = "BOA",
                    Numero = "CPT-GENERAL",
                    Type = TypeCompteBancaire.Comptes,
                    Solde = bcActif is null ? 0m : bcActif.MontantInitial - bcActif.MontantTransfere
                });
                comptesModifies = true;
            }

            // Masque les anciens comptes génériques (type Chantier mais sans chantier rattaché).
            var comptesOrphelins = await context.ComptesBancaires
                .Where(c => c.Type == TypeCompteBancaire.Chantier && c.ChantierId == null).ToListAsync();
            foreach (var orphelin in comptesOrphelins)
            {
                orphelin.IsDeleted = true;
                context.ComptesBancaires.Update(orphelin);
                comptesModifies = true;
            }

            if (comptesModifies) await context.SaveChangesAsync();

            // --- Fournisseurs & dettes ---
            if (!await context.Fournisseurs.AnyAsync())
            {
                var f1 = new Fournisseur { Nom = "Quincaillerie Tsena Be", Contact = "M. Rakoto", Telephone = "034 12 345 67", Adresse = "Toliara" };
                var f2 = new Fournisseur { Nom = "Ciments de Madagascar", Contact = "Service commercial", Telephone = "020 22 333 44", Adresse = "Antananarivo" };
                var f3 = new Fournisseur { Nom = "Aciers & Fers SARL", Contact = "Mme Rasoa", Telephone = "032 55 666 77", Adresse = "Antananarivo" };
                context.Fournisseurs.AddRange(f1, f2, f3);
                await context.SaveChangesAsync();

                context.DettesFournisseurs.AddRange(
                    new DetteFournisseur { FournisseurId = f2.Id, ChantierId = c1.Id, Libelle = "Dette ciment (500 sacs)", MontantInitial = 21_000_000m, MontantPaye = 6_000_000m, Statut = StatutDette.PartiellementPayee, DateEcheance = now.AddDays(20) },
                    new DetteFournisseur { FournisseurId = f3.Id, ChantierId = c2.Id, Libelle = "Dette fer à béton", MontantInitial = 8_400_000m, MontantPaye = 0m, Statut = StatutDette.Ouverte, DateEcheance = now.AddDays(30) },
                    new DetteFournisseur { FournisseurId = f1.Id, ChantierId = c3.Id, Libelle = "Consommables divers", MontantInitial = 1_200_000m, MontantPaye = 1_200_000m, Statut = StatutDette.Soldee, DateEcheance = now.AddDays(-5) }
                );
                await context.SaveChangesAsync();
            }

            // --- Quelques mouvements bancaires d'exemple sur le compte Budget Comptes ---
            if (!await context.MouvementsBancaires.AnyAsync())
            {
                var compteGen = await context.ComptesBancaires
                    .Where(c => c.Type == TypeCompteBancaire.Comptes).FirstOrDefaultAsync();
                if (compteGen is not null)
                {
                    context.MouvementsBancaires.AddRange(
                        new MouvementBancaire { CompteBancaireId = compteGen.Id, Type = TypeMouvementBancaire.Depot, Montant = 15_000_000m, Motif = "Encaissement décompte", Date = now.AddDays(-10) },
                        new MouvementBancaire { CompteBancaireId = compteGen.Id, Type = TypeMouvementBancaire.Frais, Montant = 45_000m, Motif = "Frais de tenue de compte", Date = now.AddDays(-2) }
                    );
                    await context.SaveChangesAsync();
                }
            }

            // --- Approvisionnement de démonstration (saisi par le chef, en Brouillon) ---
            if (!await context.Approvisionnements.AnyAsync())
            {
                context.Approvisionnements.Add(new Approvisionnement
                {
                    ChantierId = c2.Id, // Ambovombe
                    DateAppro = now,
                    Reference = $"APPRO-{chantiersDb[1].Code}-{now:yyyyMMdd}-0001",
                    Statut = StatutApprovisionnement.Brouillon,
                    Observation = "Approvisionnement du jour — à valider pour générer la prévision.",
                    Lignes = new List<ApprovisionnementLigne>
                    {
                        new() { Designation = "Ciment CEM II",   Categorie = "Gros œuvre", TypeBudget = TypeBudget.Materiel, Quantite = 50, PrixUnitaireEstime = 42_000m },
                        new() { Designation = "Gasoil groupe",    Categorie = "Carburant",  TypeBudget = TypeBudget.Compte,   Quantite = 80, PrixUnitaireEstime = 5_400m },
                        new() { Designation = "Sakafo équipe",    Categorie = "Nourriture", TypeBudget = TypeBudget.Compte,   Quantite = 12, PrixUnitaireEstime = 8_000m }
                    }
                });
                await context.SaveChangesAsync();
            }
        }

        // --- Catalogue des désignations (prix de référence pour l'autocomplétion) ---
        if (!await context.Catalogue.AnyAsync())
        {
            context.Catalogue.AddRange(
                new ArticleCatalogue { Designation = "Ciment CEM II 42.5", Categorie = "Gros œuvre", Unite = "sac", PrixUnitaire = 42_000m },
                new ArticleCatalogue { Designation = "Fer à béton Ø12", Categorie = "Ferraillage", Unite = "barre", PrixUnitaire = 38_000m },
                new ArticleCatalogue { Designation = "Gasoil", Categorie = "Carburant", Unite = "litre", PrixUnitaire = 5_400m },
                new ArticleCatalogue { Designation = "Tube PVC forage Ø125", Categorie = "Forage", Unite = "ml", PrixUnitaire = 25_000m },
                new ArticleCatalogue { Designation = "Gravillon 5/15", Categorie = "Gros œuvre", Unite = "m3", PrixUnitaire = 90_000m },
                new ArticleCatalogue { Designation = "Sable (fasika)", Categorie = "Gros œuvre", Unite = "m3", PrixUnitaire = 60_000m },
                new ArticleCatalogue { Designation = "Bois rond", Categorie = "Bois", Unite = "unité", PrixUnitaire = 8_500m },
                new ArticleCatalogue { Designation = "Sakafo équipe", Categorie = "Nourriture", Unite = "repas", PrixUnitaire = 8_000m }
            );
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Seed ETAM terminé.");
    }
}
