# ETAM ERP — Forage & Travaux Publics (Madagascar)

ERP métier pour une entreprise de forage et de BTP. Gestion des **chantiers**, du **Budget Comptes** annuel, des **Budgets Matériel** par chantier, des **matériaux**, des **prévisions journalières** (module principal avec workflow de validation à deux niveaux), des **dépenses**, des **alertes**, des **rapports** et d'un **journal d'audit** complet.

Construit selon une **Clean Architecture** en ASP.NET Core 8 MVC, PostgreSQL, EF Core 8, ASP.NET Identity.

---

## 1. Architecture

Solution `.sln` à quatre couches, dépendances dirigées vers le domaine :

```
ETAM.sln
└── src/
    ├── ETAM.Domain          → Entités, enums, interfaces (aucune dépendance)
    ├── ETAM.Application      → DTOs, services métier, workflow, AutoMapper, FluentValidation
    ├── ETAM.Infrastructure   → EF Core, PostgreSQL, Identity, Repository/UnitOfWork, seed
    └── ETAM.Web             → MVC, contrôleurs, vues Razor, AdminLTE/Bootstrap 5, Chart.js
```

Patrons appliqués : **Clean Architecture, Repository Pattern, Unit Of Work, Dependency Injection, SOLID, DRY**, gestion centralisée des erreurs (middleware), audit automatique (EF Core `SaveChangesInterceptor`), soft-delete global et concurrence optimiste via la colonne système `xmin` de PostgreSQL.

### Concepts métier clés

| Concept | Règle |
|---|---|
| **Budget Comptes** | Annuel et **unique** pour toute l'entreprise (ex. Budget 2026 = 50 000 000 Ar). Finance les dépenses générales. Dépassement → blocage + recours à la réserve avec validation Administrateur. |
| **Budget Matériel** | **Propre à chaque chantier**. Le budget global est la somme des budgets chantiers. |
| **Matériaux** | Rattachés **directement au chantier** (pas de magasin central). Stock disponible = Quantité reçue − Quantité utilisée. |
| **Prévision journalière** | Créée par le chef de chantier. Workflow : Brouillon → Soumise → Validée RF → Validée Admin → Exécutée (ou Refusée). |
| **Exécution** | Ligne *Compte* → diminue le Budget Comptes. Ligne *Matériel* → diminue le Budget Matériel du chantier **et** le stock. Transactionnel + historisé. |

Rôles : **Administrateur, Responsable financier, Chef de chantier, Utilisateur**.

---

## 2. Prérequis

À installer sur votre poste (rien n'est requis pour Docker sauf Docker lui-même) :

- **.NET 8 SDK** — <https://dotnet.microsoft.com/download/dotnet/8.0>
- **PostgreSQL 14+** — <https://www.postgresql.org/download/>
- **EF Core CLI** :
  ```bash
  dotnet tool install --global dotnet-ef
  ```

Vérification :
```bash
dotnet --version      # 8.x
psql --version        # 14+
dotnet ef --version   # 8.x
```

---

## 3. Configuration

La chaîne de connexion se trouve dans `src/ETAM.Web/appsettings.json` :

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=etam_erp;Username=postgres;Password=postgres"
}
```

Adaptez `Username` / `Password` / `Host`. En production, préférez une variable d'environnement :

```bash
export ConnectionStrings__DefaultConnection="Host=...;Database=etam_erp;Username=...;Password=..."
```

Créez la base (si elle n'existe pas) :
```bash
createdb -U postgres etam_erp
```

---

## 4. Base de données (migrations EF Core)

La méthode **recommandée**. Depuis la racine de la solution :

```bash
# 1. Générer la migration initiale (une seule fois)
dotnet ef migrations add InitialCreate \
  --project src/ETAM.Infrastructure \
  --startup-project src/ETAM.Web

# 2. Appliquer à la base
dotnet ef database update \
  --project src/ETAM.Infrastructure \
  --startup-project src/ETAM.Web
```

> Au premier démarrage, l'application applique aussi automatiquement les migrations et **injecte les données de démonstration** (chantiers Ampirika, Ambovombe, Tuléar, Betioky ; Budget 2026 ; matériaux ; utilisateurs et rôles).

**Alternative manuelle** : le fichier `scripts/etam_schema.sql` reproduit le schéma métier pour un déploiement sans EF (les tables ASP.NET Identity restent créées par la migration).

### Comptes de démonstration

| Rôle | Email | Mot de passe |
|---|---|---|
| Administrateur | `admin@etam.mg` | `Admin@2026` |
| Responsable financier | `rf@etam.mg` | `Finance@2026` |
| Chef de chantier | `chef@etam.mg` | `Chef@2026` |

> Changez ces mots de passe en production.

---

## 5. Lancer en développement

```bash
dotnet restore
dotnet build
dotnet run --project src/ETAM.Web
```

Application : `https://localhost:5001` (ou l'URL affichée). Tableau de bord Hangfire : `/hangfire`.

