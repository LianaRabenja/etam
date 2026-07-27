# Formation ETAM ERP — du problème à la solution

Document de référence pour comprendre et présenter le projet.

---

## 1. Le projet en une phrase

> ETAM ERP est un logiciel qui suit l'argent d'un chantier depuis la signature du marché
> jusqu'à la dernière dépense, et qui **empêche qu'un franc sorte sans être justifié**.

C'est la phrase à dire en premier devant votre patron. Tout le reste en découle.

---

## 2. D'où vient le besoin

ETAM est une entreprise de forage et de travaux publics à Madagascar. Elle gère
plusieurs chantiers simultanément (Tuléar, Nosy Be, Ampirika, Ambovombe…), avec des
équipes de plusieurs dizaines d'ouvriers, loin du siège.

**Le problème réel :** l'argent part vers les chantiers, et personne ne peut vérifier
facilement ce qu'il devient. Un chef de chantier peut demander de l'argent lundi, ne
rien justifier, et redemander mardi. Le patron s'en aperçoit trop tard, quand le budget
est consommé.

**Contrainte supplémentaire :** le patron n'est pas informaticien. Un tableau de bord
complexe ne sert à rien s'il ne peut pas le lire en trente secondes.

Avant le logiciel, tout se faisait sur des feuilles manuscrites : fiche de gestion des
matériaux, prévision journalière, journal de banque, bon de commande. **Ces feuilles
sont la vraie spécification du projet** — l'application les reproduit à l'écran.

---

## 3. Le vocabulaire — les 8 mots à maîtriser

Ces mots sont ceux du métier ETAM, pas des mots informatiques. Il faut les employer
exactement comme le patron les emploie.

| Mot | Ce que ça veut dire |
|---|---|
| **Marché** | Le montant total du contrat signé avec le client. Ex. 150 000 000 Ar. |
| **Bénéfice** | La marge que l'entreprise garde. Ex. 80 000 000 Ar. |
| **Budget projet** | Ce qui reste pour faire les travaux : marché − bénéfice. Ex. 70 000 000 Ar. |
| **Prévision globale** | Le plan de dépense du budget projet, ventilé par rubriques. |
| **Rubrique** | Un poste de dépense libre : Approvisionnement, Main d'œuvre, Imprévus, Transport. |
| **Approvisionnement** | La demande de besoins faite par le chef de chantier. |
| **Prévision journalière** | La demande d'argent d'un jour précis, qui sort réellement de la banque. |
| **Rapport de prévision** | Le compte rendu de ce qui a été fait avec cet argent. |

---

## 4. Le flux d'argent — le cœur du projet

C'est le mécanisme central. Sept étapes.

### Étape 1 — Le marché entre en banque

Quand on crée un chantier, on saisit deux chiffres : le **montant du marché** (150 M) et
le **bénéfice** (80 M). Le budget projet (70 M) se calcule tout seul.

Le marché est immédiatement déposé sur le compte bancaire du chantier. **Bénéfice et
budget projet restent sur le même compte** — la séparation est comptable, pas bancaire.
C'est un point que nous avons corrigé en cours de projet.

### Étape 2 — La prévision globale planifie le budget

Juste après la création du chantier, on saisit la prévision globale : comment les
70 M vont être dépensés, rubrique par rubrique.

```
Approvisionnement ....... 40 000 000
   Ciment      30 t  × 700 000  = 21 000 000
   Sable      100 m³ ×  40 000  =  4 000 000
   Fer        200 br ×  38 000  =  7 600 000
   Gravillon   84 m³ ×  90 000  =  7 560 000

Main d'œuvre ............ 10 000 000
   Maçons, Plombiers, Électricien, Peintre

Imprévus ................  5 000 000
Transport ............... 15 000 000
```

L'écran affiche en direct l'**écart** entre le total saisi et le budget projet :
« reste X » en vert, « dépassement X » en rouge.

Cette prévision suit un circuit de validation : Brouillon → Soumise → Validée par le
Correspondant → Validée par l'Administrateur → Activée.

### Étape 3 — Le chef de chantier demande ses besoins

Le chef saisit un **approvisionnement** : la liste de ce dont il a besoin. Quand il
choisit un article du catalogue, **le prix se remplit et se verrouille** — il ne peut
pas le gonfler.

### Étape 4 — L'approvisionnement devient une prévision

Le Correspondant vérifie l'approvisionnement et le valide. À ce moment, l'application
génère automatiquement une **prévision journalière**, déjà soumise dans le circuit.

