-- =====================================================================
--  ETAM — Jeu de données de DÉMONSTRATION
--
--  Crée un chantier « DEMO-01 » complet, pour voir à quoi ressemblent les
--  écrans une fois remplis : banque, plan du projet, enveloppes mensuelles,
--  plans de la semaine, prévisions journalières à tous les stades du
--  workflow, sorties d'argent, stock, dépenses, fournisseur, dette, alertes.
--
--  TOUT est rattaché au chantier de code « DEMO-01 » et au fournisseur dont
--  le nom commence par « DEMO — ». Le script demo-effacer.sql supprime
--  exactement ça, et rien d'autre : ton chantier de Diego n'est pas touché.
--
--  À LANCER SUR LA BASE LOCALE UNIQUEMENT.
--    psql "postgresql://postgres:root@localhost:5432/etam_erp" -f scripts/demo-inserer.sql
-- =====================================================================

BEGIN;

DO $$
DECLARE
    v_chantier   bigint;
    v_compte     bigint;
    v_plan       bigint;
    v_l_appro    bigint;
    v_l_mo       bigint;
    v_l_transp   bigint;
    v_env_juil   bigint;
    v_env_aout   bigint;
    v_pj1        bigint;
    v_pj2        bigint;
    v_prev1      bigint;
    v_prev2      bigint;
    v_prev3      bigint;
    v_prev4      bigint;
    v_ligne1     bigint;
    v_ligne2     bigint;
    v_ciment     bigint;
    v_fer        bigint;
    v_gasoil     bigint;
    v_fourn      bigint;
    v_dette      bigint;
BEGIN

-- ---------------------------------------------------------------------
-- 1. LE CHANTIER
--    Marché 500 M − bénéfice 200 M = budget projet 300 M.
-- ---------------------------------------------------------------------
INSERT INTO "Chantiers"
    ("Nom","Code","Localisation","Responsable","DateDebut","DateFin","Statut",
     "MontantMarche","Benefice","BudgetMateriel","Reserve","ReserveUtilisee",
     "Consommation","MaterielTransfere","PourcentageAvancement","Observation",
     "CreatedAt","IsDeleted")
VALUES
    ('DEMO — Adduction d''eau Antsirabe','DEMO-01','Antsirabe','RAKOTOARISOA Hery',
     '2026-07-01','2026-12-31',1,
     500000000, 200000000, 300000000, 0, 0,
     17500000, 100000000, 35, 'Jeu de démonstration — à supprimer',
     now(),false)
RETURNING "Id" INTO v_chantier;

-- ---------------------------------------------------------------------
-- 2. LA BANQUE
--    150 M encaissés, 100 M fléchés vers le Budget Matériel,
--    20 M retirés par les exécutions de prévision → solde 30 M.
-- ---------------------------------------------------------------------
INSERT INTO "ComptesBancaires"
    ("Nom","Banque","Numero","Devise","Solde","EstActif","Type","ChantierId","CreatedAt","IsDeleted")
VALUES
    ('DEMO — Compte Antsirabe','BNI Madagascar','000410 0086 90','Ar',
     30000000,true,0,v_chantier,now(),false)
RETURNING "Id" INTO v_compte;

INSERT INTO "MouvementsBancaires"
    ("CompteBancaireId","Date","Type","Montant","Beneficiaire","Motif","Reference",
     "EstValide","ChantierId","CreatedAt","IsDeleted")
VALUES
    (v_compte,'2026-07-02',0,150000000,NULL,'Avance de démarrage 30 %','ENC-DEMO-01',true,v_chantier,now(),false),
    (v_compte,'2026-07-05',2,100000000,NULL,'Transfert vers le Budget Matériel','TRANSF-DEMO-01',true,v_chantier,now(),false),
    (v_compte,'2026-07-28',1, 12000000,'RAKOTOARISOA Hery','PREV-DEMO-20260728 — prévision du jour','RET-DEMO-01',true,v_chantier,now(),false),
    (v_compte,'2026-08-26',1,  8000000,'RAKOTOARISOA Hery','PREV-DEMO-20260826 — prévision du jour','RET-DEMO-02',true,v_chantier,now(),false);

