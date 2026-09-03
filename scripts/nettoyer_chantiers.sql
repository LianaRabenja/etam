-- =====================================================================
--  ETAM — Remise à zéro des données de chantier
-- =====================================================================
--
--  CE QUI EST EFFACÉ
--    chantiers, comptes bancaires de chantier et leurs mouvements,
--    plans de projet, enveloppes mensuelles, prévisions journalières,
--    décaissements, dépenses, approvisionnements, matériaux et leurs
--    mouvements, rapports de travail, pièces jointes, alertes,
--    dettes rattachées à un chantier.
--
--  CE QUI EST CONSERVÉ
--    utilisateurs et rôles, catalogue des prix, fournisseurs,
--    budget du bureau, paramètres, journal d'audit,
--    comptes bancaires généraux (non rattachés à un chantier).
--
--  À LANCER AVEC
--    psql "VOTRE_CHAINE_DE_CONNEXION" -f scripts/nettoyer_chantiers.sql
--
--  L'ordre des suppressions suit les dépendances : on part des feuilles
--  et on remonte vers les chantiers. Le tout dans une transaction : en cas
--  d'erreur, rien n'est appliqué.
-- =====================================================================

BEGIN;

-- 1. Justificatifs (dépendent des prévisions, décaissements et rapports)
DELETE FROM "PiecesJointes";

-- 2. Sorties d'argent
DELETE FROM "Decaissements";

-- 3. Prévisions journalières
DELETE FROM "PrevisionLignes";
DELETE FROM "Previsions";

-- 4. Enveloppes mensuelles
DELETE FROM "PrevisionMensuelleLignes";
DELETE FROM "PrevisionsMensuelles";

-- 5. Plans de projet
DELETE FROM "PrevisionsGlobalesLignes";
DELETE FROM "PrevisionsGlobales";

-- 6. Demandes d'achat
DELETE FROM "ApprovisionnementLignes";
DELETE FROM "Approvisionnements";

-- 7. Rapports de travail
DELETE FROM "RapportTravailLignesAvancement";
DELETE FROM "RapportTravailLignesMateriaux";
DELETE FROM "RapportTravailLignesEquipements";
DELETE FROM "RapportsTravail";

-- 8. Stock
DELETE FROM "MouvementsMateriau";
DELETE FROM "Materiaux";

-- 9. Journal des dépenses et alertes
DELETE FROM "Depenses";
DELETE FROM "Alertes";

-- 10. Dettes rattachées à un chantier
--     Les dettes générales (sans chantier) sont conservées.
DELETE FROM "DettesFournisseurs" WHERE "ChantierId" IS NOT NULL;

-- 11. Banque : mouvements puis comptes de chantier
--     Les comptes généraux de l'entreprise sont conservés.
DELETE FROM "MouvementsBancaires"
 WHERE "ChantierId" IS NOT NULL
    OR "CompteBancaireId" IN (SELECT "Id" FROM "ComptesBancaires" WHERE "ChantierId" IS NOT NULL);

DELETE FROM "ComptesBancaires" WHERE "ChantierId" IS NOT NULL;

-- 12. Détacher les magasiniers de leur chantier avant de supprimer celui-ci.
--     Les comptes utilisateurs sont conservés : il suffira de les réaffecter.
UPDATE "AspNetUsers" SET "ChantierId" = NULL WHERE "ChantierId" IS NOT NULL;

-- 13. Les chantiers eux-mêmes
DELETE FROM "Chantiers";

-- 14. Remise à zéro du budget du bureau (le budget reste, ses compteurs repartent à zéro)
UPDATE "BudgetsComptes"
   SET "MontantConsomme" = 0,
       "MontantTransfere" = 0,
       "ReserveUtilisee" = 0;

COMMIT;

-- =====================================================================
--  VÉRIFICATION — doit afficher 0 partout dans la colonne « restant »
-- =====================================================================
SELECT 'Chantiers'            AS table_effacee, COUNT(*) AS restant FROM "Chantiers"
UNION ALL SELECT 'Previsions',            COUNT(*) FROM "Previsions"
UNION ALL SELECT 'PrevisionsMensuelles',  COUNT(*) FROM "PrevisionsMensuelles"
UNION ALL SELECT 'PrevisionsGlobales',    COUNT(*) FROM "PrevisionsGlobales"
UNION ALL SELECT 'Decaissements',         COUNT(*) FROM "Decaissements"
UNION ALL SELECT 'Materiaux',             COUNT(*) FROM "Materiaux"
UNION ALL SELECT 'Depenses',              COUNT(*) FROM "Depenses"
UNION ALL SELECT 'RapportsTravail',       COUNT(*) FROM "RapportsTravail";

-- =====================================================================
--  CE QUI DOIT RESTER — les compteurs ne doivent PAS être à zéro
-- =====================================================================
SELECT 'Utilisateurs'  AS table_conservee, COUNT(*) AS restant FROM "AspNetUsers"
UNION ALL SELECT 'Catalogue',       COUNT(*) FROM "Catalogue"
UNION ALL SELECT 'Fournisseurs',    COUNT(*) FROM "Fournisseurs"
UNION ALL SELECT 'BudgetsComptes',  COUNT(*) FROM "BudgetsComptes"
UNION ALL SELECT 'Parametres',      COUNT(*) FROM "Parametres";
