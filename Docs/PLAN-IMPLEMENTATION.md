# Plan d'Implémentation Global — ContainSharp (VeilleNet)

> Analyse complète du site effectuée le 28/02/2026
> Application ASP.NET Core 10 • Razor Pages • PostgreSQL • Mistral AI • Quartz • AWS SES

---

## Table des matières

1. [Résumé de l'analyse](#1-résumé-de-lanalyse)
2. [Phase 1 — Correctifs critiques (sécurité & bugs)](#phase-1--correctifs-critiques-sécurité--bugs)
3. [Phase 2 — Refactoring & qualité de code](#phase-2--refactoring--qualité-de-code)
4. [Phase 3 — Améliorations UX & contenu](#phase-3--améliorations-ux--contenu)
5. [Phase 4 — Nouvelles fonctionnalités](#phase-4--nouvelles-fonctionnalités)
6. [Phase 5 — Performance & DevOps](#phase-5--performance--devops)
7. [Estimation & priorisation](#estimation--priorisation)

---

## 1. Résumé de l'analyse

### Architecture actuelle

Le site agrège du contenu tech (C#/.NET) depuis multiples sources (RSS, GitHub API, X/Twitter API, StackOverflow) et fournit :

| Catégorie | Pages |
|-----------|-------|
| **Dashboard** | Index (hub central avec 7 flux de news) |
| **IA** | AiSummary (résumés IA), KnowledgeGraph (graphe 3D d'entités) |
| **Référence** | Liens, MCP (outils), LatestLLM, Radar (cycle de vie), Roadmap |
| **Interactif** | Training (quiz C#), Newsletter (inscription/désinscription) |
| **Archive** | History (recherche), NewsletterArchive |
| **API** | GraphApi (données graphe), AiSummaryApi (résumés) |

### Points forts identifiés
- Architecture Interface-first (DI/testabilité)
- Pattern cache-aside cohérent sur tous les services
- `IDbContextFactory` pour les jobs background (Quartz)
- Chargement parallèle (`Task.WhenAll`) sur la page Index
- Déduplication Jaccard pour les articles
- Accessibilité (skip-links, ARIA, sémantique)
- Système de newsletter avec double opt-in

### Problèmes identifiés (synthèse)

| Catégorie | Nb | Exemples clés |
|-----------|----|---------------|
| **Sécurité** | 4 | `@Html.Raw()` XSS, risque SSRF, `Thread.Sleep` async, timezone Linux |
| **Bugs** | 4 | `Thread.Sleep` en async, code mort NewsletterSendJob, timezone Linux |
| **Code smell** | 8 | ~3500 lignes de données hardcodées, God repository (50+ méthodes), services inutilisés injectés |
| **Duplication** | 6 | CSS Liens/MCP, pattern fetch-news x3, YouTube ID extraction x2 |
| **Performance** | 3 | Pas de bundling/minification, KnowledgeGraph ~1000 lignes inline, cache sans limite |

---

## Phase 1 — Correctifs critiques (sécurité & bugs)

> **Priorité : IMMÉDIATE** • Risque si non traité : fuite de données, crash en production

### 1.1 🔴 Protection XSS — `@Html.Raw()` non sanitisé

**Problème** : Les résumés IA sont injectés avec `@Html.Raw()` dans `AiSummary.cshtml`, `_NewsBlockPartial.cshtml`, et `History.cshtml` sans sanitisation préalable.

**Actions** :
- [ ] Créer un `HtmlSanitizerService` basé sur une allowlist de tags HTML sûrs (`<p>`, `<ul>`, `<li>`, `<strong>`, `<em>`, `<br>`, `<code>`)
- [ ] Appliquer la sanitisation dans le service `AiSummarizationService` avant stockage en DB
- [ ] Appliquer la sanitisation en sortie dans les vues comme filet de sécurité

### 1.2 🔴 Validation SSRF dans `AiSummarizationService`

**Problème** : `httpClient.GetStringAsync(post.Url)` récupère des URLs arbitraires pour résumé IA, ouvrant un risque de Server-Side Request Forgery.

**Actions** :
- [ ] Implémenter une allowlist de domaines autorisés (ex: `devblogs.microsoft.com`, `github.com`, `medium.com`, etc.)
- [ ] Bloquer les URLs privées (localhost, 10.x, 192.168.x, 127.x, liens `file://`)
- [ ] Ajouter un timeout strict et une limite de taille de réponse (ex: 500KB max)

### 1.3 🟡 `Thread.Sleep` dans `EmailService`

**Problème** : `Thread.Sleep(1000)` bloque le thread dans une méthode async.

**Action** :
- [ ] Remplacer par `await Task.Delay(1000)`

### 1.4 🟡 Timezone incompatible Linux/Docker

**Problème** : `"Romance Standard Time"` (Windows TZID) dans `ApplicationDbContext.SaveChangesAsync` et `DailyNewsletter` échouera sur Linux/Railway.

**Actions** :
- [ ] Ajouter le package NuGet `TimeZoneConverter`
- [ ] Remplacer par `TZConvert.GetTimeZoneInfo("Europe/Paris")` partout

### 1.5 🟡 Code mort dans `NewsletterSendJob`

**Problème** : `filteredSummaries` et `defaultTheme` sont calculés mais jamais utilisés — la newsletter envoie les résumés non filtrés.

**Action** :
- [ ] Soit utiliser `filteredSummaries` (comportement attendu), soit supprimer le code mort

### 1.6 🟡 Schema manquant sur entités DB

**Problème** : `XTrackedAccount` et `JobExecutionLog` n'ont pas le schema `containsharp`.

**Action** :
- [ ] Ajouter `[Table("x_tracked_accounts", Schema = "containsharp")]` et `[Table("job_execution_logs", Schema = "containsharp")]`
- [ ] Créer une migration EF Core

---

## Phase 2 — Refactoring & qualité de code

> **Priorité : HAUTE** • Impact : maintenabilité, lisibilité, réduction du code

### 2.1 Externaliser les données statiques (~3500 lignes → fichiers JSON)

**Fichiers concernés** :
| Service | Lignes | Contenu |
|---------|--------|---------|
| `QuestionService.cs` | ~1867 | Questions du quiz |
| `FrameworkVersionService.cs` | ~894 | Versions des frameworks |
| `LLMService.cs` | ~489 | Catalogue LLM |
| `RoadmapHelper.cs` | ~301 | Roadmap d'apprentissage |
| `LinkHelper.cs` | ~281 | Liens utiles |
| `MCPService.cs` | ~200 | Outils MCP |

**Actions** :
- [ ] Créer un dossier `Data/SeedData/` avec des fichiers JSON (un par service)
- [ ] Modifier chaque service pour charger les données depuis le JSON au démarrage
- [ ] Ajouter un `FileSystemWatcher` ou un rechargement planifié pour permettre les mises à jour sans redéploiement
- [ ] Supprimer les entrées dupliquées dans `FrameworkVersionService` (.NET 5.0, .NET Core 3.1, etc.)

### 2.2 Scinder le God Repository `INewsRepository`

**Problème** : 50+ méthodes couvrant 6 agrégats dans une seule interface.

**Actions** :
- [ ] Créer `IArticleRepository` (articles + recherche)
- [ ] Créer `IAiSummaryRepository` (résumés IA + entités)
- [ ] Créer `ISubscriberRepository` (abonnés newsletter)
- [ ] Créer `INewsletterRepository` (envois newsletter)
- [ ] Garder `INewsRepository` comme façade ou le supprimer
- [ ] Adapter tous les consommateurs

### 2.3 Extraire le pattern commun de fetch-news

**Problème** : `AINewsService`, `BlogAggregationService`, et `WinFormNewsService` ont le même pattern : cache → fetch RSS → enrich HasAiSummary → cache set.

**Actions** :
- [ ] Créer une classe `BaseNewsAggregationService<T>` avec le pattern commun
- [ ] Chaque service enfant ne définit que : les URLs des feeds, le filtre de pertinence, les catégories

### 2.4 Remplacer `Console.WriteLine` par `ILogger`

**Fichiers** : `GitHubService`, `AINewsService`, `BlogAggregationService`, `WinFormNewsService`, `StackOverflowService`, `VideoService`

- [ ] Remplacer toutes les occurrences par `_logger.LogError/LogWarning`

### 2.5 Supprimer les services injectés non utilisés

| Page | Service inutilisé |
|------|-------------------|
| `Index.cshtml.cs` | `ICacheService` |
| `History.cshtml.cs` | `INewsRepository` |
| `KnowledgeGraph.cshtml.cs` | `INewsRepository` |

- [ ] Retirer les injections et champs correspondants

### 2.6 CSS dupliqué → fichier partagé

**Problème** : ~60 lignes de CSS identiques entre `Liens.cshtml` et `MCP.cshtml` (gradients, animations, cards).

**Action** :
- [ ] Extraire dans `wwwroot/css/category-cards.css`
- [ ] Référencer via `@section Styles` dans les deux pages

### 2.7 Étendre `_NewsBlockPartial` aux sections manquantes d'Index

**Problème** : 4 sections sur 7 de la page Index (Videos, X Posts, StackOverflow, GitHub) sont codées en dur au lieu d'utiliser le partial partagé.

**Actions** :
- [ ] Adapter `NewsBlockViewModel` pour supporter les cas spécifiques (thumbnails vidéo, avatars X, tags SO)
- [ ] Convertir les 4 sections restantes pour utiliser `_NewsBlockPartial`

### 2.8 Ajouter un namespace manquant à `HistoryModel`

- [ ] Ajouter `namespace VeilleNet.Pages;` dans `History.cshtml.cs`

### 2.9 Déplacer `SitemapUrl` dans le dossier Models

- [ ] Extraire la classe `SitemapUrl` de `Sitemap.cshtml.cs` vers `Models/SitemapUrl.cs`

### 2.10 Interface `IHasTimestamps` pour les entités

- [ ] Créer l'interface avec `CreatedAt` / `UpdatedAt`
- [ ] L'implémenter sur toutes les entités persistées
- [ ] Simplifier `SaveChangesAsync` avec un `foreach` générique

---

## Phase 3 — Améliorations UX & contenu

> **Priorité : MOYENNE** • Impact : expérience utilisateur, SEO, engagement

### 3.1 Page "Nouveautés" (Release Notes agrégées)

**Concept** : Remplacer les 4 entrées hardcodées de `ReleaseNewsService` par un vrai agrégateur de release notes depuis les repos GitHub officiels (.NET, ASP.NET Core, EF Core, C#).

**Actions** :
- [ ] Créer `IReleaseNotesAggregationService` qui utilise l'API GitHub Releases
- [ ] Créer une page `Releases.cshtml` avec filtres par framework et timeline visuelle
- [ ] Ajouter à la navigation

### 3.2 Mode sombre (Dark Mode)

**Concept** : Le site utilise déjà Bootstrap 5 qui supporte `data-bs-theme="dark"`.

**Actions** :
- [ ] Ajouter un toggle dark/light dans la navbar
- [ ] Persister le choix en `localStorage`
- [ ] Adapter les CSS personnalisés (gradients, cards) pour les deux thèmes
- [ ] Respecter `prefers-color-scheme` par défaut

### 3.3 Sitemap dynamique

**Problème** : URLs hardcodées dans `Sitemap.cshtml.cs` — les nouvelles pages ne sont pas ajoutées automatiquement.

**Actions** :
- [ ] Générer le sitemap dynamiquement via réflexion sur les Razor Pages
- [ ] Ajouter les articles de la base de données (URLs individuelles)
- [ ] Ajouter l'URL de base depuis la configuration au lieu de la hardcoder

### 3.4 Fil RSS/Atom de sortie

**Concept** : Le site consomme des RSS mais n'en produit pas. Proposer un flux RSS des articles agrégés pour les utilisateurs tech.

**Actions** :
- [ ] Créer un endpoint `/feed/rss` via un controller
- [ ] Exposer les 50 derniers articles avec résumés IA
- [ ] Ajouter le `<link rel="alternate" type="application/rss+xml">` dans le layout

### 3.5 Enrichir le quiz Training

**Actions** :
- [ ] Ajouter un système de niveaux (débutant/intermédiaire/avancé)
- [ ] Afficher un badge/score final avec partage social
- [ ] Ajouter des questions sur .NET 10, Blazor, MAUI, Aspire (contenu actuel)
- [ ] Possibilité de relancer uniquement les questions ratées

### 3.6 Page "Comparaison de LLMs"

**Concept** : Transformer la page `LatestLLM` en un vrai comparateur interactif.

**Actions** :
- [ ] Ajouter un tableau de comparaison filtrable (par score, date, éditeur)
- [ ] Graphique d'évolution des scores dans le temps (Chart.js)
- [ ] Tags de capacités (code, multimodal, agents, etc.)

### 3.7 Améliorer le KnowledgeGraph

**Actions** :
- [ ] Extraire les ~1000 lignes de CSS/JS inline vers `wwwroot/css/knowledge-graph.css` et `wwwroot/js/knowledge-graph.js`
- [ ] Ajouter un panneau latéral avec les détails de l'entité cliquée
- [ ] Permettre le filtrage par source ou par période

### 3.8 Internationalisation (i18n) — préparation

**Problème** : Textes mixtes français/anglais dans les vues.

**Actions** :
- [ ] Uniformiser en français (langue principale) pour les vues
- [ ] Extraire les chaînes dans des fichiers de ressources `.resx`
- [ ] Préparer le support anglais pour le futur

---

## Phase 4 — Nouvelles fonctionnalités

> **Priorité : NORMALE** • Impact : différenciation, valeur ajoutée

### 4.1 Statistiques de contenu

**Concept** : Dashboard des métriques d'agrégation.

**Actions** :
- [ ] Nombre d'articles par source/par jour (graphique)
- [ ] Nombre de résumés IA générés vs en attente
- [ ] Nombre d'abonnés newsletter (courbe d'évolution)
- [ ] Top entités nommées dans le KnowledgeGraph

### 4.2 Recherche globale unifiée

**Concept** : Barre de recherche dans la navbar qui cherche dans tous les contenus.

**Actions** :
- [ ] Endpoint API `/api/search?q=` qui agrège : articles DB, liens, LLMs, outils MCP
- [ ] Résultats groupés par catégorie
- [ ] Autocomplétion client-side avec debounce
- [ ] Intégrer dans le layout (_Layout.cshtml)

### 4.3 Page "Écosystème .NET" (carte interactive)

**Concept** : Visualisation de l'écosystème .NET avec les relations entre frameworks, librairies, et outils — complémentaire au Radar.

**Actions** :
- [ ] Utiliser un diagramme interactif (D3.js ou Mermaid)
- [ ] Catégories : Web, Desktop, Mobile, Cloud, Data, AI/ML, Testing
- [ ] Liens vers les pages existantes (Liens, MCP, Radar)

### 4.4 Système de tags/catégories pour les articles

**Concept** : Taguer automatiquement les articles avec les entités nommées existantes.

**Actions** :
- [ ] Ajouter un filtre par tag sur la page History
- [ ] Nuage de tags sur la sidebar du dashboard
- [ ] Utiliser les `NamedEntity` déjà extraites par le service IA

### 4.5 Extension `NewsletterSendJob` avec `LoggedJobBase`

**Action** :
- [ ] Faire hériter `NewsletterSendJob` de `LoggedJobBase` pour cohérence du logging d'exécution

---

## Phase 5 — Performance & DevOps

> **Priorité : NORMALE** • Impact : temps de chargement, fiabilité

### 5.1 Bundling & minification des assets

**Actions** :
- [ ] Ajouter `WebOptimizer` ou un pipeline Vite/esbuild
- [ ] Bundler les JS (`site.js` + `output-shrink.js` + `ai-modal.js`) par page
- [ ] Minifier tous les CSS/JS en production
- [ ] Ajouter le versioning par hash pour le cache-busting

### 5.2 Limiter la taille du cache mémoire

**Problème** : `MemoryCacheService` n'a aucune limite — peut croître indéfiniment.

**Actions** :
- [ ] Configurer `SizeLimit` sur `MemoryCache`
- [ ] Attribuer un `Size` à chaque entrée de cache
- [ ] Ajouter une stratégie d'éviction (LRU)

### 5.3 Health Check endpoint

**Actions** :
- [ ] Ajouter `app.MapHealthChecks("/health")` avec vérification DB + connectivité Mistral
- [ ] Exploiter `DatabaseHealthCheck.cs` existant dans le dossier `Tools/`
- [ ] Utile pour Railway (monitoring) et load balancers

### 5.4 Optimiser `VideoService` — déduplication O(n²)

**Problème** : `videos.Any(w => w.Title == video.Title)` est O(n²).

**Action** :
- [ ] Utiliser un `HashSet<string>` pour les titres déjà vus

### 5.5 Corriger `MistralChatClientFactory` — `HttpClient` leak

**Problème** : `OpenAIChatClient` crée son propre `HttpClient` au lieu d'utiliser `IHttpClientFactory`, causant un épuisement de sockets.

**Action** :
- [ ] Injecter `IHttpClientFactory` et passer le `HttpClient` créé au constructeur

### 5.6 Ajouter des tests unitaires

**Actions** :
- [ ] Créer un projet `VeilleNet.Tests`
- [ ] Tests prioritaires : `NewsDeduplicationService`, `HtmlSanitizer`, `SummaryFilter`, `ArticleContentExtractor`
- [ ] Tests d'intégration : `NewsRepository` avec une DB PostgreSQL in-memory ou Testcontainers

---

## Estimation & priorisation

| Phase | Effort estimé | Risque si ignoré | Impact positif |
|-------|---------------|-------------------|----------------|
| **Phase 1** — Sécurité & bugs | 2-3 jours | 🔴 Critique | Protection des données, stabilité |
| **Phase 2** — Refactoring | 5-7 jours | 🟡 Moyen | -3500 lignes, maintenabilité x3 |
| **Phase 3** — UX & contenu | 5-8 jours | 🟢 Faible | Engagement, SEO, rétention |
| **Phase 4** — Fonctionnalités | 8-12 jours | 🟢 Faible | Différenciation, valeur ajoutée |
| **Phase 5** — Perf & DevOps | 2-3 jours | 🟡 Moyen | Temps de chargement, fiabilité |

### Ordre d'exécution recommandé

```
Phase 1 (immédiat)
    ↓
Phase 2.1-2.5 (refactoring critique)
    ↓
Phase 5.5-5.6 (correctifs perf + tests)
    ↓
Phase 3.2, 3.4, 3.7 (quick wins UX)
    ↓
Phase 2.6-2.10 (refactoring cosmétique)
    ↓
Phase 4.2, 4.4 (fonctionnalités à fort impact)
    ↓
Phase 3 restant → Phase 4 restant → Phase 5 restant
```

---

## Résumé des fichiers impactés

| Fichier | Phases concernées |
|---------|-------------------|
| `Program.cs` | 2.2, 5.3 |
| `Pages/Shared/_Layout.cshtml` | 3.3, 3.5, 4.4 |
| `Pages/Index.cshtml` + `.cs` | 2.5, 2.7 |
| `Pages/AiSummary.cshtml` | 1.1 |
| `Pages/KnowledgeGraph.cshtml` + `.cs` | 2.5, 3.7 |
| `Pages/History.cshtml.cs` | 2.5, 2.8 |
| `Pages/Liens.cshtml` | 2.6 |
| `Pages/MCP.cshtml` | 2.6 |
| `Pages/Sitemap.cshtml.cs` | 2.9, 3.4 |
| `Services/Agent/AiSummarizationService.cs` | 1.1, 1.2 |
| `Services/Agent/MistralChatClientFactory.cs` | 5.5 |
| `Services/Tools/EmailService.cs` | 1.3 |
| `Services/Tools/NewsletterSendJob.cs` | 1.5, 4.5 |
| `Services/Data/NewsRepository.cs` | 1.4, 2.2 |
| `Data/ApplicationDbContext.cs` | 1.4, 2.10 |
| `Models/Entities/XTrackedAccount.cs` | 1.6 |
| `Models/Entities/JobExecutionLog.cs` | 1.6 |
| `Services/News/AINewsService.cs` | 2.3, 2.4 |
| `Services/News/BlogAggregationService.cs` | 2.3, 2.4 |
| `Services/News/WinFormNewsService.cs` | 2.3, 2.4 |
| `Services/News/VideoService.cs` | 2.4, 5.4 |
| `Services/GitHubService.cs` | 2.4 |
| `Services/FrameworkVersionService.cs` | 2.1 |
| `Services/LLMService.cs` | 2.1 |
| `Services/QuestionService.cs` | 2.1 |
| `Services/LinkHelper.cs` | 2.1 |
| `Services/MCPService.cs` | 2.1 |
| `Services/RoadmapHelper.cs` | 2.1 |