-- ---------------------------------------------------------------------
-- 3. LE PLAN DU PROJET — total exactement 300 000 000
-- ---------------------------------------------------------------------
INSERT INTO "PrevisionsGlobales"
    ("ChantierId","Reference","DateCreation","Statut","Observation",
     "SoumisePar","DateSoumission","DateValidationRf","DateValidationAdmin","DateMiseEnBanque",
     "CreatedAt","IsDeleted")
VALUES
    (v_chantier,'PGLOB-DEMO-01','2026-07-03',4,'Plan de démonstration',
     'admin@etam.mg','2026-07-03','2026-07-03','2026-07-04','2026-07-04',
     now(),false)
RETURNING "Id" INTO v_plan;

INSERT INTO "PrevisionsGlobalesLignes"
    ("PrevisionGlobaleId","Rubrique","Designation","Unite","Quantite","PrixUnitaire","CreatedAt","IsDeleted")
VALUES (v_plan,'Approvisionnement','Ciment CEM II','t',100,750000,now(),false)
RETURNING "Id" INTO v_l_appro;

INSERT INTO "PrevisionsGlobalesLignes"
    ("PrevisionGlobaleId","Rubrique","Designation","Unite","Quantite","PrixUnitaire","CreatedAt","IsDeleted")
VALUES (v_plan,'Main d''œuvre','Maçons','jour',1200,30000,now(),false)
RETURNING "Id" INTO v_l_mo;

INSERT INTO "PrevisionsGlobalesLignes"
    ("PrevisionGlobaleId","Rubrique","Designation","Unite","Quantite","PrixUnitaire","CreatedAt","IsDeleted")
VALUES (v_plan,'Transport','Location camion','mois',6,8000000,now(),false)
RETURNING "Id" INTO v_l_transp;

INSERT INTO "PrevisionsGlobalesLignes"
    ("PrevisionGlobaleId","Rubrique","Designation","Unite","Quantite","PrixUnitaire","CreatedAt","IsDeleted")
VALUES
    (v_plan,'Approvisionnement','Fer à béton Ø12','barre',500,45000,now(),false),
    (v_plan,'Main d''œuvre','Chef d''équipe','jour',120,60000,now(),false),
    (v_plan,'Carburant','Gasoil','litre',12000,5500,now(),false),
    (v_plan,'Sous-traitance','Forage et équipement','forfait',1,30300000,now(),false),
    (v_plan,'Divers','Imprévus','forfait',1,15000000,now(),false);

-- ---------------------------------------------------------------------
-- 4. LES ENVELOPPES MENSUELLES
--    Juillet clôturée (reliquat reporté), août ouverte.
--    150 + 150 = 300 M, soit exactement le budget projet.
-- ---------------------------------------------------------------------
INSERT INTO "PrevisionsMensuelles"
    ("ChantierId","PrevisionGlobaleId","Annee","Mois","Reference","MontantPrevu",
     "ReportMoisPrecedent","MontantConsomme","Statut","SoumisePar","DateSoumission",
     "DateValidation","DateCloture","Observation","CreatedAt","IsDeleted")
VALUES
    (v_chantier,v_plan,2026,7,'PMENS-DEMO-01-202607',150000000,
     0,9500000,2,'admin@etam.mg','2026-07-05','2026-07-05','2026-07-31',
     'Mois clôturé — reliquat reporté sur août',now(),false)
RETURNING "Id" INTO v_env_juil;

INSERT INTO "PrevisionsMensuelles"
    ("ChantierId","PrevisionGlobaleId","Annee","Mois","Reference","MontantPrevu",
     "ReportMoisPrecedent","MontantConsomme","PrevisionMensuellePrecedenteId","Statut",
     "SoumisePar","DateSoumission","DateValidation","Observation","CreatedAt","IsDeleted")
VALUES
    (v_chantier,v_plan,2026,8,'PMENS-DEMO-01-202608',150000000,
     140500000,8000000,v_env_juil,1,
     'admin@etam.mg','2026-08-01','2026-08-01','Mois en cours',now(),false)
RETURNING "Id" INTO v_env_aout;

INSERT INTO "PrevisionMensuelleLignes"
    ("PrevisionMensuelleId","Rubrique","Designation","Montant","CreatedAt","IsDeleted")
