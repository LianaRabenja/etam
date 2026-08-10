# Prévision mensuelle, décaissements et justificatifs

Ce document décrit ce qui a changé dans le circuit de l'argent, et ce qu'il vous
reste à faire pour mettre les modifications en service.

---

## 1. Ce qui change dans le principe

**Avant.** Valider une prévision journalière sortait immédiatement tout l'argent de
la banque. Une prévision de 5 000 000 Ar débitait 5 000 000 Ar, même si seulement
190 000 Ar étaient réellement dépensés dans la journée.

**Maintenant.** Une prévision journalière est une **autorisation de dépense**, pas un
retrait. L'argent reste en banque et n'en sort qu'au moment de chaque paiement réel.

Trois niveaux emboîtés :

```
Prévision globale du projet        70 000 000 Ar   (le plan de chantier)
    └── Enveloppe mensuelle       125 000 000 Ar   (le mois)
            └── Prévision du jour   5 000 000 Ar   (le plafond de la journée)
                    └── Décaissement  190 000 Ar   (la seule sortie réelle)
```

---

## 2. Le report, concrètement

Votre exemple, tel qu'il fonctionne désormais :

| | Lundi | Mardi |
|---|---|---|
| Demandé | 5 000 000 | 3 000 000 |
| Reporté de la veille | — | + 4 810 000 |
| **Plafond du jour** | **5 000 000** | **7 810 000** |
| Décaissé | 190 000 | … |
| **Reste reporté** | **4 810 000** | … |

Sur le compte en banque, lundi, il ne sort que **190 000 Ar**. Les 4 810 000 Ar
restants n'ont jamais bougé : ils redeviennent simplement disponibles mardi.

Même mécanique au niveau du mois : à la clôture d'août, ce qui n'a pas été décaissé
se reporte sur septembre, avec la trace du mois d'origine.

---

## 3. L'accusé de réception

Quand une prévision est ouverte, elle affiche un encadré orange **« Réception de
l'argent à signer »**. Le chef de chantier saisit son nom et valide.

Tant que ce n'est pas fait, **aucun décaissement n'est possible** — le bouton
« Nouvelle sortie » n'apparaît pas.

Une fois signé, un bouton **« Imprimer l'accusé »** produit un document A4 avec la
formule « Je soussigné… reconnais avoir reçu la somme de… », les deux emplacements
de signature et le rappel du report de la veille.

---

## 4. Les factures

Sur la fiche d'une prévision, un bloc **« Factures et justificatifs »** permet de
téléverser des photos ou des PDF, avec le numéro de facture, le fournisseur et le
montant. Les images s'affichent en vignettes cliquables.

Les fichiers sont stockés **dans la base de données**, pas sur le disque du serveur :
l'hébergement recrée le conteneur à chaque mise à jour, ce qui effacerait les
fichiers déposés.

Limites en place : 5 Mo par fichier, 20 pièces par prévision, formats JPEG, PNG,
WebP, HEIC et PDF.

---

## 5. Ce que vous devez faire

### Étape 1 — Compiler

```bash
cd C:\Users\ASUS\Documents\Etam_export
dotnet build
```

**Corrigez les erreurs éventuelles avant d'aller plus loin.** Je n'ai pas de
compilateur .NET dans mon environnement : tout ce code a été écrit sans être
vérifié une seule fois. Envoyez-moi les messages d'erreur, je les corrige.

### Étape 2 — Créer la migration

```bash
dotnet ef migrations add PrevisionMensuelleEtDecaissements -p src/ETAM.Infrastructure -s src/ETAM.Web
dotnet ef database update -p src/ETAM.Infrastructure -s src/ETAM.Web
```

Je n'ai volontairement pas écrit la migration à la main : Entity Framework doit la
générer lui-même pour que l'instantané du modèle reste cohérent. Une migration
écrite à la main désynchronise ce fichier et casse toutes les suivantes.

### Étape 3 — Vérifier en local

Dans cet ordre :

1. Créer une enveloppe mensuelle sur NOSY BE, puis l'ouvrir
2. Créer une prévision journalière, la valider deux fois, l'ouvrir
3. Signer l'accusé de réception
4. Enregistrer un décaissement de 190 000 Ar
5. Vérifier que le solde du compte a bien baissé de 190 000 Ar, et pas de 5 000 000
6. Clôturer la journée, ouvrir le lendemain, vérifier le report
7. Joindre une photo de facture et vérifier qu'elle s'affiche

### Étape 4 — Déployer

Commit, push, et Render redéploie tout seul.

---

## 6. Ce qui n'est pas fait

**Le jeu de démonstration n'a pas été mis à jour.** Les deux prévisions de NOSY BE
ont été écrites directement en base avec leur statut final : elles n'ont ni
enveloppe mensuelle, ni report, ni décaissement. Elles s'afficheront avec un
plafond égal au montant demandé et zéro décaissé.

Pour une démonstration propre, le plus simple est de créer les données à la main
en suivant les sept étapes ci-dessus.

**Les alertes à 50 % ne couvrent pas encore l'enveloppe mensuelle.** Le service
d'alertes surveille les postes, les budgets et le chantier, mais pas le nouveau
niveau mensuel. La barre de progression de la fiche mensuelle passe bien au orange
puis au rouge, mais aucune alerte n'est générée.

**Le menu Dépenses n'a pas été touché.** Il continue d'afficher son bouton de
création manuelle, qui contourne toujours les contrôles — c'est le point que je
vous signalais précédemment.

---

## 7. Le point de vigilance principal

Ce changement touche au cœur du circuit de l'argent. Les anciennes prévisions déjà
exécutées en base n'ont ni `PrevisionMensuelleId`, ni `ReportVeille`, ni
`MontantDecaisse` — elles ne provoqueront pas d'erreur, mais elles apparaîtront
comme n'ayant rien décaissé, alors qu'elles avaient bien débité les budgets sous
l'ancienne logique.

Si vous avez déjà des données réelles en production, il faudra décider quoi en
faire avant de déployer. Sur un jeu de démonstration, ce n'est pas un problème.
