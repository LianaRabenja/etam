# ETAM — Manuel d'exploitation

Ce document existe pour qu'une autre personne que l'auteur puisse faire vivre
l'application. Tenez-le à jour : le jour où il sera utile, ce sera un jour difficile.

---

## 1. Ce qui compose le système

| Élément | Où | Rôle |
|---|---|---|
| Code source | GitHub — `LianaRabenja/etam`, branche `main` | Référence unique |
| Application | Render — service `etam-erp` | Le site en ligne |
| Base de données | Render — `etam-db` (PostgreSQL) | **Toutes** les données |
| Déclencheur | Auto-Deploy sur `main` | Un push déploie |

Tout pousser sur `main` met en ligne. Il n'y a pas d'étape intermédiaire :
travaillez sur une branche si la modification est risquée.

---

## 2. Déployer une modification

```
dotnet build                  # ne jamais sauter cette étape
git add -A
git commit -m "ce qui change, en une phrase"
git push
```

Puis **Render → etam-erp → Logs**. Vous attendez, dans l'ordre :

- `Build successful`
- `Applying migration '...'` (uniquement si le schéma a changé)
- `Your service is live`

Comptez 5 à 10 minutes. **Si la construction échoue, le site en service ne bouge pas** :
Render ne remplace la version en ligne qu'après une construction réussie.

---

## 3. Revenir en arrière

Le bouton *Rollback* de Render ne fonctionne pas toujours : sur les petits plans,
les images de construction anciennes sont supprimées. La méthode fiable passe par Git.

Trouver le dernier bon commit — la date et le message sont dans **Render → Deploys** :

```
git log --all --date=short --format="%h %ad %s" -20
```

Revenir dessus sans effacer l'historique :

```
git revert --no-commit <hash>..HEAD
git commit -m "Retour a la version <hash>"
git diff <hash> --stat        # doit ne RIEN afficher
git push
```

Le `git diff` vide est la preuve que le code est redevenu identique à la bonne version.
Ne poussez pas tant qu'il affiche quelque chose.

> **Attention** : revenir en arrière sur le code ne défait **pas** les migrations
> déjà appliquées à la base. Si la version fautive a modifié le schéma, vérifiez
> l'état de la base avant de conclure.

---

## 4. Sauvegarde de la base — le point vital

Sans sauvegarde, une base perdue, c'est l'entreprise sans historique : chantiers,
prévisions, justificatifs, comptes. Aucune ligne de code ne rattrape ça.

- Vérifier le **plan** de `etam-db` : les bases gratuites de Render **expirent**
  et n'ont **aucune sauvegarde automatique**
- Exporter régulièrement la base **hors de Render** (autre machine, autre service)
- **Tester la restauration** au moins une fois : une sauvegarde qu'on n'a jamais
  restaurée n'est pas une sauvegarde, c'est une hypothèse

---

## 5. Comptes et mots de passe

Les comptes initiaux sont créés au premier démarrage par `DbInitializer`.

En production, les mots de passe viennent des variables d'environnement
`ETAM_ADMIN_PASSWORD`, `ETAM_RF_PASSWORD`, `ETAM_CHEF_PASSWORD`,
`ETAM_MAGASINIER_PASSWORD` — visibles dans **Render → Settings → Environment**.

À défaut de ces variables, le code retombe sur des mots de passe de développement,
écrits en clair dans le dépôt. **Ne jamais déployer sans définir ces variables.**

Ensuite, tout se gère dans l'application : **Utilisateurs → Modifier** permet de
changer le rôle, le chantier d'affectation et le mot de passe.

> Un **Magasinier** ou un **Chef de chantier** doit être rattaché à un chantier.
> Sans affectation, il voit **tous** les chantiers.

---

## 6. Travailler en local

```
dotnet run --project src/ETAM.Web
```

L'environnement est fixé à `Development` par `src/ETAM.Web/Properties/launchSettings.json`,
ce qui fait lire `appsettings.Development.json` — le fichier qui contient la chaîne de
connexion locale. Ce fichier est ignoré par Git et n'existe donc pas sur une machine neuve :
il faut le recréer.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=etam_erp;Username=postgres;Password=VOTRE_MOT_DE_PASSE"
  }
}
```

**Ne pointez jamais une exécution locale sur la base de production** : les migrations
s'appliquent au démarrage.

---

## 7. Lancer les tests

```
dotnet test
```

Ils vérifient les règles financières et le suivi de stock. Ils ne demandent ni base
de données ni serveur, et durent quelques secondes.

**Lancez-les après toute modification touchant à l'argent ou aux stocks.**
Si un test échoue, ce n'est pas le test qui a tort.

---

## 8. Les règles métier à ne pas casser

Elles font la valeur du logiciel. Elles sont protégées par les tests — vérifiez que
les tests les couvrent encore avant de modifier l'une d'elles.

1. **Le fléchage réserve, il ne retire pas.** Transférer vers un budget ne diminue
   pas le solde bancaire. Seule l'exécution d'une prévision réelle fait sortir l'argent.
2. **On ne flèche que ce qui n'est pas déjà réservé.** Sinon la même somme pourrait
   être affectée plusieurs fois, puisque le solde ne bouge pas.
3. **Double validation avant toute sortie d'argent** : Correspondant, puis Administrateur.
4. **Pas de nouvelle prévision tant que la précédente n'est pas justifiée.** C'est le
   mécanisme qui empêche les avances de s'accumuler sans justificatif.
5. **Chaque chef et chaque magasinier ne voit que son chantier.** Le filtrage est fait
   côté serveur, pas seulement dans les menus.

---

## 9. Entretien périodique

| Quand | Quoi |
|---|---|
| Chaque semaine | Vérifier que la sauvegarde de la base existe et est récente |
| Chaque mois | Regarder les journées bloquées dans « À justifier » |
| Chaque trimestre | `dotnet list package --vulnerable` et corriger |
| Tous les 2-3 ans | Monter vers la version LTS suivante de .NET |

La version actuelle est **.NET 8**, dont le support s'arrête en **novembre 2026**.