VALUES
    (v_env_juil,'Approvisionnement','Ciment et fer',60000000,now(),false),
    (v_env_juil,'Main d''œuvre','Équipes de maçons',40000000,now(),false),
    (v_env_juil,'Transport','Camion et carburant',30000000,now(),false),
    (v_env_juil,'Divers','Imprévus du mois',20000000,now(),false),
    (v_env_aout,'Approvisionnement','Ciment et fer',65000000,now(),false),
    (v_env_aout,'Main d''œuvre','Équipes de maçons',45000000,now(),false),
    (v_env_aout,'Transport','Camion et carburant',25000000,now(),false),
    (v_env_aout,'Divers','Imprévus du mois',15000000,now(),false);

-- ---------------------------------------------------------------------
-- 5. LES PLANS DE LA SEMAINE
-- ---------------------------------------------------------------------
INSERT INTO "PlansJournaliers"
    ("PrevisionMensuelleId","ChantierId","Date","MontantPrevu","Observation","CreatedAt","IsDeleted")
VALUES (v_env_juil,v_chantier,'2026-07-28',12000000,'Coulage dalle niveau 1',now(),false)
RETURNING "Id" INTO v_pj1;

INSERT INTO "PlansJournaliers"
    ("PrevisionMensuelleId","ChantierId","Date","MontantPrevu","Observation","CreatedAt","IsDeleted")
VALUES (v_env_aout,v_chantier,'2026-08-26',8000000,'Pose canalisation secteur nord',now(),false)
RETURNING "Id" INTO v_pj2;

INSERT INTO "PlansJournaliers"
    ("PrevisionMensuelleId","ChantierId","Date","MontantPrevu","Observation","CreatedAt","IsDeleted")
VALUES
    (v_env_aout,v_chantier,'2026-08-27',3000000,'Reprise des joints',now(),false),
    (v_env_aout,v_chantier,'2026-08-28',1500000,'Nettoyage et repli partiel',now(),false);

-- ---------------------------------------------------------------------
-- 6. LES MATÉRIAUX (stock)
-- ---------------------------------------------------------------------
INSERT INTO "Materiaux"
    ("ChantierId","Categorie","Designation","Unite","Localite","Besoin",
     "QuantiteCommandee","QuantiteRecue","QuantiteUtilisee","SeuilMinimal","PrixUnitaire",
     "CreatedAt","IsDeleted")
VALUES (v_chantier,'Gros œuvre','Ciment CEM II','t','Dépôt Antsirabe',100,60,45,38,10,750000,now(),false)
RETURNING "Id" INTO v_ciment;

INSERT INTO "Materiaux"
    ("ChantierId","Categorie","Designation","Unite","Localite","Besoin",
     "QuantiteCommandee","QuantiteRecue","QuantiteUtilisee","SeuilMinimal","PrixUnitaire",
     "CreatedAt","IsDeleted")
VALUES (v_chantier,'Ferraillage','Fer à béton Ø12','barre','Dépôt Antsirabe',500,300,300,285,50,45000,now(),false)
RETURNING "Id" INTO v_fer;

INSERT INTO "Materiaux"
    ("ChantierId","Categorie","Designation","Unite","Localite","Besoin",
     "QuantiteCommandee","QuantiteRecue","QuantiteUtilisee","SeuilMinimal","PrixUnitaire",
     "CreatedAt","IsDeleted")
VALUES (v_chantier,'Carburant','Gasoil','litre','Cuve chantier',12000,4000,4000,3950,500,5500,now(),false)
RETURNING "Id" INTO v_gasoil;

INSERT INTO "MouvementsMateriau"
    ("MateriauxId","DateMouvement","BesoinOuObjectif","QuantiteEntree","QuantiteSortie",
     "Motif","SoldeSurBesoin","SoldeEnStock","CreatedAt","IsDeleted")
VALUES
    (v_ciment,'2026-07-10','Coulage fondations',45,0,'Livraison fournisseur',55,45,now(),false),
    (v_ciment,'2026-07-28','Coulage dalle',0,38,'Sortie chantier',55,7,now(),false),
    (v_fer,'2026-07-12','Ferraillage poteaux',300,0,'Livraison fournisseur',200,300,now(),false),
    (v_fer,'2026-08-26','Ferraillage dalle',0,285,'Sortie chantier',200,15,now(),false),
    (v_gasoil,'2026-08-01','Groupe et camion',4000,0,'Plein cuve',8000,4000,now(),false),
    (v_gasoil,'2026-08-26','Groupe et camion',0,3950,'Consommation du mois',8000,50,now(),false);

