# ETAM — Scénario de présentation

Script de démonstration. Les phrases en *italique* sont à dire, les actions entre
crochets sont à faire. Durée visée : **12 à 15 minutes**, questions comprises.

---

## Avant de commencer — checklist

À faire **10 minutes avant**, pas au dernier moment :

- [ ] Ouvrir le site et se connecter — sur Render, la première page met ~50 s à répondre
- [ ] Vérifier que le chantier **NOSY BE** est bien présent
- [ ] Vérifier que la prévision du **28/07** est en état « Exécutée » (c'est elle qui porte la démonstration)
- [ ] Ouvrir un **second onglet** déjà connecté en **Chef de chantier** (pour l'acte 4)
- [ ] Fermer les onglets inutiles, mettre le navigateur en plein écran
- [ ] Avoir la **fiche matériaux papier** sous la main
- [ ] Zoom du navigateur à 110 % si vous présentez sur un écran partagé

Comptes : Administrateur `admin@etam.mg`, Chef de chantier `chef@etam.mg`.
En production, les mots de passe sont ceux du tableau de bord Render.

---

## Acte 0 — L'accroche (1 min, sans écran)

Ne commencez pas par le logiciel. Commencez par le problème.

> *« Aujourd'hui, quand de l'argent part sur un chantier, il se passe souvent
> plusieurs semaines avant qu'on sache ce qu'il est devenu. Le chef demande lundi,
> dépense, et redemande jeudi. Personne ne peut vérifier facilement.*
>
> *Ce que je vais vous montrer, c'est un outil qui suit cet argent de bout en bout,
> et qui empêche qu'un franc ressorte tant que le précédent n'est pas justifié. »*

Puis seulement, ouvrez l'écran.

---

## Acte 1 — L'argent du chantier (2 min)

**[Menu Chantiers → bouton Détail sur NOSY BE]**

Pointez la bande du haut :

> *« Voilà un marché de 150 millions. 80 millions restent à l'entreprise,
> 70 millions partent en travaux. C'est cette répartition que le logiciel suit. »*

**[Onglet Banque]**

> *« L'argent est entré en banque à la signature. Ensuite on transfère vers le
> budget matériel, au fur et à mesure — jamais tout d'un coup. »*

Faites défiler rapidement les onglets sans les commenter en détail : Matériaux,
Approvisionnements, Prévisions, Dépenses, Dettes, Rapports de travail, Alertes.

> *« Tout ce qui concerne ce chantier est réuni ici. »*

---

## Acte 2 — Le plan de dépense (2 min)

**[Menu Prévision globale → Détail sur la prévision de NOSY BE]**

> *« Avant de dépenser, on établit le plan : comment les 70 millions vont être
> utilisés, poste par poste. »*

Montrez les rubriques et leurs sous-totaux : Approvisionnement, Main d'œuvre,
Imprévus, Transport.

> *« Ciment, sable, fer, main d'œuvre… chaque poste a son enveloppe.
> Le total correspond exactement au budget projet. »*

---

## Acte 3 — Une dépense réelle (2 min)

**[Menu Prévisions Journalières → ouvrir celle du 28/07/2026]**

> *« Voici une journée de dépense : 1 740 000 ariary. »*

Pointez la colonne **Poste prévu** :

> *« Chaque ligne est rattachée à son enveloppe. 700 000 de ciment, sur les
> 21 millions prévus pour le ciment. »*

Puis la colonne **Reste sur l'enveloppe** :

> *« Et là, ce qu'il reste après cette dépense. On suit la descente ligne par
> ligne, comme un relevé de compte. »*

---

## Acte 4 — Le moment fort (4 min)

C'est le cœur de la démonstration. Ne le racontez pas : **faites-le en direct.**

**[Menu Rapports de prévision → onglet « À justifier »]**

Laissez la ligne rouge s'afficher deux secondes avant de parler.

> *« Cet argent est sorti le 28 juillet. Personne n'a encore dit ce qu'il en a fait. »*

**[Menu Prévisions Journalières → Nouvelle prévision]**

Montrez que NOSY BE est **grisé** dans la liste des chantiers :

> *« Et tant que ce n'est pas justifié, on ne peut plus rien demander pour ce
> chantier. Le système refuse. »*

Insistez sur le point que votre interlocuteur va se demander :

> *« Les autres chantiers, eux, continuent normalement. Le blocage ne concerne
> que le chantier fautif. »*

**[Second onglet, compte Chef de chantier → ouvrir la prévision du 28/07]**

Saisissez un compte rendu court, à la main, devant lui :

> *« Coulage des fondations zone A, 6 tonnes de ciment utilisées, acompte versé
> aux maçons. »*

**[Envoyer le compte rendu]**

**[Revenir sur l'onglet Administrateur → rafraîchir → Réceptionner les travaux]**

> *« Je lis, je valide. Et maintenant… »*

**[Retour sur Nouvelle prévision — NOSY BE est de nouveau sélectionnable]**

> *« …le chantier peut repartir. »*

---

## Acte 5 — Les alertes (1 min 30)

**[Menu Alertes]**

> *« Vous n'avez pas besoin d'aller chercher l'information, elle vient à vous.
> Dès que la moitié d'une enveloppe est consommée, une alerte part.
> Au-delà de 100 %, elle passe en rouge. »*

Montrez une alerte de seuil et une alerte « travaux à justifier ».

> *« Et ça se recalcule tout seul, toutes les heures. »*

---

## Acte 6 — Le magasinier (1 min 30)

**[Menu Stock (Matériaux) → Détail sur FER-10]**

Posez la fiche papier à côté de l'écran.

> *« C'est exactement sa feuille. Mêmes colonnes, même logique.
> La différence : les soldes se calculent tout seuls, et personne ne peut
> effacer une ligne sans que ça se voie. »*

Précisez le cloisonnement :

> *« Chaque magasinier ne voit que son chantier. »*

---

## Acte 7 — La traçabilité et la clôture (1 min)

**[Menu Journal d'audit]**

> *« Chaque création, chaque modification, chaque suppression est enregistrée :
> qui, quand, depuis quel poste. »*

**[Cliquer un bouton PDF, n'importe où]**

> *« Et tout est exportable en PDF ou en Excel, si vous voulez du papier. »*

Puis concluez, **sans écran** :

> *« En résumé : double validation avant toute sortie d'argent, blocage tant que
> le précédent n'est pas justifié, alerte automatique à mi-parcours, et tout est
> tracé. »*

---

## Les questions qui vont venir

**« Et si quelqu'un modifie un prix ? »**
Impossible sur un article du catalogue : le prix est imposé, et pas seulement à
l'écran — le serveur le réimpose à l'enregistrement.

**« Et si le chef écrit n'importe quoi dans son compte rendu ? »**
Vous le lui renvoyez avec un motif. Tant que vous n'avez pas réceptionné, son
chantier reste bloqué.

**« Qui a fait quoi ? »**
Le journal d'audit, avec le nom, l'heure et l'adresse IP.

**« Le magasinier peut-il voir les autres chantiers ? »**
Non. Le contrôle est fait côté serveur, pas seulement masqué à l'écran.

**« Ça coûte combien ? »**
Actuellement gratuit sur Render. Deux limites à connaître : le site s'endort après
15 minutes d'inactivité, et la base gratuite expire au bout de 90 jours. Le passage
à l'offre payante est de l'ordre de 7 $ par mois.

**« C'est prêt à être utilisé ? »**
Le cycle complet fonctionne. Restent à faire : créer les vrais chantiers, créer un
magasinier par chantier, et changer les mots de passe.

---

## À annoncer vous-même, sans attendre la question

Cela vous rendra plus crédible que de laisser découvrir les limites :

- Les données affichées sont un **jeu de démonstration**, les vrais chantiers restent à créer
- Il n'y a **pas de tests automatisés** : les vérifications ont été faites manuellement
- Les **emails ne partent pas** (mode simulation) — la fonction « mot de passe oublié » ne
  fonctionne donc pas, mais le changement de mot de passe en ligne, si
- Aucune **reprise des archives papier** n'a été faite

---

## Les erreurs à éviter

**Ne montrez pas tous les menus.** Vous en avez une quinzaine ; le scénario n'en
utilise que sept. Le reste dilue le message.

**Ne parlez pas de technique** — pas de « Entity Framework », pas de « migration »,
pas de « API ». Votre interlocuteur veut savoir si son argent est protégé.

**Ne vous excusez pas** de ce qui manque. Annoncez-le une fois, calmement, et passez.

**Ne cliquez pas au hasard** pendant que vous parlez. Une action, une phrase.

**Si quelque chose plante**, ne cherchez pas à réparer devant lui : passez à la suite
et notez le point. Une démonstration interrompue par du débogage perd toute sa force.

---

## Repli si la connexion est mauvaise

Ayez des captures d'écran des cinq moments clés :

1. La bande de répartition du marché sur la fiche chantier
2. La prévision globale avec ses rubriques
3. L'onglet « À justifier » avec la ligne rouge
4. Le message de refus de nouvelle prévision
5. La fiche matériaux FER-10

Avec ces cinq images, vous pouvez tenir toute la présentation sans le site.
