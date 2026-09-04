-- =====================================================================
--  ETAM — Réparation d'une base restaurée depuis une sauvegarde
--
--  SYMPTÔME TRAITÉ
--  Toutes les pages s'affichent, mais AUCUN enregistrement ne passe :
--  chaque formulaire renvoie « Une erreur est survenue », dans tous les
--  menus. Deux causes possibles, traitées toutes les deux ici.
--
--  CAUSE 1 — Les compteurs d'identifiants
--  Après restauration, les lignes ont leurs identifiants d'origine mais
--  les compteurs sont restés à 1. La base refuse alors toute insertion
--  (« duplicate key »). Et comme CHAQUE enregistrement écrit aussi une
--  ligne dans le journal d'audit, un seul compteur cassé suffit à
--  bloquer l'application entière — d'où le symptôme sur tous les menus.
--
--  CAUSE 2 — Les clés de chiffrement
--  La table DataProtectionKeys signe les cookies de connexion et les
--  jetons anti-CSRF des formulaires. Des clés venues d'une autre machine
--  font échouer la vérification de tous les POST — donc de tous les
--  enregistrements — sans gêner l'affichage. On les vide : l'application
--  en régénère au démarrage suivant.
--
--  À LANCER SUR LA BASE EN LIGNE
--    psql "EXTERNAL_DATABASE_URL" -f scripts/reparer-en-ligne.sql
-- =====================================================================

-- ---------------------------------------------------------------------
--  1. Resynchroniser tous les compteurs d'identifiants
-- ---------------------------------------------------------------------
DO $$
DECLARE
    r        record;
    v_seq    text;
    v_max    bigint;
    v_n      int := 0;
BEGIN
    FOR r IN
        SELECT DISTINCT c.table_name
          FROM information_schema.columns c
          JOIN information_schema.tables t
            ON t.table_schema = c.table_schema
           AND t.table_name   = c.table_name
           AND t.table_type   = 'BASE TABLE'
         WHERE c.table_schema = 'public'
           AND c.column_name  = 'Id'
    LOOP
        v_seq := pg_get_serial_sequence(format('public.%I', r.table_name), 'Id');
        CONTINUE WHEN v_seq IS NULL;   -- clé texte (tables Identity) : pas de compteur

        EXECUTE format('SELECT COALESCE(MAX("Id"), 0) FROM public.%I', r.table_name)
           INTO v_max;

        PERFORM setval(v_seq, GREATEST(v_max, 1), true);
        v_n := v_n + 1;
        RAISE NOTICE '  % -> prochain identifiant %', rpad(r.table_name, 34), v_max + 1;
    END LOOP;

    RAISE NOTICE '=== % compteurs resynchronises ===', v_n;
END $$;

-- ---------------------------------------------------------------------
--  2. Repartir sur des clés de chiffrement propres
--     Conséquence attendue : tout le monde est déconnecté et devra se
--     reconnecter. C'est normal, et c'est le prix à payer une seule fois.
-- ---------------------------------------------------------------------
DELETE FROM "DataProtectionKeys";

-- =====================================================================
--  VÉRIFICATION
-- =====================================================================
SELECT 'Cles de chiffrement (doit etre 0)' AS controle,
       (SELECT count(*)::text FROM "DataProtectionKeys") AS valeur
UNION ALL
SELECT 'Journal d''audit : max Id',
       (SELECT COALESCE(MAX("Id"),0)::text FROM "AuditLogs")
UNION ALL
SELECT 'Journal d''audit : prochain Id',
       (SELECT (last_value + CASE WHEN is_called THEN 1 ELSE 0 END)::text
          FROM pg_sequences
         WHERE schemaname = 'public' AND sequencename LIKE 'AuditLogs%' LIMIT 1)
UNION ALL
SELECT 'Chantiers : max Id',
       (SELECT COALESCE(MAX("Id"),0)::text FROM "Chantiers")
UNION ALL
SELECT 'Chantiers : prochain Id',
       (SELECT (last_value + CASE WHEN is_called THEN 1 ELSE 0 END)::text
          FROM pg_sequences
         WHERE schemaname = 'public' AND sequencename LIKE 'Chantiers%' LIMIT 1);