L'Administrateur et le Correspondant peuvent aussi créer une prévision directement,
sans passer par l'approvisionnement.

### Étape 5 — La prévision est validée puis exécutée

Circuit : Soumise → Validée Correspondant → Validée Administrateur → **Exécutée**.

L'exécution, c'est le moment où **l'argent sort réellement** de la banque.

Chaque ligne de la prévision est **rattachée à sa ligne de la prévision globale**.
Exemple : « Ciment 1 T, 700 000 Ar » est rattaché à l'enveloppe « Ciment 21 000 000 Ar ».
C'est ce lien qui permet de savoir, à tout moment, combien il reste sur chaque poste.

### Étape 6 — Les travaux doivent être justifiés

**C'est le mécanisme anti-détournement.**

Une fois la prévision exécutée, le chef doit écrire ce qu'il a fait de l'argent :
« Coulage des fondations zone A, 6 t de ciment consommées, équipe de 12 maçons. »

L'Administrateur lit ce compte rendu et choisit :
- **Réceptionner les travaux** → le cycle se ferme
- **Renvoyer au chef** avec un motif, si c'est trop vague

### Étape 7 — Le blocage

**Tant que l'Administrateur n'a pas réceptionné, aucune nouvelle prévision ne peut être
créée pour ce chantier.**

Le blocage est **par chantier** : si Tuléar a justifié, Tuléar peut continuer, même si
Nosy Be est bloqué. Sur l'écran de saisie, les chantiers bloqués apparaissent grisés
avec la raison exacte.

---

## 5. Les rôles — qui peut faire quoi

| Rôle | Ce qu'il fait |
|---|---|
| **Administrateur** | Voit tout. Valide en dernier. **Réceptionne les travaux.** Gère les utilisateurs, le budget, les paramètres. |
| **Correspondant** | Valide les approvisionnements et les prévisions en premier niveau. Peut créer des prévisions. |
| **Chef de chantier** | Demande les approvisionnements. **Rend compte des travaux réalisés.** |
| **Magasinier** | Tient la fiche de stock. **Ne voit que son chantier.** |

Un magasinier est rattaché à un chantier. S'il tente d'ouvrir la fiche d'un autre
chantier, l'accès est refusé — le cloisonnement est vérifié côté serveur, pas seulement
masqué à l'écran.

---

## 6. Les modules, un par un

### Tableau de bord
Indicateurs financiers (budgets restants, dépenses du mois, trésorerie, dettes) et
quatre graphiques. C'est l'écran d'accueil de l'Administrateur.

### Chantiers
Liste avec bouton **Détail**. La fiche chantier affiche en haut la répartition
Marché → Bénéfice + Budget projet, puis des onglets : Banque, Matériaux,
Approvisionnements, Prévisions, Dépenses, Dettes, **Rapports de travail**, Alertes.

### Prévision globale
Saisie et consultation du plan de dépense par rubriques, avec circuit de validation.

### Approvisionnements
Demandes de besoins du chef de chantier, converties en prévisions à la validation.

### Prévisions journalières
Le circuit de validation quotidien et l'exécution.

### Rapports de prévision
**L'écran le plus important pour le patron.** Quatre onglets :
- **À justifier** — l'argent est sorti, rien n'est rendu (affiché en rouge)
- **À réceptionner** — le chef a rendu, l'admin doit valider
- **Clôturés** — cycle terminé
- **Tous**

Avec filtre par chantier et export PDF/Excel.

### Stock (Matériaux)
Pour le magasinier, la reproduction exacte de sa fiche papier. La liste des articles,
puis **Détail** sur un article ouvre sa fiche :

| Date | Besoin | Entrée | Sortie | Solde sur besoin | Solde en stock | Motif |
|---|---|---|---|---|---|---|

Saisie possible de plusieurs lignes d'un coup, avec calcul des soldes en direct.

### Rapports de travail
Le rapport hebdomadaire d'avancement (celui du modèle Tuléar N°06) : informations
générales, avancement par zone, suivi des matériaux, équipements, problèmes, suggestions.

### Alertes
Notifications automatiques, recalculées toutes les heures.

### Journal d'audit
Chaque création, modification et suppression est enregistrée automatiquement avec
l'utilisateur, l'adresse IP, la date et l'élément concerné.

### Trésorerie, Fournisseurs, Dettes, Catalogue, Utilisateurs, Paramètres
Modules de gestion classiques déjà présents dans l'application d'origine.

