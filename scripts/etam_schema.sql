-- =====================================================================
--  ETAM ERP - Schéma PostgreSQL (référence / secours)
--  NOTE : la méthode recommandée reste la migration EF Core
--         (voir README, section « Base de données »). Ce script reflète
--         le modèle de domaine pour un déploiement manuel éventuel.
--  Les colonnes système d'audit ASP.NET Identity ne sont PAS incluses ici :
--         elles sont créées par la migration EF Core.
-- =====================================================================

CREATE TABLE IF NOT EXISTS "Chantiers" (
    "Id"                    BIGSERIAL PRIMARY KEY,
    "Nom"                   VARCHAR(150) NOT NULL,
    "Code"                  VARCHAR(30)  NOT NULL,
    "Localisation"          VARCHAR(150),
    "Responsable"           VARCHAR(120),
    "DateDebut"             TIMESTAMPTZ  NOT NULL,
    "DateFin"               TIMESTAMPTZ,
    "Statut"                INT NOT NULL DEFAULT 0,
    "BudgetMateriel"        NUMERIC(18,2) NOT NULL DEFAULT 0,
    "Reserve"               NUMERIC(18,2) NOT NULL DEFAULT 0,
    "ReserveUtilisee"       NUMERIC(18,2) NOT NULL DEFAULT 0,
    "Consommation"          NUMERIC(18,2) NOT NULL DEFAULT 0,
    "PourcentageAvancement" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Observation"           VARCHAR(1000),
    "CreatedAt"             TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"             TIMESTAMPTZ,
    "CreatedBy"             VARCHAR(150),
    "UpdatedBy"             VARCHAR(150),
    "IsDeleted"             BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Chantiers_Code" ON "Chantiers"("Code");

CREATE TABLE IF NOT EXISTS "BudgetsComptes" (
    "Id"              BIGSERIAL PRIMARY KEY,
    "Annee"           INT NOT NULL,
    "Libelle"         VARCHAR(120) NOT NULL,
    "MontantInitial"  NUMERIC(18,2) NOT NULL DEFAULT 0,
    "MontantConsomme" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "Reserve"         NUMERIC(18,2) NOT NULL DEFAULT 0,
    "ReserveUtilisee" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "EstActif"        BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"       TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"       TIMESTAMPTZ,
    "CreatedBy"       VARCHAR(150),
    "UpdatedBy"       VARCHAR(150),
    "IsDeleted"       BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_BudgetsComptes_Annee" ON "BudgetsComptes"("Annee");

CREATE TABLE IF NOT EXISTS "Materiaux" (
    "Id"               BIGSERIAL PRIMARY KEY,
    "ChantierId"       BIGINT NOT NULL REFERENCES "Chantiers"("Id") ON DELETE CASCADE,
    "Categorie"        VARCHAR(80) NOT NULL,
    "Designation"      VARCHAR(150) NOT NULL,
    "Unite"            VARCHAR(20) NOT NULL,
    "QuantiteCommandee" NUMERIC(18,3) NOT NULL DEFAULT 0,
    "QuantiteRecue"     NUMERIC(18,3) NOT NULL DEFAULT 0,
    "QuantiteUtilisee"  NUMERIC(18,3) NOT NULL DEFAULT 0,
    "SeuilMinimal"      NUMERIC(18,3) NOT NULL DEFAULT 0,
    "PrixUnitaire"      NUMERIC(18,2) NOT NULL DEFAULT 0,
    "CreatedAt"        TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"        TIMESTAMPTZ,
    "CreatedBy"        VARCHAR(150),
    "UpdatedBy"        VARCHAR(150),
    "IsDeleted"        BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX IF NOT EXISTS "IX_Materiaux_Chantier_Designation" ON "Materiaux"("ChantierId","Designation");

CREATE TABLE IF NOT EXISTS "Previsions" (
    "Id"                  BIGSERIAL PRIMARY KEY,
    "ChantierId"          BIGINT NOT NULL REFERENCES "Chantiers"("Id") ON DELETE RESTRICT,
    "DatePrevision"       TIMESTAMPTZ NOT NULL,
    "Reference"           VARCHAR(60) NOT NULL,
    "Statut"              INT NOT NULL DEFAULT 0,
    "SoumisePar"          VARCHAR(450),
    "DateSoumission"      TIMESTAMPTZ,
    "ValideeParRfId"      VARCHAR(450),
    "DateValidationRf"    TIMESTAMPTZ,
    "ValideeParAdminId"   VARCHAR(450),
    "DateValidationAdmin" TIMESTAMPTZ,
    "DateExecution"       TIMESTAMPTZ,
    "MotifRefus"          VARCHAR(500),
    "Observation"         VARCHAR(1000),
    "CreatedAt"           TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"           TIMESTAMPTZ,
    "CreatedBy"           VARCHAR(150),
    "UpdatedBy"           VARCHAR(150),
    "IsDeleted"           BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Previsions_Reference" ON "Previsions"("Reference");
CREATE INDEX IF NOT EXISTS "IX_Previsions_Chantier_Date" ON "Previsions"("ChantierId","DatePrevision");

CREATE TABLE IF NOT EXISTS "PrevisionLignes" (
    "Id"                     BIGSERIAL PRIMARY KEY,
    "PrevisionJournaliereId" BIGINT NOT NULL REFERENCES "Previsions"("Id") ON DELETE CASCADE,
    "Designation"            VARCHAR(150) NOT NULL,
    "Categorie"              VARCHAR(80) NOT NULL,
    "TypeBudget"             INT NOT NULL DEFAULT 0,
    "MateriauId"             BIGINT REFERENCES "Materiaux"("Id") ON DELETE SET NULL,
    "Quantite"               NUMERIC(18,3) NOT NULL DEFAULT 0,
    "PrixUnitaireEstime"     NUMERIC(18,2) NOT NULL DEFAULT 0,
    "Observation"            VARCHAR(500),
    "CreatedAt"              TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"              TIMESTAMPTZ,
    "CreatedBy"              VARCHAR(150),
    "UpdatedBy"              VARCHAR(150),
    "IsDeleted"              BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "Depenses" (
    "Id"                     BIGSERIAL PRIMARY KEY,
    "Date"                   TIMESTAMPTZ NOT NULL,
    "ChantierId"             BIGINT NOT NULL REFERENCES "Chantiers"("Id") ON DELETE RESTRICT,
    "PrevisionJournaliereId" BIGINT REFERENCES "Previsions"("Id") ON DELETE SET NULL,
    "Categorie"              VARCHAR(80) NOT NULL,
    "Designation"            VARCHAR(150) NOT NULL,
    "Quantite"               NUMERIC(18,3) NOT NULL DEFAULT 0,
    "PrixUnitaire"           NUMERIC(18,2) NOT NULL DEFAULT 0,
    "BudgetConcerne"         INT NOT NULL DEFAULT 0,
    "Justificatif"           VARCHAR(250),
    "Observation"            VARCHAR(500),
    "CreatedAt"              TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"              TIMESTAMPTZ,
    "CreatedBy"              VARCHAR(150),
    "UpdatedBy"              VARCHAR(150),
    "IsDeleted"              BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX IF NOT EXISTS "IX_Depenses_Date" ON "Depenses"("Date");

CREATE TABLE IF NOT EXISTS "Alertes" (
    "Id"          BIGSERIAL PRIMARY KEY,
    "Type"        INT NOT NULL,
    "Niveau"      INT NOT NULL DEFAULT 0,
    "Titre"       VARCHAR(150) NOT NULL,
    "Message"     VARCHAR(1000) NOT NULL,
    "ChantierId"  BIGINT REFERENCES "Chantiers"("Id") ON DELETE SET NULL,
    "EstLue"      BOOLEAN NOT NULL DEFAULT FALSE,
    "DateLecture" TIMESTAMPTZ,
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"   TIMESTAMPTZ,
    "CreatedBy"   VARCHAR(150),
    "UpdatedBy"   VARCHAR(150),
    "IsDeleted"   BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX IF NOT EXISTS "IX_Alertes_Lue_Date" ON "Alertes"("EstLue","CreatedAt");

CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id"             BIGSERIAL PRIMARY KEY,
    "Action"         INT NOT NULL,
    "Entite"         VARCHAR(100),
    "CleEntite"      VARCHAR(60),
    "UtilisateurId"  VARCHAR(450),
    "UtilisateurNom" VARCHAR(150),
    "AdresseIp"      VARCHAR(60),
    "Navigateur"     VARCHAR(300),
    "AncienneValeur" TEXT,
    "NouvelleValeur" TEXT,
    "DateAction"     TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedAt"      TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"      TIMESTAMPTZ,
    "CreatedBy"      VARCHAR(150),
    "UpdatedBy"      VARCHAR(150),
    "IsDeleted"      BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_DateAction" ON "AuditLogs"("DateAction");

CREATE TABLE IF NOT EXISTS "Parametres" (
    "Id"          BIGSERIAL PRIMARY KEY,
    "Cle"         VARCHAR(100) NOT NULL,
    "Valeur"      VARCHAR(1000),
    "Groupe"      VARCHAR(60),
    "Description" VARCHAR(300),
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"   TIMESTAMPTZ,
    "CreatedBy"   VARCHAR(150),
    "UpdatedBy"   VARCHAR(150),
    "IsDeleted"   BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Parametres_Cle" ON "Parametres"("Cle");
