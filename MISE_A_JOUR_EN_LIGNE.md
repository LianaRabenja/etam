# Mettre à jour le site en ligne, sans travailler en local

Tout se fait depuis votre ordinateur en trois commandes git, puis dans le tableau
de bord Render. Vous n'avez ni à compiler, ni à installer psql.

---

## Étape 1 — Envoyer le code

Ouvrez PowerShell dans `C:\Users\ASUS\Documents\Etam_export` :

```
git add -A
git commit -m "Menu simplifie, nettoyage des chantiers, donnees d'exemple desactivees"
git push
```

C'est tout. Render détecte le push et reconstruit l'application automatiquement.

**Si la construction échoue, votre site actuel reste en ligne.** Render ne remplace
la version en service que si la nouvelle se construit correctement. Vous ne risquez
donc pas de casser ce qui fonctionne aujourd'hui.

---

## Étape 2 — Suivre la construction

Tableau de bord Render › service **etam-erp** › onglet **Logs**.

Comptez 5 à 10 minutes. Cherchez, dans cet ordre :

| Ce que vous devez voir | Ce que ça veut dire |
|---|---|
| `Build successful` | Le code a été compilé |
| `Applying migration '20260810173109_PrevisionMensuelleEtDecaissements'` | Les nouvelles tables sont créées |
| `Données de démonstration désactivées` | NOSY BE ne sera plus recréé |
| `Your service is live` | C'est en ligne |

Si vous voyez `Build failed` ou une erreur rouge, copiez le message et envoyez-le
moi. Le site en cours n'aura pas bougé.

---

## Étape 3 — Effacer les données de chantier

Le nettoyage est intégré à l'application. Il se déclenche par une variable, le
temps d'un redémarrage.

### 3a. Ajouter la variable

Render › service **etam-erp** › onglet **Environment** › **Add Environment Variable**

| Key | Value |
|---|---|
| `ETAM_NETTOYER_CHANTIERS` | `OUI-EFFACER` |

La valeur doit être écrite **exactement** ainsi, en majuscules avec le tiret.
Toute autre valeur est ignorée — c'est volontaire, pour éviter un effacement
accidentel.

Cliquez **Save Changes**. Render redémarre le service.

### 3b. Vérifier dans les logs

Vous devez voir, dans cet ordre :

```
NETTOYAGE DEMANDÉ : suppression de toutes les données de chantier.
NETTOYAGE TERMINÉ. Retirez maintenant la variable ETAM_NETTOYER_CHANTIERS...
```

Si vous lisez à la place `Le nettoyage des données de chantier a échoué`, rien n'a
été supprimé — la base est intacte. Envoyez-moi le message d'erreur.

### 3c. Retirer la variable — ne pas oublier

Retournez dans **Environment**, supprimez `ETAM_NETTOYER_CHANTIERS`, puis
**Save Changes**.

> Tant que cette variable existe, la base sera vidée à **chaque** redémarrage du
> service. Et Render redémarre tout seul, notamment après une mise en veille.
> C'est l'étape à ne pas oublier.

---

## Étape 4 — Vérifier sur le site

Connectez-vous en administrateur.

| Écran | Ce que vous devez voir |
|---|---|
| Menu de gauche | Sept entrées numérotées, plus « Réglages et suivi » replié en bas |
| 1. Chantiers | Liste vide |
| 4. Enveloppe du mois | Liste vide |
| 5. Prévision du jour | Liste vide |
| Réglages › Catalogue des prix | Vos articles, toujours là |
| Réglages › Utilisateurs | Vos comptes, toujours là |
| Réglages › Fournisseurs | Vos fournisseurs, toujours là |

Si le catalogue ou les utilisateurs ont disparu, prévenez-moi immédiatement :
ce n'est pas le comportement prévu.

---

## Étape 5 — Créer votre premier vrai chantier

Suivez les numéros du menu, de 1 à 7. Le détail avec les montants exacts est dans
`RECOMMENCER_A_ZERO.md`, à partir de l'étape C.

En résumé :

1. **Chantiers** — le chantier, avec marché et bénéfice
2. **Banques** — son compte, puis **cliquer Transférer** (l'étape qu'on oublie)
3. **Plan du projet** — la répartition du budget travaux
4. **Enveloppe du mois** — le montant du mois, puis **Ouvrir l'enveloppe**
5. **Prévision du jour** — la journée, validée deux fois puis exécutée
6. **Sorties d'argent** — chaque paiement réel
7. **Comptes rendus** — la justification, puis la réception

---

## Ce qui reste vrai

Le site s'endort après 15 minutes sans visite. La première page met environ
50 secondes. Ouvrez-le 10 minutes avant toute démonstration.

La base gratuite expire au bout de 90 jours.

---

## Si vous préférez la méthode classique

Le fichier `scripts/nettoyer_chantiers.sql` contient les mêmes suppressions, à
lancer depuis pgAdmin ou psql avec la chaîne de connexion de Render
(onglet **Connect** de la base `etam-db`). Le résultat est identique, avec en
plus deux tableaux de vérification affichés à la fin.

Utilisez l'une **ou** l'autre méthode, pas les deux.
