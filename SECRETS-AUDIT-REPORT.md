# 🔍 Audit des secrets dans l'historique Git

**Date de l'audit** : 2026-03-03  
**Périmètre** : Tous les commits de toutes les branches du dépôt `matmout/veillenet`

---

## Résumé

| Type de secret | Trouvé en clair ? | Détails |
|---|---|---|
| Mistral API Key | ❌ Non | Toujours vide (`""`) dans `appsettings.json` |
| AWS Access Key | ❌ Non | Vide ou placeholder `${AWS_ACCESS_KEY}` |
| AWS Secret Key | ❌ Non | Vide ou placeholder `${AWS_SECRET_KEY}` |
| GitHub Token | ❌ Non | Seulement le placeholder `votre_token_github` dans README.md |
| X/Twitter Bearer Token | ❌ Non | Toujours vide (`""`) |
| X/Twitter Consumer Key/Secret | ❌ Non | Toujours vide (`""`) |
| Database Connection String | ❌ Non | Toujours vide (`""`) |
| **DataProtection Master Key** | ⚠️ **OUI** | Clé de chiffrement ASP.NET committée en clair |

---

## 🚨 Problème trouvé : DataProtection Master Key

### Fichier concerné
`keys/key-1423bbd6-b6e6-4e42-bf12-5a376bd460f9.xml`

### Détails du commit

- **Commit SHA** : `36d77132ff0473fb83d58225beacfea65782e649`
- **Auteur** : matmout <matthieu.trachsel@gmail.com>
- **Date** : Sun Mar 1 23:57:47 2026 +0100
- **Message** : Refonte news/quiz : seed JSON, sécurité, UI factorisée

### Contenu sensible

Le fichier contient une **clé maître ASP.NET DataProtection non chiffrée** (AES_256_CBC + HMACSHA256).
Cette clé est utilisée pour protéger les cookies de session, les tokens anti-forgery (CSRF) et tout
autre contenu chiffré par le mécanisme DataProtection d'ASP.NET Core.

```xml
<masterKey p4:requiresEncryption="true">
  <!-- Warning: the key below is in an unencrypted form. -->
  <value>Dx+yq+FSbGAV1HurlT7HuIOsFNFW/FD8OUDeokridXJj66zSKw70ZLvnGGq/tQ558RF9E7G3F2JE/KMSFCDl0Q==</value>
</masterKey>
```

### Impact

- Un attaquant possédant cette clé pourrait **déchiffrer les cookies de session** et les **tokens CSRF**.
- La clé expire le **2026-03-07** mais reste dans l'historique Git indéfiniment.
- Le répertoire `keys/` est bien listé dans `.gitignore` mais le fichier a été committé **avant** que le `.gitignore` soit pris en compte.

### Recommandations

1. **Faire une rotation de la clé** : Supprimer le fichier `keys/key-1423bbd6-b6e6-4e42-bf12-5a376bd460f9.xml` et redémarrer l'application pour en générer une nouvelle.
2. **Retirer le fichier du suivi Git** : Fait dans ce PR via `git rm --cached`.
3. **Purger l'historique Git** via `git filter-branch` ou [BFG Repo-Cleaner](https://rtyley.github.io/bfg-repo-cleaner/) pour supprimer définitivement la clé de l'historique. Cette étape est **indispensable** que le dépôt soit public ou privé, car toute personne ayant accès au dépôt pourrait exploiter la clé pour déchiffrer des cookies de session et des tokens CSRF.

---

## ⚠️ Fichiers suivis malgré le .gitignore

Les fichiers suivants sont listés dans `.gitignore` mais restent suivis par Git car ils ont été committés avant l'ajout des règles :

| Fichier | Contient des secrets ? | Action |
|---|---|---|
| `appsettings.json` | Non (valeurs vides) | Retiré du suivi Git |
| `appsettings.Development.json` | Non (placeholders `${}`) | Retiré du suivi Git |
| `cache/official-x-posts.json` | Non (données publiques) | Retiré du suivi Git |
| `keys/key-*.xml` | **OUI** (masterKey) | Retiré du suivi Git |

---

## ✅ Clés API officielles — Aucune fuite détectée

Après analyse exhaustive de l'ensemble des commits et de tous les fichiers (`*.json`, `*.cs`, `*.yml`, `*.xml`, `*.env`, `*.md`), **aucune clé API officielle n'a été trouvée en clair** dans l'historique Git :

- Tous les champs sensibles dans `appsettings.json` et `appsettings.Development.json` sont soit **vides** (`""`), soit des **références à des variables d'environnement** (`${AWS_ACCESS_KEY}`).
- Les fichiers de documentation (`README.md`, `SECURITY-*.md`) utilisent uniquement des **placeholders** (`votre_token_github`, `"..."`, etc.).
- Aucun pattern correspondant à des clés API connues (format `sk-*`, `AKIA*`, `ghp_*`, `github_pat_*`, etc.) n'a été détecté.

---

## Méthodologie

1. Recherche par `git log -p -S` sur les termes : `ApiKey`, `BearerToken`, `AwsSecretKey`, `AwsAccessKey`, `ConnectionString`, `ConsumerKey`, `ConsumerSecret`, `Token`, `Password`.
2. Recherche par regex sur les patterns de clés API connues : `sk-*`, `AKIA*`, `ghp_*`, `gho_*`, `github_pat_*`, `xox[baprs]-*`, `AIza*`, `ya29.*`.
3. Inspection manuelle de tous les fichiers de configuration à chaque commit.
4. Vérification du contenu du répertoire `keys/`.
5. Analyse des fichiers `docker-compose*.yml`, `railway.toml`, `launchSettings.json`.
