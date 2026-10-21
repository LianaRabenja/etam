-- =====================================================================
--  ETAM — NETTOYAGE AVANT TESTS EN PRÉ-PRODUCTION
--
--  Efface TOUTES les données métier et garde UNIQUEMENT :
--    • les comptes utilisateurs, rôles et leurs rattachements (AspNet*)
--    • l'historique des migrations (__EFMigrationsHistory)
--    • les clés de protection des données (DataProtectionKeys)
--      → sans elles, tout le monde est déconnecté et les formulaires
--        ouverts tombent en erreur « jeton antiforgery invalide ».
--
--  Tout le reste repart à zéro, compteurs d'identifiants compris :
--  ton premier chantier aura l'Id 1, ta première prévision l'Id 1.
--
--  Le schéma « hangfire » n'est pas touché : il ne contient que la file
--  des tâches planifiées, aucune donnée métier.
--
--  POURQUOI PAS « TRUNCATE ... CASCADE » :
--  CASCADE se propage aux tables qui RÉFÉRENCENT celles qu'on vide.
--  « TRUNCATE "Chantiers" CASCADE » viderait donc AspNetUsers, qui porte
--  une clé étrangère ChantierId — exactement ce qu'on veut garder.
--  Ici la liste est explicite et SANS cascade : si une table manquait,
--  PostgreSQL REFUSE le TRUNCATE au lieu d'effacer en silence. L'erreur
--  est le garde-fou.
-- =====================================================================


-- ---------------------------------------------------------------------
--  ÉTAPE 0 — SAUVEGARDE. À faire AVANT, dans un terminal, pas dans psql.
-- ---------------------------------------------------------------------
--   pg_dump "<EXTERNAL_DATABASE_URL>" -Fc -f etam_avant_nettoyage.dump
--
--   Vérifie que le fichier existe et qu'il ne fait pas 0 octet.
--   Sans lui, il n'y a aucun retour en arrière possible.
-- ---------------------------------------------------------------------


BEGIN;


-- ---------------------------------------------------------------------
--  1. État avant nettoyage — pour comparer après
-- ---------------------------------------------------------------------
SELECT 'AVANT' AS moment,
       (SELECT count(*) FROM "AspNetUsers")         AS utilisateurs,
       (SELECT count(*) FROM "Chantiers")           AS chantiers,
       (SELECT count(*) FROM "Previsions")          AS previsions,
       (SELECT count(*) FROM "MouvementsBancaires") AS mouvements,
       (SELECT count(*) FROM "ComptesBancaires")    AS comptes;


-- ---------------------------------------------------------------------
--  2. Détacher les utilisateurs de leur chantier
--
--  AspNetUsers.ChantierId pointe vers Chantiers. Tant qu'un utilisateur
--  y est rattaché, le TRUNCATE est refusé. On coupe le lien d'abord :
--  les comptes restent intacts, ils ne sont simplement plus affectés à
--  un chantier. Tu les réaffecteras depuis l'écran Utilisateurs.
-- ---------------------------------------------------------------------
UPDATE "AspNetUsers" SET "ChantierId" = NULL WHERE "ChantierId" IS NOT NULL;


-- ---------------------------------------------------------------------
--  3. Vider toutes les tables métier, en une seule instruction
--
--  Un seul TRUNCATE pour toutes : les contraintes entre ces tables sont
--  ignorées puisqu'elles sont vidées ensemble. RESTART IDENTITY remet
--  les séquences d'Id à 1.
-- ---------------------------------------------------------------------
TRUNCATE TABLE
    -- Chantiers et budgets
    "Chantiers",
    "BudgetsComptes",

    -- Plan du projet
    "PrevisionsGlobales",
    "PrevisionsGlobalesLignes",

    -- Enveloppe du mois et prévision de la semaine
    "PrevisionsMensuelles",
    "PrevisionMensuelleLignes",
    "PlansJournaliers",

    -- Prévisions réelles et ce qui s'y rattache
    "Previsions",
    "PrevisionLignes",
    "AutresDepensesJour",
    "Decaissements",
    "PiecesJointes",

    -- Demandes du chantier
    "Approvisionnements",
    "ApprovisionnementLignes",

    -- Rapports de travail
    "RapportsTravail",
    "RapportTravailLignesAvancement",
    "RapportTravailLignesMateriaux",
    "RapportTravailLignesEquipements",

    -- Banque
    "ComptesBancaires",
    "MouvementsBancaires",
    "Banques",

    -- Stock et achats
    "Materiaux",
    "MouvementsMateriau",
    "Fournisseurs",
    "DettesFournisseurs",
    "Catalogue",

    -- Dépenses diverses
    "Depenses",

    -- Journal et paramètres
    "Alertes",
    "AuditLogs",
    "Parametres"
RESTART IDENTITY;


-- ---------------------------------------------------------------------
--  4. État après nettoyage
--
--  Attendu : utilisateurs inchangé, tout le reste à 0.
-- ---------------------------------------------------------------------
SELECT 'APRES' AS moment,
       (SELECT count(*) FROM "AspNetUsers")         AS utilisateurs,
       (SELECT count(*) FROM "Chantiers")           AS chantiers,
       (SELECT count(*) FROM "Previsions")          AS previsions,
       (SELECT count(*) FROM "MouvementsBancaires") AS mouvements,
       (SELECT count(*) FROM "ComptesBancaires")    AS comptes;


-- ---------------------------------------------------------------------
--  5. Les comptes conservés, avec leurs rôles
-- ---------------------------------------------------------------------
SELECT u."UserName", u."Email", r."Name" AS role, u."ChantierId"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r      ON r."Id" = ur."RoleId"
ORDER BY r."Name", u."UserName";


-- ---------------------------------------------------------------------
--  6. Contrôle : aucune table métier ne doit avoir été oubliée
--
--  Liste les tables du schéma public qui contiennent encore des lignes.
--  Seules AspNet*, __EFMigrationsHistory et DataProtectionKeys ont le
--  droit d'y figurer. Toute autre ligne signale une table que j'ai
--  manquée — dis-le-moi avant de valider.
-- ---------------------------------------------------------------------
SELECT relname AS table_non_vide, n_live_tup AS lignes
FROM pg_stat_user_tables
WHERE schemaname = 'public'
  AND n_live_tup > 0
ORDER BY n_live_tup DESC;


-- =====================================================================
--  RIEN N'EST ENCORE ENREGISTRÉ.
--
--  Lis les résultats ci-dessus. Si tout est conforme :
--      COMMIT;
--
--  Au moindre doute :
--      ROLLBACK;
--
--  Tant que tu n'as pas tapé l'un des deux, la base est inchangée pour
--  les autres connexions.
-- =====================================================================
