-- =====================================================================
--  ETAM — Test de diagnostic : la base accepte-t-elle une insertion ?
--
--  Tente d'insérer une enveloppe mensuelle, puis ANNULE tout.
--  Rien n'est écrit durablement : c'est un test, pas une modification.
--
--    psql "URL_RENDER" -f scripts/test-insertion.sql
--
--  RÉSULTAT ATTENDU : « INSERT 0 1 » puis « ROLLBACK ».
--  Toute autre sortie est le message d'erreur qu'il me faut.
-- =====================================================================

BEGIN;

INSERT INTO "PrevisionsMensuelles"
    ("ChantierId","Annee","Mois","Reference","MontantPrevu",
     "ReportMoisPrecedent","MontantConsomme","Statut","CreatedAt","IsDeleted")
SELECT "Id", 2026, 7, 'TEST-DIAG', 1, 0, 0, 0, now(), false
  FROM "Chantiers"
 WHERE "Code" = 'DIEG';

-- Le journal d'audit est écrit à CHAQUE enregistrement par l'application :
-- si son compteur est cassé, tout est bloqué. On le teste aussi.
INSERT INTO "AuditLogs"
    ("Action","Entite","CleEntite","UtilisateurNom","DateAction","CreatedAt","IsDeleted")
VALUES (2, 'TestDiagnostic', '0', 'diagnostic', now(), now(), false);

ROLLBACK;

-- =====================================================================
--  Si le script s'arrête plus haut, le message d'erreur est la réponse.
--  S'il arrive ici, la base est saine et le problème vient de
--  l'application (jeton de sécurité, session, clé de chiffrement).
-- =====================================================================
SELECT 'La base accepte les insertions.' AS resultat;
