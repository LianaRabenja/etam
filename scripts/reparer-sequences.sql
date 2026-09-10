-- =====================================================================
--  ETAM — Resynchronisation des compteurs d'identifiants
--
--  POURQUOI
--  Après une restauration de sauvegarde, les lignes arrivent avec leurs
--  identifiants d'origine (Chantier 1, 2, 3...), mais le compteur qui
--  attribue les prochains identifiants, lui, reste à 1. La première
--  insertion réessaie donc l'identifiant 1, qui existe déjà : la base
--  refuse avec « duplicate key value violates unique constraint ».
--
--  Résultat : la lecture fonctionne parfaitement, et TOUT enregistrement
--  échoue, dans tous les menus. C'est exactement le symptôme observé.
--
--  Ce script repositionne chaque compteur juste après le plus grand
--  identifiant existant. Il ne touche à aucune donnée.
--
--  À LANCER SUR LA BASE CONCERNÉE (ici : Render)
--    psql "EXTERNAL_DATABASE_URL" -f scripts/reparer-sequences.sql
-- =====================================================================

DO $$
DECLARE
    r         record;
    v_seq     text;
    v_max     bigint;
    v_repare  int := 0;
BEGIN
    FOR r IN
        SELECT c.table_name
          FROM information_schema.columns c
         WHERE c.table_schema = 'public'
           AND c.column_name  = 'Id'
           AND c.is_identity  = 'YES'
           OR  (c.table_schema = 'public'
                AND c.column_name = 'Id'
                AND c.column_default LIKE 'nextval%')
    LOOP
        v_seq := pg_get_serial_sequence(format('public.%I', r.table_name), 'Id');
        CONTINUE WHEN v_seq IS NULL;

        EXECUTE format('SELECT COALESCE(MAX("Id"), 0) FROM public.%I', r.table_name)
           INTO v_max;

        -- setval avec is_called = true : le prochain identifiant sera v_max + 1.
        PERFORM setval(v_seq, GREATEST(v_max, 1), true);
        v_repare := v_repare + 1;

        RAISE NOTICE 'Compteur % repositionne a % (table %).', v_seq, GREATEST(v_max, 1), r.table_name;
    END LOOP;

    RAISE NOTICE '--- % compteurs resynchronises. ---', v_repare;
END $$;

-- =====================================================================
--  VÉRIFICATION — « prochain » doit être supérieur à « max_id »
-- =====================================================================
SELECT 'Chantiers' AS table_,
       (SELECT COALESCE(MAX("Id"),0) FROM "Chantiers")            AS max_id,
       (SELECT last_value FROM pg_sequences
         WHERE schemaname='public' AND sequencename LIKE 'Chantiers%' LIMIT 1) AS compteur
UNION ALL
SELECT 'Previsions',
       (SELECT COALESCE(MAX("Id"),0) FROM "Previsions"),
       (SELECT last_value FROM pg_sequences
         WHERE schemaname='public' AND sequencename LIKE 'Previsions_%' LIMIT 1)
UNION ALL
SELECT 'MouvementsBancaires',
       (SELECT COALESCE(MAX("Id"),0) FROM "MouvementsBancaires"),
       (SELECT last_value FROM pg_sequences
         WHERE schemaname='public' AND sequencename LIKE 'MouvementsBancaires%' LIMIT 1);
