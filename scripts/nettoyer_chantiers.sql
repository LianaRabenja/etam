-- =====================================================================
--  ETAM — Remise à zéro des données d'exploitation
--  Mis à jour pour le schéma du 13/08/2026 (PlansJournaliers, AutresDepensesJour)
-- =====================================================================
--
--  CE QUI EST EFFACÉ
--    chantiers ; TOUS les comptes bancaires et leurs mouvements ;
--    plans de projet, enveloppes mensuelles, plans journaliers,
--    prévisions journalières, sorties d'argent (décaissements),
--    autres dépenses du jour, dépenses, approvisionnements,
--    matériaux et leurs mouvements, rapports de travail,
--    pièces jointes, alertes, dettes fournisseurs.
--
--  CE QUI EST CONSERVÉ
--    utilisateurs et rôles, catalogue des prix, fournisseurs,
--    budget du bureau (compteurs remis à zéro), paramètres,
--    journal d'audit, clés de chiffrement.
--
--  À LANCER AVEC (base locale de développement)
--    psql "postgresql://postgres:root@localhost:5432/etam_erp" -f scripts/nettoyer_chantiers.sql
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

-- 3. Prévisions journalières, puis les plans journaliers qui les portent.
--    Previsions référence PlansJournaliers : on vide les prévisions d'abord.
DELETE FROM "PrevisionLignes";
DELETE FROM "Previsions";
DELETE FROM "AutresDepensesJour";
DELETE FROM "PlansJournaliers";

-- 4. Enveloppes mensuelles (référencées par PlansJournaliers, vidés ci-dessus)
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

-- 10. Dettes fournisseurs (les fournisseurs eux-mêmes sont conservés)
DELETE FROM "DettesFournisseurs";

-- 11. Banque : mouvements puis TOUS les comptes.
--     Vous ressaisirez vos comptes réels après le nettoyage.
DELETE FROM "MouvementsBancaires";
DELETE FROM "ComptesBancaires";

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

-- 15. Les identifiants repartent à 1 sur les tables vidées.
--     Purement cosmétique : TUL-01 aura l'Id 1 et non l'Id 47.
DO $$
DECLARE
    t    text;
    seq  text;
BEGIN
    FOREACH t IN ARRAY ARRAY[
        'Chantiers','ComptesBancaires','MouvementsBancaires','Previsions',
        'PrevisionLignes','PlansJournaliers','AutresDepensesJour',
        'PrevisionsMensuelles','PrevisionMensuelleLignes',
        'PrevisionsGlobales','PrevisionsGlobalesLignes',
        'Decaissements','PiecesJointes','Depenses','Alertes',
        'Materiaux','MouvementsMateriau','Approvisionnements',
        'ApprovisionnementLignes','RapportsTravail','DettesFournisseurs'
    ]
    LOOP
        seq := pg_get_serial_sequence(format('public.%I', t), 'Id');
        IF seq IS NOT NULL THEN
            EXECUTE format('ALTER SEQUENCE %s RESTART WITH 1', seq);
        END IF;
    END LOOP;
END $$;

COMMIT;

-- =====================================================================
--  VÉRIFICATION — doit afficher 0 partout dans la colonne « restant »
-- =====================================================================
SELECT 'Chantiers'            AS table_effacee, COUNT(*) AS restant FROM "Chantiers"
UNION ALL SELECT 'ComptesBancaires',      COUNT(*) FROM "ComptesBancaires"
UNION ALL SELECT 'MouvementsBancaires',   COUNT(*) FROM "MouvementsBancaires"
UNION ALL SELECT 'Previsions',            COUNT(*) FROM "Previsions"
UNION ALL SELECT 'PlansJournaliers',      COUNT(*) FROM "PlansJournaliers"
UNION ALL SELECT 'AutresDepensesJour',    COUNT(*) FROM "AutresDepensesJour"
UNION ALL SELECT 'PrevisionsMensuelles',  COUNT(*) FROM "PrevisionsMensuelles"
UNION ALL SELECT 'PrevisionsGlobales',    COUNT(*) FROM "PrevisionsGlobales"
UNION ALL SELECT 'Decaissements',         COUNT(*) FROM "Decaissements"
UNION ALL SELECT 'Materiaux',             COUNT(*) FROM "Materiaux"
UNION ALL SELECT 'Depenses',              COUNT(*) FROM "Depenses"
UNION ALL SELECT 'DettesFournisseurs',    COUNT(*) FROM "DettesFournisseurs"
UNION ALL SELECT 'RapportsTravail',       COUNT(*) FROM "RapportsTravail";

-- =====================================================================
--  CE QUI DOIT RESTER — les compteurs ne doivent PAS être à zéro
-- =====================================================================
SELECT 'Utilisateurs'  AS table_conservee, COUNT(*) AS restant FROM "AspNetUsers"
UNION ALL SELECT 'Catalogue',       COUNT(*) FROM "Catalogue"
UNION ALL SELECT 'Fournisseurs',    COUNT(*) FROM "Fournisseurs"
UNION ALL SELECT 'BudgetsComptes',  COUNT(*) FROM "BudgetsComptes"
UNION ALL SELECT 'Parametres',      COUNT(*) FROM "Parametres";
