# Redéployer ETAM sur Render avec les données locales

La base gratuite a expiré. On en recrée une, et on y transfère la base locale —
chantier de Diego, compte BGFI, enveloppes, utilisateurs — au lieu de tout retaper.

**L'ordre compte.** La restauration doit se faire *avant* que l'application ne se
connecte à la nouvelle base. Sinon elle y crée le schéma et les comptes, et la
restauration se heurte à des doublons.

C'est pour ça qu'on met le service web en pause à l'étape 3 : dès que la nouvelle
base existe, Render peut relancer le service tout seul, et il prendrait la base
vide de vitesse.

---

## Étape 0 — Sauvegarder le local AVANT de toucher à Render

Fais l'étape 2 ci-dessous en premier. Tant que le fichier `etam_local.sql` n'est
pas sur ton disque, ne supprime rien sur Render.

---

## Étape 1 — Pousser le code

```powershell
cd C:\Users\ASUS\Documents\Etam_export
git add -A
git commit -m "Budget Materiel calcule, compte bancaire saisi, plafond de flechage, tri des dates, annulation d'enveloppe"
git push
```

Le dépôt est `https://github.com/LianaRabenja/etam.git`, branche `main`.

Render va reconstruire tout seul. La construction échouera au démarrage faute de
base — c'est normal à ce stade, on la branche à l'étape 5.

---

## Étape 2 — Sauvegarder la base locale

Arrête l'application locale (`Ctrl+C`) pour figer les données.

Dans **pgAdmin** : serveur local › Databases › clic droit sur **etam_erp** › **Backup…**

| Champ | Valeur |
|---|---|
| Filename | `C:\Users\ASUS\Documents\etam_local.sql` |
| Format | **Plain** |
| Encoding | UTF8 |

Puis onglet **Dump options** :

- **Do not save › Owner** : activé
- **Do not save › Privileges** : activé

Ces deux options sont importantes. Sur Render l'utilisateur s'appelle `etam`, pas
`postgres` : sans elles, la restauration échouerait sur des rôles inexistants.

Clique **Backup**. Le fichier fait quelques centaines de Ko.

---

## Étape 3 — Mettre le service en pause, supprimer l'ancienne base, créer la nouvelle

**3a. Mettre le service web en pause.** Render › service **etam-erp** › onglet
**Settings** › tout en bas › **Suspend Web Service**.

Sans cette pause, le service se relancerait dès que la nouvelle base apparaît,
créerait le schéma et les trois comptes dans une base vide, et la restauration de
l'étape 4 échouerait sur des doublons.

**3b. Supprimer l'ancienne base.** Render › base **etam-db** (expirée) › onglet
**Settings** › **Delete Database**. Render demande de retaper le nom pour confirmer.

Rien n'est perdu : une base gratuite expirée n'est plus lisible, et de toute façon
tes données à jour sont dans `etam_local.sql`.

**3c. Créer la nouvelle base.** **New +** › **PostgreSQL**

| Champ | Valeur |
|---|---|
| Name | `etam-db` — **le même nom que l'ancienne** |
| Database | `etam_erp` |
| User | `etam` |
| Region | Frankfurt |
| Plan | Free |

Le nom identique n'est pas un détail : `render.yaml` relie le service à sa base par
`fromDatabase: name: etam-db`. En reprenant le nom, la liaison se rétablit seule.
Si Render refuse le nom parce que la suppression n'est pas encore propagée, attends
quelques minutes plutôt que de prendre `etam-db-2` — sinon il faudra coller la
chaîne à la main à l'étape 5.

Attends que le statut passe à **Available**, puis onglet **Connect**. Note les deux
adresses, tu auras besoin des deux :

- **External Database URL** — pour pgAdmin, depuis ton PC
- **Internal Database URL** — pour le service web, à l'étape 5

---

## Étape 4 — Restaurer la sauvegarde dans la base Render

**4a. Enregistrer la base Render dans pgAdmin.** Clic droit sur *Servers* ›
*Register* › *Server…*

