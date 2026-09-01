--
-- PostgreSQL database dump
--

\restrict NC40AjHHGkH87Ya8fX4nyZFvjAnEb5fcfB0wxnHcTvkQlQcIJpdpVhJ5Fze1qNs

-- Dumped from database version 14.20
-- Dumped by pg_dump version 14.20

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public."RapportsTravail" DROP CONSTRAINT IF EXISTS "FK_RapportsTravail_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."RapportTravailLignesMateriaux" DROP CONSTRAINT IF EXISTS "FK_RapportTravailLignesMateriaux_RapportsTravail_RapportTravai~";
ALTER TABLE IF EXISTS ONLY public."RapportTravailLignesEquipements" DROP CONSTRAINT IF EXISTS "FK_RapportTravailLignesEquipements_RapportsTravail_RapportTrav~";
ALTER TABLE IF EXISTS ONLY public."RapportTravailLignesAvancement" DROP CONSTRAINT IF EXISTS "FK_RapportTravailLignesAvancement_RapportsTravail_RapportTrava~";
ALTER TABLE IF EXISTS ONLY public."Previsions" DROP CONSTRAINT IF EXISTS "FK_Previsions_Previsions_PrevisionPrecedenteId";
ALTER TABLE IF EXISTS ONLY public."Previsions" DROP CONSTRAINT IF EXISTS "FK_Previsions_PrevisionsMensuelles_PrevisionMensuelleId";
ALTER TABLE IF EXISTS ONLY public."Previsions" DROP CONSTRAINT IF EXISTS "FK_Previsions_PlansJournaliers_PlanJournalierId";
ALTER TABLE IF EXISTS ONLY public."Previsions" DROP CONSTRAINT IF EXISTS "FK_Previsions_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."PrevisionsMensuelles" DROP CONSTRAINT IF EXISTS "FK_PrevisionsMensuelles_PrevisionsMensuelles_PrevisionMensuell~";
ALTER TABLE IF EXISTS ONLY public."PrevisionsMensuelles" DROP CONSTRAINT IF EXISTS "FK_PrevisionsMensuelles_PrevisionsGlobales_PrevisionGlobaleId";
ALTER TABLE IF EXISTS ONLY public."PrevisionsMensuelles" DROP CONSTRAINT IF EXISTS "FK_PrevisionsMensuelles_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."PrevisionsGlobales" DROP CONSTRAINT IF EXISTS "FK_PrevisionsGlobales_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."PrevisionsGlobalesLignes" DROP CONSTRAINT IF EXISTS "FK_PrevisionsGlobalesLignes_PrevisionsGlobales_PrevisionGlobal~";
ALTER TABLE IF EXISTS ONLY public."PrevisionMensuelleLignes" DROP CONSTRAINT IF EXISTS "FK_PrevisionMensuelleLignes_PrevisionsMensuelles_PrevisionMens~";
ALTER TABLE IF EXISTS ONLY public."PrevisionMensuelleLignes" DROP CONSTRAINT IF EXISTS "FK_PrevisionMensuelleLignes_PrevisionsGlobalesLignes_Prevision~";
ALTER TABLE IF EXISTS ONLY public."PrevisionLignes" DROP CONSTRAINT IF EXISTS "FK_PrevisionLignes_Previsions_PrevisionJournaliereId";
ALTER TABLE IF EXISTS ONLY public."PrevisionLignes" DROP CONSTRAINT IF EXISTS "FK_PrevisionLignes_PrevisionsGlobalesLignes_PrevisionGlobaleLi~";
ALTER TABLE IF EXISTS ONLY public."PrevisionLignes" DROP CONSTRAINT IF EXISTS "FK_PrevisionLignes_Materiaux_MateriauId";
ALTER TABLE IF EXISTS ONLY public."PrevisionLignes" DROP CONSTRAINT IF EXISTS "FK_PrevisionLignes_DettesFournisseurs_DetteFournisseurId";
ALTER TABLE IF EXISTS ONLY public."PlansJournaliers" DROP CONSTRAINT IF EXISTS "FK_PlansJournaliers_PrevisionsMensuelles_PrevisionMensuelleId";
ALTER TABLE IF EXISTS ONLY public."PlansJournaliers" DROP CONSTRAINT IF EXISTS "FK_PlansJournaliers_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."PiecesJointes" DROP CONSTRAINT IF EXISTS "FK_PiecesJointes_RapportsTravail_RapportTravailId";
ALTER TABLE IF EXISTS ONLY public."PiecesJointes" DROP CONSTRAINT IF EXISTS "FK_PiecesJointes_Previsions_PrevisionJournaliereId";
ALTER TABLE IF EXISTS ONLY public."PiecesJointes" DROP CONSTRAINT IF EXISTS "FK_PiecesJointes_Decaissements_DecaissementId";
ALTER TABLE IF EXISTS ONLY public."MouvementsMateriau" DROP CONSTRAINT IF EXISTS "FK_MouvementsMateriau_Materiaux_MateriauxId";
ALTER TABLE IF EXISTS ONLY public."MouvementsBancaires" DROP CONSTRAINT IF EXISTS "FK_MouvementsBancaires_Fournisseurs_FournisseurId";
ALTER TABLE IF EXISTS ONLY public."MouvementsBancaires" DROP CONSTRAINT IF EXISTS "FK_MouvementsBancaires_DettesFournisseurs_DetteFournisseurId";
ALTER TABLE IF EXISTS ONLY public."MouvementsBancaires" DROP CONSTRAINT IF EXISTS "FK_MouvementsBancaires_ComptesBancaires_CompteBancaireId";
ALTER TABLE IF EXISTS ONLY public."MouvementsBancaires" DROP CONSTRAINT IF EXISTS "FK_MouvementsBancaires_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."Materiaux" DROP CONSTRAINT IF EXISTS "FK_Materiaux_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."DettesFournisseurs" DROP CONSTRAINT IF EXISTS "FK_DettesFournisseurs_Fournisseurs_FournisseurId";
ALTER TABLE IF EXISTS ONLY public."DettesFournisseurs" DROP CONSTRAINT IF EXISTS "FK_DettesFournisseurs_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."Depenses" DROP CONSTRAINT IF EXISTS "FK_Depenses_Previsions_PrevisionJournaliereId";
ALTER TABLE IF EXISTS ONLY public."Depenses" DROP CONSTRAINT IF EXISTS "FK_Depenses_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."Decaissements" DROP CONSTRAINT IF EXISTS "FK_Decaissements_Previsions_PrevisionJournaliereId";
ALTER TABLE IF EXISTS ONLY public."Decaissements" DROP CONSTRAINT IF EXISTS "FK_Decaissements_PrevisionLignes_PrevisionLigneId";
ALTER TABLE IF EXISTS ONLY public."Decaissements" DROP CONSTRAINT IF EXISTS "FK_Decaissements_ComptesBancaires_CompteBancaireId";
ALTER TABLE IF EXISTS ONLY public."ComptesBancaires" DROP CONSTRAINT IF EXISTS "FK_ComptesBancaires_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."AutresDepensesJour" DROP CONSTRAINT IF EXISTS "FK_AutresDepensesJour_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS "FK_AspNetUserTokens_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "FK_AspNetUserRoles_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "FK_AspNetUserRoles_AspNetRoles_RoleId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS "FK_AspNetUserLogins_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS "FK_AspNetUserClaims_AspNetUsers_UserId";
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS "FK_AspNetRoleClaims_AspNetRoles_RoleId";
ALTER TABLE IF EXISTS ONLY public."Approvisionnements" DROP CONSTRAINT IF EXISTS "FK_Approvisionnements_Previsions_PrevisionJournaliereId";
ALTER TABLE IF EXISTS ONLY public."Approvisionnements" DROP CONSTRAINT IF EXISTS "FK_Approvisionnements_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY public."ApprovisionnementLignes" DROP CONSTRAINT IF EXISTS "FK_ApprovisionnementLignes_Materiaux_MateriauId";
ALTER TABLE IF EXISTS ONLY public."ApprovisionnementLignes" DROP CONSTRAINT IF EXISTS "FK_ApprovisionnementLignes_DettesFournisseurs_DetteFournisseur~";
ALTER TABLE IF EXISTS ONLY public."ApprovisionnementLignes" DROP CONSTRAINT IF EXISTS "FK_ApprovisionnementLignes_Approvisionnements_Approvisionnemen~";
ALTER TABLE IF EXISTS ONLY public."Alertes" DROP CONSTRAINT IF EXISTS "FK_Alertes_Chantiers_ChantierId";
ALTER TABLE IF EXISTS ONLY hangfire.state DROP CONSTRAINT IF EXISTS state_jobid_fkey;
ALTER TABLE IF EXISTS ONLY hangfire.jobparameter DROP CONSTRAINT IF EXISTS jobparameter_jobid_fkey;
DROP INDEX IF EXISTS public."UserNameIndex";
DROP INDEX IF EXISTS public."RoleNameIndex";
DROP INDEX IF EXISTS public."IX_RapportsTravail_Statut";
DROP INDEX IF EXISTS public."IX_RapportsTravail_ChantierId_PeriodeFin";
DROP INDEX IF EXISTS public."IX_RapportTravailLignesMateriaux_RapportTravailId";
DROP INDEX IF EXISTS public."IX_RapportTravailLignesEquipements_RapportTravailId";
DROP INDEX IF EXISTS public."IX_RapportTravailLignesAvancement_RapportTravailId";
DROP INDEX IF EXISTS public."IX_Previsions_Reference";
DROP INDEX IF EXISTS public."IX_Previsions_PrevisionPrecedenteId";
DROP INDEX IF EXISTS public."IX_Previsions_PrevisionMensuelleId";
DROP INDEX IF EXISTS public."IX_Previsions_PlanJournalierId";
DROP INDEX IF EXISTS public."IX_Previsions_ChantierId_DatePrevision";
DROP INDEX IF EXISTS public."IX_PrevisionsMensuelles_Reference";
DROP INDEX IF EXISTS public."IX_PrevisionsMensuelles_PrevisionMensuellePrecedenteId";
DROP INDEX IF EXISTS public."IX_PrevisionsMensuelles_PrevisionGlobaleId";
DROP INDEX IF EXISTS public."IX_PrevisionsMensuelles_ChantierId_Annee_Mois";
DROP INDEX IF EXISTS public."IX_PrevisionsGlobales_ChantierId";
DROP INDEX IF EXISTS public."IX_PrevisionsGlobalesLignes_PrevisionGlobaleId";
DROP INDEX IF EXISTS public."IX_PrevisionMensuelleLignes_PrevisionMensuelleId_Rubrique";
DROP INDEX IF EXISTS public."IX_PrevisionMensuelleLignes_PrevisionGlobaleLigneId";
DROP INDEX IF EXISTS public."IX_PrevisionLignes_PrevisionJournaliereId";
DROP INDEX IF EXISTS public."IX_PrevisionLignes_PrevisionGlobaleLigneId";
DROP INDEX IF EXISTS public."IX_PrevisionLignes_MateriauId";
DROP INDEX IF EXISTS public."IX_PrevisionLignes_DetteFournisseurId";
DROP INDEX IF EXISTS public."IX_PlansJournaliers_PrevisionMensuelleId";
DROP INDEX IF EXISTS public."IX_PlansJournaliers_ChantierId_Date";
DROP INDEX IF EXISTS public."IX_PiecesJointes_RapportTravailId";
DROP INDEX IF EXISTS public."IX_PiecesJointes_PrevisionJournaliereId";
DROP INDEX IF EXISTS public."IX_PiecesJointes_DecaissementId";
DROP INDEX IF EXISTS public."IX_Parametres_Cle";
DROP INDEX IF EXISTS public."IX_MouvementsMateriau_MateriauxId_DateMouvement";
DROP INDEX IF EXISTS public."IX_MouvementsBancaires_FournisseurId";
DROP INDEX IF EXISTS public."IX_MouvementsBancaires_DetteFournisseurId";
DROP INDEX IF EXISTS public."IX_MouvementsBancaires_CompteBancaireId_Date";
DROP INDEX IF EXISTS public."IX_MouvementsBancaires_ChantierId";
DROP INDEX IF EXISTS public."IX_Materiaux_ChantierId_Designation";
DROP INDEX IF EXISTS public."IX_Fournisseurs_Nom";
DROP INDEX IF EXISTS public."IX_DettesFournisseurs_Statut";
DROP INDEX IF EXISTS public."IX_DettesFournisseurs_FournisseurId";
DROP INDEX IF EXISTS public."IX_DettesFournisseurs_ChantierId";
DROP INDEX IF EXISTS public."IX_Depenses_PrevisionJournaliereId";
DROP INDEX IF EXISTS public."IX_Depenses_Date";
DROP INDEX IF EXISTS public."IX_Depenses_ChantierId";
DROP INDEX IF EXISTS public."IX_Decaissements_PrevisionLigneId";
DROP INDEX IF EXISTS public."IX_Decaissements_PrevisionJournaliereId_Date";
DROP INDEX IF EXISTS public."IX_Decaissements_Date";
DROP INDEX IF EXISTS public."IX_Decaissements_CompteBancaireId";
DROP INDEX IF EXISTS public."IX_ComptesBancaires_ChantierId";
DROP INDEX IF EXISTS public."IX_Chantiers_Code";
DROP INDEX IF EXISTS public."IX_Catalogue_Designation";
DROP INDEX IF EXISTS public."IX_BudgetsComptes_Annee";
DROP INDEX IF EXISTS public."IX_AutresDepensesJour_Date";
DROP INDEX IF EXISTS public."IX_AutresDepensesJour_ChantierId";
DROP INDEX IF EXISTS public."IX_AuditLogs_DateAction";
DROP INDEX IF EXISTS public."IX_AspNetUserRoles_RoleId";
DROP INDEX IF EXISTS public."IX_AspNetUserLogins_UserId";
DROP INDEX IF EXISTS public."IX_AspNetUserClaims_UserId";
DROP INDEX IF EXISTS public."IX_AspNetRoleClaims_RoleId";
DROP INDEX IF EXISTS public."IX_Approvisionnements_Reference";
DROP INDEX IF EXISTS public."IX_Approvisionnements_PrevisionJournaliereId";
DROP INDEX IF EXISTS public."IX_Approvisionnements_ChantierId_DateAppro";
DROP INDEX IF EXISTS public."IX_ApprovisionnementLignes_MateriauId";
DROP INDEX IF EXISTS public."IX_ApprovisionnementLignes_DetteFournisseurId";
DROP INDEX IF EXISTS public."IX_ApprovisionnementLignes_ApprovisionnementId";
DROP INDEX IF EXISTS public."IX_Alertes_EstLue_CreatedAt";
DROP INDEX IF EXISTS public."IX_Alertes_ChantierId";
DROP INDEX IF EXISTS public."EmailIndex";
DROP INDEX IF EXISTS hangfire.ix_hangfire_state_jobid;
DROP INDEX IF EXISTS hangfire.ix_hangfire_set_key_score;
DROP INDEX IF EXISTS hangfire.ix_hangfire_set_expireat;
DROP INDEX IF EXISTS hangfire.ix_hangfire_list_expireat;
DROP INDEX IF EXISTS hangfire.ix_hangfire_jobqueue_queueandfetchedat;
DROP INDEX IF EXISTS hangfire.ix_hangfire_jobqueue_jobidandqueue;
DROP INDEX IF EXISTS hangfire.ix_hangfire_jobqueue_fetchedat_queue_jobid;
DROP INDEX IF EXISTS hangfire.ix_hangfire_jobparameter_jobidandname;
DROP INDEX IF EXISTS hangfire.ix_hangfire_job_statename;
DROP INDEX IF EXISTS hangfire.ix_hangfire_job_expireat;
DROP INDEX IF EXISTS hangfire.ix_hangfire_hash_expireat;
DROP INDEX IF EXISTS hangfire.ix_hangfire_counter_key;
DROP INDEX IF EXISTS hangfire.ix_hangfire_counter_expireat;
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
ALTER TABLE IF EXISTS ONLY public."RapportsTravail" DROP CONSTRAINT IF EXISTS "PK_RapportsTravail";
ALTER TABLE IF EXISTS ONLY public."RapportTravailLignesMateriaux" DROP CONSTRAINT IF EXISTS "PK_RapportTravailLignesMateriaux";
ALTER TABLE IF EXISTS ONLY public."RapportTravailLignesEquipements" DROP CONSTRAINT IF EXISTS "PK_RapportTravailLignesEquipements";
ALTER TABLE IF EXISTS ONLY public."RapportTravailLignesAvancement" DROP CONSTRAINT IF EXISTS "PK_RapportTravailLignesAvancement";
ALTER TABLE IF EXISTS ONLY public."PrevisionsMensuelles" DROP CONSTRAINT IF EXISTS "PK_PrevisionsMensuelles";
ALTER TABLE IF EXISTS ONLY public."PrevisionsGlobalesLignes" DROP CONSTRAINT IF EXISTS "PK_PrevisionsGlobalesLignes";
ALTER TABLE IF EXISTS ONLY public."PrevisionsGlobales" DROP CONSTRAINT IF EXISTS "PK_PrevisionsGlobales";
ALTER TABLE IF EXISTS ONLY public."Previsions" DROP CONSTRAINT IF EXISTS "PK_Previsions";
ALTER TABLE IF EXISTS ONLY public."PrevisionMensuelleLignes" DROP CONSTRAINT IF EXISTS "PK_PrevisionMensuelleLignes";
ALTER TABLE IF EXISTS ONLY public."PrevisionLignes" DROP CONSTRAINT IF EXISTS "PK_PrevisionLignes";
ALTER TABLE IF EXISTS ONLY public."PlansJournaliers" DROP CONSTRAINT IF EXISTS "PK_PlansJournaliers";
ALTER TABLE IF EXISTS ONLY public."PiecesJointes" DROP CONSTRAINT IF EXISTS "PK_PiecesJointes";
ALTER TABLE IF EXISTS ONLY public."Parametres" DROP CONSTRAINT IF EXISTS "PK_Parametres";
ALTER TABLE IF EXISTS ONLY public."MouvementsMateriau" DROP CONSTRAINT IF EXISTS "PK_MouvementsMateriau";
ALTER TABLE IF EXISTS ONLY public."MouvementsBancaires" DROP CONSTRAINT IF EXISTS "PK_MouvementsBancaires";
ALTER TABLE IF EXISTS ONLY public."Materiaux" DROP CONSTRAINT IF EXISTS "PK_Materiaux";
ALTER TABLE IF EXISTS ONLY public."Fournisseurs" DROP CONSTRAINT IF EXISTS "PK_Fournisseurs";
ALTER TABLE IF EXISTS ONLY public."DettesFournisseurs" DROP CONSTRAINT IF EXISTS "PK_DettesFournisseurs";
ALTER TABLE IF EXISTS ONLY public."Depenses" DROP CONSTRAINT IF EXISTS "PK_Depenses";
ALTER TABLE IF EXISTS ONLY public."Decaissements" DROP CONSTRAINT IF EXISTS "PK_Decaissements";
ALTER TABLE IF EXISTS ONLY public."DataProtectionKeys" DROP CONSTRAINT IF EXISTS "PK_DataProtectionKeys";
ALTER TABLE IF EXISTS ONLY public."ComptesBancaires" DROP CONSTRAINT IF EXISTS "PK_ComptesBancaires";
ALTER TABLE IF EXISTS ONLY public."Chantiers" DROP CONSTRAINT IF EXISTS "PK_Chantiers";
ALTER TABLE IF EXISTS ONLY public."Catalogue" DROP CONSTRAINT IF EXISTS "PK_Catalogue";
ALTER TABLE IF EXISTS ONLY public."BudgetsComptes" DROP CONSTRAINT IF EXISTS "PK_BudgetsComptes";
ALTER TABLE IF EXISTS ONLY public."AutresDepensesJour" DROP CONSTRAINT IF EXISTS "PK_AutresDepensesJour";
ALTER TABLE IF EXISTS ONLY public."AuditLogs" DROP CONSTRAINT IF EXISTS "PK_AuditLogs";
ALTER TABLE IF EXISTS ONLY public."AspNetUsers" DROP CONSTRAINT IF EXISTS "PK_AspNetUsers";
ALTER TABLE IF EXISTS ONLY public."AspNetUserTokens" DROP CONSTRAINT IF EXISTS "PK_AspNetUserTokens";
ALTER TABLE IF EXISTS ONLY public."AspNetUserRoles" DROP CONSTRAINT IF EXISTS "PK_AspNetUserRoles";
ALTER TABLE IF EXISTS ONLY public."AspNetUserLogins" DROP CONSTRAINT IF EXISTS "PK_AspNetUserLogins";
ALTER TABLE IF EXISTS ONLY public."AspNetUserClaims" DROP CONSTRAINT IF EXISTS "PK_AspNetUserClaims";
ALTER TABLE IF EXISTS ONLY public."AspNetRoles" DROP CONSTRAINT IF EXISTS "PK_AspNetRoles";
ALTER TABLE IF EXISTS ONLY public."AspNetRoleClaims" DROP CONSTRAINT IF EXISTS "PK_AspNetRoleClaims";
ALTER TABLE IF EXISTS ONLY public."Approvisionnements" DROP CONSTRAINT IF EXISTS "PK_Approvisionnements";
ALTER TABLE IF EXISTS ONLY public."ApprovisionnementLignes" DROP CONSTRAINT IF EXISTS "PK_ApprovisionnementLignes";
ALTER TABLE IF EXISTS ONLY public."Alertes" DROP CONSTRAINT IF EXISTS "PK_Alertes";
ALTER TABLE IF EXISTS ONLY hangfire.state DROP CONSTRAINT IF EXISTS state_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.set DROP CONSTRAINT IF EXISTS set_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.set DROP CONSTRAINT IF EXISTS set_key_value_key;
ALTER TABLE IF EXISTS ONLY hangfire.server DROP CONSTRAINT IF EXISTS server_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.schema DROP CONSTRAINT IF EXISTS schema_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.lock DROP CONSTRAINT IF EXISTS lock_resource_key;
ALTER TABLE IF EXISTS ONLY hangfire.list DROP CONSTRAINT IF EXISTS list_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.jobqueue DROP CONSTRAINT IF EXISTS jobqueue_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.jobparameter DROP CONSTRAINT IF EXISTS jobparameter_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.job DROP CONSTRAINT IF EXISTS job_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.hash DROP CONSTRAINT IF EXISTS hash_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.hash DROP CONSTRAINT IF EXISTS hash_key_field_key;
ALTER TABLE IF EXISTS ONLY hangfire.counter DROP CONSTRAINT IF EXISTS counter_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.aggregatedcounter DROP CONSTRAINT IF EXISTS aggregatedcounter_pkey;
ALTER TABLE IF EXISTS ONLY hangfire.aggregatedcounter DROP CONSTRAINT IF EXISTS aggregatedcounter_key_key;
ALTER TABLE IF EXISTS hangfire.state ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.set ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.list ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.jobqueue ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.jobparameter ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.job ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.hash ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.counter ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS hangfire.aggregatedcounter ALTER COLUMN id DROP DEFAULT;
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP TABLE IF EXISTS public."RapportsTravail";
DROP TABLE IF EXISTS public."RapportTravailLignesMateriaux";
DROP TABLE IF EXISTS public."RapportTravailLignesEquipements";
DROP TABLE IF EXISTS public."RapportTravailLignesAvancement";
DROP TABLE IF EXISTS public."PrevisionsMensuelles";
DROP TABLE IF EXISTS public."PrevisionsGlobalesLignes";
DROP TABLE IF EXISTS public."PrevisionsGlobales";
DROP TABLE IF EXISTS public."Previsions";
DROP TABLE IF EXISTS public."PrevisionMensuelleLignes";
DROP TABLE IF EXISTS public."PrevisionLignes";
DROP TABLE IF EXISTS public."PlansJournaliers";
DROP TABLE IF EXISTS public."PiecesJointes";
DROP TABLE IF EXISTS public."Parametres";
DROP TABLE IF EXISTS public."MouvementsMateriau";
DROP TABLE IF EXISTS public."MouvementsBancaires";
DROP TABLE IF EXISTS public."Materiaux";
DROP TABLE IF EXISTS public."Fournisseurs";
DROP TABLE IF EXISTS public."DettesFournisseurs";
DROP TABLE IF EXISTS public."Depenses";
DROP TABLE IF EXISTS public."Decaissements";
DROP TABLE IF EXISTS public."DataProtectionKeys";
DROP TABLE IF EXISTS public."ComptesBancaires";
DROP TABLE IF EXISTS public."Chantiers";
DROP TABLE IF EXISTS public."Catalogue";
DROP TABLE IF EXISTS public."BudgetsComptes";
DROP TABLE IF EXISTS public."AutresDepensesJour";
DROP TABLE IF EXISTS public."AuditLogs";
DROP TABLE IF EXISTS public."AspNetUsers";
DROP TABLE IF EXISTS public."AspNetUserTokens";
DROP TABLE IF EXISTS public."AspNetUserRoles";
DROP TABLE IF EXISTS public."AspNetUserLogins";
DROP TABLE IF EXISTS public."AspNetUserClaims";
DROP TABLE IF EXISTS public."AspNetRoles";
DROP TABLE IF EXISTS public."AspNetRoleClaims";
DROP TABLE IF EXISTS public."Approvisionnements";
DROP TABLE IF EXISTS public."ApprovisionnementLignes";
DROP TABLE IF EXISTS public."Alertes";
DROP SEQUENCE IF EXISTS hangfire.state_id_seq;
DROP TABLE IF EXISTS hangfire.state;
DROP SEQUENCE IF EXISTS hangfire.set_id_seq;
DROP TABLE IF EXISTS hangfire.set;
DROP TABLE IF EXISTS hangfire.server;
DROP TABLE IF EXISTS hangfire.schema;
DROP TABLE IF EXISTS hangfire.lock;
DROP SEQUENCE IF EXISTS hangfire.list_id_seq;
DROP TABLE IF EXISTS hangfire.list;
DROP SEQUENCE IF EXISTS hangfire.jobqueue_id_seq;
DROP TABLE IF EXISTS hangfire.jobqueue;
DROP SEQUENCE IF EXISTS hangfire.jobparameter_id_seq;
DROP TABLE IF EXISTS hangfire.jobparameter;
DROP SEQUENCE IF EXISTS hangfire.job_id_seq;
DROP TABLE IF EXISTS hangfire.job;
DROP SEQUENCE IF EXISTS hangfire.hash_id_seq;
DROP TABLE IF EXISTS hangfire.hash;
DROP SEQUENCE IF EXISTS hangfire.counter_id_seq;
DROP TABLE IF EXISTS hangfire.counter;
DROP SEQUENCE IF EXISTS hangfire.aggregatedcounter_id_seq;
DROP TABLE IF EXISTS hangfire.aggregatedcounter;
DROP SCHEMA IF EXISTS hangfire;
--
-- Name: hangfire; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA hangfire;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: aggregatedcounter; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.aggregatedcounter (
    id bigint NOT NULL,
    key text NOT NULL,
    value bigint NOT NULL,
    expireat timestamp with time zone
);


