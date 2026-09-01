using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ETAM.Infrastructure.Persistence.Seed;

/// <summary>
/// Applique les migrations et crée un jeu de données minimal :
/// les rôles, les comptes de base et UN chantier d'exemple (NOSY BE).
/// Les autres chantiers seront créés directement depuis l'application.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Mot de passe initial d'un compte : lu depuis une variable d'environnement.
    /// Aucun mot de passe de production n'est stocké dans le code source.
    /// </summary>
    private static string MotDePasse(string variable, string valeurDeveloppement)
        => Environment.GetEnvironmentVariable(variable) is { Length: > 0 } v ? v : valeurDeveloppement;

    /// <summary>
    /// Efface toutes les données liées aux chantiers, en conservant les utilisateurs,
    /// le catalogue des prix, les fournisseurs, le budget du bureau, les paramètres
    /// et le journal d'audit.
    ///
    /// L'ordre suit les dépendances : on part des feuilles et on remonte vers les
    /// chantiers. Le tout dans une transaction, donc sans état intermédiaire possible.
    /// </summary>
    /// <summary>
    /// Vide entièrement la base de son contenu métier et remet les compteurs à zéro.
    /// Les utilisateurs et les rôles sont conservés : sans eux, personne ne pourrait
    /// se reconnecter. Le catalogue, les fournisseurs et les paramètres sont recréés
    /// automatiquement au démarrage suivant.
    /// </summary>
    private static async Task ToutEffacerAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogWarning("EFFACEMENT TOTAL DEMANDÉ : toutes les données métier vont être supprimées.");

        // Previsions référence PlansJournaliers sans ON DELETE : il faut vider
        // les prévisions AVANT les plans journaliers, sinon la clé étrangère
        // fait échouer tout le lot.
        const string sql = """
            DELETE FROM "PiecesJointes";
            DELETE FROM "Decaissements";
            DELETE FROM "PrevisionLignes";
            DELETE FROM "Previsions";
            DELETE FROM "AutresDepensesJour";
            DELETE FROM "PlansJournaliers";
            DELETE FROM "PrevisionMensuelleLignes";
            DELETE FROM "PrevisionsMensuelles";
            DELETE FROM "PrevisionsGlobalesLignes";
            DELETE FROM "PrevisionsGlobales";
            DELETE FROM "ApprovisionnementLignes";
            DELETE FROM "Approvisionnements";
            DELETE FROM "RapportTravailLignesAvancement";
            DELETE FROM "RapportTravailLignesMateriaux";
            DELETE FROM "RapportTravailLignesEquipements";
            DELETE FROM "RapportsTravail";
            DELETE FROM "MouvementsMateriau";
            DELETE FROM "Materiaux";
            DELETE FROM "Depenses";
            DELETE FROM "Alertes";
            DELETE FROM "DettesFournisseurs";
            DELETE FROM "MouvementsBancaires";
            DELETE FROM "ComptesBancaires";
            UPDATE "AspNetUsers" SET "ChantierId" = NULL;
            DELETE FROM "Chantiers";
            DELETE FROM "Fournisseurs";
            DELETE FROM "Catalogue";
            DELETE FROM "BudgetsComptes";
            DELETE FROM "Parametres";
            DELETE FROM "AuditLogs";
            """;

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
            await transaction.CommitAsync();
            logger.LogWarning(
                "EFFACEMENT TOTAL TERMINÉ. La base ne contient plus que les comptes utilisateurs. " +
                "RETIREZ MAINTENANT la variable ETAM_TOUT_EFFACER, sinon la base sera vidée " +
                "à chaque redémarrage du service.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "L'effacement total a échoué. Rien n'a été supprimé.");
        }
    }

    private static async Task NettoyerDonneesChantiersAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogWarning("NETTOYAGE DEMANDÉ : suppression de toutes les données de chantier.");

        // Pas de BEGIN/COMMIT dans le texte SQL : Npgsql n'enveloppe pas
        // automatiquement un lot de plusieurs instructions. La transaction est
        // ouverte explicitement ci-dessous, sinon un échec en cours de route
        // laisserait la base à moitié nettoyée.
        // Previsions référence PlansJournaliers sans ON DELETE : il faut vider
        // les prévisions AVANT les plans journaliers, sinon la clé étrangère
        // fait échouer tout le lot.
        const string sql = """
            DELETE FROM "PiecesJointes";
            DELETE FROM "Decaissements";
            DELETE FROM "PrevisionLignes";
            DELETE FROM "Previsions";
            DELETE FROM "AutresDepensesJour";
            DELETE FROM "PlansJournaliers";
            DELETE FROM "PrevisionMensuelleLignes";
            DELETE FROM "PrevisionsMensuelles";
            DELETE FROM "PrevisionsGlobalesLignes";
            DELETE FROM "PrevisionsGlobales";
            DELETE FROM "ApprovisionnementLignes";
            DELETE FROM "Approvisionnements";
            DELETE FROM "RapportTravailLignesAvancement";
            DELETE FROM "RapportTravailLignesMateriaux";
            DELETE FROM "RapportTravailLignesEquipements";
            DELETE FROM "RapportsTravail";
            DELETE FROM "MouvementsMateriau";
            DELETE FROM "Materiaux";
            DELETE FROM "Depenses";
            DELETE FROM "Alertes";
            DELETE FROM "DettesFournisseurs";
            DELETE FROM "MouvementsBancaires";
            DELETE FROM "ComptesBancaires";
            UPDATE "AspNetUsers" SET "ChantierId" = NULL WHERE "ChantierId" IS NOT NULL;
            DELETE FROM "Chantiers";
            UPDATE "BudgetsComptes"
               SET "MontantConsomme" = 0, "MontantTransfere" = 0, "ReserveUtilisee" = 0;
            """;

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
            await transaction.CommitAsync();
            logger.LogWarning(
                "NETTOYAGE TERMINÉ. Retirez maintenant la variable ETAM_NETTOYER_CHANTIERS, " +
                "sinon la base sera vidée à chaque redémarrage.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Le nettoyage des données de chantier a échoué. Rien n'a été supprimé.");
        }
    }

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        await context.Database.MigrateAsync();

        // --- Nettoyage des données de chantier, à la demande ---
        // Déclenché uniquement si ETAM_NETTOYER_CHANTIERS vaut exactement OUI-EFFACER.
        // Pensez à retirer la variable juste après : sinon la base serait vidée
        // à chaque redémarrage du service.
        if (Environment.GetEnvironmentVariable("ETAM_NETTOYER_CHANTIERS") == "OUI-EFFACER")
        {
            await NettoyerDonneesChantiersAsync(context, logger);
        }

        // Effacement TOTAL : tout le contenu métier, y compris le catalogue des prix,
        // les fournisseurs, le budget du bureau et le journal d'audit.
        // Seuls les comptes utilisateurs et les rôles survivent — sans eux, plus
        // personne ne pourrait se connecter pour reconstruire quoi que ce soit.
        if (Environment.GetEnvironmentVariable("ETAM_TOUT_EFFACER") == "OUI-TOUT-EFFACER")
        {
            await ToutEffacerAsync(context, logger);
        }

        // --- Migration douce de l'ancien libellé de rôle ---
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

        if (Environment.GetEnvironmentVariable("ETAM_ADMIN_PASSWORD") is null)
            logger.LogWarning("ETAM_ADMIN_PASSWORD n'est pas défini : mots de passe de développement utilisés. " +
                              "À changer impérativement après la première connexion en production.");

        // --- Comptes de base ---
        async Task CreerUtilisateurAsync(string email, string nom, string fonction, string role,
                                          string variableMdp, string mdpDev, long? chantierId = null)
        {
            if (await userManager.FindByEmailAsync(email) is not null) return;
            var u = new ApplicationUser
            {
                UserName = email, Email = email, EmailConfirmed = true,
                NomComplet = nom, Fonction = fonction, EstActif = true, ChantierId = chantierId
            };
            var res = await userManager.CreateAsync(u, MotDePasse(variableMdp, mdpDev));
            if (res.Succeeded) await userManager.AddToRoleAsync(u, role);
            else logger.LogError("Création de {Email} échouée : {Erreurs}", email,
                    string.Join(" ", res.Errors.Select(e => e.Description)));
        }

        await CreerUtilisateurAsync("admin@etam.mg", "Administrateur ETAM", "Direction",
            RolesEtam.Administrateur, "ETAM_ADMIN_PASSWORD", "Admin@2026");
        await CreerUtilisateurAsync("rf@etam.mg", "Correspondant ETAM", "Finance",
            RolesEtam.ResponsableFinancier, "ETAM_RF_PASSWORD", "Finance@2026");
        await CreerUtilisateurAsync("chef@etam.mg", "Chef de Chantier", "Exploitation",
            RolesEtam.ChefDeChantier, "ETAM_CHEF_PASSWORD", "Chef@2026");

        // --- Budget Comptes annuel ---
        if (!await context.BudgetsComptes.AnyAsync(b => b.Annee == 2026))
        {
            context.BudgetsComptes.Add(new BudgetCompte
            {
                Annee = 2026, Libelle = "Budget 2026", MontantInitial = 50_000_000m,
                MontantTransfere = 10_000_000m, MontantConsomme = 2_500_000m,
                Reserve = 5_000_000m, ReserveUtilisee = 0m, EstActif = true
            });
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

        // --- Catalogue de référence (prix imposés lors des saisies) ---
        if (!await context.Catalogue.AnyAsync())
        {
            context.Catalogue.AddRange(
                new ArticleCatalogue { Designation = "Ciment", Categorie = "Gros œuvre", Unite = "t", PrixUnitaire = 700_000m },
                new ArticleCatalogue { Designation = "Sable", Categorie = "Gros œuvre", Unite = "m³", PrixUnitaire = 40_000m },
                new ArticleCatalogue { Designation = "Gravillon", Categorie = "Gros œuvre", Unite = "m³", PrixUnitaire = 90_000m },
                new ArticleCatalogue { Designation = "Fer à béton Ø10", Categorie = "Ferraillage", Unite = "barre", PrixUnitaire = 38_000m },
                new ArticleCatalogue { Designation = "Bois rond", Categorie = "Bois", Unite = "unité", PrixUnitaire = 8_500m },
                new ArticleCatalogue { Designation = "Gasoil", Categorie = "Carburant", Unite = "litre", PrixUnitaire = 5_400m }
            );
            await context.SaveChangesAsync();
        }

        // =====================================================================
        //  CHANTIER D'EXEMPLE — NOSY BE
        //  Marché 150 M = bénéfice 80 M + budget projet 70 M (même compte bancaire).
        // =====================================================================
        // Le chantier de démonstration n'est créé QUE si on le demande explicitement.
        // Sans cette variable, le logiciel démarre vide : c'est le comportement voulu
        // dès que l'on saisit ses vrais chantiers, et cela évite qu'un nettoyage de
        // la base soit annulé par le redémarrage suivant.
        var donneesExemple = string.Equals(
            Environment.GetEnvironmentVariable("ETAM_DONNEES_EXEMPLE"),
            "true", StringComparison.OrdinalIgnoreCase);

        if (!donneesExemple)
        {
            logger.LogInformation(
                "Données de démonstration désactivées. Pour les recréer, définissez " +
                "ETAM_DONNEES_EXEMPLE=true puis redémarrez.");
            return;
        }

        var d0 = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);

        var chantierExistant = await context.Chantiers.FirstOrDefaultAsync(c => c.Code == "NOS-01");
        if (chantierExistant is not null)
        {
            // Le chantier est déjà là : on ne recrée rien, mais on rattrape le rapport de
            // travail s'il manque (cas d'un premier démarrage où sa table n'existait pas encore).
            if (!await context.RapportsTravail.AnyAsync(r => r.ChantierId == chantierExistant.Id))
            {
                context.RapportsTravail.Add(ConstruireRapportTravail(chantierExistant.Id, d0));
                await context.SaveChangesAsync();
                logger.LogInformation("Seed ETAM : rapport de travail créé (rattrapage).");
            }

            // Rattrapage : la rubrique Transport manquait, le plan ne couvrait donc pas
            // la totalité du budget projet (55 160 000 au lieu de 70 000 000).
            var pgExistante = await context.PrevisionsGlobales
                .Include(p => p.Lignes)
                .FirstOrDefaultAsync(p => p.ChantierId == chantierExistant.Id);
            if (pgExistante is not null && !pgExistante.Lignes.Any(l => l.Rubrique == "Transport"))
            {
                context.PrevisionsGlobalesLignes.AddRange(
                    new PrevisionGlobaleLigne { PrevisionGlobaleId = pgExistante.Id, Rubrique = "Transport",
                        Designation = "Location camion", Unite = "mois", Quantite = 5, PrixUnitaire = 2_000_000m },
                    new PrevisionGlobaleLigne { PrevisionGlobaleId = pgExistante.Id, Rubrique = "Transport",
                        Designation = "Carburant", Unite = "forfait", Quantite = 1, PrixUnitaire = 4_840_000m });
                await context.SaveChangesAsync();
                logger.LogInformation("Seed ETAM : rubrique Transport ajoutée (rattrapage).");
            }

            // Rattrapage : le flux bancaire initial était faux — un retrait direct était
            // enregistré au lieu d'un transfert vers le Budget Matériel, et rien n'était
            // réellement transféré. On rétablit le flux correct.
            if (chantierExistant.MaterielTransfere <= 0)
            {
                var compteExistant = await context.ComptesBancaires
                    .FirstOrDefaultAsync(c => c.ChantierId == chantierExistant.Id
                                           && c.Type == TypeCompteBancaire.Chantier);
                if (compteExistant is not null)
                {
                    // Annule le retrait erroné « Prévision journalière ».
                    var retraitErrone = await context.MouvementsBancaires.FirstOrDefaultAsync(
                        m => m.CompteBancaireId == compteExistant.Id
                          && m.Reference == "PREV-NOS-01-20260728");
                    if (retraitErrone is not null)
                    {
                        retraitErrone.IsDeleted = true;
                        context.MouvementsBancaires.Update(retraitErrone);
                    }

                    // Enregistre le transfert vers le Budget Matériel.
                    context.MouvementsBancaires.Add(new MouvementBancaire
                    {
                        CompteBancaireId = compteExistant.Id, ChantierId = chantierExistant.Id,
                        Date = d0.AddDays(2), Type = TypeMouvementBancaire.Virement,
                        Montant = 40_000_000m,
                        Motif = "Transfert vers le Budget Matériel — NOSY BE",
                        Reference = "TRANSF-NOS-01", EstValide = true
                    });

                    compteExistant.Solde = 150_000_000m - 40_000_000m;
                    context.ComptesBancaires.Update(compteExistant);

                    chantierExistant.MaterielTransfere = 40_000_000m;
                    chantierExistant.Consommation = 1_140_000m;
                    context.Chantiers.Update(chantierExistant);

                    await context.SaveChangesAsync();
                    logger.LogInformation("Seed ETAM : flux bancaire corrigé (rattrapage transfert).");
                }
            }

            logger.LogInformation("Seed ETAM : déjà initialisé.");
            return;
        }

        var nosyBe = new Chantier
        {
            Nom = "NOSY BE",
            Code = "NOS-01",
            Localisation = "Nosy Be, Diana",
            Responsable = "RADO Andrianina",
            DateDebut = d0,
            Statut = StatutChantier.EnCours,
            MontantMarche = 150_000_000m,
            Benefice = 80_000_000m,
            BudgetMateriel = 40_000_000m,
            Reserve = 5_000_000m,
            ReserveUtilisee = 0m,
            Consommation = 0m,
            MaterielTransfere = 0m,
            PourcentageAvancement = 15,
            Observation = "Marché 150 M : bénéfice 80 M, budget projet 70 M."
        };
        context.Chantiers.Add(nosyBe);
        await context.SaveChangesAsync();

        // Magasinier dédié à ce chantier
        await CreerUtilisateurAsync("magasinier@etam.mg", "Magasinier NOSY BE", "Logistique",
            RolesEtam.Magasinier, "ETAM_MAGASINIER_PASSWORD", "Stock@2026", nosyBe.Id);

        // --- Banque : le marché entre en banque (bénéfice + budget projet sur le même compte) ---
        var compte = new CompteBancaire
        {
            Nom = "Compte NOSY BE", Banque = "BNI", Numero = "CH-NOS-01",
            Type = TypeCompteBancaire.Chantier, ChantierId = nosyBe.Id, Solde = 0m
        };
        context.ComptesBancaires.Add(compte);
        await context.SaveChangesAsync();

        context.MouvementsBancaires.AddRange(
            new MouvementBancaire { CompteBancaireId = compte.Id, ChantierId = nosyBe.Id, Date = d0,
                Type = TypeMouvementBancaire.Depot, Montant = 150_000_000m,
                Motif = "Encaissement du marché — NOSY BE", Reference = "MARCHE-NOS-01", EstValide = true },
            // Transfert vers le Budget Matériel du chantier : c'est CE mouvement qui rend
            // l'argent utilisable par les prévisions (une prévision ne débite pas la banque
            // directement, elle consomme le budget préalablement transféré).
            new MouvementBancaire { CompteBancaireId = compte.Id, ChantierId = nosyBe.Id, Date = d0.AddDays(2),
                Type = TypeMouvementBancaire.Virement, Montant = 40_000_000m,
                Motif = "Transfert vers le Budget Matériel — NOSY BE",
                Reference = "TRANSF-NOS-01", EstValide = true }
        );
        compte.Solde = 150_000_000m - 40_000_000m;
        context.ComptesBancaires.Update(compte);

        // Le chantier dispose donc réellement de 40 000 000 Ar de Budget Matériel.
        nosyBe.MaterielTransfere = 40_000_000m;
        nosyBe.Consommation = 1_140_000m;   // consommé par la prévision du 28/07 (ligne Matériel)
        context.Chantiers.Update(nosyBe);
        await context.SaveChangesAsync();

        // --- Prévision globale du projet (70 M) ---
        var previsionGlobale = new PrevisionGlobale
        {
            ChantierId = nosyBe.Id,
            Reference = "PGLOB-NOS-01-20260720-0001",
            DateCreation = d0,
            Statut = StatutPrevisionGlobale.MiseEnBanque,
            Observation = "Budget projet 70 000 000 Ar (marché 150 M − bénéfice 80 M).",
            SoumisePar = "chef@etam.mg", DateSoumission = d0,
            ValideeParRfId = "rf@etam.mg", DateValidationRf = d0.AddDays(1),
            ValideeParAdminId = "admin@etam.mg", DateValidationAdmin = d0.AddDays(1),
            DateMiseEnBanque = d0.AddDays(1),
            Lignes = new List<PrevisionGlobaleLigne>
            {
                // Approvisionnement — 40 000 000
                new() { Rubrique = "Approvisionnement", Designation = "Ciment", Unite = "t",  Quantite = 30, PrixUnitaire = 700_000m },
                new() { Rubrique = "Approvisionnement", Designation = "Sable",  Unite = "m³", Quantite = 100, PrixUnitaire = 40_000m },
                new() { Rubrique = "Approvisionnement", Designation = "Fer à béton Ø10", Unite = "barre", Quantite = 200, PrixUnitaire = 38_000m },
                new() { Rubrique = "Approvisionnement", Designation = "Gravillon", Unite = "m³", Quantite = 84, PrixUnitaire = 90_000m },
                // Main d'œuvre — 10 000 000
                new() { Rubrique = "Main d'œuvre", Designation = "Maçons (tâcherons)", Unite = "forfait", Quantite = 1, PrixUnitaire = 4_000_000m },
                new() { Rubrique = "Main d'œuvre", Designation = "Plombiers",   Unite = "forfait", Quantite = 1, PrixUnitaire = 3_000_000m },
                new() { Rubrique = "Main d'œuvre", Designation = "Électricien", Unite = "forfait", Quantite = 1, PrixUnitaire = 2_000_000m },
                new() { Rubrique = "Main d'œuvre", Designation = "Peintre",     Unite = "forfait", Quantite = 1, PrixUnitaire = 1_000_000m },
                // Imprévus — 5 000 000
                new() { Rubrique = "Imprévus", Designation = "Santé / hospitalisation", Unite = "forfait", Quantite = 1, PrixUnitaire = 3_000_000m },
                new() { Rubrique = "Imprévus", Designation = "Social et divers",        Unite = "forfait", Quantite = 1, PrixUnitaire = 2_000_000m },
                // Transport — 14 840 000 : complète le plan pour atteindre exactement
                // le budget projet de 70 000 000 Ar (marché 150 M − bénéfice 80 M).
                new() { Rubrique = "Transport", Designation = "Location camion", Unite = "mois",    Quantite = 5, PrixUnitaire = 2_000_000m },
                new() { Rubrique = "Transport", Designation = "Carburant",       Unite = "forfait", Quantite = 1, PrixUnitaire = 4_840_000m }
            }
        };
        context.PrevisionsGlobales.Add(previsionGlobale);
        await context.SaveChangesAsync();

        var lCiment = previsionGlobale.Lignes.First(l => l.Designation == "Ciment");
        var lSable  = previsionGlobale.Lignes.First(l => l.Designation == "Sable");
        var lFer    = previsionGlobale.Lignes.First(l => l.Designation == "Fer à béton Ø10");
        var lMacons = previsionGlobale.Lignes.First(l => l.Designation == "Maçons (tâcherons)");
        var lSante  = previsionGlobale.Lignes.First(l => l.Designation == "Santé / hospitalisation");

        // --- Matériaux du chantier (2 articles) ---
        var ciment = new Materiau
        {
            ChantierId = nosyBe.Id, Categorie = "Gros œuvre", Designation = "Ciment",
            Localite = "Centre", Unite = "t", Besoin = 30,
            QuantiteCommandee = 30, QuantiteRecue = 10, QuantiteUtilisee = 6,
            SeuilMinimal = 3, PrixUnitaire = 700_000m
        };
        var fer = new Materiau
        {
            ChantierId = nosyBe.Id, Categorie = "Ferraillage", Designation = "Fer à béton Ø10",
            Localite = "Centre", Unite = "barre", Besoin = 200,
            QuantiteCommandee = 200, QuantiteRecue = 120, QuantiteUtilisee = 45,
            SeuilMinimal = 20, PrixUnitaire = 38_000m
        };
        context.Materiaux.AddRange(ciment, fer);
        await context.SaveChangesAsync();

        context.MouvementsMateriau.AddRange(
            new MouvementMateriau { MateriauxId = ciment.Id, DateMouvement = d0.AddDays(1),
                QuantiteEntree = 10, SoldeSurBesoin = 20, SoldeEnStock = 10, Motif = "Réception fournisseur" },
            new MouvementMateriau { MateriauxId = ciment.Id, DateMouvement = d0.AddDays(5),
                QuantiteSortie = 6, SoldeSurBesoin = 20, SoldeEnStock = 4, Motif = "Fondations zone A" },
            new MouvementMateriau { MateriauxId = fer.Id, DateMouvement = d0.AddDays(1),
                QuantiteEntree = 120, SoldeSurBesoin = 80, SoldeEnStock = 120, Motif = "Réception fournisseur" },
            new MouvementMateriau { MateriauxId = fer.Id, DateMouvement = d0.AddDays(6),
                QuantiteSortie = 45, SoldeSurBesoin = 80, SoldeEnStock = 75, Motif = "Ferraillage poutres" }
        );
        await context.SaveChangesAsync();

        // --- Un approvisionnement en brouillon (à valider → deviendra une prévision) ---
        context.Approvisionnements.Add(new Approvisionnement
        {
            ChantierId = nosyBe.Id,
            DateAppro = d0.AddDays(10),
            Reference = "APPRO-NOS-01-20260730-0001",
            Statut = StatutApprovisionnement.Brouillon,
            Observation = "Besoins de la semaine — à valider pour générer la prévision.",
            Lignes = new List<ApprovisionnementLigne>
            {
                new() { Designation = "Ciment", Categorie = "Gros œuvre", TypeBudget = TypeBudget.Materiel, Quantite = 2, PrixUnitaireEstime = 700_000m },
                new() { Designation = "Sable",  Categorie = "Gros œuvre", TypeBudget = TypeBudget.Materiel, Quantite = 10, PrixUnitaireEstime = 40_000m }
            }
        });
        await context.SaveChangesAsync();

        // --- Deux prévisions journalières : une clôturée, une à justifier ---
        context.Previsions.AddRange(
            // 1) Cycle complet : exécutée, compte rendu rendu et réceptionné par l'Administrateur
            new PrevisionJournaliere
            {
                ChantierId = nosyBe.Id, DatePrevision = d0.AddDays(3),
                Reference = "PREV-NOS-01-20260723-0001",
                Statut = StatutPrevision.Cloturee,
                SoumisePar = "chef@etam.mg", DateSoumission = d0.AddDays(3),
                ValideeParRfId = "rf@etam.mg", DateValidationRf = d0.AddDays(3),
                ValideeParAdminId = "admin@etam.mg", DateValidationAdmin = d0.AddDays(3),
                DateExecution = d0.AddDays(3),
                Observation = "Prévision du 23/07/2026.",
                RapportRealisation = "Coulage des fondations de la zone A terminé. 6 t de ciment et 30 m³ de sable " +
                                     "consommés. Équipe de 12 maçons, journée complète. Aucun incident.",
                DateRapport = d0.AddDays(4),
                RapportValideParId = "admin@etam.mg", DateValidationRapport = d0.AddDays(5),
                Lignes = new List<PrevisionLigne>
                {
                    new() { Designation = "Ciment", Categorie = "Gros œuvre", TypeBudget = TypeBudget.Materiel,
                            Quantite = 6, PrixUnitaireEstime = 700_000m, PrevisionGlobaleLigneId = lCiment.Id },
                    new() { Designation = "Sable", Categorie = "Gros œuvre", TypeBudget = TypeBudget.Materiel,
                            Quantite = 30, PrixUnitaireEstime = 40_000m, PrevisionGlobaleLigneId = lSable.Id }
                }
            },
            // 2) Exécutée sans compte rendu : BLOQUE toute nouvelle prévision sur ce chantier
            new PrevisionJournaliere
            {
                ChantierId = nosyBe.Id, DatePrevision = d0.AddDays(8),
                Reference = "PREV-NOS-01-20260728-0002",
                Statut = StatutPrevision.Executee,
                SoumisePar = "chef@etam.mg", DateSoumission = d0.AddDays(8),
                ValideeParRfId = "rf@etam.mg", DateValidationRf = d0.AddDays(8),
                ValideeParAdminId = "admin@etam.mg", DateValidationAdmin = d0.AddDays(8),
                DateExecution = d0.AddDays(8),
                Observation = "Prévision du 28/07/2026 — total 1 740 000 Ar retirés en banque.",
                Lignes = new List<PrevisionLigne>
                {
                    new() { Designation = "Fer à béton Ø10", Categorie = "Ferraillage", TypeBudget = TypeBudget.Materiel,
                            Quantite = 30, PrixUnitaireEstime = 38_000m, PrevisionGlobaleLigneId = lFer.Id },
                    new() { Designation = "Acompte maçons", Categorie = "Main d'œuvre", TypeBudget = TypeBudget.Compte,
                            Quantite = 1, PrixUnitaireEstime = 400_000m, PrevisionGlobaleLigneId = lMacons.Id },
                    new() { Designation = "Imprévu santé", Categorie = "Imprévus", TypeBudget = TypeBudget.Compte,
                            Quantite = 1, PrixUnitaireEstime = 200_000m, PrevisionGlobaleLigneId = lSante.Id }
                }
            }
        );
        await context.SaveChangesAsync();

        // --- Une dépense, un fournisseur, une dette ---
        context.Depenses.Add(new Depense
        {
            Date = d0.AddDays(3), ChantierId = nosyBe.Id, Categorie = "Gros œuvre",
            Designation = "Ciment fondations", Quantite = 6, PrixUnitaire = 700_000m,
            BudgetConcerne = TypeBudget.Materiel, Justificatif = "FAC-2026-001"
        });

        var fournisseur = new Fournisseur
        {
            Nom = "Quincaillerie Nosy Be", Contact = "M. Rakoto",
            Telephone = "034 12 345 67", Adresse = "Nosy Be"
        };
        context.Fournisseurs.Add(fournisseur);
        await context.SaveChangesAsync();

        context.DettesFournisseurs.Add(new DetteFournisseur
        {
            FournisseurId = fournisseur.Id, ChantierId = nosyBe.Id,
            Libelle = "Livraison ciment et fer", MontantInitial = 4_000_000m, MontantPaye = 1_000_000m,
            Statut = StatutDette.PartiellementPayee, DateEcheance = d0.AddDays(30)
        });
        await context.SaveChangesAsync();

        // --- Un rapport de travail hebdomadaire ---
        context.RapportsTravail.Add(ConstruireRapportTravail(nosyBe.Id, d0));
        await context.SaveChangesAsync();

        logger.LogInformation("Seed ETAM terminé : chantier NOSY BE créé.");
    }

    /// <summary>Rapport de travail d'exemple (extrait pour pouvoir être créé en rattrapage).</summary>
    private static RapportTravail ConstruireRapportTravail(long chantierId, DateTime d0) => new()
    {
        ChantierId = chantierId,
        Numero = "01",
        PeriodeDebut = d0,
        PeriodeFin = d0.AddDays(6),
        Lieu = "Nosy Be",
        EntrepriseExecutante = "ETAM",
        ConducteurTravaux = "RADO Andrianina",
        EffectifCadres = 3,
        EffectifOuvriers = 25,
        HoraireMatin = "07h - 11h30",
        HoraireApresMidi = "13h - 17h",
        ConditionsMeteo = "Conditions favorables sur toute la période.",
        Statut = StatutRapportTravail.Valide,
        SoumisPar = "chef@etam.mg", DateSoumission = d0.AddDays(7),
        ValideParId = "admin@etam.mg", DateValidation = d0.AddDays(8),
        ResumeSuiviPlanning = "Semaine 1 conforme au planning : implantation, terrassement et fondations de la zone A.",
        ProblemesRencontres = "Aucun problème majeur.",
        Suggestions = "Prévoir un stock tampon de ciment pour éviter les ruptures.",
        LignesAvancement = new List<RapportTravailAvancementLigne>
        {
            new() { Zone = "Zone A", TravauxRealises = "Implantation et terrassement", NiveauAvancement = "100 %", Observations = "Terminé" },
            new() { Zone = "Zone A", TravauxRealises = "Fondations", NiveauAvancement = "80 %", Observations = "Reste le séchage" }
        },
        LignesMateriaux = new List<RapportTravailMateriauLigne>
        {
            new() { Materiau = "Ciment", Unite = "t", StockInitial = 0, Entree = 10, QuantiteUtilisee = 6, StockRestant = 4 },
            new() { Materiau = "Fer à béton Ø10", Unite = "barre", StockInitial = 0, Entree = 120, QuantiteUtilisee = 45, StockRestant = 75 }
        },
        LignesEquipements = new List<RapportTravailEquipementLigne>
        {
            new() { Equipement = "01 bétonnière 500L", Etat = "Bon", Observation = "Entretien à jour" },
            new() { Equipement = "01 groupe électrogène", Etat = "Bon", Observation = "Rien à signaler" }
        }
    };
}
