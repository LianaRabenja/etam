# Dans quel ordre utiliser le logiciel

Le menu de gauche est maintenant numéroté. **Vous descendez de haut en bas, dans
l'ordre.** Ce qui est en haut se remplit en premier.

---

## Le menu, de haut en bas

```
Tableau de bord

1 · ON PRÉPARE                  ← à faire une seule fois, au début
    Chantiers
    Banques
    Catalogue des prix
    Fournisseurs
    Utilisateurs

2 · ON PLANIFIE L'ARGENT        ← une fois par projet, puis une fois par mois
    Plan du projet
    Enveloppe du mois
    Budget du bureau

3 · ON DÉPENSE                  ← tous les jours
    Demandes d'achat
    Prévision du jour
    Sorties d'argent

4 · ON JUSTIFIE                 ← après chaque journée
    Comptes rendus à recevoir
    Rapports de travail

5 · LE MAGASIN
    Stock des matériaux

6 · ON SURVEILLE                ← quand vous voulez, rien à saisir
    Alertes
    Dettes fournisseurs
    Budget matériel
    Journal des dépenses
    Rapports et statistiques
    Journal d'audit
    Paramètres
```

---

## Créer un nouveau chantier — les 6 étapes

À faire dans cet ordre. Chaque étape a besoin de la précédente.

### Étape 1 — Créer le chantier

**Menu : 1 · On prépare › Chantiers › Nouveau chantier**

Remplissez :

- Code, par exemple `TUL-02`
- Nom, par exemple `TULEAR`
- Date de début
- **Montant du marché** : ce que le client a signé, par exemple 90 000 000
- **Bénéfice** : ce que l'entreprise garde, par exemple 45 000 000

Le logiciel calcule tout seul : **90 000 000 − 45 000 000 = 45 000 000 Ar pour les
travaux**. C'est le maximum que ce chantier pourra jamais dépenser.

### Étape 2 — Créer son compte en banque

**Menu : 1 · On prépare › Banques › Nouveau compte**

- Nom : `Compte TULEAR`
- Type : **Chantier**
- Chantier rattaché : TULEAR
- Solde de départ : le montant réellement présent sur ce compte

Puis, sur ce compte, cliquez **Transférer** pour envoyer de l'argent vers le budget
matériel du chantier.

**Sans ce transfert, aucune dépense ne sera possible.** C'est l'erreur la plus
fréquente.

### Étape 3 — Créer le magasinier

**Menu : 1 · On prépare › Utilisateurs › Nouvel utilisateur**

- Rôle : **Magasinier**
- Chantier : TULEAR

Il ne verra que ce chantier, jamais les autres.

### Étape 4 — Faire le plan du projet

**Menu : 2 · On planifie l'argent › Plan du projet › Nouveau**

Répartissez les 45 000 000 Ar par poste : approvisionnement, main d'œuvre,
transport, imprévus.

**Le total doit tomber exactement sur 45 000 000.** Si ça ne tombe pas juste,
ajoutez une ligne « Imprévus » pour absorber la différence.

Puis faites valider ce plan.

### Étape 5 — Ouvrir le premier mois

**Menu : 2 · On planifie l'argent › Enveloppe du mois › Nouveau mois**

- Chantier : TULEAR
- Mois : celui qui commence
- Montant : combien ce chantier consomme ce mois-ci, par exemple 12 000 000

Puis cliquez **Ouvrir l'enveloppe**.

Tant que ce n'est pas fait, aucune prévision journalière ne pourra être ouverte.

### Étape 6 — Saisir le stock de départ

**Menu : 5 · Le magasin › Stock des matériaux**

Pour chaque matériau : la désignation, l'unité, le **besoin total** pour tout le
chantier, et le prix.

Le magasinier prendra le relais ensuite.

---

## Une journée normale, une fois le chantier créé

C'est la boucle que vous répéterez tous les jours.

| | Menu | Qui | Ce qu'on fait |
|---|---|---|---|
| 1 | Prévision du jour | Finance ou Direction | Créer la prévision, la valider deux fois, l'exécuter |
| 2 | Prévision du jour | Chef de chantier | Signer l'accusé de réception de l'argent |
| 3 | Sorties d'argent | Chef de chantier | Enregistrer chaque paiement réel, au fil de la journée |
| 4 | Prévision du jour | Chef de chantier | Joindre les photos de factures |
| 5 | Prévision du jour | Chef de chantier | Écrire le compte rendu des travaux |
| 6 | Comptes rendus à recevoir | Direction | Lire et réceptionner |

Tant que l'étape 6 n'est pas faite, **le chantier ne peut pas demander de nouvel
argent**.

---

## À la fin du mois

**Menu : 2 · On planifie l'argent › Enveloppe du mois › le mois › Clôturer**

Le logiciel calcule ce qui n'a pas été dépensé et le reporte automatiquement sur le
mois suivant.

Il refusera de clôturer tant qu'une journée du mois n'est pas réceptionnée.

---

## Les trois erreurs qui bloquent tout

**Pas de transfert vers le budget matériel.** Vous verrez le message « Aucun
transfert vers le Budget Matériel ». Retournez à l'étape 2.

**Pas d'enveloppe mensuelle ouverte.** Vous verrez « Aucune enveloppe mensuelle
ouverte pour ce mois ». Retournez à l'étape 5.

**Accusé de réception non signé.** Le bouton « Nouvelle sortie » n'apparaît pas.
Le chef doit d'abord signer sur la fiche de la prévision.