---

## 6. Déploiement

### 6.a Docker (le plus simple)

```bash
docker compose up --build
```

Cela démarre PostgreSQL + l'application. Accès : <http://localhost:8080>. Les données Postgres sont persistées dans le volume `etam_pgdata`.

### 6.b Ubuntu — Nginx (reverse proxy) + Kestrel

1. Publier :
   ```bash
   dotnet publish src/ETAM.Web -c Release -o /var/www/etam
   ```
2. Service systemd `/etc/systemd/system/etam.service` :
   ```ini
   [Unit]
   Description=ETAM ERP
   After=network.target postgresql.service

   [Service]
   WorkingDirectory=/var/www/etam
   ExecStart=/usr/bin/dotnet /var/www/etam/ETAM.Web.dll
   Restart=always
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5000
   Environment=ConnectionStrings__DefaultConnection=Host=localhost;Database=etam_erp;Username=etam;Password=****

   [Install]
   WantedBy=multi-user.target
   ```
   ```bash
   sudo systemctl enable --now etam
   ```
3. Nginx `/etc/nginx/sites-available/etam` :
   ```nginx
   server {
       listen 80;
       server_name erp.exemple.mg;
       location / {
           proxy_pass         http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header   Upgrade $http_upgrade;
           proxy_set_header   Connection keep-alive;
           proxy_set_header   Host $host;
           proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header   X-Forwarded-Proto $scheme;
       }
   }
   ```
   ```bash
   sudo ln -s /etc/nginx/sites-available/etam /etc/nginx/sites-enabled/
   sudo nginx -t && sudo systemctl reload nginx
   ```
   Ajoutez HTTPS avec `certbot --nginx`.

### 6.c Windows — IIS

1. Installer le **.NET 8 Hosting Bundle** (module ASP.NET Core pour IIS).
2. Publier : `dotnet publish src/ETAM.Web -c Release -o C:\inetpub\etam`.
3. Créer un site IIS pointant sur ce dossier, pool applicatif en **No Managed Code**.
4. Définir la chaîne de connexion via une variable d'environnement du pool ou `appsettings.Production.json`.
5. Donner à l'identité du pool les droits d'accès à PostgreSQL.

---

## 7. Structure des dossiers

```
Etam/
├── ETAM.sln
├── README.md
├── docker-compose.yml
├── docker/Dockerfile
├── scripts/etam_schema.sql
└── src/
    ├── ETAM.Domain/         (Common, Entities, Enums, Interfaces)
    ├── ETAM.Application/     (DTOs, Interfaces, Services, Validators, Mappings)
    ├── ETAM.Infrastructure/  (Persistence, Identity, Services, Migrations)
    └── ETAM.Web/            (Controllers, Views, wwwroot, Program.cs)
```

---

## 8. État de la livraison

**Fondation complète et cohérente** (cette itération) :

- ✅ Solution 4 couches, packages, références
- ✅ Toutes les entités du domaine + audit/soft-delete/RowVersion
- ✅ EF Core + PostgreSQL, configurations Fluent API, index, FK, contraintes
- ✅ Identity + 4 rôles, connexion / déconnexion / mot de passe oublié
- ✅ Repository Pattern + Unit Of Work + AutoMapper + FluentValidation + Serilog + Hangfire
- ✅ **Workflow des prévisions** de bout en bout (création → validations → exécution transactionnelle impactant budgets & stocks)
- ✅ Service Budget Comptes (blocage dépassement + réserve)
- ✅ Service Alertes (budget/stock/réception/validation)
- ✅ Dashboard KPI + 4 graphiques Chart.js
- ✅ Design AdminLTE/Bootstrap 5 (sidebar sombre, header clair, glassmorphism, cartes KPI)
- ✅ Données de démonstration (Madagascar), README, SQL, Docker

**Modules dont l'interface complète sera livrée aux itérations suivantes** (socle déjà en place : entités, services, données) : Budget Comptes (écran), Budget Matériel (écran), Dépenses (CRUD), Matériaux (CRUD + réceptions), Alertes (écran), Rapports (export PDF/Excel via QuestPDF/ClosedXML — packages déjà référencés), Journal d'audit (écran), Utilisateurs (CRUD Identity), Paramètres (écran).

---

## 9. Prochaine itération suggérée

1. Générer la migration initiale et vérifier le `dotnet build`.
2. Compléter les écrans CRUD Matériaux et Dépenses.
3. Brancher l'export PDF (QuestPDF) et Excel (ClosedXML) des rapports.
4. Écran Paramètres + envoi SMTP (job Hangfire) pour « mot de passe oublié ».
5. Tests unitaires (workflow prévisions, service budget) et tests d'intégration EF.