---

## 7. Les quatre verrous anti-détournement

C'est le message central de votre présentation.

**Verrou 1 — La double validation**
Aucune dépense ne sort sans passer par le Correspondant puis l'Administrateur.

**Verrou 2 — Le blocage sur justification**
L'argent d'hier doit être justifié avant de pouvoir demander celui de demain.

**Verrou 3 — Les alertes à 50 %**
Dès que la moitié d'une enveloppe est consommée, une alerte est levée — sur chaque
ligne prévue, chaque rubrique, le budget projet, le Budget Matériel, le Budget Comptes
et chaque matériau. Au-delà de 100 %, l'alerte passe en critique.

**Verrou 4 — La traçabilité**
Prix du catalogue verrouillés (côté serveur aussi), journal d'audit automatique,
et rattachement de chaque dépense à son enveloppe prévue.

---

## 8. L'architecture technique

Pour vous, pas pour le patron.

### Les couches

```
ETAM.Domain         entités, enums, interfaces      (aucune dépendance)
ETAM.Application    DTOs, services métier, validateurs
ETAM.Infrastructure EF Core, Identity, PostgreSQL, services techniques
ETAM.Web            contrôleurs MVC, vues Razor
```

Règle : les dépendances vont toujours **vers l'intérieur**. `Domain` ne connaît personne.

### Les entités clés que nous avons ajoutées

| Entité | Rôle |
|---|---|
| `MouvementMateriau` | Une ligne de la fiche de stock (date, entrée, sortie, soldes, motif) |
| `PrevisionGlobale` / `PrevisionGlobaleLigne` | Le plan de dépense et ses lignes par rubrique |

Champs ajoutés à l'existant :
- `Chantier` → `MontantMarche`, `Benefice`, et `BudgetProjet` (calculé)
- `Materiau` → `Localite`, `Besoin`
- `PrevisionLigne` → `PrevisionGlobaleLigneId` (le rattachement)
- `PrevisionJournaliere` → `RapportRealisation`, `DateRapport`, `RapportValideParId`, `MotifRefusRapport`
- `ApplicationUser` → `ChantierId` (cloisonnement magasinier)

### Les statuts

`StatutPrevision` : Brouillon → Soumise → ValidéeRF → ValidéeAdmin → **Exécutée** →
**RapportSoumis** → **Clôturée** (ou Refusée)

Les trois derniers sont ceux que nous avons ajoutés — ils portent tout le mécanisme
de justification.

### Technologies

ASP.NET Core 8 (MVC), C#, Entity Framework Core 8, PostgreSQL, ASP.NET Identity,
AutoMapper, FluentValidation, Serilog, Hangfire (tâches planifiées), Bootstrap 5,
DataTables, Chart.js, QuestPDF (PDF), ClosedXML (Excel), Docker, Render.

### Où trouver quoi dans le code

| Je cherche | Fichier |
|---|---|
| Le blocage des prévisions | `PrevisionController.cs` → `TrouverPrevisionBloquanteAsync` |
| Les alertes 50 % | `AlerteService.cs` → `EvaluerSeuils50Async` |
| Le verrouillage des prix | `PrevisionController.cs` / `ApprovisionnementController.cs` → `AppliquerPrixCatalogueAsync` |
| Le journal d'audit | `AuditableEntityInterceptor.cs` |
| Le cloisonnement magasinier | `MateriauxController.cs` → `ChantierAffecteAsync` |
| Les exports | `Services/ExportService.cs` |
| Les données de démo | `DbInitializer.cs` |

---

## 9. Le déroulé de démonstration

L'ordre compte. Racontez une histoire, ne faites pas le tour des menus.

**Avant de commencer :** ouvrez le site 2 minutes avant. Sur l'offre gratuite de Render,
la première page met ~50 secondes à répondre après une période d'inactivité.

**1. Poser le problème (30 secondes, sans écran)**
« Aujourd'hui, quand l'argent part sur un chantier, on ne sait pas ce qu'il devient
avant longtemps. Je vais vous montrer comment on le suit maintenant. »

**2. Le chantier NOSY BE — Chantiers → Détail**
Montrez la bande du haut : 150 M → 80 M bénéfice + 70 M budget projet.
« Voilà votre marché. Voilà ce que vous gardez. Voilà ce qui part en travaux. »

