# Déployer ETAM ERP sur Render

## Avant de commencer

Vérifiez qu'aucun secret n'est présent dans le dépôt :

- `src/ETAM.Web/appsettings.json` → la chaîne de connexion doit être **vide**
- `src/ETAM.Web/appsettings.Development.json` → contient votre mot de passe local, il est **ignoré par Git**

Si le fichier `appsettings.Development.json` a déjà été committé auparavant, retirez-le du suivi :

```bash
git rm --cached src/ETAM.Web/appsettings.Development.json
git commit -m "Retire la configuration locale du suivi Git"
```

## 1. Envoyer le code sur GitHub

```bash
cd C:\Users\ASUS\Documents\Etam_export
git init
git add .
git commit -m "ETAM ERP"
git branch -M main
git remote add origin https://github.com/VOTRE-COMPTE/etam-erp.git
git push -u origin main
```

## 2. Créer les services sur Render

Le fichier `render.yaml` décrit déjà tout (base PostgreSQL + application).

1. Allez sur [dashboard.render.com](https://dashboard.render.com) → **New** → **Blueprint**
2. Connectez votre dépôt GitHub `etam-erp`
3. Render détecte `render.yaml` et propose de créer :
   - `etam-db` — base PostgreSQL
   - `etam-erp` — l'application web (Docker)
4. Cliquez **Apply**

Le premier déploiement prend 5 à 10 minutes (compilation .NET).

## 3. Récupérer les mots de passe initiaux

Render génère automatiquement les mots de passe des comptes. Pour les lire :

**Dashboard → etam-erp → Environment**

Vous y trouverez :

| Variable | Compte |
|---|---|
| `ETAM_ADMIN_PASSWORD` | admin@etam.mg |
| `ETAM_RF_PASSWORD` | rf@etam.mg |
| `ETAM_CHEF_PASSWORD` | chef@etam.mg |
| `ETAM_MAGASINIER_PASSWORD` | magasinier@etam.mg |

Cliquez sur l'icône œil pour révéler chaque valeur.

## 4. Se connecter

Ouvrez l'URL fournie par Render (`https://etam-erp.onrender.com`) et connectez-vous avec
`admin@etam.mg` et le mot de passe `ETAM_ADMIN_PASSWORD`.

La base est créée automatiquement au premier démarrage (migrations + chantier d'exemple NOSY BE).

## 5. Après la mise en ligne — à faire immédiatement

1. **Changez les mots de passe** des quatre comptes depuis l'application
2. Créez vos vrais chantiers (le chantier NOSY BE d'exemple peut être supprimé)
3. Créez un magasinier par chantier (Utilisateurs → Nouvel utilisateur → rôle Magasinier + chantier)

## Bon à savoir sur l'offre gratuite

- L'application se met en veille après 15 minutes sans visite ; la première visite suivante prend ~50 secondes à répondre.
- La base PostgreSQL gratuite **expire après 90 jours**. Prévoyez une sauvegarde ou le passage à l'offre payante (7 $/mois) avant l'échéance.
- Sauvegarde manuelle de la base :

```bash
pg_dump "URL_EXTERNE_FOURNIE_PAR_RENDER" > sauvegarde_etam.sql
```

## Variables d'environnement (référence)

| Variable | Rôle |
|---|---|
| `ETAM_CONNECTION` | Chaîne de connexion PostgreSQL (remplie automatiquement par Render) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `RENDER` | `true` — désactive la redirection HTTPS interne (gérée par le proxy Render) |
| `ETAM_*_PASSWORD` | Mots de passe initiaux des comptes créés au premier démarrage |

L'application accepte la chaîne de connexion aussi bien au format `postgresql://…` (Render)
qu'au format classique `Host=…;Username=…`.
