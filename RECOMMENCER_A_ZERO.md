# Repartir de zéro et créer votre premier chantier

Trois choses ont changé :

1. Un script efface toutes les données de chantier, en gardant vos utilisateurs,
   votre catalogue et vos fournisseurs.
2. Le logiciel ne recrée plus le chantier de démonstration au redémarrage.
3. Le menu est réduit à **7 entrées numérotées** au lieu d'une vingtaine.

---

## Le nouveau menu

```
Tableau de bord

DANS CET ORDRE
   1. Chantiers
   2. Banques
   3. Plan du projet
   4. Enveloppe du mois
   5. Prévision du jour
   6. Sorties d'argent
   7. Comptes rendus

AU QUOTIDIEN
   Stock des matériaux
   Alertes

▸ Réglages et suivi          (replié — cliquez pour ouvrir)
```

Vous descendez de 1 à 7, dans l'ordre. Vous ne sautez pas d'étape.

Tout le reste — catalogue, fournisseurs, utilisateurs, budgets, journaux,
statistiques — est rangé dans le repli du bas. Ça existe toujours, mais ça ne
vous encombre plus.

---

## Étape A — Effacer les données de chantier

### En local

```
psql "Host=localhost;Database=etam;Username=postgres;Password=root" -f scripts/nettoyer_chantiers.sql
```

Adaptez la chaîne à votre configuration. Si `psql` n'est pas reconnu, ouvrez
pgAdmin, sélectionnez la base, ouvrez l'outil de requête et collez le contenu
du fichier `scripts/nettoyer_chantiers.sql`.

### Sur Render

Tableau de bord Render › base `etam-db` › onglet **Connect** › copiez la
commande `PSQL Command`, puis ajoutez `-f scripts/nettoyer_chantiers.sql` à la fin.

### Ce que vous devez voir

Le script se termine par deux tableaux de vérification.

Le premier — les tables effacées — doit afficher **0 partout**.
Le second — ce qui est conservé — ne doit **pas** être à zéro.

---

## Étape B — Empêcher la recréation automatique

C'est déjà fait dans le code. Le chantier NOSY BE ne sera recréé que si la
variable `ETAM_DONNEES_EXEMPLE` vaut `true`.

Sur Render, vérifiez que cette variable **n'existe pas** dans l'onglet
Environment. Si elle y est, supprimez-la.

Au démarrage, vous verrez dans les logs :

```
Données de démonstration désactivées.
```

C'est le comportement attendu.

---

## Étape C — Créer votre premier chantier

Suivez les numéros du menu. Voici un exemple complet avec des chiffres simples.

### 1. Chantiers › Nouveau chantier

| Champ | Valeur |
|---|---|
| Code | `TUL-01` |
| Nom | `TULEAR` |
| Date de début | aujourd'hui |
| Montant du marché | `90000000` |
| Bénéfice | `50000000` |
| Statut | En cours |

Enregistrez, puis ouvrez le détail. Vous devez lire :
**Argent disponible pour les travaux : 40 000 000 Ar** (90 M − 50 M).

C'est le plafond absolu du chantier. Retenez ce chiffre.

### 2. Banques › Nouveau compte

| Champ | Valeur |
|---|---|
| Nom | `Compte TULEAR` |
| Banque | `BNI` |
| Type | **Chantier** |
| Chantier | TULEAR |
| Solde de départ | `90000000` |

Le marché entre en banque. Bénéfice et budget travaux sont sur le même compte :
le logiciel les suit séparément, il ne les sépare pas physiquement.

**Puis, sur ce compte, cliquez « Transférer ».** Envoyez `15000000` vers le budget
matériel du chantier.

> C'est l'étape que tout le monde oublie. Sans transfert, vous aurez plus tard le
> message « Aucun transfert vers le Budget Matériel » et rien ne fonctionnera.

### 3. Plan du projet › Nouveau

Chantier TULEAR. Répartissez les 40 000 000 Ar :