**3. La prévision globale**
Montrez les rubriques et leurs sous-totaux.
« Voilà comment les 70 M sont prévus. Tout écart se verra. »

**4. Une prévision journalière exécutée**
Ouvrez celle du 28/07. Montrez que chaque ligne est rattachée à son enveloppe.
« 700 000 de ciment aujourd'hui, sur 21 millions prévus. »

**5. Le moment fort — Rapports de prévision**
Onglet « À justifier ». La ligne rouge.
« Cet argent est sorti et personne n'a encore dit ce qu'il en a fait. »

Puis essayez de créer une nouvelle prévision pour ce chantier → **refus**.
« Le système l'empêche. Tant qu'il n'a pas rendu compte, il ne peut pas redemander. »

**6. Débloquer devant lui**
Saisissez le compte rendu (compte chef), réceptionnez-le (compte admin), et remontrez
que la création redevient possible.
« Le cycle est fermé, on peut repartir. »

**7. Les alertes**
Menu Alertes : les seuils 50 % et les travaux non justifiés.
« Vous n'avez pas besoin d'aller chercher : ça vient à vous. »

**8. La fiche magasinier**
Stock → Détail d'un article. Mettez la fiche papier à côté.
« C'est exactement sa feuille, mais les soldes se calculent tout seuls. »

**9. Finir sur un export**
Cliquez sur PDF. « Et vous pouvez tout sortir en PDF ou en Excel. »

---

## 10. Les questions que votre patron va poser

**« Et si quelqu'un modifie un prix ? »**
Impossible sur un article du catalogue : le prix est imposé, et pas seulement à l'écran —
le serveur le réimpose à l'enregistrement.

**« Et si le chef écrit n'importe quoi dans son rapport ? »**
Vous pouvez le lui renvoyer avec un motif. Tant que vous n'avez pas réceptionné,
son chantier reste bloqué.

**« Qui a fait quoi ? »**
Journal d'audit : chaque action est enregistrée avec le nom, l'heure et l'adresse IP.

**« Est-ce que je peux voir un chantier en particulier ? »**
Oui, filtre par chantier sur les rapports de prévision, et fiche complète par chantier.

**« Est-ce que le magasinier peut voir les autres chantiers ? »**
Non. Il est rattaché au sien, et l'accès aux autres est refusé par le serveur.

**« Ça coûte combien à faire tourner ? »**
Actuellement gratuit sur Render, avec deux limites à connaître : le site s'endort après
15 minutes d'inactivité, et la base de données gratuite expire au bout de 90 jours.
Le passage à l'offre payante est de l'ordre de 7 $/mois pour la base.

---

## 11. Ce qu'il faut annoncer honnêtement

Ne les cachez pas : les annoncer vous-même vous rend crédible.

- **Aucun test automatisé.** Les vérifications ont été faites manuellement.
- **Pas d'environnement de préproduction** : on déploie directement en production.
- **La base gratuite expire à 90 jours** — il faudra prévoir une sauvegarde ou l'offre payante.
- **Les emails sont en mode simulation** — la fonction « mot de passe oublié » n'envoie
  pas réellement de message. Le changement de mot de passe en ligne, lui, fonctionne.
- **Les données actuelles sont un jeu de démonstration** (chantier NOSY BE). Les vrais
  chantiers restent à créer.
- **Aucune reprise des archives papier** n'a été faite.

---

## 12. Ce qui reste à faire

1. Vérifier que le fichier `appsettings.Development.json` n'est pas sur GitHub
   (`git ls-files | findstr appsettings`) — il contient un mot de passe de base
2. Réactiver la vérification TLS de Git (`git config --global http.sslVerify true`)
3. Pousser le dernier lot de corrections et vérifier le déploiement
4. Changer les mots de passe des quatre comptes
5. Créer les vrais chantiers et leurs magasiniers
6. Supprimer le dossier `Etam_export/` en double dans le dépôt
7. Prévoir la sauvegarde de la base avant l'échéance des 90 jours

---

## 13. Le résumé en 30 secondes

> « Le logiciel suit l'argent du chantier de bout en bout. On saisit le marché, on
> planifie le budget par rubriques, et chaque demande d'argent est validée à deux
> niveaux. Surtout : tant que le chef n'a pas justifié ce qu'il a fait de l'argent
> précédent, il ne peut pas en redemander. Et dès qu'on atteint la moitié d'une
> enveloppe, vous êtes prévenu automatiquement. Tout est tracé, tout est exportable. »