-- ---------------------------------------------------------------------
-- 7. LES PRÉVISIONS JOURNALIÈRES — un exemplaire à chaque stade
-- ---------------------------------------------------------------------

-- 7a. Clôturée : exécutée, justifiée, réceptionnée. Reliquat 2 500 000.
INSERT INTO "Previsions"
    ("ChantierId","PrevisionMensuelleId","PlanJournalierId","DatePrevision","Reference","Statut",
     "SoumisePar","DateSoumission","DateValidationRf","DateValidationAdmin","DateExecution",
     "RapportRealisation","DateRapport","DateValidationRapport",
     "DateAccuseReception","MontantAccuse","AccuseNomSignataire",
     "ReportVeille","MontantDecaisse","Observation","CreatedAt","IsDeleted")
VALUES
    (v_chantier,v_env_juil,v_pj1,'2026-07-28','PREV-DEMO-20260728-1001',7,
     'chef@etam.mg','2026-07-27','2026-07-27','2026-07-28','2026-07-28',
     'Dalle coulée sur 180 m². Deux camions de sable non utilisés.','2026-07-29','2026-07-30',
     '2026-07-28',12000000,'RAKOTOARISOA Hery',
     0,9500000,'Journée complète',now(),false)
RETURNING "Id" INTO v_prev1;

INSERT INTO "PrevisionLignes"
    ("PrevisionJournaliereId","Designation","Categorie","TypeBudget","MateriauId",
     "PrevisionGlobaleLigneId","Quantite","PrixUnitaireEstime","CreatedAt","IsDeleted")
VALUES (v_prev1,'Ciment CEM II','Gros œuvre',1,v_ciment,v_l_appro,8,750000,now(),false)
RETURNING "Id" INTO v_ligne1;

INSERT INTO "PrevisionLignes"
    ("PrevisionJournaliereId","Designation","Categorie","TypeBudget","MateriauId",
     "PrevisionGlobaleLigneId","Quantite","PrixUnitaireEstime","CreatedAt","IsDeleted")
VALUES (v_prev1,'Maçons','Main d''œuvre',1,NULL,v_l_mo,120,30000,now(),false)
RETURNING "Id" INTO v_ligne2;

INSERT INTO "PrevisionLignes"
    ("PrevisionJournaliereId","Designation","Categorie","TypeBudget","MateriauId",
     "PrevisionGlobaleLigneId","Quantite","PrixUnitaireEstime","CreatedAt","IsDeleted")
VALUES (v_prev1,'Location camion','Transport',1,NULL,v_l_transp,1,2400000,now(),false);

-- 7b. Exécutée : argent sorti, réception signée, décaissements faits.
INSERT INTO "Previsions"
    ("ChantierId","PrevisionMensuelleId","PlanJournalierId","DatePrevision","Reference","Statut",
     "SoumisePar","DateSoumission","DateValidationRf","DateValidationAdmin","DateExecution",
     "DateAccuseReception","MontantAccuse","AccuseNomSignataire",
     "ReportVeille","PrevisionPrecedenteId","MontantDecaisse","Observation","CreatedAt","IsDeleted")
VALUES
    (v_chantier,v_env_aout,v_pj2,'2026-08-26','PREV-DEMO-20260826-1002',4,
     'chef@etam.mg','2026-08-25','2026-08-25','2026-08-26','2026-08-26',
     '2026-08-26',10500000,'RAKOTOARISOA Hery',
     2500000,v_prev1,8000000,'Pose canalisation',now(),false)
RETURNING "Id" INTO v_prev2;

INSERT INTO "PrevisionLignes"
    ("PrevisionJournaliereId","Designation","Categorie","TypeBudget","MateriauId",
     "PrevisionGlobaleLigneId","Quantite","PrixUnitaireEstime","CreatedAt","IsDeleted")