| Rubrique | Désignation | Quantité | Prix | Total |
|---|---|---|---|---|
| Approvisionnement | Ciment | 30 | 700 000 | 21 000 000 |
| Approvisionnement | Fer à béton Ø10 | 200 | 38 000 | 7 600 000 |
| Main d'œuvre | Maçons | 240 | 25 000 | 6 000 000 |
| Transport | Camion | 1 | 3 400 000 | 3 400 000 |
| Imprévus | Provision | 1 | 2 000 000 | 2 000 000 |
| | | | **Total** | **40 000 000** |

Le total doit tomber exactement sur 40 000 000. Puis soumettez et validez.

### 4. Enveloppe du mois › Nouveau mois

| Champ | Valeur |
|---|---|
| Chantier | TULEAR |
| Année / Mois | l'année et le mois en cours |
| Montant prévu | `15000000` |
| Approvisionnement | `9000000` |
| Main d'œuvre | `4000000` |
| Transport | `1500000` |
| Imprévus | `500000` |

Enregistrez, puis **Ouvrir l'enveloppe**.

> Le montant du mois ne peut pas dépasser 40 000 000, et la somme de tous vos mois
> non plus. Si vous saisissez trop, le logiciel refuse et vous dit ce qu'il reste.

### 5. Prévision du jour › Nouvelle prévision

Chantier TULEAR, date d'aujourd'hui. Trois lignes :

| Désignation | Quantité | Prix | Total |
|---|---|---|---|
| Ciment | 2 | 700 000 | 1 400 000 |
| Fer à béton Ø10 | 50 | 38 000 | 1 900 000 |
| Maçons | 68 | 25 000 | 1 700 000 |
| | | **Total** | **5 000 000** |

Puis, dans l'ordre : **Soumettre** › **Valider (Finance)** › **Valider (Direction)**
› **Exécuter**.

À ce stade, **rien n'est sorti de la banque**. Allez le vérifier dans Banques : le
solde est toujours à 90 000 000. C'est normal, c'est le principe.

### 6. Signer la réception

Sur la fiche de la prévision, un encadré orange apparaît :
**« Réception de l'argent à signer »**.

Saisissez le nom du chef de chantier et validez.

Le bouton **« Nouvelle sortie »** apparaît alors. Avant la signature, il n'existe pas.

### 7. Sorties d'argent

Toujours sur la fiche de la prévision, cliquez **Nouvelle sortie** :

| Champ | Valeur |
|---|---|
| Montant | `190000` |
| Bénéficiaire | `RAKOTO Jean` |
| Motif | `Achat de gravillon` |
| Mode | Espèces |
| Compte débité | Compte TULEAR |
| Budget | Budget Matériel du chantier |

Enregistrez.

**Retournez dans Banques.** Le solde est passé de 90 000 000 à **89 810 000**.
Il a baissé de 190 000 Ar, pas de 5 000 000.

C'est le cœur du système. Si vous voyez ça, tout fonctionne.

### 8. Le lendemain — le report

Revenez sur la prévision d'aujourd'hui : elle affiche
**Reste à dépenser : 4 810 000 Ar**.

Faites écrire le compte rendu par le chef, réceptionnez-le
(menu **7. Comptes rendus**), puis créez la prévision de demain avec 3 000 000 Ar.

Sur sa fiche, vous lirez :

| | |
|---|---|
| Demandé | 3 000 000 |
| Reporté de la veille | + 4 810 000 |
| **Plafond du jour** | **7 810 000** |

---

## Les trois blocages possibles, et leur cause

| Message | Cause | Où corriger |
|---|---|---|
| « Aucun transfert vers le Budget Matériel » | Vous avez sauté le transfert | Menu 2 › le compte › Transférer |
| « Aucune enveloppe mensuelle ouverte » | Le mois n'existe pas ou n'est pas validé | Menu 4 › Ouvrir l'enveloppe |
| Le bouton « Nouvelle sortie » n'apparaît pas | La réception n'est pas signée | Sur la fiche de prévision, encadré orange |

---

## Pour appliquer tout ça

```
dotnet build
dotnet run --project src/ETAM.Web
```

Puis, une fois satisfait :

```
git add -A
git commit -m "Menu simplifie, donnees d'exemple desactivees, script de nettoyage"
git push
```

Le script SQL, lui, se lance à part : il ne part pas avec le déploiement.
Lancez-le une fois en local, et une fois sur Render.