--
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.aggregatedcounter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.aggregatedcounter_id_seq OWNED BY hangfire.aggregatedcounter.id;


--
-- Name: counter; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.counter (
    id bigint NOT NULL,
    key text NOT NULL,
    value bigint NOT NULL,
    expireat timestamp with time zone
);


--
-- Name: counter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.counter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: counter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.counter_id_seq OWNED BY hangfire.counter.id;


--
-- Name: hash; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.hash (
    id bigint NOT NULL,
    key text NOT NULL,
    field text NOT NULL,
    value text,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: hash_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.hash_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: hash_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.hash_id_seq OWNED BY hangfire.hash.id;


--
-- Name: job; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.job (
    id bigint NOT NULL,
    stateid bigint,
    statename text,
    invocationdata jsonb NOT NULL,
    arguments jsonb NOT NULL,
    createdat timestamp with time zone NOT NULL,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: job_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.job_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: job_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.job_id_seq OWNED BY hangfire.job.id;


--
-- Name: jobparameter; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.jobparameter (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    name text NOT NULL,
    value text,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: jobparameter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.jobparameter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: jobparameter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.jobparameter_id_seq OWNED BY hangfire.jobparameter.id;


--
-- Name: jobqueue; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.jobqueue (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    queue text NOT NULL,
    fetchedat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: jobqueue_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.jobqueue_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: jobqueue_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.jobqueue_id_seq OWNED BY hangfire.jobqueue.id;


--
-- Name: list; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.list (
    id bigint NOT NULL,
    key text NOT NULL,
    value text,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: list_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.list_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: list_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.list_id_seq OWNED BY hangfire.list.id;


--
-- Name: lock; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.lock (
    resource text NOT NULL,
    updatecount integer DEFAULT 0 NOT NULL,
    acquired timestamp with time zone
);


--
-- Name: schema; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.schema (
    version integer NOT NULL
);


--
-- Name: server; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.server (
    id text NOT NULL,
    data jsonb,
    lastheartbeat timestamp with time zone NOT NULL,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: set; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.set (
    id bigint NOT NULL,
    key text NOT NULL,
    score double precision NOT NULL,
    value text NOT NULL,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: set_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.set_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: set_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.set_id_seq OWNED BY hangfire.set.id;


--
-- Name: state; Type: TABLE; Schema: hangfire; Owner: -
--

CREATE TABLE hangfire.state (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    name text NOT NULL,
    reason text,
    createdat timestamp with time zone NOT NULL,
    data jsonb,
    updatecount integer DEFAULT 0 NOT NULL
);


--
-- Name: state_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: -
--

CREATE SEQUENCE hangfire.state_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: state_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: -
--

ALTER SEQUENCE hangfire.state_id_seq OWNED BY hangfire.state.id;


--
-- Name: Alertes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Alertes" (
    "Id" bigint NOT NULL,
    "Type" integer NOT NULL,
    "Niveau" integer NOT NULL,
    "Titre" character varying(150) NOT NULL,
    "Message" character varying(1000) NOT NULL,
    "ChantierId" bigint,
    "EstLue" boolean NOT NULL,
    "DateLecture" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: Alertes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Alertes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Alertes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ApprovisionnementLignes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ApprovisionnementLignes" (
    "Id" bigint NOT NULL,
    "ApprovisionnementId" bigint NOT NULL,
    "Designation" character varying(150) NOT NULL,
    "Categorie" character varying(80) NOT NULL,
    "TypeBudget" integer NOT NULL,
    "MateriauId" bigint,
    "Quantite" numeric(18,3) NOT NULL,
    "PrixUnitaireEstime" numeric(18,2) NOT NULL,
    "Observation" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "DetteFournisseurId" bigint
);


--
-- Name: ApprovisionnementLignes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."ApprovisionnementLignes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ApprovisionnementLignes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Approvisionnements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Approvisionnements" (
    "Id" bigint NOT NULL,
    "ChantierId" bigint NOT NULL,
    "DateAppro" timestamp with time zone NOT NULL,
    "Reference" character varying(60) NOT NULL,
    "Statut" integer NOT NULL,
    "Observation" character varying(1000),
    "PrevisionJournaliereId" bigint,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: Approvisionnements_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Approvisionnements" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Approvisionnements_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetRoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoleClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetRoles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetRoles" (
    "Id" text NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text
);


--
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserClaims" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUserClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" text NOT NULL
);


--
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL
);


--
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text
);


--
-- Name: AspNetUsers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AspNetUsers" (
    "Id" text NOT NULL,
    "NomComplet" text,
    "Fonction" text,
    "EstActif" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "DerniereConnexion" timestamp with time zone,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "ChantierId" bigint
);


--
-- Name: AuditLogs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AuditLogs" (
    "Id" bigint NOT NULL,
    "Action" integer NOT NULL,
    "Entite" character varying(100),
    "CleEntite" character varying(60),
    "UtilisateurId" character varying(450),
    "UtilisateurNom" character varying(150),
    "AdresseIp" character varying(60),
    "Navigateur" character varying(300),
    "AncienneValeur" text,
    "NouvelleValeur" text,
    "DateAction" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: AuditLogs_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AuditLogs" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AuditLogs_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AutresDepensesJour; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AutresDepensesJour" (
    "Id" bigint NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Libelle" character varying(200) NOT NULL,
    "Montant" numeric(18,2) NOT NULL,
    "Ordre" integer NOT NULL,
    "ChantierId" bigint,
    "Observation" character varying(300),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: AutresDepensesJour_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AutresDepensesJour" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AutresDepensesJour_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: BudgetsComptes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."BudgetsComptes" (
    "Id" bigint NOT NULL,
    "Annee" integer NOT NULL,
    "Libelle" character varying(120) NOT NULL,
    "MontantInitial" numeric(18,2) NOT NULL,
    "MontantConsomme" numeric(18,2) NOT NULL,
    "Reserve" numeric(18,2) NOT NULL,
    "ReserveUtilisee" numeric(18,2) NOT NULL,
    "EstActif" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "MontantTransfere" numeric(18,2) DEFAULT 0.0 NOT NULL
);


--
-- Name: BudgetsComptes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."BudgetsComptes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."BudgetsComptes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Catalogue; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Catalogue" (
    "Id" bigint NOT NULL,
    "Designation" character varying(150) NOT NULL,
    "Categorie" character varying(80),
    "Unite" character varying(20),
    "PrixUnitaire" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: Catalogue_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Catalogue" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Catalogue_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Chantiers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Chantiers" (
    "Id" bigint NOT NULL,
    "Nom" character varying(150) NOT NULL,
    "Code" character varying(30) NOT NULL,
    "Localisation" character varying(150),
    "Responsable" character varying(120),
    "DateDebut" timestamp with time zone NOT NULL,
    "DateFin" timestamp with time zone,
    "Statut" integer NOT NULL,
    "BudgetMateriel" numeric(18,2) NOT NULL,
    "Reserve" numeric(18,2) NOT NULL,
    "ReserveUtilisee" numeric(18,2) NOT NULL,
    "Consommation" numeric(18,2) NOT NULL,
    "PourcentageAvancement" double precision NOT NULL,
    "Observation" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "MaterielTransfere" numeric(18,2) DEFAULT 0.0 NOT NULL,
    "Benefice" numeric(18,2) DEFAULT 0.0 NOT NULL,
    "MontantMarche" numeric(18,2) DEFAULT 0.0 NOT NULL
);


--
-- Name: Chantiers_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Chantiers" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Chantiers_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ComptesBancaires; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ComptesBancaires" (
    "Id" bigint NOT NULL,
    "Nom" character varying(120) NOT NULL,
    "Banque" character varying(80) NOT NULL,
    "Numero" character varying(60),
    "Devise" character varying(10) NOT NULL,
    "Solde" numeric(18,2) NOT NULL,
    "EstActif" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "ChantierId" bigint,
    "Type" integer DEFAULT 0 NOT NULL
);


--
-- Name: ComptesBancaires_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."ComptesBancaires" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ComptesBancaires_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: DataProtectionKeys; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."DataProtectionKeys" (
    "Id" integer NOT NULL,
    "FriendlyName" text,
    "Xml" text
);


--
-- Name: DataProtectionKeys_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."DataProtectionKeys" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."DataProtectionKeys_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Decaissements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Decaissements" (
    "Id" bigint NOT NULL,
    "PrevisionJournaliereId" bigint NOT NULL,
    "PrevisionLigneId" bigint,
    "Date" timestamp with time zone NOT NULL,
    "Beneficiaire" character varying(150) NOT NULL,
    "Motif" character varying(300) NOT NULL,
    "Montant" numeric(18,2) NOT NULL,
    "Mode" integer NOT NULL,
    "CompteBancaireId" bigint NOT NULL,
    "BudgetConcerne" integer NOT NULL,
    "Reference" character varying(80),
    "AccuseNom" character varying(150),
    "DateAccuse" timestamp with time zone,
    "Observation" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: Decaissements_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Decaissements" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Decaissements_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Depenses; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Depenses" (
    "Id" bigint NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "ChantierId" bigint NOT NULL,
    "PrevisionJournaliereId" bigint,
    "Categorie" character varying(80) NOT NULL,
    "Designation" character varying(150) NOT NULL,
    "Quantite" numeric(18,3) NOT NULL,
    "PrixUnitaire" numeric(18,2) NOT NULL,
    "BudgetConcerne" integer NOT NULL,
    "Justificatif" character varying(250),
    "Observation" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: Depenses_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Depenses" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Depenses_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: DettesFournisseurs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."DettesFournisseurs" (
    "Id" bigint NOT NULL,
    "FournisseurId" bigint NOT NULL,
    "ChantierId" bigint,
    "Libelle" character varying(200) NOT NULL,
    "MontantInitial" numeric(18,2) NOT NULL,
    "MontantPaye" numeric(18,2) NOT NULL,
    "DateEcheance" timestamp with time zone,
    "Statut" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: DettesFournisseurs_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."DettesFournisseurs" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."DettesFournisseurs_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Fournisseurs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Fournisseurs" (
    "Id" bigint NOT NULL,
    "Nom" character varying(150) NOT NULL,
    "Contact" character varying(120),
    "Telephone" character varying(40),
    "Adresse" character varying(250),
    "Nif" character varying(40),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: Fournisseurs_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Fournisseurs" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Fournisseurs_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Materiaux; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Materiaux" (
    "Id" bigint NOT NULL,
    "ChantierId" bigint NOT NULL,
    "Categorie" character varying(80) NOT NULL,
    "Designation" character varying(150) NOT NULL,
    "Unite" character varying(20) NOT NULL,
    "QuantiteCommandee" numeric(18,3) NOT NULL,
    "QuantiteRecue" numeric(18,3) NOT NULL,
    "QuantiteUtilisee" numeric(18,3) NOT NULL,
    "SeuilMinimal" numeric(18,3) NOT NULL,
    "PrixUnitaire" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "Besoin" numeric(18,3) DEFAULT 0.0 NOT NULL,
    "Localite" character varying(100)
);


--
-- Name: Materiaux_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Materiaux" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Materiaux_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: MouvementsBancaires; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."MouvementsBancaires" (
    "Id" bigint NOT NULL,
    "CompteBancaireId" bigint NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Type" integer NOT NULL,
    "Montant" numeric(18,2) NOT NULL,
    "Beneficiaire" character varying(150),
    "Motif" character varying(300),
    "Reference" character varying(60),
    "ChantierId" bigint,
    "FournisseurId" bigint,
    "DetteFournisseurId" bigint,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "DemandePar" character varying(150),
    "EstValide" boolean DEFAULT true NOT NULL
);


--
-- Name: MouvementsBancaires_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."MouvementsBancaires" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MouvementsBancaires_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: MouvementsMateriau; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."MouvementsMateriau" (
    "Id" bigint NOT NULL,
    "MateriauxId" bigint NOT NULL,
    "DateMouvement" timestamp with time zone NOT NULL,
    "BesoinOuObjectif" character varying(150),
    "QuantiteEntree" numeric(18,3) DEFAULT 0.0 NOT NULL,
    "QuantiteSortie" numeric(18,3) DEFAULT 0.0 NOT NULL,
    "Motif" character varying(100),
    "SoldeEnStock" numeric(18,3) DEFAULT 0.0 NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "SoldeSurBesoin" numeric(18,3) DEFAULT 0.0 NOT NULL
);


--
-- Name: MouvementsMateriau_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."MouvementsMateriau" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MouvementsMateriau_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Parametres; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Parametres" (
    "Id" bigint NOT NULL,
    "Cle" character varying(100) NOT NULL,
    "Valeur" character varying(1000),
    "Groupe" character varying(60),
    "Description" character varying(300),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: Parametres_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Parametres" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Parametres_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: PiecesJointes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PiecesJointes" (
    "Id" bigint NOT NULL,
    "PrevisionJournaliereId" bigint,
    "DecaissementId" bigint,
    "RapportTravailId" bigint,
    "NomFichier" character varying(255) NOT NULL,
    "TypeMime" character varying(100) NOT NULL,
    "Taille" bigint NOT NULL,
    "Contenu" bytea NOT NULL,
    "Description" character varying(300),
    "MontantFacture" numeric(18,2),
    "NumeroPiece" character varying(80),
    "Emetteur" character varying(150),
    "DateAjout" timestamp with time zone NOT NULL,
    "AjouteParId" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: PiecesJointes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."PiecesJointes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."PiecesJointes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: PlansJournaliers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PlansJournaliers" (
    "Id" bigint NOT NULL,
    "PrevisionMensuelleId" bigint NOT NULL,
    "ChantierId" bigint NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "MontantPrevu" numeric(18,2) NOT NULL,
    "Observation" character varying(300),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: PlansJournaliers_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."PlansJournaliers" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."PlansJournaliers_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: PrevisionLignes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PrevisionLignes" (
    "Id" bigint NOT NULL,
    "PrevisionJournaliereId" bigint NOT NULL,
    "Designation" character varying(150) NOT NULL,
    "Categorie" character varying(80) NOT NULL,
    "TypeBudget" integer NOT NULL,
    "MateriauId" bigint,
    "Quantite" numeric(18,3) NOT NULL,
    "PrixUnitaireEstime" numeric(18,2) NOT NULL,
    "Observation" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "DetteFournisseurId" bigint,
    "PrevisionGlobaleLigneId" bigint
);


--
-- Name: PrevisionLignes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."PrevisionLignes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."PrevisionLignes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: PrevisionMensuelleLignes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PrevisionMensuelleLignes" (
    "Id" bigint NOT NULL,
    "PrevisionMensuelleId" bigint NOT NULL,
    "Rubrique" character varying(80) NOT NULL,
    "Designation" character varying(150),
    "Montant" numeric(18,2) NOT NULL,
    "PrevisionGlobaleLigneId" bigint,
    "Observation" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: PrevisionMensuelleLignes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."PrevisionMensuelleLignes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."PrevisionMensuelleLignes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Previsions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Previsions" (
    "Id" bigint NOT NULL,
    "ChantierId" bigint NOT NULL,
    "DatePrevision" timestamp with time zone NOT NULL,
    "Reference" character varying(60) NOT NULL,
    "Statut" integer NOT NULL,
    "SoumisePar" text,
    "DateSoumission" timestamp with time zone,
    "ValideeParRfId" text,
    "DateValidationRf" timestamp with time zone,
    "ValideeParAdminId" text,
    "DateValidationAdmin" timestamp with time zone,
    "DateExecution" timestamp with time zone,
    "MotifRefus" character varying(500),
    "Observation" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL,
    "DateRapport" timestamp with time zone,
    "DateValidationRapport" timestamp with time zone,
    "MotifRefusRapport" character varying(500),
    "RapportRealisation" character varying(2000),
    "RapportValideParId" text,
    "AccuseNomSignataire" character varying(150),
    "AccuseReceptionParId" text,
    "DateAccuseReception" timestamp with time zone,
    "MontantAccuse" numeric(18,2),
    "MontantDecaisse" numeric(18,2) DEFAULT 0.0 NOT NULL,
    "PrevisionMensuelleId" bigint,
    "PrevisionPrecedenteId" bigint,
    "ReportVeille" numeric(18,2) DEFAULT 0.0 NOT NULL,
    "PlanJournalierId" bigint
);


--
-- Name: PrevisionsGlobales; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PrevisionsGlobales" (
    "Id" bigint NOT NULL,
    "ChantierId" bigint NOT NULL,
    "Reference" character varying(60) NOT NULL,
    "DateCreation" timestamp with time zone NOT NULL,
    "Statut" integer NOT NULL,
    "Observation" character varying(500),
    "SoumisePar" text,
    "DateSoumission" timestamp with time zone,
    "ValideeParRfId" text,
    "DateValidationRf" timestamp with time zone,
    "ValideeParAdminId" text,
    "DateValidationAdmin" timestamp with time zone,
    "MotifRefus" character varying(500),
    "DateMiseEnBanque" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: PrevisionsGlobalesLignes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PrevisionsGlobalesLignes" (
    "Id" bigint NOT NULL,
    "PrevisionGlobaleId" bigint NOT NULL,
    "Rubrique" character varying(100) NOT NULL,
    "Designation" character varying(150) NOT NULL,
    "Unite" character varying(20),
    "Quantite" numeric(18,3) NOT NULL,
    "PrixUnitaire" numeric(18,2) NOT NULL,
    "Observation" character varying(300),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: PrevisionsGlobalesLignes_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."PrevisionsGlobalesLignes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."PrevisionsGlobalesLignes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: PrevisionsGlobales_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."PrevisionsGlobales" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."PrevisionsGlobales_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: PrevisionsMensuelles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PrevisionsMensuelles" (
    "Id" bigint NOT NULL,
    "ChantierId" bigint NOT NULL,
    "PrevisionGlobaleId" bigint,
    "Annee" integer NOT NULL,
    "Mois" integer NOT NULL,
    "Reference" character varying(60) NOT NULL,
    "MontantPrevu" numeric(18,2) NOT NULL,
    "ReportMoisPrecedent" numeric(18,2) NOT NULL,
    "MontantConsomme" numeric(18,2) NOT NULL,
    "PrevisionMensuellePrecedenteId" bigint,
    "Statut" integer NOT NULL,
    "SoumisePar" text,
    "DateSoumission" timestamp with time zone,
    "ValideeParId" text,
    "DateValidation" timestamp with time zone,
    "MotifRefus" character varying(500),
    "DateCloture" timestamp with time zone,
    "ClotureeParId" text,
    "Observation" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean NOT NULL
);


--
-- Name: PrevisionsMensuelles_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."PrevisionsMensuelles" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."PrevisionsMensuelles_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Previsions_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Previsions" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Previsions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: RapportTravailLignesAvancement; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RapportTravailLignesAvancement" (
    "Id" bigint NOT NULL,
    "RapportTravailId" bigint NOT NULL,
    "Zone" character varying(150) NOT NULL,
    "TravauxRealises" text NOT NULL,
    "NiveauAvancement" character varying(150) NOT NULL,
    "Observations" text,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean DEFAULT false NOT NULL
);


--
-- Name: RapportTravailLignesAvancement_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."RapportTravailLignesAvancement" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."RapportTravailLignesAvancement_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: RapportTravailLignesEquipements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RapportTravailLignesEquipements" (
    "Id" bigint NOT NULL,
    "RapportTravailId" bigint NOT NULL,
    "Equipement" character varying(150) NOT NULL,
    "Etat" character varying(80),
    "Observation" text,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean DEFAULT false NOT NULL
);


--
-- Name: RapportTravailLignesEquipements_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."RapportTravailLignesEquipements" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."RapportTravailLignesEquipements_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: RapportTravailLignesMateriaux; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RapportTravailLignesMateriaux" (
    "Id" bigint NOT NULL,
    "RapportTravailId" bigint NOT NULL,
    "Materiau" character varying(150) NOT NULL,
    "Unite" character varying(20),
    "QuantiteUtilisee" numeric(18,3) DEFAULT 0 NOT NULL,
    "StockInitial" numeric(18,3) DEFAULT 0 NOT NULL,
    "Entree" numeric(18,3) DEFAULT 0 NOT NULL,
    "StockRestant" numeric(18,3) DEFAULT 0 NOT NULL,
    "Observations" text,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean DEFAULT false NOT NULL
);


--
-- Name: RapportTravailLignesMateriaux_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."RapportTravailLignesMateriaux" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."RapportTravailLignesMateriaux_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: RapportsTravail; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RapportsTravail" (
    "Id" bigint NOT NULL,
    "ChantierId" bigint NOT NULL,
    "Numero" character varying(20) NOT NULL,
    "PeriodeDebut" timestamp with time zone NOT NULL,
    "PeriodeFin" timestamp with time zone NOT NULL,
    "Lieu" character varying(200),
    "EntrepriseExecutante" character varying(150),
    "ConducteurTravaux" character varying(150),
    "EffectifCadres" integer DEFAULT 0 NOT NULL,
    "EffectifOuvriers" integer DEFAULT 0 NOT NULL,
    "HoraireMatin" character varying(50),
    "HoraireApresMidi" character varying(50),
    "ConditionsMeteo" character varying(300),
    "ResumeSuiviPlanning" text,
    "ProblemesRencontres" text,
    "Suggestions" text,
    "Statut" integer DEFAULT 0 NOT NULL,
    "SoumisPar" text,
    "DateSoumission" timestamp with time zone,
    "ValideParId" text,
    "DateValidation" timestamp with time zone,
    "MotifRefus" character varying(500),
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    "IsDeleted" boolean DEFAULT false NOT NULL
);


--
-- Name: RapportsTravail_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."RapportsTravail" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."RapportsTravail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Name: aggregatedcounter id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.aggregatedcounter ALTER COLUMN id SET DEFAULT nextval('hangfire.aggregatedcounter_id_seq'::regclass);


--
-- Name: counter id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.counter ALTER COLUMN id SET DEFAULT nextval('hangfire.counter_id_seq'::regclass);


--
-- Name: hash id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.hash ALTER COLUMN id SET DEFAULT nextval('hangfire.hash_id_seq'::regclass);


--
-- Name: job id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.job ALTER COLUMN id SET DEFAULT nextval('hangfire.job_id_seq'::regclass);


--
-- Name: jobparameter id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.jobparameter ALTER COLUMN id SET DEFAULT nextval('hangfire.jobparameter_id_seq'::regclass);


--
-- Name: jobqueue id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.jobqueue ALTER COLUMN id SET DEFAULT nextval('hangfire.jobqueue_id_seq'::regclass);


--
-- Name: list id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.list ALTER COLUMN id SET DEFAULT nextval('hangfire.list_id_seq'::regclass);


--
-- Name: set id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.set ALTER COLUMN id SET DEFAULT nextval('hangfire.set_id_seq'::regclass);


--
-- Name: state id; Type: DEFAULT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.state ALTER COLUMN id SET DEFAULT nextval('hangfire.state_id_seq'::regclass);


--
-- Data for Name: aggregatedcounter; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.aggregatedcounter (id, key, value, expireat) FROM stdin;
\.


--
-- Data for Name: counter; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.counter (id, key, value, expireat) FROM stdin;
\.


--
-- Data for Name: hash; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.hash (id, key, field, value, expireat, updatecount) FROM stdin;
\.


--
-- Data for Name: job; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) FROM stdin;
\.


--
-- Data for Name: jobparameter; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.jobparameter (id, jobid, name, value, updatecount) FROM stdin;
\.


--
-- Data for Name: jobqueue; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) FROM stdin;
\.


--
-- Data for Name: list; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.list (id, key, value, expireat, updatecount) FROM stdin;
\.


--
-- Data for Name: lock; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.lock (resource, updatecount, acquired) FROM stdin;
\.


--
-- Data for Name: schema; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.schema (version) FROM stdin;
22
\.


--
-- Data for Name: server; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.server (id, data, lastheartbeat, updatecount) FROM stdin;
\.


--
-- Data for Name: set; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.set (id, key, score, value, expireat, updatecount) FROM stdin;
\.


--
-- Data for Name: state; Type: TABLE DATA; Schema: hangfire; Owner: -
--

COPY hangfire.state (id, jobid, name, reason, createdat, data, updatecount) FROM stdin;
\.


--
-- Data for Name: Alertes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Alertes" ("Id", "Type", "Niveau", "Titre", "Message", "ChantierId", "EstLue", "DateLecture", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: ApprovisionnementLignes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."ApprovisionnementLignes" ("Id", "ApprovisionnementId", "Designation", "Categorie", "TypeBudget", "MateriauId", "Quantite", "PrixUnitaireEstime", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DetteFournisseurId") FROM stdin;
\.


--
-- Data for Name: Approvisionnements; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Approvisionnements" ("Id", "ChantierId", "DateAppro", "Reference", "Statut", "Observation", "PrevisionJournaliereId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetRoleClaims" ("Id", "RoleId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetRoles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") FROM stdin;
82b2b886-97b4-439c-9e0c-ffe3efbebace	Administrateur	ADMINISTRATEUR	\N
fc1c5806-6fe4-42a8-9a98-d1a888d25445	Correspondant	CORRESPONDANT	\N
3c66582d-e4fc-4886-a5d4-5149074a6937	Chef de chantier	CHEF DE CHANTIER	\N
8e8531e6-f90b-4b9f-b865-85f0f15bec57	Magasinier	MAGASINIER	\N
\.


--
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId") FROM stdin;
\.


--
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserRoles" ("UserId", "RoleId") FROM stdin;
6c307f65-9df5-44f6-9974-790797d90b88	82b2b886-97b4-439c-9e0c-ffe3efbebace
bb4f98f8-244e-458b-835d-956ee7ba6edf	fc1c5806-6fe4-42a8-9a98-d1a888d25445
d36ce25e-043f-4af4-9659-2ffd857df968	3c66582d-e4fc-4886-a5d4-5149074a6937
\.


--
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value") FROM stdin;
\.


--
-- Data for Name: AspNetUsers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUsers" ("Id", "NomComplet", "Fonction", "EstActif", "CreatedAt", "DerniereConnexion", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "ChantierId") FROM stdin;
6c307f65-9df5-44f6-9974-790797d90b88	Administrateur ETAM	Direction	t	2026-08-31 10:44:20.86283+02	\N	admin@etam.mg	ADMIN@ETAM.MG	admin@etam.mg	ADMIN@ETAM.MG	t	AQAAAAIAAYagAAAAEJZNEhxboYJa0O7+nY5+IZGRcwJdP3etBPFMluguPO2l5cmyDj9tBoijRq/LXHpZ3w==	PTL5Z75THA4KF2NXCQIO623244PGYHDE	fa2f3ef9-960e-4174-a370-92a7d2be2bf1	\N	f	f	\N	t	0	\N
bb4f98f8-244e-458b-835d-956ee7ba6edf	Correspondant ETAM	Finance	t	2026-08-31 10:44:21.21233+02	\N	rf@etam.mg	RF@ETAM.MG	rf@etam.mg	RF@ETAM.MG	t	AQAAAAIAAYagAAAAELd8rt0ILlb5xuvXL5hPJiL8TtCCDJKRG6LwgodtaIHutfI0KgXVC7lUfnY3efbXKA==	DWZQUS3HKBMTRWOE4LU56K7ZK5ITLLKD	251f9c8b-3ac6-4d78-bf4a-83efcd51772c	\N	f	f	\N	t	0	\N
d36ce25e-043f-4af4-9659-2ffd857df968	Chef de Chantier	Exploitation	t	2026-08-31 10:44:21.343235+02	\N	chef@etam.mg	CHEF@ETAM.MG	chef@etam.mg	CHEF@ETAM.MG	t	AQAAAAIAAYagAAAAEA9hMwh3Rd1mPXzFxGC/fqNcE9r2gZeh5y5RcMKGh5fO19k0NspJxY8RISvRT5wAig==	NFO6A2N7IOZJB7DSEOHQAGQPR3ZRC5R7	1f9daef3-080f-4f43-8f4b-7417ea4bee1a	\N	f	f	\N	t	0	\N
\.


--
-- Data for Name: AuditLogs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AuditLogs" ("Id", "Action", "Entite", "CleEntite", "UtilisateurId", "UtilisateurNom", "AdresseIp", "Navigateur", "AncienneValeur", "NouvelleValeur", "DateAction", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
1	0	\N	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 13:08:21.926032+02	2026-08-31 13:08:21.924366+02	\N	\N	\N	f
2	2	Chantier	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Construction de 6 forages PAAEP Diego	2026-08-31 15:29:17.974664+02	2026-08-31 15:29:17.974664+02	\N	admin@etam.mg	\N	f
3	2	Compte bancaire	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	SARL ETAM	2026-08-31 15:29:18.312846+02	2026-08-31 15:29:18.312846+02	\N	admin@etam.mg	\N	f
4	2	Mouvement bancaire	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	ENC-DIEG	2026-08-31 15:29:18.430565+02	2026-08-31 15:29:18.430565+02	\N	admin@etam.mg	\N	f
5	3	Compte bancaire	1	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	SARL ETAM	2026-08-31 15:29:18.430565+02	2026-08-31 15:29:18.430565+02	\N	admin@etam.mg	\N	f
6	2	PrevisionMensuelle	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	septembre 2026	2026-08-31 15:54:48.473023+02	2026-08-31 15:54:48.473023+02	\N	admin@etam.mg	\N	f
7	2	PrevisionMensuelle	1	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PMENS-DIEG-202609 — 400 000 000 Ar + report 0 Ar	2026-08-31 15:54:48.514606+02	2026-08-31 15:54:48.514515+02	\N	\N	\N	f
8	3	PrevisionMensuelle	1	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	septembre 2026	2026-08-31 15:54:58.189247+02	2026-08-31 15:54:58.189247+02	\N	admin@etam.mg	\N	f
9	5	PrevisionMensuelle	1	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 15:54:58.213784+02	2026-08-31 15:54:58.213759+02	\N	\N	\N	f
10	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260728-9938	2026-08-31 17:44:06.096958+02	2026-08-31 17:44:06.096958+02	\N	admin@etam.mg	\N	f
11	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	vivre cadre	2026-08-31 17:44:06.096958+02	2026-08-31 17:44:06.096958+02	\N	admin@etam.mg	\N	f
12	2	PrevisionJournaliere	1	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 17:44:06.433895+02	2026-08-31 17:44:06.433779+02	\N	\N	\N	f
13	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260730-4254	2026-08-31 17:48:39.927537+02	2026-08-31 17:48:39.927537+02	\N	admin@etam.mg	\N	f
14	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 17:48:39.927537+02	2026-08-31 17:48:39.927537+02	\N	admin@etam.mg	\N	f
15	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Ceremonie traditionnelle de benediction (Joro be)	2026-08-31 17:48:39.927537+02	2026-08-31 17:48:39.927537+02	\N	admin@etam.mg	\N	f
16	2	PrevisionJournaliere	2	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 17:48:39.951396+02	2026-08-31 17:48:39.95137+02	\N	\N	\N	f
17	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260729-9012	2026-08-31 17:49:44.582969+02	2026-08-31 17:49:44.582969+02	\N	admin@etam.mg	\N	f
18	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 17:49:44.582969+02	2026-08-31 17:49:44.582969+02	\N	admin@etam.mg	\N	f
19	2	PrevisionJournaliere	3	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 17:49:44.594+02	2026-08-31 17:49:44.59398+02	\N	\N	\N	f
20	3	Chantier	1	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Construction de 6 forages PAAEP Diego	2026-08-31 17:50:19.678439+02	2026-08-31 17:50:19.678439+02	\N	admin@etam.mg	\N	f
21	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260731-9417	2026-08-31 17:54:45.369024+02	2026-08-31 17:54:45.369024+02	\N	admin@etam.mg	\N	f
22	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 17:54:45.369024+02	2026-08-31 17:54:45.369024+02	\N	admin@etam.mg	\N	f
23	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Loyer base juillet 2026	2026-08-31 17:54:45.369024+02	2026-08-31 17:54:45.369024+02	\N	admin@etam.mg	\N	f
24	2	PrevisionJournaliere	4	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 17:54:45.390426+02	2026-08-31 17:54:45.390408+02	\N	\N	\N	f
25	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260716-3922	2026-08-31 17:57:18.696355+02	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f
26	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 17:57:18.696355+02	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f
27	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant voiture de liaison 	2026-08-31 17:57:18.696355+02	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f
28	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Eau vive 	2026-08-31 17:57:18.696355+02	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f
29	2	PrevisionJournaliere	5	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 17:57:18.711322+02	2026-08-31 17:57:18.711293+02	\N	\N	\N	f
30	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260717-9537	2026-08-31 18:01:06.618747+02	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f
31	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:01:06.618747+02	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f
32	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant voiture de liaison 	2026-08-31 18:01:06.618747+02	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f
33	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	connexion christophe	2026-08-31 18:01:06.618747+02	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f
34	2	PrevisionJournaliere	6	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:01:06.652236+02	2026-08-31 18:01:06.652216+02	\N	\N	\N	f
35	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260803-0592	2026-08-31 18:18:38.041119+02	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f
36	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:18:38.041119+02	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f
37	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location voiture de liaison du 003/08/2026	2026-08-31 18:18:38.041119+02	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f
38	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant du 03/08/2026	2026-08-31 18:18:38.041119+02	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f
39	2	PrevisionJournaliere	7	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:18:38.068733+02	2026-08-31 18:18:38.06872+02	\N	\N	\N	f
40	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260831-5942	2026-08-31 18:20:25.811884+02	2026-08-31 18:20:25.811884+02	\N	admin@etam.mg	\N	f
41	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:20:25.811884+02	2026-08-31 18:20:25.811884+02	\N	admin@etam.mg	\N	f
42	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Prestation femme de menage du 27/07/2026au 02/08/2026	2026-08-31 18:20:25.811884+02	2026-08-31 18:20:25.811884+02	\N	admin@etam.mg	\N	f
43	2	PrevisionJournaliere	8	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:20:25.835284+02	2026-08-31 18:20:25.835272+02	\N	\N	\N	f
44	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260805-3804	2026-08-31 18:24:00.189088+02	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f
45	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:24:00.189088+02	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f
46	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Chemise dossier	2026-08-31 18:24:00.189088+02	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f
47	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	papier Ram	2026-08-31 18:24:00.189088+02	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f
48	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	reliure journal	2026-08-31 18:24:00.189088+02	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f
49	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location vehicule du 05/08/2026	2026-08-31 18:24:00.189088+02	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f
50	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Carburant vehicule du 05/08/2026	2026-08-31 18:24:00.189088+02	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f
51	2	PrevisionJournaliere	9	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:24:00.219237+02	2026-08-31 18:24:00.219226+02	\N	\N	\N	f
52	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260806-0670	2026-08-31 18:27:23.475546+02	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f
53	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:27:23.475546+02	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f
54	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Rubalise(rouleau 10 m)	2026-08-31 18:27:23.475546+02	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f
55	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	connexion christophe	2026-08-31 18:27:23.475546+02	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f
56	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location vehicule du 6/08/2026	2026-08-31 18:27:23.475546+02	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f
57	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant vehicule du 06/08/2026	2026-08-31 18:27:23.475546+02	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f
58	2	PrevisionJournaliere	10	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:27:23.510575+02	2026-08-31 18:27:23.510562+02	\N	\N	\N	f
59	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260807-9175	2026-08-31 18:30:37.037529+02	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f
60	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:30:37.037529+02	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f
61	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location vehicule du 06/08/2026	2026-08-31 18:30:37.037529+02	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f
62	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant du 06/08/2026	2026-08-31 18:30:37.037529+02	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f
63	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	table bois dure 	2026-08-31 18:30:37.037529+02	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f
64	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Seza bois dure	2026-08-31 18:30:37.037529+02	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f
65	2	PrevisionJournaliere	11	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:30:37.059239+02	2026-08-31 18:30:37.059228+02	\N	\N	\N	f
66	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260808-8743	2026-08-31 18:33:31.7256+02	2026-08-31 18:33:31.7256+02	\N	admin@etam.mg	\N	f
67	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:33:31.7256+02	2026-08-31 18:33:31.7256+02	\N	admin@etam.mg	\N	f
68	2	PrevisionJournaliere	12	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:33:31.754654+02	2026-08-31 18:33:31.754643+02	\N	\N	\N	f
69	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260810-2467	2026-08-31 18:36:53.02778+02	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f
70	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:36:53.02778+02	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f
71	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location bajaj  pour christophe , travaux Mahaleja aller-retour	2026-08-31 18:36:53.02778+02	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f
72	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	complémentarité budget Joro be  	2026-08-31 18:36:53.02778+02	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f
73	2	PrevisionJournaliere	13	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:36:53.050904+02	2026-08-31 18:36:53.050892+02	\N	\N	\N	f
74	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260812-0096	2026-08-31 18:39:43.808044+02	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f
75	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:39:43.808044+02	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f
76	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Payement femme de menage du 27/07/2026au 02/08/2026	2026-08-31 18:39:43.808044+02	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f
77	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Location bajaj pour christophe, travaux mahaleja aller-retour	2026-08-31 18:39:43.808044+02	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f
78	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	table imprimante	2026-08-31 18:39:43.808044+02	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f
79	2	PrevisionJournaliere	14	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:39:43.901827+02	2026-08-31 18:39:43.901816+02	\N	\N	\N	f
80	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260813-1089	2026-08-31 18:43:50.711476+02	2026-08-31 18:43:50.711476+02	\N	admin@etam.mg	\N	f
81	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:43:50.711476+02	2026-08-31 18:43:50.711476+02	\N	admin@etam.mg	\N	f
82	2	PrevisionJournaliere	15	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:43:50.733604+02	2026-08-31 18:43:50.733593+02	\N	\N	\N	f
83	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260815-3143	2026-08-31 18:44:56.581727+02	2026-08-31 18:44:56.581727+02	\N	admin@etam.mg	\N	f
84	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:44:56.581727+02	2026-08-31 18:44:56.581727+02	\N	admin@etam.mg	\N	f
85	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	bajaj deplacement  Mahaleja	2026-08-31 18:44:56.581727+02	2026-08-31 18:44:56.581727+02	\N	admin@etam.mg	\N	f
86	2	PrevisionJournaliere	16	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:44:56.61669+02	2026-08-31 18:44:56.61668+02	\N	\N	\N	f
87	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260817-6319	2026-08-31 18:51:14.697179+02	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f
88	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:51:14.697179+02	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f
89	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Payement femme de menage du 27/07/2026au 02/08/2026	2026-08-31 18:51:14.697179+02	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f
90	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Location journee bajaj, deplacement demarrage ouverture acces  Mahaleja	2026-08-31 18:51:14.697179+02	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f
91	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Achat gilet vert EPI pour mains d oeuvre ouverture acces au champ captant de Mahaleja 	2026-08-31 18:51:14.697179+02	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f
92	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Achat gantde travail  EPI pour mains d oeuvre ouverture acces au champ captant de Mahaleja 	2026-08-31 18:51:14.697179+02	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f
93	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Main d'oeuvre ouverture Mahaleja	2026-08-31 18:51:14.697179+02	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f
94	2	PrevisionJournaliere	17	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:51:14.729177+02	2026-08-31 18:51:14.729166+02	\N	\N	\N	f
95	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260819-8547	2026-08-31 18:52:50.201706+02	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f
96	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:52:50.201706+02	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f
97	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Chaussure de securite N 40	2026-08-31 18:52:50.201706+02	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f
98	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Ensemble de chantier	2026-08-31 18:52:50.201706+02	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f
99	2	PrevisionJournaliere	18	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:52:50.22448+02	2026-08-31 18:52:50.224471+02	\N	\N	\N	f
100	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260820-9917	2026-08-31 18:54:10.950538+02	2026-08-31 18:54:10.950538+02	\N	admin@etam.mg	\N	f
101	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 18:54:10.950538+02	2026-08-31 18:54:10.950538+02	\N	admin@etam.mg	\N	f
102	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location bajaj  deplacement a Mahaleja 	2026-08-31 18:54:10.950538+02	2026-08-31 18:54:10.950538+02	\N	admin@etam.mg	\N	f
103	2	PrevisionJournaliere	19	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 18:54:10.971539+02	2026-08-31 18:54:10.971529+02	\N	\N	\N	f
104	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260819-6703	2026-08-31 19:28:06.90424+02	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f
105	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 19:28:06.90424+02	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f
106	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location bajaj , deplacement demarrage ouverture  acces mahaleja du 20/08/2026	2026-08-31 19:28:06.90424+02	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f
107	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Achat gilet de reunion DT et Aina 	2026-08-31 19:28:06.90424+02	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f
108	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Confection logo Etam epi Aina et christophe 	2026-08-31 19:28:06.90424+02	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f
109	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	main d'oeuvre ouverture chantier mahaleja	2026-08-31 19:28:06.90424+02	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f
110	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	tenuedetravail avec  logo etam	2026-08-31 19:28:06.90424+02	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f
111	2	PrevisionJournaliere	20	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 19:28:06.935575+02	2026-08-31 19:28:06.935565+02	\N	\N	\N	f
112	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260820-1923	2026-08-31 19:28:45.845565+02	2026-08-31 19:28:45.845565+02	\N	admin@etam.mg	\N	f
113	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 19:28:45.845565+02	2026-08-31 19:28:45.845565+02	\N	admin@etam.mg	\N	f
114	2	PrevisionJournaliere	21	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 19:28:45.863562+02	2026-08-31 19:28:45.863551+02	\N	\N	\N	f
115	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260821-8624	2026-08-31 19:38:24.751019+02	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f
116	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 19:38:24.751019+02	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f
117	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Complementarite achat chaussure de securite christophe et Aina	2026-08-31 19:38:24.751019+02	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f
118	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Chaussure de securite Santatra QHSE n 36	2026-08-31 19:38:24.751019+02	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f
119	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	gilet Santatra QHSE avec logo	2026-08-31 19:38:24.751019+02	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f
120	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	ensemblechantier Santatra QHSE avec logo	2026-08-31 19:38:24.751019+02	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f
121	2	PrevisionJournaliere	22	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 19:38:24.784802+02	2026-08-31 19:38:24.78479+02	\N	\N	\N	f
122	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260824-7864	2026-08-31 19:45:34.438582+02	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f
123	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-08-31 19:45:34.438582+02	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f
124	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Fandriana Santatra 	2026-08-31 19:45:34.438582+02	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f
125	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Kidoro Santatra 	2026-08-31 19:45:34.438582+02	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f
126	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Payement femme  de menage 	2026-08-31 19:45:34.438582+02	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f
127	2	PrevisionJournaliere	23	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-08-31 19:45:34.466436+02	2026-08-31 19:45:34.466418+02	\N	\N	\N	f
128	0	\N	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:14:28.775784+02	2026-09-01 10:14:28.77577+02	\N	\N	\N	f
129	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:16:54.271188+02	2026-09-01 10:16:54.271188+02	\N	admin@etam.mg	\N	f
130	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Prestation femme de menage du 27/07/2026au 02/08/2026	2026-09-01 10:16:54.271188+02	2026-09-01 10:16:54.271188+02	\N	admin@etam.mg	\N	f
131	3	Prévision	8	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260831-5942	2026-09-01 10:16:54.271188+02	2026-09-01 10:16:54.271188+02	\N	admin@etam.mg	\N	f
132	4	PrevisionLigne	16	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:16:54.271188+02	2026-09-01 10:16:54.271188+02	\N	admin@etam.mg	\N	f
133	4	PrevisionLigne	17	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Prestation femme de menage du 27/07/2026au 02/08/2026	2026-09-01 10:16:54.271188+02	2026-09-01 10:16:54.271188+02	\N	admin@etam.mg	\N	f
134	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260825-2768	2026-09-01 10:24:04.122812+02	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f
135	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:24:04.122812+02	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f
136	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location vehicule du 25/08/2026 Aina, Christophe	2026-09-01 10:24:04.122812+02	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f
137	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant vehicule du 25/08/2026	2026-09-01 10:24:04.122812+02	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f
138	2	PrevisionJournaliere	24	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:24:04.151881+02	2026-09-01 10:24:04.151872+02	\N	\N	\N	f
139	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260826-7821	2026-09-01 10:26:33.535257+02	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f
140	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:26:33.535257+02	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f
141	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	location vehicule Tojo du 25/08/2026	2026-09-01 10:26:33.535257+02	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f
142	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Replein Tojo du 25/08/2026	2026-09-01 10:26:33.535257+02	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f
143	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Transfert Tojo PAAEP hotel-aeroport du 26/082026	2026-09-01 10:26:33.535257+02	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f
144	2	PrevisionJournaliere	25	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:26:33.542523+02	2026-09-01 10:26:33.542427+02	\N	\N	\N	f
148	2	PrevisionJournaliere	26	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:28:04.953064+02	2026-09-01 10:28:04.953055+02	\N	\N	\N	f
145	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260721-4123	2026-09-01 10:28:04.938832+02	2026-09-01 10:28:04.938832+02	\N	admin@etam.mg	\N	f
146	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:28:04.938832+02	2026-09-01 10:28:04.938832+02	\N	admin@etam.mg	\N	f
147	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant voiture de liaison 	2026-09-01 10:28:04.938832+02	2026-09-01 10:28:04.938832+02	\N	admin@etam.mg	\N	f
149	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260722-3080	2026-09-01 10:29:36.936664+02	2026-09-01 10:29:36.936664+02	\N	admin@etam.mg	\N	f
150	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:29:36.936664+02	2026-09-01 10:29:36.936664+02	\N	admin@etam.mg	\N	f
151	2	PrevisionJournaliere	27	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:29:36.960187+02	2026-09-01 10:29:36.960178+02	\N	\N	\N	f
152	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260720-2093	2026-09-01 10:32:06.938158+02	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f
153	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:32:06.938158+02	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f
154	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Payement femme de menage du 27/07/2026au 02/08/2026	2026-09-01 10:32:06.938158+02	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f
155	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	carburant voiture de liaison 	2026-09-01 10:32:06.938158+02	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f
156	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	voiture de liaison du 14 au 17/07/2026	2026-09-01 10:32:06.938158+02	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f
157	2	PrevisionJournaliere	28	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:32:06.953949+02	2026-09-01 10:32:06.953937+02	\N	\N	\N	f
158	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260723-9047	2026-09-01 10:33:07.350314+02	2026-09-01 10:33:07.350314+02	\N	admin@etam.mg	\N	f
159	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:33:07.350314+02	2026-09-01 10:33:07.350314+02	\N	admin@etam.mg	\N	f
160	2	PrevisionJournaliere	29	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:33:07.378906+02	2026-09-01 10:33:07.378895+02	\N	\N	\N	f
161	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260724-8930	2026-09-01 10:34:35.865304+02	2026-09-01 10:34:35.865304+02	\N	admin@etam.mg	\N	f
162	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:34:35.865304+02	2026-09-01 10:34:35.865304+02	\N	admin@etam.mg	\N	f
163	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	main d'oeuvre 	2026-09-01 10:34:35.865304+02	2026-09-01 10:34:35.865304+02	\N	admin@etam.mg	\N	f
164	2	PrevisionJournaliere	30	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:34:35.896096+02	2026-09-01 10:34:35.896085+02	\N	\N	\N	f
165	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260725-9942	2026-09-01 10:35:07.361353+02	2026-09-01 10:35:07.361353+02	\N	admin@etam.mg	\N	f
166	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:35:07.361353+02	2026-09-01 10:35:07.361353+02	\N	admin@etam.mg	\N	f
167	2	PrevisionJournaliere	31	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:35:07.372166+02	2026-09-01 10:35:07.372157+02	\N	\N	\N	f
168	2	Prévision	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	PREV-DIEG-20260727-3045	2026-09-01 10:38:52.750797+02	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f
169	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Vivre Cadre 	2026-09-01 10:38:52.750797+02	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f
170	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	Payement femme de menage du 27/07/2026au 02/08/2026	2026-09-01 10:38:52.750797+02	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f
171	2	PrevisionLigne	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	voiture de liaison du 20 au 21/07/2026	2026-09-01 10:38:52.750797+02	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f
172	2	PrevisionJournaliere	32	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 10:38:52.766458+02	2026-09-01 10:38:52.766448+02	\N	\N	\N	f
173	0	\N	\N	6c307f65-9df5-44f6-9974-790797d90b88	admin@etam.mg	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36	\N	\N	2026-09-01 19:23:18.794977+02	2026-09-01 19:23:18.794962+02	\N	\N	\N	f
\.


--
-- Data for Name: AutresDepensesJour; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AutresDepensesJour" ("Id", "Date", "Libelle", "Montant", "Ordre", "ChantierId", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: BudgetsComptes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."BudgetsComptes" ("Id", "Annee", "Libelle", "MontantInitial", "MontantConsomme", "Reserve", "ReserveUtilisee", "EstActif", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "MontantTransfere") FROM stdin;
2	2026	Budget 2026	0.00	0.00	0.00	0.00	t	2026-08-31 12:58:11.32871+02	\N	système	\N	f	0.00
\.


--
-- Data for Name: Catalogue; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Catalogue" ("Id", "Designation", "Categorie", "Unite", "PrixUnitaire", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
7	Ciment	Gros œuvre	t	700000.00	2026-08-31 12:58:11.639395+02	\N	système	\N	f
8	Sable	Gros œuvre	m³	40000.00	2026-08-31 12:58:11.639395+02	\N	système	\N	f
9	Gravillon	Gros œuvre	m³	90000.00	2026-08-31 12:58:11.639395+02	\N	système	\N	f
10	Fer à béton Ø10	Ferraillage	barre	38000.00	2026-08-31 12:58:11.639395+02	\N	système	\N	f
11	Bois rond	Bois	unité	8500.00	2026-08-31 12:58:11.639395+02	\N	système	\N	f
12	Gasoil	Carburant	litre	5400.00	2026-08-31 12:58:11.639395+02	\N	système	\N	f
\.


--
-- Data for Name: Chantiers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Chantiers" ("Id", "Nom", "Code", "Localisation", "Responsable", "DateDebut", "DateFin", "Statut", "BudgetMateriel", "Reserve", "ReserveUtilisee", "Consommation", "PourcentageAvancement", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "MaterielTransfere", "Benefice", "MontantMarche") FROM stdin;
1	Construction de 6 forages PAAEP Diego	DIEG	Diego	Christophe	2026-07-15 02:00:00+02	2026-11-15 01:00:00+01	1	800000000.00	0.00	0.00	0.00	0	\N	2026-08-31 15:29:17.974664+02	2026-08-31 17:50:19.678439+02	admin@etam.mg	admin@etam.mg	f	0.00	2302330000.00	3102330000.00
\.


--
-- Data for Name: ComptesBancaires; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."ComptesBancaires" ("Id", "Nom", "Banque", "Numero", "Devise", "Solde", "EstActif", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "ChantierId", "Type") FROM stdin;
1	SARL ETAM	BGFI Madagascar	41000869011-66	Ar	620466000.00	t	2026-08-31 15:29:18.312846+02	2026-08-31 15:29:18.430565+02	admin@etam.mg	admin@etam.mg	f	1	0
\.


--
-- Data for Name: DataProtectionKeys; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DataProtectionKeys" ("Id", "FriendlyName", "Xml") FROM stdin;
1	key-e356f128-cd2b-465a-a447-135adc238aaf	<key id="e356f128-cd2b-465a-a447-135adc238aaf" version="1"><creationDate>2026-08-31T08:44:22.6778502Z</creationDate><activationDate>2026-08-31T08:44:22.6131105Z</activationDate><expirationDate>2026-11-29T08:44:22.6131105Z</expirationDate><descriptor deserializerType="Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60"><descriptor><encryption algorithm="AES_256_CBC" /><validation algorithm="HMACSHA256" /><masterKey p4:requiresEncryption="true" xmlns:p4="http://schemas.asp.net/2015/03/dataProtection"><!-- Warning: the key below is in an unencrypted form. --><value>b3Ch8fx0davOAqigCUpEXCKSCJ7+xzAmyxteAH2WuDTUcb21Hx9t2tDpKHM7L4zTPhteu65wkS4s6jmGoaHLXw==</value></masterKey></descriptor></descriptor></key>
\.


--
-- Data for Name: Decaissements; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Decaissements" ("Id", "PrevisionJournaliereId", "PrevisionLigneId", "Date", "Beneficiaire", "Motif", "Montant", "Mode", "CompteBancaireId", "BudgetConcerne", "Reference", "AccuseNom", "DateAccuse", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: Depenses; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Depenses" ("Id", "Date", "ChantierId", "PrevisionJournaliereId", "Categorie", "Designation", "Quantite", "PrixUnitaire", "BudgetConcerne", "Justificatif", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: DettesFournisseurs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DettesFournisseurs" ("Id", "FournisseurId", "ChantierId", "Libelle", "MontantInitial", "MontantPaye", "DateEcheance", "Statut", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: Fournisseurs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Fournisseurs" ("Id", "Nom", "Contact", "Telephone", "Adresse", "Nif", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: Materiaux; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Materiaux" ("Id", "ChantierId", "Categorie", "Designation", "Unite", "QuantiteCommandee", "QuantiteRecue", "QuantiteUtilisee", "SeuilMinimal", "PrixUnitaire", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "Besoin", "Localite") FROM stdin;
\.


--
-- Data for Name: MouvementsBancaires; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."MouvementsBancaires" ("Id", "CompteBancaireId", "Date", "Type", "Montant", "Beneficiaire", "Motif", "Reference", "ChantierId", "FournisseurId", "DetteFournisseurId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DemandePar", "EstValide") FROM stdin;
1	1	2026-08-31 15:29:18.36691+02	0	620466000.00	\N	Avance de demarrage 20%	ENC-DIEG	1	\N	\N	2026-08-31 15:29:18.430565+02	\N	admin@etam.mg	\N	f	\N	t
\.


--
-- Data for Name: MouvementsMateriau; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."MouvementsMateriau" ("Id", "MateriauxId", "DateMouvement", "BesoinOuObjectif", "QuantiteEntree", "QuantiteSortie", "Motif", "SoldeEnStock", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "SoldeSurBesoin") FROM stdin;
\.


--
-- Data for Name: Parametres; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Parametres" ("Id", "Cle", "Valeur", "Groupe", "Description", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
6	Entreprise.Nom	ETAM - Forage & Travaux Publics	Général	\N	2026-08-31 12:58:11.547727+02	\N	système	\N	f
7	Entreprise.Devise	Ar	Général	\N	2026-08-31 12:58:11.547727+02	\N	système	\N	f
8	Comptabilite.Exercice	2026	Comptabilité	\N	2026-08-31 12:58:11.547727+02	\N	système	\N	f
9	Alerte.SeuilBudgetPct	15	Alertes	\N	2026-08-31 12:58:11.547727+02	\N	système	\N	f
10	Alerte.SeuilReceptionPct	90	Alertes	\N	2026-08-31 12:58:11.547727+02	\N	système	\N	f
\.


--
-- Data for Name: PiecesJointes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PiecesJointes" ("Id", "PrevisionJournaliereId", "DecaissementId", "RapportTravailId", "NomFichier", "TypeMime", "Taille", "Contenu", "Description", "MontantFacture", "NumeroPiece", "Emetteur", "DateAjout", "AjouteParId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: PlansJournaliers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PlansJournaliers" ("Id", "PrevisionMensuelleId", "ChantierId", "Date", "MontantPrevu", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: PrevisionLignes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PrevisionLignes" ("Id", "PrevisionJournaliereId", "Designation", "Categorie", "TypeBudget", "MateriauId", "Quantite", "PrixUnitaireEstime", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DetteFournisseurId", "PrevisionGlobaleLigneId") FROM stdin;
1	1	vivre cadre	Consommable	1	\N	1.000	30000.00	\N	2026-08-31 17:44:06.096958+02	\N	admin@etam.mg	\N	f	\N	\N
2	2	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-08-31 17:48:39.927537+02	\N	admin@etam.mg	\N	f	\N	\N
3	2	Ceremonie traditionnelle de benediction (Joro be)	Consommable	1	\N	1.000	1950000.00	\N	2026-08-31 17:48:39.927537+02	\N	admin@etam.mg	\N	f	\N	\N
4	3	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-08-31 17:49:44.582969+02	\N	admin@etam.mg	\N	f	\N	\N
5	4	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-08-31 17:54:45.369024+02	\N	admin@etam.mg	\N	f	\N	\N
6	4	Loyer base juillet 2026	Consommable	1	\N	1.000	900000.00	\N	2026-08-31 17:54:45.369024+02	\N	admin@etam.mg	\N	f	\N	\N
7	5	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f	\N	\N
8	5	carburant voiture de liaison 	Carburant	1	\N	20.000	4860.00	\N	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f	\N	\N
9	5	Eau vive 	Eau	1	\N	2.000	20000.00	\N	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f	\N	\N
10	6	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f	\N	\N
11	6	carburant voiture de liaison 	Carburant	1	\N	20.000	4860.00	\N	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f	\N	\N
12	6	connexion christophe	Consommable	1	\N	1.000	25000.00	\N	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f	\N	\N
13	7	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f	\N	\N
14	7	location voiture de liaison du 003/08/2026	Location	1	\N	1.000	150000.00	\N	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f	\N	\N
15	7	carburant du 03/08/2026	Carburant	1	\N	10.000	4860.00	\N	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f	\N	\N
18	9	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f	\N	\N
19	9	Chemise dossier	Consommable	1	\N	10.000	2500.00	\N	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f	\N	\N
20	9	papier Ram	Consommable	1	\N	1.000	27000.00	\N	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f	\N	\N
21	9	reliure journal	Consommable	1	\N	1.000	12000.00	\N	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f	\N	\N
22	9	location vehicule du 05/08/2026	Consommable	1	\N	1.000	150000.00	\N	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f	\N	\N
23	9	Carburant vehicule du 05/08/2026	Carburant	1	\N	20.000	4860.00	\N	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f	\N	\N
24	10	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f	\N	\N
25	10	Rubalise(rouleau 10 m)	Consommable	1	\N	1.000	30000.00	\N	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f	\N	\N
26	10	connexion christophe	Consommable	1	\N	1.000	25000.00	\N	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f	\N	\N
27	10	location vehicule du 6/08/2026	Consommable	1	\N	1.000	150000.00	\N	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f	\N	\N
28	10	carburant vehicule du 06/08/2026	Carburant	1	\N	20.000	4860.00	\N	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f	\N	\N
29	11	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f	\N	\N
30	11	location vehicule du 06/08/2026	Location	1	\N	1.000	150000.00	\N	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f	\N	\N
31	11	carburant du 06/08/2026	Carburant	1	\N	10.000	4860.00	\N	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f	\N	\N
32	11	table bois dure 	Consommable	1	\N	2.000	200000.00	\N	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f	\N	\N
33	11	Seza bois dure	Consommable	1	\N	8.000	50000.00	\N	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f	\N	\N
34	12	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:33:31.7256+02	\N	admin@etam.mg	\N	f	\N	\N
35	13	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f	\N	\N
36	13	location bajaj  pour christophe , travaux Mahaleja aller-retour	Consommable	1	\N	1.000	60000.00	\N	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f	\N	\N
37	13	complémentarité budget Joro be  	Consommable	1	\N	1.000	350000.00	\N	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f	\N	\N
38	14	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f	\N	\N
39	14	Payement femme de menage du 27/07/2026au 02/08/2026	Consommable	1	\N	7.000	10000.00	\N	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f	\N	\N
40	14	Location bajaj pour christophe, travaux mahaleja aller-retour	Location	1	\N	1.000	60000.00	\N	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f	\N	\N
41	14	table imprimante	Consommable	1	\N	1.000	60000.00	\N	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f	\N	\N
42	15	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:43:50.711476+02	\N	admin@etam.mg	\N	f	\N	\N
43	16	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:44:56.581727+02	\N	admin@etam.mg	\N	f	\N	\N
44	16	bajaj deplacement  Mahaleja	Transport	1	\N	1.000	60000.00	\N	2026-08-31 18:44:56.581727+02	\N	admin@etam.mg	\N	f	\N	\N
45	17	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f	\N	\N
46	17	Payement femme de menage du 27/07/2026au 02/08/2026	Consommable	1	\N	7.000	10000.00	\N	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f	\N	\N
47	17	Location journee bajaj, deplacement demarrage ouverture acces  Mahaleja	Location	1	\N	1.000	70000.00	\N	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f	\N	\N
48	17	Achat gilet vert EPI pour mains d oeuvre ouverture acces au champ captant de Mahaleja 	Consommable	1	\N	10.000	10000.00	\N	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f	\N	\N
49	17	Achat gantde travail  EPI pour mains d oeuvre ouverture acces au champ captant de Mahaleja 	Consommable	1	\N	10.000	9000.00	\N	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f	\N	\N
50	17	Main d'oeuvre ouverture Mahaleja	Consommable	1	\N	20.000	10000.00	\N	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f	\N	\N
51	18	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f	\N	\N
52	18	Chaussure de securite N 40	Consommable	1	\N	2.000	55000.00	\N	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f	\N	\N
53	18	Ensemble de chantier	Consommable	1	\N	2.000	80000.00	\N	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f	\N	\N
54	19	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:54:10.950538+02	\N	admin@etam.mg	\N	f	\N	\N
55	19	location bajaj  deplacement a Mahaleja 	Location	1	\N	1.000	70000.00	\N	2026-08-31 18:54:10.950538+02	\N	admin@etam.mg	\N	f	\N	\N
56	20	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f	\N	\N
57	20	location bajaj , deplacement demarrage ouverture  acces mahaleja du 20/08/2026	Consommable	1	\N	1.000	70000.00	\N	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f	\N	\N
58	20	Achat gilet de reunion DT et Aina 	Consommable	1	\N	2.000	55000.00	\N	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f	\N	\N
59	20	Confection logo Etam epi Aina et christophe 	Consommable	1	\N	2.000	10000.00	\N	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f	\N	\N
60	20	main d'oeuvre ouverture chantier mahaleja	Consommable	1	\N	100.000	2800.00	\N	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f	\N	\N
61	20	tenuedetravail avec  logo etam	Consommable	1	\N	1.000	70000.00	\N	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f	\N	\N
62	21	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 19:28:45.845565+02	\N	admin@etam.mg	\N	f	\N	\N
63	22	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f	\N	\N
64	22	Complementarite achat chaussure de securite christophe et Aina	Consommable	1	\N	1.000	110000.00	\N	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f	\N	\N
65	22	Chaussure de securite Santatra QHSE n 36	Consommable	1	\N	1.000	110000.00	\N	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f	\N	\N
66	22	gilet Santatra QHSE avec logo	Consommable	1	\N	1.000	65000.00	\N	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f	\N	\N
67	22	ensemblechantier Santatra QHSE avec logo	Consommable	1	\N	1.000	80000.00	\N	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f	\N	\N
68	23	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f	\N	\N
69	23	Fandriana Santatra 	Consommable	1	\N	1.000	200000.00	\N	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f	\N	\N
70	23	Kidoro Santatra 	Consommable	1	\N	1.000	200000.00	\N	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f	\N	\N
71	23	Payement femme  de menage 	Consommable	1	\N	1.000	65000.00	\N	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f	\N	\N
16	8	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-08-31 18:20:25.811884+02	2026-09-01 10:16:54.271188+02	admin@etam.mg	admin@etam.mg	t	\N	\N
17	8	Prestation femme de menage du 27/07/2026au 02/08/2026	Consommable	1	\N	7.000	10000.00	\N	2026-08-31 18:20:25.811884+02	2026-09-01 10:16:54.271188+02	admin@etam.mg	admin@etam.mg	t	\N	\N
72	8	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-09-01 10:16:54.271188+02	\N	admin@etam.mg	\N	f	\N	\N
73	8	Prestation femme de menage du 27/07/2026au 02/08/2026	Consommable	1	\N	7.000	10000.00	\N	2026-09-01 10:16:54.271188+02	\N	admin@etam.mg	\N	f	\N	\N
74	24	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f	\N	\N
75	24	location vehicule du 25/08/2026 Aina, Christophe	Consommable	1	\N	1.000	200000.00	\N	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f	\N	\N
76	24	carburant vehicule du 25/08/2026	Consommable	1	\N	20.000	4860.00	\N	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f	\N	\N
77	25	Vivre Cadre 	Consommable	1	\N	1.000	40000.00	\N	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f	\N	\N
78	25	location vehicule Tojo du 25/08/2026	Consommable	1	\N	1.000	200000.00	\N	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f	\N	\N
79	25	Replein Tojo du 25/08/2026	Consommable	1	\N	1.000	81000.00	\N	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f	\N	\N
80	25	Transfert Tojo PAAEP hotel-aeroport du 26/082026	Consommable	1	\N	1.000	60000.00	\N	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f	\N	\N
81	26	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-09-01 10:28:04.938832+02	\N	admin@etam.mg	\N	f	\N	\N
82	26	carburant voiture de liaison 	Consommable	1	\N	20.000	4860.00	\N	2026-09-01 10:28:04.938832+02	\N	admin@etam.mg	\N	f	\N	\N
83	27	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-09-01 10:29:36.936664+02	\N	admin@etam.mg	\N	f	\N	\N
84	28	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f	\N	\N
85	28	Payement femme de menage du 27/07/2026au 02/08/2026	Consommable	1	\N	7.000	10000.00	\N	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f	\N	\N
86	28	carburant voiture de liaison 	Carburant	1	\N	20.000	4860.00	\N	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f	\N	\N
87	28	voiture de liaison du 14 au 17/07/2026	Transport	1	\N	4.000	150000.00	\N	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f	\N	\N
88	29	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-09-01 10:33:07.350314+02	\N	admin@etam.mg	\N	f	\N	\N
89	30	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-09-01 10:34:35.865304+02	\N	admin@etam.mg	\N	f	\N	\N
90	30	main d'oeuvre 	Consommable	1	\N	1.000	40000.00	\N	2026-09-01 10:34:35.865304+02	\N	admin@etam.mg	\N	f	\N	\N
91	31	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-09-01 10:35:07.361353+02	\N	admin@etam.mg	\N	f	\N	\N
92	32	Vivre Cadre 	Consommable	1	\N	1.000	30000.00	\N	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f	\N	\N
93	32	Payement femme de menage du 27/07/2026au 02/08/2026	Consommable	1	\N	7.000	10000.00	\N	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f	\N	\N
94	32	voiture de liaison du 20 au 21/07/2026	Transport	1	\N	2.000	150000.00	\N	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f	\N	\N
\.


--
-- Data for Name: PrevisionMensuelleLignes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PrevisionMensuelleLignes" ("Id", "PrevisionMensuelleId", "Rubrique", "Designation", "Montant", "PrevisionGlobaleLigneId", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: Previsions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Previsions" ("Id", "ChantierId", "DatePrevision", "Reference", "Statut", "SoumisePar", "DateSoumission", "ValideeParRfId", "DateValidationRf", "ValideeParAdminId", "DateValidationAdmin", "DateExecution", "MotifRefus", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DateRapport", "DateValidationRapport", "MotifRefusRapport", "RapportRealisation", "RapportValideParId", "AccuseNomSignataire", "AccuseReceptionParId", "DateAccuseReception", "MontantAccuse", "MontantDecaisse", "PrevisionMensuelleId", "PrevisionPrecedenteId", "ReportVeille", "PlanJournalierId") FROM stdin;
1	1	2026-07-28 02:00:00+02	PREV-DIEG-20260728-9938	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 17:44:06.096958+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
2	1	2026-07-30 02:00:00+02	PREV-DIEG-20260730-4254	0	\N	\N	\N	\N	\N	\N	\N	\N	Realisation de Joro be a mahaleza ; vivre du 30/07/2026	2026-08-31 17:48:39.927537+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
3	1	2026-07-29 02:00:00+02	PREV-DIEG-20260729-9012	0	\N	\N	\N	\N	\N	\N	\N	\N	vivre cadre du 29/07/2026	2026-08-31 17:49:44.582969+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
4	1	2026-07-31 02:00:00+02	PREV-DIEG-20260731-9417	0	\N	\N	\N	\N	\N	\N	\N	\N	loyer du mois juillet , vivre 	2026-08-31 17:54:45.369024+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
5	1	2026-07-16 02:00:00+02	PREV-DIEG-20260716-3922	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 17:57:18.696355+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
6	1	2026-07-17 02:00:00+02	PREV-DIEG-20260717-9537	0	\N	\N	\N	\N	\N	\N	\N	\N	éducationnel instruction des comitesde gestion des 4 systemes presidences(horaire de pompage, horaire d'exploitation) 	2026-08-31 18:01:06.618747+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
7	1	2026-08-03 02:00:00+02	PREV-DIEG-20260803-0592	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:18:38.041119+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
9	1	2026-08-05 02:00:00+02	PREV-DIEG-20260805-3804	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:24:00.189088+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
10	1	2026-08-06 02:00:00+02	PREV-DIEG-20260806-0670	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:27:23.475546+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
11	1	2026-08-07 02:00:00+02	PREV-DIEG-20260807-9175	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:30:37.037529+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
12	1	2026-08-08 02:00:00+02	PREV-DIEG-20260808-8743	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:33:31.7256+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
13	1	2026-08-10 02:00:00+02	PREV-DIEG-20260810-2467	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:36:53.02778+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
14	1	2026-08-12 02:00:00+02	PREV-DIEG-20260812-0096	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:39:43.808044+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
15	1	2026-08-13 02:00:00+02	PREV-DIEG-20260813-1089	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:43:50.711476+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
16	1	2026-08-15 02:00:00+02	PREV-DIEG-20260815-3143	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:44:56.581727+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
17	1	2026-08-17 02:00:00+02	PREV-DIEG-20260817-6319	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:51:14.697179+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
18	1	2026-08-19 02:00:00+02	PREV-DIEG-20260819-8547	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:52:50.201706+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
19	1	2026-08-20 02:00:00+02	PREV-DIEG-20260820-9917	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:54:10.950538+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
20	1	2026-08-19 02:00:00+02	PREV-DIEG-20260819-6703	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 19:28:06.90424+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
21	1	2026-08-20 02:00:00+02	PREV-DIEG-20260820-1923	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 19:28:45.845565+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
22	1	2026-08-21 02:00:00+02	PREV-DIEG-20260821-8624	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 19:38:24.751019+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
23	1	2026-08-24 02:00:00+02	PREV-DIEG-20260824-7864	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 19:45:34.438582+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
8	1	2026-08-04 02:00:00+02	PREV-DIEG-20260831-5942	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-08-31 18:20:25.811884+02	2026-09-01 10:16:54.271188+02	admin@etam.mg	admin@etam.mg	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
24	1	2026-08-25 02:00:00+02	PREV-DIEG-20260825-2768	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:24:04.122812+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
25	1	2026-08-26 02:00:00+02	PREV-DIEG-20260826-7821	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:26:33.535257+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
26	1	2026-07-21 02:00:00+02	PREV-DIEG-20260721-4123	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:28:04.938832+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
27	1	2026-07-22 02:00:00+02	PREV-DIEG-20260722-3080	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:29:36.936664+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
28	1	2026-07-20 02:00:00+02	PREV-DIEG-20260720-2093	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:32:06.938158+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
29	1	2026-07-23 02:00:00+02	PREV-DIEG-20260723-9047	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:33:07.350314+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
30	1	2026-07-24 02:00:00+02	PREV-DIEG-20260724-8930	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:34:35.865304+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
31	1	2026-07-25 02:00:00+02	PREV-DIEG-20260725-9942	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:35:07.361353+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
32	1	2026-07-27 02:00:00+02	PREV-DIEG-20260727-3045	0	\N	\N	\N	\N	\N	\N	\N	\N	\N	2026-09-01 10:38:52.750797+02	\N	admin@etam.mg	\N	f	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	\N	0.00	\N
\.


--
-- Data for Name: PrevisionsGlobales; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PrevisionsGlobales" ("Id", "ChantierId", "Reference", "DateCreation", "Statut", "Observation", "SoumisePar", "DateSoumission", "ValideeParRfId", "DateValidationRf", "ValideeParAdminId", "DateValidationAdmin", "MotifRefus", "DateMiseEnBanque", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: PrevisionsGlobalesLignes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PrevisionsGlobalesLignes" ("Id", "PrevisionGlobaleId", "Rubrique", "Designation", "Unite", "Quantite", "PrixUnitaire", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: PrevisionsMensuelles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."PrevisionsMensuelles" ("Id", "ChantierId", "PrevisionGlobaleId", "Annee", "Mois", "Reference", "MontantPrevu", "ReportMoisPrecedent", "MontantConsomme", "PrevisionMensuellePrecedenteId", "Statut", "SoumisePar", "DateSoumission", "ValideeParId", "DateValidation", "MotifRefus", "DateCloture", "ClotureeParId", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
1	1	\N	2026	9	PMENS-DIEG-202609	400000000.00	0.00	0.00	\N	1	admin@etam.mg	2026-08-31 15:54:48.387852+02	6c307f65-9df5-44f6-9974-790797d90b88	2026-08-31 15:54:58.189085+02	\N	\N	\N	enveloppe pour le premier mois 	2026-08-31 15:54:48.473023+02	2026-08-31 15:54:58.189247+02	admin@etam.mg	admin@etam.mg	f
\.


--
-- Data for Name: RapportTravailLignesAvancement; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RapportTravailLignesAvancement" ("Id", "RapportTravailId", "Zone", "TravauxRealises", "NiveauAvancement", "Observations", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: RapportTravailLignesEquipements; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RapportTravailLignesEquipements" ("Id", "RapportTravailId", "Equipement", "Etat", "Observation", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: RapportTravailLignesMateriaux; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RapportTravailLignesMateriaux" ("Id", "RapportTravailId", "Materiau", "Unite", "QuantiteUtilisee", "StockInitial", "Entree", "StockRestant", "Observations", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: RapportsTravail; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RapportsTravail" ("Id", "ChantierId", "Numero", "PeriodeDebut", "PeriodeFin", "Lieu", "EntrepriseExecutante", "ConducteurTravaux", "EffectifCadres", "EffectifOuvriers", "HoraireMatin", "HoraireApresMidi", "ConditionsMeteo", "ResumeSuiviPlanning", "ProblemesRencontres", "Suggestions", "Statut", "SoumisPar", "DateSoumission", "ValideParId", "DateValidation", "MotifRefus", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260706133148_InitialCreate	8.0.7
20260707100609_Tresorerie	8.0.7
20260707100634_Approvisionnement	8.0.7
20260714064138_RolesEtTransferts	8.0.7
20260714070147_FicheChantier	8.0.7
20260714132456_Catalogue	8.0.7
20260720143756_AddMouvementMateriau	8.0.7
20260720171821_FicheMateriauDetails	8.0.7
20260725181346_PrevisionGlobale	8.0.7
20260727064801_MarcheEtPrevisionGlobale	8.0.7
20260727072524_RapportTravauxEtAlertes	8.0.7
20260727080339_MagasinierParChantier	8.0.7
20260727120000_RattrapageRapportsTravail	8.0.7
20260810173109_PrevisionMensuelleEtDecaissements	8.0.7
20260812174306_PrevisionJournaliereEtablie	8.0.7
20260812175723_ClesDeChiffrement	8.0.7
20260813132739_PlanJournalierEtClesDeChiffrement	8.0.7
20260813141404_AutresDepensesJour	8.0.7
\.


--
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.aggregatedcounter_id_seq', 1, false);


--
-- Name: counter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.counter_id_seq', 1, false);


--
-- Name: hash_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.hash_id_seq', 1, false);


--
-- Name: job_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.job_id_seq', 1, false);


--
-- Name: jobparameter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.jobparameter_id_seq', 1, false);


--
-- Name: jobqueue_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.jobqueue_id_seq', 1, false);


--
-- Name: list_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.list_id_seq', 1, false);


--
-- Name: set_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.set_id_seq', 1, false);


--
-- Name: state_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: -
--

SELECT pg_catalog.setval('hangfire.state_id_seq', 1, false);


--
-- Name: Alertes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Alertes_Id_seq"', 1, false);


--
-- Name: ApprovisionnementLignes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."ApprovisionnementLignes_Id_seq"', 1, false);


--
-- Name: Approvisionnements_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Approvisionnements_Id_seq"', 1, false);


--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AspNetRoleClaims_Id_seq"', 1, false);


--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AspNetUserClaims_Id_seq"', 1, false);


--
-- Name: AuditLogs_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AuditLogs_Id_seq"', 173, true);


--
-- Name: AutresDepensesJour_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AutresDepensesJour_Id_seq"', 1, false);


--
-- Name: BudgetsComptes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."BudgetsComptes_Id_seq"', 2, true);


--
-- Name: Catalogue_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Catalogue_Id_seq"', 12, true);


--
-- Name: Chantiers_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Chantiers_Id_seq"', 1, true);


--
-- Name: ComptesBancaires_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."ComptesBancaires_Id_seq"', 1, true);


--
-- Name: DataProtectionKeys_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."DataProtectionKeys_Id_seq"', 1, true);


--
-- Name: Decaissements_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Decaissements_Id_seq"', 1, false);


--
-- Name: Depenses_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Depenses_Id_seq"', 1, false);


--
-- Name: DettesFournisseurs_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."DettesFournisseurs_Id_seq"', 1, false);


--
-- Name: Fournisseurs_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Fournisseurs_Id_seq"', 1, false);


--
-- Name: Materiaux_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Materiaux_Id_seq"', 1, false);


--
-- Name: MouvementsBancaires_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."MouvementsBancaires_Id_seq"', 1, true);


--
-- Name: MouvementsMateriau_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."MouvementsMateriau_Id_seq"', 1, false);


--
-- Name: Parametres_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Parametres_Id_seq"', 10, true);


--
-- Name: PiecesJointes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."PiecesJointes_Id_seq"', 1, false);


--
-- Name: PlansJournaliers_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."PlansJournaliers_Id_seq"', 1, false);


--
-- Name: PrevisionLignes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."PrevisionLignes_Id_seq"', 94, true);


--
-- Name: PrevisionMensuelleLignes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."PrevisionMensuelleLignes_Id_seq"', 1, false);


--
-- Name: PrevisionsGlobalesLignes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."PrevisionsGlobalesLignes_Id_seq"', 1, false);


--
-- Name: PrevisionsGlobales_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."PrevisionsGlobales_Id_seq"', 1, false);


--
-- Name: PrevisionsMensuelles_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."PrevisionsMensuelles_Id_seq"', 1, true);


--
-- Name: Previsions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Previsions_Id_seq"', 32, true);


--
-- Name: RapportTravailLignesAvancement_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."RapportTravailLignesAvancement_Id_seq"', 1, false);


--
-- Name: RapportTravailLignesEquipements_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."RapportTravailLignesEquipements_Id_seq"', 1, false);


--
-- Name: RapportTravailLignesMateriaux_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."RapportTravailLignesMateriaux_Id_seq"', 1, false);


--
-- Name: RapportsTravail_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."RapportsTravail_Id_seq"', 1, false);


--
-- Name: aggregatedcounter aggregatedcounter_key_key; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.aggregatedcounter
    ADD CONSTRAINT aggregatedcounter_key_key UNIQUE (key);


--
-- Name: aggregatedcounter aggregatedcounter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.aggregatedcounter
    ADD CONSTRAINT aggregatedcounter_pkey PRIMARY KEY (id);


--
-- Name: counter counter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.counter
    ADD CONSTRAINT counter_pkey PRIMARY KEY (id);


--
-- Name: hash hash_key_field_key; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.hash
    ADD CONSTRAINT hash_key_field_key UNIQUE (key, field);


--
-- Name: hash hash_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.hash
    ADD CONSTRAINT hash_pkey PRIMARY KEY (id);


--
-- Name: job job_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.job
    ADD CONSTRAINT job_pkey PRIMARY KEY (id);


--
-- Name: jobparameter jobparameter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.jobparameter
    ADD CONSTRAINT jobparameter_pkey PRIMARY KEY (id);


--
-- Name: jobqueue jobqueue_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.jobqueue
    ADD CONSTRAINT jobqueue_pkey PRIMARY KEY (id);


--
-- Name: list list_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.list
    ADD CONSTRAINT list_pkey PRIMARY KEY (id);


--
-- Name: lock lock_resource_key; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.lock
    ADD CONSTRAINT lock_resource_key UNIQUE (resource);

ALTER TABLE ONLY hangfire.lock REPLICA IDENTITY USING INDEX lock_resource_key;


--
-- Name: schema schema_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.schema
    ADD CONSTRAINT schema_pkey PRIMARY KEY (version);


--
-- Name: server server_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.server
    ADD CONSTRAINT server_pkey PRIMARY KEY (id);


--
-- Name: set set_key_value_key; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.set
    ADD CONSTRAINT set_key_value_key UNIQUE (key, value);


--
-- Name: set set_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.set
    ADD CONSTRAINT set_pkey PRIMARY KEY (id);


--
-- Name: state state_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.state
    ADD CONSTRAINT state_pkey PRIMARY KEY (id);


--
-- Name: Alertes PK_Alertes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Alertes"
    ADD CONSTRAINT "PK_Alertes" PRIMARY KEY ("Id");


--
-- Name: ApprovisionnementLignes PK_ApprovisionnementLignes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ApprovisionnementLignes"
    ADD CONSTRAINT "PK_ApprovisionnementLignes" PRIMARY KEY ("Id");


--
-- Name: Approvisionnements PK_Approvisionnements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Approvisionnements"
    ADD CONSTRAINT "PK_Approvisionnements" PRIMARY KEY ("Id");


--
-- Name: AspNetRoleClaims PK_AspNetRoleClaims; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetRoles PK_AspNetRoles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoles"
    ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");


--
-- Name: AspNetUserClaims PK_AspNetUserClaims; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetUserLogins PK_AspNetUserLogins; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");


--
-- Name: AspNetUserRoles PK_AspNetUserRoles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");


--
-- Name: AspNetUserTokens PK_AspNetUserTokens; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");


--
-- Name: AspNetUsers PK_AspNetUsers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");


--
-- Name: AuditLogs PK_AuditLogs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AuditLogs"
    ADD CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id");


--
-- Name: AutresDepensesJour PK_AutresDepensesJour; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AutresDepensesJour"
    ADD CONSTRAINT "PK_AutresDepensesJour" PRIMARY KEY ("Id");


--
-- Name: BudgetsComptes PK_BudgetsComptes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BudgetsComptes"
    ADD CONSTRAINT "PK_BudgetsComptes" PRIMARY KEY ("Id");


--
-- Name: Catalogue PK_Catalogue; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Catalogue"
    ADD CONSTRAINT "PK_Catalogue" PRIMARY KEY ("Id");


--
-- Name: Chantiers PK_Chantiers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Chantiers"
    ADD CONSTRAINT "PK_Chantiers" PRIMARY KEY ("Id");


--
-- Name: ComptesBancaires PK_ComptesBancaires; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ComptesBancaires"
    ADD CONSTRAINT "PK_ComptesBancaires" PRIMARY KEY ("Id");


--
-- Name: DataProtectionKeys PK_DataProtectionKeys; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."DataProtectionKeys"
    ADD CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY ("Id");


--
-- Name: Decaissements PK_Decaissements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Decaissements"
    ADD CONSTRAINT "PK_Decaissements" PRIMARY KEY ("Id");


--
-- Name: Depenses PK_Depenses; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Depenses"
    ADD CONSTRAINT "PK_Depenses" PRIMARY KEY ("Id");


--
-- Name: DettesFournisseurs PK_DettesFournisseurs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."DettesFournisseurs"
    ADD CONSTRAINT "PK_DettesFournisseurs" PRIMARY KEY ("Id");


--
-- Name: Fournisseurs PK_Fournisseurs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Fournisseurs"
    ADD CONSTRAINT "PK_Fournisseurs" PRIMARY KEY ("Id");


--
-- Name: Materiaux PK_Materiaux; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Materiaux"
    ADD CONSTRAINT "PK_Materiaux" PRIMARY KEY ("Id");


--
-- Name: MouvementsBancaires PK_MouvementsBancaires; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MouvementsBancaires"
    ADD CONSTRAINT "PK_MouvementsBancaires" PRIMARY KEY ("Id");


--
-- Name: MouvementsMateriau PK_MouvementsMateriau; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MouvementsMateriau"
    ADD CONSTRAINT "PK_MouvementsMateriau" PRIMARY KEY ("Id");


--
-- Name: Parametres PK_Parametres; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Parametres"
    ADD CONSTRAINT "PK_Parametres" PRIMARY KEY ("Id");


--
-- Name: PiecesJointes PK_PiecesJointes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PiecesJointes"
    ADD CONSTRAINT "PK_PiecesJointes" PRIMARY KEY ("Id");


--
-- Name: PlansJournaliers PK_PlansJournaliers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PlansJournaliers"
    ADD CONSTRAINT "PK_PlansJournaliers" PRIMARY KEY ("Id");


--
-- Name: PrevisionLignes PK_PrevisionLignes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionLignes"
    ADD CONSTRAINT "PK_PrevisionLignes" PRIMARY KEY ("Id");


--
-- Name: PrevisionMensuelleLignes PK_PrevisionMensuelleLignes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionMensuelleLignes"
    ADD CONSTRAINT "PK_PrevisionMensuelleLignes" PRIMARY KEY ("Id");


--
-- Name: Previsions PK_Previsions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Previsions"
    ADD CONSTRAINT "PK_Previsions" PRIMARY KEY ("Id");


--
-- Name: PrevisionsGlobales PK_PrevisionsGlobales; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsGlobales"
    ADD CONSTRAINT "PK_PrevisionsGlobales" PRIMARY KEY ("Id");


--
-- Name: PrevisionsGlobalesLignes PK_PrevisionsGlobalesLignes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsGlobalesLignes"
    ADD CONSTRAINT "PK_PrevisionsGlobalesLignes" PRIMARY KEY ("Id");


--
-- Name: PrevisionsMensuelles PK_PrevisionsMensuelles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsMensuelles"
    ADD CONSTRAINT "PK_PrevisionsMensuelles" PRIMARY KEY ("Id");


--
-- Name: RapportTravailLignesAvancement PK_RapportTravailLignesAvancement; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportTravailLignesAvancement"
    ADD CONSTRAINT "PK_RapportTravailLignesAvancement" PRIMARY KEY ("Id");


--
-- Name: RapportTravailLignesEquipements PK_RapportTravailLignesEquipements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportTravailLignesEquipements"
    ADD CONSTRAINT "PK_RapportTravailLignesEquipements" PRIMARY KEY ("Id");


--
-- Name: RapportTravailLignesMateriaux PK_RapportTravailLignesMateriaux; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportTravailLignesMateriaux"
    ADD CONSTRAINT "PK_RapportTravailLignesMateriaux" PRIMARY KEY ("Id");


--
-- Name: RapportsTravail PK_RapportsTravail; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportsTravail"
    ADD CONSTRAINT "PK_RapportsTravail" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: ix_hangfire_counter_expireat; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_counter_expireat ON hangfire.counter USING btree (expireat);


--
-- Name: ix_hangfire_counter_key; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_counter_key ON hangfire.counter USING btree (key);


--
-- Name: ix_hangfire_hash_expireat; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_hash_expireat ON hangfire.hash USING btree (expireat);


--
-- Name: ix_hangfire_job_expireat; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_job_expireat ON hangfire.job USING btree (expireat);


--
-- Name: ix_hangfire_job_statename; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_job_statename ON hangfire.job USING btree (statename);


--
-- Name: ix_hangfire_jobparameter_jobidandname; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_jobparameter_jobidandname ON hangfire.jobparameter USING btree (jobid, name);


--
-- Name: ix_hangfire_jobqueue_fetchedat_queue_jobid; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_jobqueue_fetchedat_queue_jobid ON hangfire.jobqueue USING btree (fetchedat NULLS FIRST, queue, jobid);


--
-- Name: ix_hangfire_jobqueue_jobidandqueue; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_jobqueue_jobidandqueue ON hangfire.jobqueue USING btree (jobid, queue);


--
-- Name: ix_hangfire_jobqueue_queueandfetchedat; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_jobqueue_queueandfetchedat ON hangfire.jobqueue USING btree (queue, fetchedat);


--
-- Name: ix_hangfire_list_expireat; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_list_expireat ON hangfire.list USING btree (expireat);


--
-- Name: ix_hangfire_set_expireat; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_set_expireat ON hangfire.set USING btree (expireat);


--
-- Name: ix_hangfire_set_key_score; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_set_key_score ON hangfire.set USING btree (key, score);


--
-- Name: ix_hangfire_state_jobid; Type: INDEX; Schema: hangfire; Owner: -
--

CREATE INDEX ix_hangfire_state_jobid ON hangfire.state USING btree (jobid);


--
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- Name: IX_Alertes_ChantierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Alertes_ChantierId" ON public."Alertes" USING btree ("ChantierId");


--
-- Name: IX_Alertes_EstLue_CreatedAt; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Alertes_EstLue_CreatedAt" ON public."Alertes" USING btree ("EstLue", "CreatedAt");


--
-- Name: IX_ApprovisionnementLignes_ApprovisionnementId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ApprovisionnementLignes_ApprovisionnementId" ON public."ApprovisionnementLignes" USING btree ("ApprovisionnementId");


--
-- Name: IX_ApprovisionnementLignes_DetteFournisseurId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ApprovisionnementLignes_DetteFournisseurId" ON public."ApprovisionnementLignes" USING btree ("DetteFournisseurId");


--
-- Name: IX_ApprovisionnementLignes_MateriauId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ApprovisionnementLignes_MateriauId" ON public."ApprovisionnementLignes" USING btree ("MateriauId");


--
-- Name: IX_Approvisionnements_ChantierId_DateAppro; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Approvisionnements_ChantierId_DateAppro" ON public."Approvisionnements" USING btree ("ChantierId", "DateAppro");


--
-- Name: IX_Approvisionnements_PrevisionJournaliereId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Approvisionnements_PrevisionJournaliereId" ON public."Approvisionnements" USING btree ("PrevisionJournaliereId");


--
-- Name: IX_Approvisionnements_Reference; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Approvisionnements_Reference" ON public."Approvisionnements" USING btree ("Reference");


--
-- Name: IX_AspNetRoleClaims_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");


--
-- Name: IX_AspNetUserClaims_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");


--
-- Name: IX_AspNetUserLogins_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");


--
-- Name: IX_AspNetUserRoles_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");


--
-- Name: IX_AuditLogs_DateAction; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AuditLogs_DateAction" ON public."AuditLogs" USING btree ("DateAction");


--
-- Name: IX_AutresDepensesJour_ChantierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AutresDepensesJour_ChantierId" ON public."AutresDepensesJour" USING btree ("ChantierId");


--
-- Name: IX_AutresDepensesJour_Date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AutresDepensesJour_Date" ON public."AutresDepensesJour" USING btree ("Date");


--
-- Name: IX_BudgetsComptes_Annee; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_BudgetsComptes_Annee" ON public."BudgetsComptes" USING btree ("Annee");


--
-- Name: IX_Catalogue_Designation; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Catalogue_Designation" ON public."Catalogue" USING btree ("Designation");


--
-- Name: IX_Chantiers_Code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Chantiers_Code" ON public."Chantiers" USING btree ("Code");


--
-- Name: IX_ComptesBancaires_ChantierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ComptesBancaires_ChantierId" ON public."ComptesBancaires" USING btree ("ChantierId");


--
-- Name: IX_Decaissements_CompteBancaireId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Decaissements_CompteBancaireId" ON public."Decaissements" USING btree ("CompteBancaireId");


--
-- Name: IX_Decaissements_Date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Decaissements_Date" ON public."Decaissements" USING btree ("Date");


--
-- Name: IX_Decaissements_PrevisionJournaliereId_Date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Decaissements_PrevisionJournaliereId_Date" ON public."Decaissements" USING btree ("PrevisionJournaliereId", "Date");


--
-- Name: IX_Decaissements_PrevisionLigneId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Decaissements_PrevisionLigneId" ON public."Decaissements" USING btree ("PrevisionLigneId");


--
-- Name: IX_Depenses_ChantierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Depenses_ChantierId" ON public."Depenses" USING btree ("ChantierId");


--
-- Name: IX_Depenses_Date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Depenses_Date" ON public."Depenses" USING btree ("Date");


--
-- Name: IX_Depenses_PrevisionJournaliereId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Depenses_PrevisionJournaliereId" ON public."Depenses" USING btree ("PrevisionJournaliereId");


--
-- Name: IX_DettesFournisseurs_ChantierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_DettesFournisseurs_ChantierId" ON public."DettesFournisseurs" USING btree ("ChantierId");


--
-- Name: IX_DettesFournisseurs_FournisseurId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_DettesFournisseurs_FournisseurId" ON public."DettesFournisseurs" USING btree ("FournisseurId");


--
-- Name: IX_DettesFournisseurs_Statut; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_DettesFournisseurs_Statut" ON public."DettesFournisseurs" USING btree ("Statut");


--
-- Name: IX_Fournisseurs_Nom; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Fournisseurs_Nom" ON public."Fournisseurs" USING btree ("Nom");


--
-- Name: IX_Materiaux_ChantierId_Designation; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Materiaux_ChantierId_Designation" ON public."Materiaux" USING btree ("ChantierId", "Designation");


--
-- Name: IX_MouvementsBancaires_ChantierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MouvementsBancaires_ChantierId" ON public."MouvementsBancaires" USING btree ("ChantierId");


--
-- Name: IX_MouvementsBancaires_CompteBancaireId_Date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MouvementsBancaires_CompteBancaireId_Date" ON public."MouvementsBancaires" USING btree ("CompteBancaireId", "Date");


--
-- Name: IX_MouvementsBancaires_DetteFournisseurId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MouvementsBancaires_DetteFournisseurId" ON public."MouvementsBancaires" USING btree ("DetteFournisseurId");


--
-- Name: IX_MouvementsBancaires_FournisseurId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MouvementsBancaires_FournisseurId" ON public."MouvementsBancaires" USING btree ("FournisseurId");


--
-- Name: IX_MouvementsMateriau_MateriauxId_DateMouvement; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MouvementsMateriau_MateriauxId_DateMouvement" ON public."MouvementsMateriau" USING btree ("MateriauxId", "DateMouvement" DESC);


--
-- Name: IX_Parametres_Cle; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Parametres_Cle" ON public."Parametres" USING btree ("Cle");


--
-- Name: IX_PiecesJointes_DecaissementId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PiecesJointes_DecaissementId" ON public."PiecesJointes" USING btree ("DecaissementId");


--
-- Name: IX_PiecesJointes_PrevisionJournaliereId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PiecesJointes_PrevisionJournaliereId" ON public."PiecesJointes" USING btree ("PrevisionJournaliereId");


--
-- Name: IX_PiecesJointes_RapportTravailId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PiecesJointes_RapportTravailId" ON public."PiecesJointes" USING btree ("RapportTravailId");


--
-- Name: IX_PlansJournaliers_ChantierId_Date; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_PlansJournaliers_ChantierId_Date" ON public."PlansJournaliers" USING btree ("ChantierId", "Date");


--
-- Name: IX_PlansJournaliers_PrevisionMensuelleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PlansJournaliers_PrevisionMensuelleId" ON public."PlansJournaliers" USING btree ("PrevisionMensuelleId");


--
-- Name: IX_PrevisionLignes_DetteFournisseurId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionLignes_DetteFournisseurId" ON public."PrevisionLignes" USING btree ("DetteFournisseurId");


--
-- Name: IX_PrevisionLignes_MateriauId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionLignes_MateriauId" ON public."PrevisionLignes" USING btree ("MateriauId");


--
-- Name: IX_PrevisionLignes_PrevisionGlobaleLigneId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionLignes_PrevisionGlobaleLigneId" ON public."PrevisionLignes" USING btree ("PrevisionGlobaleLigneId");


--
-- Name: IX_PrevisionLignes_PrevisionJournaliereId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionLignes_PrevisionJournaliereId" ON public."PrevisionLignes" USING btree ("PrevisionJournaliereId");


--
-- Name: IX_PrevisionMensuelleLignes_PrevisionGlobaleLigneId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionMensuelleLignes_PrevisionGlobaleLigneId" ON public."PrevisionMensuelleLignes" USING btree ("PrevisionGlobaleLigneId");


--
-- Name: IX_PrevisionMensuelleLignes_PrevisionMensuelleId_Rubrique; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionMensuelleLignes_PrevisionMensuelleId_Rubrique" ON public."PrevisionMensuelleLignes" USING btree ("PrevisionMensuelleId", "Rubrique");


--
-- Name: IX_PrevisionsGlobalesLignes_PrevisionGlobaleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionsGlobalesLignes_PrevisionGlobaleId" ON public."PrevisionsGlobalesLignes" USING btree ("PrevisionGlobaleId");


--
-- Name: IX_PrevisionsGlobales_ChantierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionsGlobales_ChantierId" ON public."PrevisionsGlobales" USING btree ("ChantierId");


--
-- Name: IX_PrevisionsMensuelles_ChantierId_Annee_Mois; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_PrevisionsMensuelles_ChantierId_Annee_Mois" ON public."PrevisionsMensuelles" USING btree ("ChantierId", "Annee", "Mois");


--
-- Name: IX_PrevisionsMensuelles_PrevisionGlobaleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionsMensuelles_PrevisionGlobaleId" ON public."PrevisionsMensuelles" USING btree ("PrevisionGlobaleId");


--
-- Name: IX_PrevisionsMensuelles_PrevisionMensuellePrecedenteId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_PrevisionsMensuelles_PrevisionMensuellePrecedenteId" ON public."PrevisionsMensuelles" USING btree ("PrevisionMensuellePrecedenteId");


--
-- Name: IX_PrevisionsMensuelles_Reference; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_PrevisionsMensuelles_Reference" ON public."PrevisionsMensuelles" USING btree ("Reference");


--
-- Name: IX_Previsions_ChantierId_DatePrevision; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Previsions_ChantierId_DatePrevision" ON public."Previsions" USING btree ("ChantierId", "DatePrevision");


--
-- Name: IX_Previsions_PlanJournalierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Previsions_PlanJournalierId" ON public."Previsions" USING btree ("PlanJournalierId");


--
-- Name: IX_Previsions_PrevisionMensuelleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Previsions_PrevisionMensuelleId" ON public."Previsions" USING btree ("PrevisionMensuelleId");


--
-- Name: IX_Previsions_PrevisionPrecedenteId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Previsions_PrevisionPrecedenteId" ON public."Previsions" USING btree ("PrevisionPrecedenteId");


--
-- Name: IX_Previsions_Reference; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Previsions_Reference" ON public."Previsions" USING btree ("Reference");


--
-- Name: IX_RapportTravailLignesAvancement_RapportTravailId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_RapportTravailLignesAvancement_RapportTravailId" ON public."RapportTravailLignesAvancement" USING btree ("RapportTravailId");


--
-- Name: IX_RapportTravailLignesEquipements_RapportTravailId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_RapportTravailLignesEquipements_RapportTravailId" ON public."RapportTravailLignesEquipements" USING btree ("RapportTravailId");


--
-- Name: IX_RapportTravailLignesMateriaux_RapportTravailId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_RapportTravailLignesMateriaux_RapportTravailId" ON public."RapportTravailLignesMateriaux" USING btree ("RapportTravailId");


--
-- Name: IX_RapportsTravail_ChantierId_PeriodeFin; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_RapportsTravail_ChantierId_PeriodeFin" ON public."RapportsTravail" USING btree ("ChantierId", "PeriodeFin");


--
-- Name: IX_RapportsTravail_Statut; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_RapportsTravail_Statut" ON public."RapportsTravail" USING btree ("Statut");


--
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");


--
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");


--
-- Name: jobparameter jobparameter_jobid_fkey; Type: FK CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.jobparameter
    ADD CONSTRAINT jobparameter_jobid_fkey FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: state state_jobid_fkey; Type: FK CONSTRAINT; Schema: hangfire; Owner: -
--

ALTER TABLE ONLY hangfire.state
    ADD CONSTRAINT state_jobid_fkey FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: Alertes FK_Alertes_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Alertes"
    ADD CONSTRAINT "FK_Alertes_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE SET NULL;


--
-- Name: ApprovisionnementLignes FK_ApprovisionnementLignes_Approvisionnements_Approvisionnemen~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ApprovisionnementLignes"
    ADD CONSTRAINT "FK_ApprovisionnementLignes_Approvisionnements_Approvisionnemen~" FOREIGN KEY ("ApprovisionnementId") REFERENCES public."Approvisionnements"("Id") ON DELETE CASCADE;


--
-- Name: ApprovisionnementLignes FK_ApprovisionnementLignes_DettesFournisseurs_DetteFournisseur~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ApprovisionnementLignes"
    ADD CONSTRAINT "FK_ApprovisionnementLignes_DettesFournisseurs_DetteFournisseur~" FOREIGN KEY ("DetteFournisseurId") REFERENCES public."DettesFournisseurs"("Id") ON DELETE SET NULL;


--
-- Name: ApprovisionnementLignes FK_ApprovisionnementLignes_Materiaux_MateriauId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ApprovisionnementLignes"
    ADD CONSTRAINT "FK_ApprovisionnementLignes_Materiaux_MateriauId" FOREIGN KEY ("MateriauId") REFERENCES public."Materiaux"("Id") ON DELETE SET NULL;


--
-- Name: Approvisionnements FK_Approvisionnements_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Approvisionnements"
    ADD CONSTRAINT "FK_Approvisionnements_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE RESTRICT;


--
-- Name: Approvisionnements FK_Approvisionnements_Previsions_PrevisionJournaliereId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Approvisionnements"
    ADD CONSTRAINT "FK_Approvisionnements_Previsions_PrevisionJournaliereId" FOREIGN KEY ("PrevisionJournaliereId") REFERENCES public."Previsions"("Id") ON DELETE SET NULL;


--
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AutresDepensesJour FK_AutresDepensesJour_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AutresDepensesJour"
    ADD CONSTRAINT "FK_AutresDepensesJour_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE SET NULL;


--
-- Name: ComptesBancaires FK_ComptesBancaires_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ComptesBancaires"
    ADD CONSTRAINT "FK_ComptesBancaires_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE SET NULL;


--
-- Name: Decaissements FK_Decaissements_ComptesBancaires_CompteBancaireId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Decaissements"
    ADD CONSTRAINT "FK_Decaissements_ComptesBancaires_CompteBancaireId" FOREIGN KEY ("CompteBancaireId") REFERENCES public."ComptesBancaires"("Id") ON DELETE RESTRICT;


--
-- Name: Decaissements FK_Decaissements_PrevisionLignes_PrevisionLigneId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Decaissements"
    ADD CONSTRAINT "FK_Decaissements_PrevisionLignes_PrevisionLigneId" FOREIGN KEY ("PrevisionLigneId") REFERENCES public."PrevisionLignes"("Id") ON DELETE SET NULL;


--
-- Name: Decaissements FK_Decaissements_Previsions_PrevisionJournaliereId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Decaissements"
    ADD CONSTRAINT "FK_Decaissements_Previsions_PrevisionJournaliereId" FOREIGN KEY ("PrevisionJournaliereId") REFERENCES public."Previsions"("Id") ON DELETE CASCADE;


--
-- Name: Depenses FK_Depenses_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Depenses"
    ADD CONSTRAINT "FK_Depenses_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE RESTRICT;


--
-- Name: Depenses FK_Depenses_Previsions_PrevisionJournaliereId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Depenses"
    ADD CONSTRAINT "FK_Depenses_Previsions_PrevisionJournaliereId" FOREIGN KEY ("PrevisionJournaliereId") REFERENCES public."Previsions"("Id") ON DELETE SET NULL;


--
-- Name: DettesFournisseurs FK_DettesFournisseurs_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."DettesFournisseurs"
    ADD CONSTRAINT "FK_DettesFournisseurs_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE SET NULL;


--
-- Name: DettesFournisseurs FK_DettesFournisseurs_Fournisseurs_FournisseurId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."DettesFournisseurs"
    ADD CONSTRAINT "FK_DettesFournisseurs_Fournisseurs_FournisseurId" FOREIGN KEY ("FournisseurId") REFERENCES public."Fournisseurs"("Id") ON DELETE CASCADE;


--
-- Name: Materiaux FK_Materiaux_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Materiaux"
    ADD CONSTRAINT "FK_Materiaux_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE CASCADE;


--
-- Name: MouvementsBancaires FK_MouvementsBancaires_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MouvementsBancaires"
    ADD CONSTRAINT "FK_MouvementsBancaires_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE SET NULL;


--
-- Name: MouvementsBancaires FK_MouvementsBancaires_ComptesBancaires_CompteBancaireId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MouvementsBancaires"
    ADD CONSTRAINT "FK_MouvementsBancaires_ComptesBancaires_CompteBancaireId" FOREIGN KEY ("CompteBancaireId") REFERENCES public."ComptesBancaires"("Id") ON DELETE CASCADE;


--
-- Name: MouvementsBancaires FK_MouvementsBancaires_DettesFournisseurs_DetteFournisseurId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MouvementsBancaires"
    ADD CONSTRAINT "FK_MouvementsBancaires_DettesFournisseurs_DetteFournisseurId" FOREIGN KEY ("DetteFournisseurId") REFERENCES public."DettesFournisseurs"("Id") ON DELETE SET NULL;


--
-- Name: MouvementsBancaires FK_MouvementsBancaires_Fournisseurs_FournisseurId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MouvementsBancaires"
    ADD CONSTRAINT "FK_MouvementsBancaires_Fournisseurs_FournisseurId" FOREIGN KEY ("FournisseurId") REFERENCES public."Fournisseurs"("Id") ON DELETE SET NULL;


--
-- Name: MouvementsMateriau FK_MouvementsMateriau_Materiaux_MateriauxId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MouvementsMateriau"
    ADD CONSTRAINT "FK_MouvementsMateriau_Materiaux_MateriauxId" FOREIGN KEY ("MateriauxId") REFERENCES public."Materiaux"("Id") ON DELETE CASCADE;


--
-- Name: PiecesJointes FK_PiecesJointes_Decaissements_DecaissementId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PiecesJointes"
    ADD CONSTRAINT "FK_PiecesJointes_Decaissements_DecaissementId" FOREIGN KEY ("DecaissementId") REFERENCES public."Decaissements"("Id") ON DELETE CASCADE;


--
-- Name: PiecesJointes FK_PiecesJointes_Previsions_PrevisionJournaliereId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PiecesJointes"
    ADD CONSTRAINT "FK_PiecesJointes_Previsions_PrevisionJournaliereId" FOREIGN KEY ("PrevisionJournaliereId") REFERENCES public."Previsions"("Id") ON DELETE CASCADE;


--
-- Name: PiecesJointes FK_PiecesJointes_RapportsTravail_RapportTravailId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PiecesJointes"
    ADD CONSTRAINT "FK_PiecesJointes_RapportsTravail_RapportTravailId" FOREIGN KEY ("RapportTravailId") REFERENCES public."RapportsTravail"("Id") ON DELETE CASCADE;


--
-- Name: PlansJournaliers FK_PlansJournaliers_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PlansJournaliers"
    ADD CONSTRAINT "FK_PlansJournaliers_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE RESTRICT;


--
-- Name: PlansJournaliers FK_PlansJournaliers_PrevisionsMensuelles_PrevisionMensuelleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PlansJournaliers"
    ADD CONSTRAINT "FK_PlansJournaliers_PrevisionsMensuelles_PrevisionMensuelleId" FOREIGN KEY ("PrevisionMensuelleId") REFERENCES public."PrevisionsMensuelles"("Id") ON DELETE CASCADE;


--
-- Name: PrevisionLignes FK_PrevisionLignes_DettesFournisseurs_DetteFournisseurId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionLignes"
    ADD CONSTRAINT "FK_PrevisionLignes_DettesFournisseurs_DetteFournisseurId" FOREIGN KEY ("DetteFournisseurId") REFERENCES public."DettesFournisseurs"("Id") ON DELETE SET NULL;


--
-- Name: PrevisionLignes FK_PrevisionLignes_Materiaux_MateriauId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionLignes"
    ADD CONSTRAINT "FK_PrevisionLignes_Materiaux_MateriauId" FOREIGN KEY ("MateriauId") REFERENCES public."Materiaux"("Id") ON DELETE SET NULL;


--
-- Name: PrevisionLignes FK_PrevisionLignes_PrevisionsGlobalesLignes_PrevisionGlobaleLi~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionLignes"
    ADD CONSTRAINT "FK_PrevisionLignes_PrevisionsGlobalesLignes_PrevisionGlobaleLi~" FOREIGN KEY ("PrevisionGlobaleLigneId") REFERENCES public."PrevisionsGlobalesLignes"("Id") ON DELETE SET NULL;


--
-- Name: PrevisionLignes FK_PrevisionLignes_Previsions_PrevisionJournaliereId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionLignes"
    ADD CONSTRAINT "FK_PrevisionLignes_Previsions_PrevisionJournaliereId" FOREIGN KEY ("PrevisionJournaliereId") REFERENCES public."Previsions"("Id") ON DELETE CASCADE;


--
-- Name: PrevisionMensuelleLignes FK_PrevisionMensuelleLignes_PrevisionsGlobalesLignes_Prevision~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionMensuelleLignes"
    ADD CONSTRAINT "FK_PrevisionMensuelleLignes_PrevisionsGlobalesLignes_Prevision~" FOREIGN KEY ("PrevisionGlobaleLigneId") REFERENCES public."PrevisionsGlobalesLignes"("Id") ON DELETE SET NULL;


--
-- Name: PrevisionMensuelleLignes FK_PrevisionMensuelleLignes_PrevisionsMensuelles_PrevisionMens~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionMensuelleLignes"
    ADD CONSTRAINT "FK_PrevisionMensuelleLignes_PrevisionsMensuelles_PrevisionMens~" FOREIGN KEY ("PrevisionMensuelleId") REFERENCES public."PrevisionsMensuelles"("Id") ON DELETE CASCADE;


--
-- Name: PrevisionsGlobalesLignes FK_PrevisionsGlobalesLignes_PrevisionsGlobales_PrevisionGlobal~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsGlobalesLignes"
    ADD CONSTRAINT "FK_PrevisionsGlobalesLignes_PrevisionsGlobales_PrevisionGlobal~" FOREIGN KEY ("PrevisionGlobaleId") REFERENCES public."PrevisionsGlobales"("Id") ON DELETE CASCADE;


--
-- Name: PrevisionsGlobales FK_PrevisionsGlobales_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsGlobales"
    ADD CONSTRAINT "FK_PrevisionsGlobales_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE CASCADE;


--
-- Name: PrevisionsMensuelles FK_PrevisionsMensuelles_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsMensuelles"
    ADD CONSTRAINT "FK_PrevisionsMensuelles_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE RESTRICT;


--
-- Name: PrevisionsMensuelles FK_PrevisionsMensuelles_PrevisionsGlobales_PrevisionGlobaleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsMensuelles"
    ADD CONSTRAINT "FK_PrevisionsMensuelles_PrevisionsGlobales_PrevisionGlobaleId" FOREIGN KEY ("PrevisionGlobaleId") REFERENCES public."PrevisionsGlobales"("Id") ON DELETE SET NULL;


--
-- Name: PrevisionsMensuelles FK_PrevisionsMensuelles_PrevisionsMensuelles_PrevisionMensuell~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PrevisionsMensuelles"
    ADD CONSTRAINT "FK_PrevisionsMensuelles_PrevisionsMensuelles_PrevisionMensuell~" FOREIGN KEY ("PrevisionMensuellePrecedenteId") REFERENCES public."PrevisionsMensuelles"("Id") ON DELETE RESTRICT;


--
-- Name: Previsions FK_Previsions_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Previsions"
    ADD CONSTRAINT "FK_Previsions_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE RESTRICT;


--
-- Name: Previsions FK_Previsions_PlansJournaliers_PlanJournalierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Previsions"
    ADD CONSTRAINT "FK_Previsions_PlansJournaliers_PlanJournalierId" FOREIGN KEY ("PlanJournalierId") REFERENCES public."PlansJournaliers"("Id");


--
-- Name: Previsions FK_Previsions_PrevisionsMensuelles_PrevisionMensuelleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Previsions"
    ADD CONSTRAINT "FK_Previsions_PrevisionsMensuelles_PrevisionMensuelleId" FOREIGN KEY ("PrevisionMensuelleId") REFERENCES public."PrevisionsMensuelles"("Id") ON DELETE RESTRICT;


--
-- Name: Previsions FK_Previsions_Previsions_PrevisionPrecedenteId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Previsions"
    ADD CONSTRAINT "FK_Previsions_Previsions_PrevisionPrecedenteId" FOREIGN KEY ("PrevisionPrecedenteId") REFERENCES public."Previsions"("Id") ON DELETE RESTRICT;


--
-- Name: RapportTravailLignesAvancement FK_RapportTravailLignesAvancement_RapportsTravail_RapportTrava~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportTravailLignesAvancement"
    ADD CONSTRAINT "FK_RapportTravailLignesAvancement_RapportsTravail_RapportTrava~" FOREIGN KEY ("RapportTravailId") REFERENCES public."RapportsTravail"("Id") ON DELETE CASCADE;


--
-- Name: RapportTravailLignesEquipements FK_RapportTravailLignesEquipements_RapportsTravail_RapportTrav~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportTravailLignesEquipements"
    ADD CONSTRAINT "FK_RapportTravailLignesEquipements_RapportsTravail_RapportTrav~" FOREIGN KEY ("RapportTravailId") REFERENCES public."RapportsTravail"("Id") ON DELETE CASCADE;


--
-- Name: RapportTravailLignesMateriaux FK_RapportTravailLignesMateriaux_RapportsTravail_RapportTravai~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportTravailLignesMateriaux"
    ADD CONSTRAINT "FK_RapportTravailLignesMateriaux_RapportsTravail_RapportTravai~" FOREIGN KEY ("RapportTravailId") REFERENCES public."RapportsTravail"("Id") ON DELETE CASCADE;


--
-- Name: RapportsTravail FK_RapportsTravail_Chantiers_ChantierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RapportsTravail"
    ADD CONSTRAINT "FK_RapportsTravail_Chantiers_ChantierId" FOREIGN KEY ("ChantierId") REFERENCES public."Chantiers"("Id") ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

\unrestrict NC40AjHHGkH87Ya8fX4nyZFvjAnEb5fcfB0wxnHcTvkQlQcIJpdpVhJ5Fze1qNs