- Onglet **General** : Name = `Render ETAM`
- Onglet **Connection** : recopie hôte, port, base, utilisateur et mot de passe
  depuis l'*External Database URL*, qui a la forme
  `postgresql://UTILISATEUR:MOTDEPASSE@HOTE/BASE`
- Onglet **Parameters** : ajoute `SSL mode` = **Require** — Render refuse les
  connexions non chiffrées

**4b. Exécuter le fichier.** Sélectionne la base `etam_erp` de *Render ETAM*, ouvre
le **Query Tool**, puis l'icône **Open File** et choisis `etam_local.sql`. Exécute
avec **F5**.

Compte une à deux minutes. Des messages `NOTICE` peuvent apparaître, c'est normal ;
seules les lignes `ERROR` posent problème.

**4c. Vérifier.** Toujours sur la base Render :

```sql
SELECT 'Chantiers' t, count(*) FROM "Chantiers"
UNION ALL SELECT 'Comptes', count(*) FROM "ComptesBancaires"
UNION ALL SELECT 'Enveloppes', count(*) FROM "PrevisionsMensuelles"
UNION ALL SELECT 'Utilisateurs', count(*) FROM "AspNetUsers"
UNION ALL SELECT 'Migrations', count(*) FROM "__EFMigrationsHistory";
```

`Migrations` doit être à 17. C'est ce qui empêchera l'application de rejouer les
migrations au démarrage.

---

## Étape 5 — Brancher le service web et le réveiller

Render › service **etam-erp** › onglet **Environment**

- `ETAM_CONNECTION` : si tu as repris le nom `etam-db`, la liaison est déjà bonne,
  ne touche à rien. Sinon, colle l'**Internal Database URL** de la nouvelle base.
- Vérifie que `ETAM_DONNEES_EXEMPLE` **n'existe pas** — sinon le chantier de
  démonstration NOSY BE reviendra
- Vérifie que `ETAM_NETTOYER_CHANTIERS` et `ETAM_TOUT_EFFACER` **n'existent pas** —
  ils videraient la base à chaque redémarrage

Puis onglet **Settings** › **Resume Web Service** pour lever la pause de l'étape 3a,
et **Manual Deploy › Deploy latest commit**.

---

## Étape 6 — Lire les logs

Onglet **Logs**. Tu dois voir :

| Attendu | Signification |
|---|---|
| `Your service is live` | En ligne |
| `Données de démonstration désactivées.` | Le seed n'a rien recréé |

Tu ne dois **pas** voir de ligne `Applying migration` : la base restaurée est déjà
à jour. Si tu en vois, c'est que la restauration n'a pas pris — reviens à l'étape 4.

---

## Étape 7 — Les mots de passe

Les comptes viennent de ta base locale : ils gardent les mots de passe de
développement (`Admin@2026`, `Finance@2026`, `Chef@2026`).

Les variables `ETAM_ADMIN_PASSWORD` & co. de Render **ne les changeront pas** : le
seed ne fixe un mot de passe qu'à la création d'un compte, et ces comptes existent
déjà.

**Connecte-toi et change les trois mots de passe immédiatement.** Le site est
public.

---

## Étape 8 — Vérifier l'application

| Écran | Attendu |
|---|---|
| 1. Chantiers | Construction de 6 forages PAAEP Diego |
| Fiche du chantier | Budget projet 800 000 000, banque BGFI 620 466 000 |
| 2. Banques | Compte SARL ETAM, n° 41000869011-66 |
| 4. Enveloppe du mois | Tes enveloppes de juillet et août |
| Réglages › Catalogue | Tes vrais prix |

---

## À savoir

Le service s'endort après 15 minutes sans visite ; la première page met environ
50 secondes à répondre. Ouvre le site 10 minutes avant une démonstration.

La base gratuite expire de nouveau au bout de 90 jours. Refais cette procédure le
moment venu — ou passe à un hébergeur dont la base gratuite ne meurt pas.