VALUES
    (v_prev2,'Fer à béton Ø12','Ferraillage',1,v_fer,NULL,100,45000,now(),false),
    (v_prev2,'Gasoil','Carburant',1,v_gasoil,NULL,500,5500,now(),false),
    (v_prev2,'Maçons','Main d''œuvre',1,NULL,v_l_mo,25,30000,now(),false);

-- 7c. Soumise : en attente de validation.
INSERT INTO "Previsions"
    ("ChantierId","PrevisionMensuelleId","DatePrevision","Reference","Statut",
     "SoumisePar","DateSoumission","ReportVeille","MontantDecaisse","Observation","CreatedAt","IsDeleted")
VALUES
    (v_chantier,v_env_aout,'2026-08-27','PREV-DEMO-20260827-1003',1,
     'chef@etam.mg','2026-08-26',0,0,'En attente du Correspondant',now(),false)
RETURNING "Id" INTO v_prev3;

INSERT INTO "PrevisionLignes"
    ("PrevisionJournaliereId","Designation","Categorie","TypeBudget",
     "Quantite","PrixUnitaireEstime","CreatedAt","IsDeleted")
VALUES
    (v_prev3,'Maçons','Main d''œuvre',1,60,30000,now(),false),
    (v_prev3,'Sable','Gros œuvre',1,6,200000,now(),false);

-- 7d. Brouillon : en cours de saisie.
INSERT INTO "Previsions"
    ("ChantierId","PrevisionMensuelleId","DatePrevision","Reference","Statut",
     "ReportVeille","MontantDecaisse","Observation","CreatedAt","IsDeleted")
VALUES
    (v_chantier,v_env_aout,'2026-08-28','PREV-DEMO-20260828-1004',0,
     0,0,'Brouillon du chef de chantier',now(),false)
RETURNING "Id" INTO v_prev4;

INSERT INTO "PrevisionLignes"
    ("PrevisionJournaliereId","Designation","Categorie","TypeBudget",
     "Quantite","PrixUnitaireEstime","CreatedAt","IsDeleted")
VALUES (v_prev4,'Nettoyage et repli','Divers',1,1,1500000,now(),false);

-- ---------------------------------------------------------------------
-- 8. LES SORTIES D'ARGENT — total 17 500 000
-- ---------------------------------------------------------------------
INSERT INTO "Decaissements"
    ("PrevisionJournaliereId","PrevisionLigneId","Date","Beneficiaire","Motif","Montant",
     "Mode","CompteBancaireId","BudgetConcerne","Reference","AccuseNom","DateAccuse",
     "CreatedAt","IsDeleted")
VALUES
    (v_prev1,v_ligne1,'2026-07-28','SOAMIARY Ets','Achat 8 t de ciment',5500000,
     0,v_compte,1,'DEC-DEMO-001','RAKOTO Jean','2026-07-28',now(),false),
    (v_prev1,v_ligne2,'2026-07-28','Équipe maçons','Paie de la journée',4000000,
     0,v_compte,1,'DEC-DEMO-002','RAKOTO Jean','2026-07-28',now(),false),
    (v_prev2,NULL,'2026-08-26','TSIRIRY Quincaillerie','Fer à béton et gasoil',8000000,
     2,v_compte,1,'DEC-DEMO-003','RAKOTOARISOA Hery','2026-08-26',now(),false);

-- ---------------------------------------------------------------------
-- 9. LES DÉPENSES (journal)
-- ---------------------------------------------------------------------
INSERT INTO "Depenses"
    ("Date","ChantierId","PrevisionJournaliereId","Categorie","Designation",
     "Quantite","PrixUnitaire","BudgetConcerne","Justificatif","Observation","CreatedAt","IsDeleted")
VALUES
    ('2026-07-28',v_chantier,v_prev1,'Gros œuvre','Ciment CEM II',8,750000,1,'Facture SOAM-4471',NULL,now(),false),
    ('2026-07-28',v_chantier,v_prev1,'Main d''œuvre','Paie maçons',120,30000,1,'État de paie 07-28',NULL,now(),false),
    ('2026-08-26',v_chantier,v_prev2,'Ferraillage','Fer à béton Ø12',100,45000,1,'Facture TSI-8812',NULL,now(),false),
    ('2026-08-10',v_chantier,NULL,'Frais généraux','Fournitures de bureau chantier',1,320000,0,'Reçu 2210',NULL,now(),false);

