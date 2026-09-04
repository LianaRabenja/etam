-- =====================================================================
--  ETAM — Suppression du jeu de DÉMONSTRATION
--
--  Supprime tout ce qui a été créé par demo-inserer.sql, et RIEN d'autre.
--  Chaque suppression est filtrée sur le chantier de code « DEMO-01 » ou
--  sur le fournisseur « DEMO — ... ». Le chantier de Diego, ses enveloppes,
--  ses prévisions et ses utilisateurs ne sont pas concernés.
--
--    psql "postgresql://postgres:root@localhost:5432/etam_erp" -f scripts/demo-effacer.sql
-- =====================================================================

BEGIN;

DO $$
DECLARE
    v_chantier bigint;
BEGIN

SELECT "Id" INTO v_chantier FROM "Chantiers" WHERE "Code" = 'DEMO-01';

IF v_chantier IS NULL THEN
    RAISE NOTICE 'Aucun chantier DEMO-01 : rien a supprimer.';
    RETURN;
END IF;

-- L'ordre suit les dependances : des feuilles vers le chantier.

DELETE FROM "PiecesJointes"
 WHERE "PrevisionJournaliereId" IN (SELECT "Id" FROM "Previsions" WHERE "ChantierId" = v_chantier)
    OR "DecaissementId" IN (
        SELECT d."Id" FROM "Decaissements" d
        JOIN "Previsions" p ON p."Id" = d."PrevisionJournaliereId"
        WHERE p."ChantierId" = v_chantier);

DELETE FROM "Decaissements"
 WHERE "PrevisionJournaliereId" IN (SELECT "Id" FROM "Previsions" WHERE "ChantierId" = v_chantier);

DELETE FROM "Depenses" WHERE "ChantierId" = v_chantier;

DELETE FROM "PrevisionLignes"
 WHERE "PrevisionJournaliereId" IN (SELECT "Id" FROM "Previsions" WHERE "ChantierId" = v_chantier);

DELETE FROM "Previsions" WHERE "ChantierId" = v_chantier;

DELETE FROM "AutresDepensesJour" WHERE "ChantierId" = v_chantier;
DELETE FROM "PlansJournaliers"   WHERE "ChantierId" = v_chantier;

DELETE FROM "PrevisionMensuelleLignes"
 WHERE "PrevisionMensuelleId" IN (SELECT "Id" FROM "PrevisionsMensuelles" WHERE "ChantierId" = v_chantier);

-- Le report d'un mois pointe sur le precedent : on casse le lien avant de supprimer.
UPDATE "PrevisionsMensuelles" SET "PrevisionMensuellePrecedenteId" = NULL
 WHERE "ChantierId" = v_chantier;
DELETE FROM "PrevisionsMensuelles" WHERE "ChantierId" = v_chantier;

DELETE FROM "PrevisionsGlobalesLignes"
 WHERE "PrevisionGlobaleId" IN (SELECT "Id" FROM "PrevisionsGlobales" WHERE "ChantierId" = v_chantier);
DELETE FROM "PrevisionsGlobales" WHERE "ChantierId" = v_chantier;

DELETE FROM "ApprovisionnementLignes"
 WHERE "ApprovisionnementId" IN (SELECT "Id" FROM "Approvisionnements" WHERE "ChantierId" = v_chantier);
DELETE FROM "Approvisionnements" WHERE "ChantierId" = v_chantier;

DELETE FROM "RapportTravailLignesAvancement"
 WHERE "RapportTravailId" IN (SELECT "Id" FROM "RapportsTravail" WHERE "ChantierId" = v_chantier);
DELETE FROM "RapportTravailLignesMateriaux"
 WHERE "RapportTravailId" IN (SELECT "Id" FROM "RapportsTravail" WHERE "ChantierId" = v_chantier);
DELETE FROM "RapportTravailLignesEquipements"
 WHERE "RapportTravailId" IN (SELECT "Id" FROM "RapportsTravail" WHERE "ChantierId" = v_chantier);
DELETE FROM "RapportsTravail" WHERE "ChantierId" = v_chantier;

DELETE FROM "MouvementsMateriau"
 WHERE "MateriauxId" IN (SELECT "Id" FROM "Materiaux" WHERE "ChantierId" = v_chantier);
DELETE FROM "Materiaux" WHERE "ChantierId" = v_chantier;

DELETE FROM "Alertes" WHERE "ChantierId" = v_chantier;

DELETE FROM "MouvementsBancaires"
 WHERE "ChantierId" = v_chantier
    OR "CompteBancaireId" IN (SELECT "Id" FROM "ComptesBancaires" WHERE "ChantierId" = v_chantier);

DELETE FROM "DettesFournisseurs" WHERE "ChantierId" = v_chantier;
DELETE FROM "ComptesBancaires"   WHERE "ChantierId" = v_chantier;

-- Detache d'eventuels utilisateurs rattaches a ce chantier (les comptes restent).
UPDATE "AspNetUsers" SET "ChantierId" = NULL WHERE "ChantierId" = v_chantier;

DELETE FROM "Chantiers" WHERE "Id" = v_chantier;

-- Le fournisseur de demonstration, reconnaissable a son prefixe.
DELETE FROM "DettesFournisseurs"
 WHERE "FournisseurId" IN (SELECT "Id" FROM "Fournisseurs" WHERE "Nom" LIKE 'DEMO —%');
DELETE FROM "Fournisseurs" WHERE "Nom" LIKE 'DEMO —%';

RAISE NOTICE 'Jeu de demonstration supprime.';

END $$;

COMMIT;

-- =====================================================================
--  VÉRIFICATION — doit afficher 0, puis ton chantier de Diego intact
-- =====================================================================
SELECT 'Reste de DEMO (doit etre 0)' AS controle, count(*)::text AS valeur
  FROM "Chantiers" WHERE "Code" = 'DEMO-01'
UNION ALL
SELECT 'Chantiers restants', string_agg("Code" || ' — ' || "Nom", ' | ')
  FROM "Chantiers" WHERE NOT "IsDeleted";