-- ---------------------------------------------------------------------
-- 10. AUTRES DÉPENSES DU JOUR
-- ---------------------------------------------------------------------
INSERT INTO "AutresDepensesJour"
    ("Date","Libelle","Montant","Ordre","ChantierId","Observation","CreatedAt","IsDeleted")
VALUES
    ('2026-08-26','Frais de mission chef de chantier',150000,1,v_chantier,NULL,now(),false),
    ('2026-08-26','Communication et internet',80000,2,v_chantier,NULL,now(),false),
    ('2026-08-27','Location groupe électrogène',450000,1,v_chantier,NULL,now(),false);

-- ---------------------------------------------------------------------
-- 11. FOURNISSEUR ET DETTE
-- ---------------------------------------------------------------------
INSERT INTO "Fournisseurs"
    ("Nom","Contact","Telephone","Adresse","Nif","CreatedAt","IsDeleted")
VALUES ('DEMO — SOAMIARY Ets','RANDRIA Paul','034 12 345 67','Antsirabe','1234567890',now(),false)
RETURNING "Id" INTO v_fourn;

INSERT INTO "DettesFournisseurs"
    ("FournisseurId","ChantierId","Libelle","MontantInitial","MontantPaye","DateEcheance",
     "Statut","CreatedAt","IsDeleted")
VALUES (v_fourn,v_chantier,'Livraison ciment juillet',12000000,5500000,'2026-09-30',1,now(),false)
RETURNING "Id" INTO v_dette;

-- ---------------------------------------------------------------------
-- 12. ALERTES
-- ---------------------------------------------------------------------
INSERT INTO "Alertes"
    ("Type","Niveau","Titre","Message","ChantierId","EstLue","CreatedAt","IsDeleted")
VALUES
    (3,1,'Stock faible : Ciment CEM II',
     'Il reste 7 t en stock pour un seuil minimal de 10 t sur DEMO — Adduction d''eau Antsirabe.',
     v_chantier,false,now(),false),
    (4,2,'Stock critique : Gasoil',
     'Il reste 50 litres pour un seuil de 500 litres. Réapprovisionnement urgent.',
     v_chantier,false,now(),false),
    (6,0,'Prévision en attente de validation',
     'La prévision PREV-DEMO-20260827-1003 attend la validation du Correspondant.',
     v_chantier,false,now(),false);

RAISE NOTICE 'Jeu de démonstration créé : chantier % (DEMO-01).', v_chantier;

END $$;

COMMIT;

-- =====================================================================
--  VÉRIFICATION
-- =====================================================================
SELECT 'Chantiers DEMO'  AS objet, count(*) AS n FROM "Chantiers" WHERE "Code"='DEMO-01'
UNION ALL SELECT 'Comptes',      count(*) FROM "ComptesBancaires" b JOIN "Chantiers" c ON c."Id"=b."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Mouvements',   count(*) FROM "MouvementsBancaires" m JOIN "Chantiers" c ON c."Id"=m."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Plan lignes',  count(*) FROM "PrevisionsGlobalesLignes" l JOIN "PrevisionsGlobales" g ON g."Id"=l."PrevisionGlobaleId" JOIN "Chantiers" c ON c."Id"=g."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Enveloppes',   count(*) FROM "PrevisionsMensuelles" p JOIN "Chantiers" c ON c."Id"=p."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Plans jour',   count(*) FROM "PlansJournaliers" p JOIN "Chantiers" c ON c."Id"=p."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Previsions',   count(*) FROM "Previsions" p JOIN "Chantiers" c ON c."Id"=p."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Decaissements',count(*) FROM "Decaissements" d JOIN "Previsions" p ON p."Id"=d."PrevisionJournaliereId" JOIN "Chantiers" c ON c."Id"=p."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Materiaux',    count(*) FROM "Materiaux" m JOIN "Chantiers" c ON c."Id"=m."ChantierId" WHERE c."Code"='DEMO-01'
UNION ALL SELECT 'Alertes',      count(*) FROM "Alertes" a JOIN "Chantiers" c ON c."Id"=a."ChantierId" WHERE c."Code"='DEMO-01';
