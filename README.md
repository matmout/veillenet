# ContainSharp - Votre dose quotidienne de C#

<div align="center">
  <img src="wwwroot/icons/containsharp.png" alt="ContainSharp Logo" width="128" height="128" />
  
  **Dashboard de veille technologique pour l'écosystème .NET**
  
  🌐 **[containsharp.com](https://containsharp.com)** - Site public en production
  
  [![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
  [![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
  [![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-512BD4)](https://docs.microsoft.com/en-us/aspnet/core/)
  [![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap)](https://getbootstrap.com/)
</div>

---

## 📋 Table des matières

- [À propos](#-à-propos)
- [Fonctionnalités](#-fonctionnalités)
- [Stack technique](#-stack-technique)
- [Architecture](#-architecture)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Déploiement](#-déploiement)
- [Performance et optimisation](#-performance-et-optimisation)
- [Accessibilité](#-accessibilité)
- [Développement](#-développement)

---

## 🎯 À propos

**ContainSharp** est un dashboard de veille technologique centralisant toutes les informations essentielles de l'écosystème .NET et C#. Le projet est développé en **ASP.NET Core avec Razor Pages** et utilise **.NET 10**.

### Problème résolu

Rester à jour sur l'écosystème .NET peut être difficile avec la multitude de sources d'information disponibles (blogs officiels, GitHub, releases, outils IA, etc.). Les développeurs doivent consulter de nombreux sites différents chaque jour.

### Solution apportée

ContainSharp centralise toutes ces informations en un seul endroit avec une interface moderne inspirée de Visual Studio, offrant :
- Agrégation automatique des sources officielles
- Mise en cache intelligente pour des performances optimales
- Interface responsive et accessible (WCAG 2.1 AA)
- Aucune base de données requise
- Déploiement simple avec Docker

---

## ✨ Fonctionnalités

### 🏠 Dashboard principal
- **Simulation de console Visual Studio** : Affichage animé des dernières actualités style "Build Output"
- **Agrégation de blogs .NET** : Articles des blogs officiels Microsoft (.NET, ASP.NET Core, Visual Studio, C#)
- **Release News** : Dernières versions de .NET, C#, ASP.NET Core avec liens vers les release notes
- **Actualités IA** : Suivi des outils de code assisté par IA (GitHub Copilot, OpenAI Codex, Claude, Mistral)
- **WinForms News** : Actualités spécifiques à Windows Forms
- **GitHub Trending** : Projets C# les plus populaires et récents (créés dans les 30 derniers jours)
- **Vidéos C#** : Dernières vidéos YouTube sur C# et .NET

### 🔗 Pages spécialisées

#### Liens utiles (`/Liens`)
Collection organisée de ressources essentielles pour développeurs C# :
- **Documentation officielle** : MSDN, docs.microsoft.com, C# guide
- **Outils de développement** : Visual Studio, VS Code, Rider
- **Apprentissage** : Tutoriels, cours, certifications
- **Communauté** : Forums, Discord, Reddit

#### MCP Tools (`/MCP`)
Liste des serveurs Model Context Protocol (MCP) pour les agents IA :
- **Serveurs de développement** : GitHub, GitLab, npm
- **Bases de données** : PostgreSQL, SQLite, MySQL
- **Services cloud** : AWS, Azure, Google Cloud
- **Outils utilitaires** : Filesystem, Fetch, Memory

#### LLM (`/LatestLLM`)
Comparateur et suivi des derniers modèles de langage (LLMs) :
- **Providers** : OpenAI, Anthropic, Google, Meta, Mistral, xAI
- **Informations** : Date de sortie, contexte, prix, capacités
- **Comparaison** : Tableau comparatif interactif

#### Training (`/Training`)
Quiz interactif pour tester ses connaissances en C# :
- **Questions variées** : Syntaxe, LINQ, async/await, patterns, performance
- **Difficulté progressive** : Débutant à expert
- **Feedback immédiat** : Explications détaillées
- **Suivi de progression** : Statistiques locales

### 📰 Newsletter (fonctionnalité désactivée)
Inscription pour recevoir des actualités personnalisées par email (en préparation)

---

## 🛠️ Stack technique

### Backend
- **Framework** : ASP.NET Core 10.0 (Razor Pages)
- **Langage** : C# 12.0 avec nullable reference types activés
- **Architecture** : Clean Architecture avec séparation Services/Models/Pages
- **Injection de dépendances** : Built-in DI Container
- **Cache** : In-Memory Cache (`IMemoryCache`) - pas de base de données
- **HTTP** : `IHttpClientFactory` pour les appels API
- **Parsing RSS/Atom** : `System.ServiceModel.Syndication`

### Frontend
- **UI Framework** : Bootstrap 5.3
- **Icons** : Bootstrap Icons 1.11+
- **Fonts** : Share Tech (Google Fonts) pour l'effet "terminal"
- **CSS** : CSS custom properties, animations keyframes, gradients
- **JavaScript** : Vanilla JS pour les animations (typing effect, console simulation)
- **Responsive** : Mobile-first design

### APIs et sources de données
- **GitHub API** : Projets trending, releases
- **RSS/Atom Feeds** :
  - Microsoft .NET Blog
  - ASP.NET Blog  
  - Visual Studio Blog
  - C# Blog
  - WinForms Blog
- **YouTube RSS** : Vidéos C# et .NET
- **Agrégation IA** : Sources diverses (OpenAI, Anthropic, etc.)

### DevOps et déploiement
- **Containerisation** : Docker avec Dockerfile multi-stage
- **Orchestration** : Docker Compose
- **CI/CD** : Compatible avec Railway, Azure, AWS
- **Configuration** : appsettings.json avec overrides par environnement

---

## 🏗️ Architecture

> 📖 **Pour une documentation détaillée de l'architecture**, consultez [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md)
> 
> Ce document contient :
> - Diagrammes d'architecture ASCII
> - Flux de données détaillés
> - Patterns et principes appliqués
> - Stratégies de cache et sécurité
> - Plans d'évolution

### Structure du projet

```
ContainSharp/
├── Models/                           # Modèles de données (POCOs)
│   ├── BlogPost.cs                  # Articles de blog
│   ├── AINews.cs                    # Actualités IA
│   ├── GitHubProject.cs             # Projets GitHub trending
│   ├── ReleaseNews.cs               # Versions de .NET/C#
│   ├── WinFormNews.cs               # Actualités WinForms
│   ├── Video.cs                     # Vidéos YouTube C#
│   ├── LLM.cs                       # Modèles de langage
│   ├── MCPTool.cs                   # Serveurs MCP
│   ├── UsefulLink.cs                # Liens utiles
│   ├── Question.cs                  # Questions quiz
│   └── NewsletterSubscription.cs    # Inscriptions newsletter
│
├── Services/                         # Couche métier (Business Logic)
│   ├── CacheService.cs              # Abstraction du cache mémoire
│   ├── BlogAggregationService.cs    # Agrégation blogs .NET
│   ├── AINewsService.cs             # Agrégation actualités IA
│   ├── GitHubService.cs             # API GitHub (trending)
│   ├── ReleaseNewsService.cs        # Releases .NET/C#/ASP.NET
│   ├── WinFormNewsService.cs        # Blog WinForms
│   ├── VideoService.cs              # YouTube RSS feeds
│   ├── LLMService.cs                # Données LLM providers
│   ├── MCPService.cs                # Serveurs MCP
│   ├── QuestionService.cs           # Questions C# quiz
│   ├── NewsletterService.cs         # Gestion newsletter
│   └── HtmlSanitizer.cs             # Sécurité XSS
│
├── Pages/                            # Pages Razor (UI)
│   ├── Index.cshtml(.cs)            # Dashboard principal
│   ├── Liens.cshtml(.cs)            # Liens utiles
│   ├── MCP.cshtml(.cs)              # Serveurs MCP
│   ├── LatestLLM.cshtml(.cs)        # Comparateur LLM
│   ├── Training.cshtml(.cs)         # Quiz C#
│   ├── About.cshtml(.cs)            # À propos
│   ├── Newsletter.cshtml(.cs)       # Inscription newsletter
│   ├── Privacy.cshtml(.cs)          # Politique de confidentialité
│   ├── Sitemap.cshtml(.cs)          # Sitemap XML
│   └── Shared/
│       ├── _Layout.cshtml           # Layout principal
│       └── _Layout.cshtml.css       # Styles scoped au layout
│
├── wwwroot/                          # Ressources statiques
│   ├── css/
│   │   ├── site.css                 # Styles globaux
│   │   └── scrollbar.css            # Custom scrollbar
│   ├── icons/
│   │   ├── containsharp.svg         # Logo SVG
│   │   └── containsharp.png         # Logo PNG
│   ├── images/
│   │   └── about.jpg                # Photo About
│   └── lib/                         # Bibliothèques front-end
│       └── bootstrap/
│
├── Docs/                             # Documentation
│   └── MCP_SERVICE.md               # Documentation MCP
│
├── Program.cs                        # Point d'entrée, configuration services
├── VeilleNet.csproj                 # Fichier de projet .NET
├── appsettings.json                 # Configuration production
├── appsettings.Development.json     # Configuration développement
├── Dockerfile                        # Image Docker multi-stage
├── docker-compose.yml               # Orchestration Docker
├── railway.toml                     # Config Railway deployment
├── ACCESSIBILITY.md                 # Documentation accessibilité
├── PERFORMANCE.md                   # Optimisations performance
└── README.md                         # Ce fichier
```

### Principes d'architecture

#### 1. Separation of Concerns
- **Models** : Données pures (DTOs/POCOs), pas de logique
- **Services** : Logique métier, appels API, parsing RSS
- **Pages** : Présentation uniquement, délègue aux services

#### 2. Dependency Injection
Tous les services sont enregistrés dans `Program.cs` :
```csharp
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IBlogAggregationService, BlogAggregationService>();
builder.Services.AddSingleton<IGitHubService, GitHubService>();
// ... autres services
```

#### 3. Caching Strategy
- **Cache en mémoire** : Toutes les données sont cachées (pas de DB)
- **Durées de cache** :
  - Blogs : 1 heure
  - GitHub trending : 6 heures
  - Releases : 24 heures
  - Vidéos : 1 heure
  - Données statiques (LLM, MCP, Links) : 24 heures
- **Pattern** : Check cache → Si vide, fetch data → Store in cache

#### 4. Error Handling
- **Try-catch silencieux** : Les erreurs retournent des listes vides
- **Graceful degradation** : Si une source échoue, les autres s'affichent
- **Pas de plantage** : L'application reste fonctionnelle même si toutes les API échouent

#### 5. Security
- **HTML Sanitization** : `HtmlSanitizer` nettoie tout contenu HTML externe
- **XSS Protection** : Encodage automatique par Razor
- **No database** : Pas d'injection SQL possible
- **External links** : `rel="noopener noreferrer"` sur tous les liens externes
- **HTTPS** : Redirection automatique en production

---

## 💻 Installation

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (version 10.0.100 ou supérieure)
- Système d'exploitation : Windows, macOS ou Linux
- (Optionnel) Docker pour le déploiement containerisé

### Installation locale

#### 1. Cloner le repository
```bash
git clone https://github.com/matmout/veillenet.git
cd veillenet
```

#### 2. Restaurer les dépendances
```bash
dotnet restore
```

#### 3. Lancer l'application
```bash
dotnet run
```

L'application sera accessible à :
- **HTTPS** : https://localhost:5001
- **HTTP** : http://localhost:5000

#### 4. (Optionnel) Mode développement avec hot-reload
```bash
dotnet watch run
```

Les modifications de code seront automatiquement recompilées et le navigateur rafraîchi.

---

## ⚙️ Configuration

### Variables d'environnement

Aucune variable d'environnement requise pour un déploiement basique. L'application fonctionne sans configuration.

### Configuration optionnelle

#### GitHub API Token (recommandé)
Pour éviter les limitations de rate limit de l'API GitHub :

**Méthode 1 : User Secrets (développement)**
```bash
dotnet user-secrets init
dotnet user-secrets set "GitHub:Token" "votre_token_github"
```

**Méthode 2 : Variable d'environnement (production)**
```bash
export GitHub__Token="votre_token_github"
```

**Méthode 3 : appsettings.json (NON recommandé)**
```json
{
  "GitHub": {
    "Token": "votre_token_github"
  }
}
```

⚠️ **Ne jamais commiter de token dans le code source !**

#### Obtenir un token GitHub
1. Aller sur https://github.com/settings/tokens
2. Générer un nouveau token (classic)
3. Sélectionner les permissions : `public_repo` (lecture seule)
4. Copier le token généré

### Configuration du cache

Les durées de cache peuvent être modifiées dans chaque service :

```csharp
// Dans Services/BlogAggregationService.cs
private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

// Dans Services/GitHubService.cs  
private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(6);

// Dans Services/ReleaseNewsService.cs
private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);
```

### Configuration HTTPS (production)

Pour un déploiement en production, configurez un certificat SSL/TLS valide.

---

## 🚀 Déploiement

### Option 1 : Déploiement Docker

#### Build de l'image
```bash
docker build -t containsharp .
```

#### Lancement du container
```bash
docker run -d -p 8080:8080 --name containsharp containsharp
```

L'application sera accessible sur `http://localhost:8080`

#### Avec Docker Compose
```bash
docker-compose up -d
```

### Option 2 : Déploiement Railway

Le projet inclut un fichier `railway.toml` pour un déploiement simplifié sur [Railway](https://railway.app).

1. Créer un compte sur Railway
2. Connecter votre repository GitHub
3. Railway détectera automatiquement le Dockerfile
4. L'application sera déployée avec HTTPS automatique

### Option 3 : Déploiement Azure App Service

```bash
# Publier l'application
dotnet publish -c Release -o ./publish

# Déployer sur Azure (avec Azure CLI)
az webapp up --name containsharp --resource-group myResourceGroup
```

### Option 4 : Déploiement manuel

```bash
# Publier en mode Release
dotnet publish -c Release -o ./publish

# Copier les fichiers sur le serveur
scp -r ./publish/* user@server:/var/www/containsharp/

# Sur le serveur, configurer un reverse proxy (nginx, Apache, etc.)
```

### Reverse Proxy (nginx exemple)

```nginx
server {
    listen 80;
    server_name containsharp.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## ⚡ Performance et optimisation

### Stratégies de cache
- **In-memory caching** : Toutes les données API sont mises en cache
- **Durées optimisées** : Balance entre fraîcheur des données et charge serveur
- **Pas de base de données** : Élimine complètement la latence DB

### Optimisations frontend
- **Lazy loading** : Images chargées uniquement quand visibles
- **Async/defer scripts** : Scripts non-bloquants
- **CSS minification** : En production via bundling
- **Preconnect** : Connexions anticipées aux CDN (fonts.googleapis.com)
- **Critical CSS** : Inline des styles critiques

### Optimisations backend
- **HttpClientFactory** : Réutilisation des connexions HTTP
- **Async/await** : I/O non-bloquant partout
- **Parallel requests** : `Task.WhenAll()` pour requêtes simultanées
- **Response compression** : Gzip/Brotli activé
- **Static file caching** : Cache navigateur pour assets statiques

### Métriques de performance
- **Time to First Byte (TTFB)** : < 200ms (avec cache chaud)
- **Largest Contentful Paint (LCP)** : < 2.5s
- **First Input Delay (FID)** : < 100ms
- **Cumulative Layout Shift (CLS)** : < 0.1

### Optimisation images
Voir [PERFORMANCE.md](PERFORMANCE.md) pour convertir PNG en WebP :
- Réduction de taille : ~30-50%
- Format moderne avec fallback PNG
- `<picture>` element pour compatibilité navigateurs

---

## ♿ Accessibilité

ContainSharp est conforme **WCAG 2.1 Level AA**. Voir [ACCESSIBILITY.md](ACCESSIBILITY.md) pour les détails complets.

### Fonctionnalités d'accessibilité

#### Contraste des couleurs
- ✅ Tous les textes passent le ratio 4.5:1 minimum (AA)
- ✅ Textes principaux passent 7:1 (AAA) : 11.55:1
- ✅ Palette testée sur fond sombre (#1e1e1e)

#### Navigation au clavier
- ✅ Tous les éléments interactifs accessibles via Tab
- ✅ Skip link "Skip to main content" pour navigation rapide
- ✅ Focus indicators visibles sur tous les éléments
- ✅ Ordre de tabulation logique

#### ARIA et sémantique
- ✅ Landmarks ARIA (`role="banner"`, `role="main"`, `role="contentinfo"`)
- ✅ Labels descriptifs sur tous les éléments interactifs
- ✅ `aria-label` et `aria-hidden` utilisés correctement
- ✅ HTML5 sémantique (`<header>`, `<nav>`, `<main>`, `<footer>`)

#### Lecteurs d'écran
- ✅ Testé avec NVDA, JAWS, VoiceOver
- ✅ Textes alternatifs sur toutes les images
- ✅ Liens explicites (pas de "cliquez ici")
- ✅ Boutons et formulaires bien labellisés

#### Responsive et adaptabilité
- ✅ Design responsive mobile-first
- ✅ Zoom jusqu'à 200% sans perte de fonctionnalité
- ✅ Pas de scroll horizontal
- ✅ Touch targets ≥ 44x44px

---

## 🧑‍💻 Développement

### Commandes utiles

```bash
# Build du projet
dotnet build

# Exécution en mode développement
dotnet run

# Exécution avec hot-reload
dotnet watch run

# Tests (si ajoutés)
dotnet test

# Publier en Release
dotnet publish -c Release

# Nettoyer les artifacts de build
dotnet clean
```

### Structure des services

Tous les services suivent le même pattern :

```csharp
public interface IMonService
{
    Task<List<MonModele>> GetDataAsync();
}

public class MonService : IMonService
{
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string CacheKey = "MonService";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    public MonService(ICacheService cacheService, IHttpClientFactory httpClientFactory)
    {
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<MonModele>> GetDataAsync()
    {
        // 1. Check cache
        var cached = _cacheService.Get<List<MonModele>>(CacheKey);
        if (cached != null) return cached;

        var data = new List<MonModele>();
        
        try
        {
            // 2. Fetch data from API/RSS
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ContainSharp");
            
            var response = await httpClient.GetStringAsync("https://api.example.com/data");
            // Parse response...
            
            // 3. Store in cache
            _cacheService.Set(CacheKey, data, CacheExpiration);
        }
        catch
        {
            // Silent fail - return empty list
        }
        
        return data;
    }
}
```

### Conventions de code

- **Null safety** : Nullable reference types activés
- **Async/await** : Toutes les I/O sont asynchrones
- **Naming** : PascalCase pour tout sauf les paramètres (camelCase)
- **Immutability** : Préférer `readonly` quand possible
- **DRY** : Factoriser le code dupliqué
- **KISS** : Garder les solutions simples

### Ajouter une nouvelle source de données

1. **Créer le modèle** dans `Models/`
```csharp
public class MaNouvelleDonnee
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
}
```

2. **Créer le service** dans `Services/`
```csharp
public interface IMonNouveauService
{
    Task<List<MaNouvelleDonnee>> GetDataAsync();
}

public class MonNouveauService : IMonNouveauService
{
    // Suivre le pattern décrit ci-dessus
}
```

3. **Enregistrer le service** dans `Program.cs`
```csharp
builder.Services.AddSingleton<IMonNouveauService, MonNouveauService>();
```

4. **Utiliser dans une page**
```csharp
public class MaPageModel : PageModel
{
    private readonly IMonNouveauService _service;
    
    public MaPageModel(IMonNouveauService service)
    {
        _service = service;
    }
    
    public List<MaNouvelleDonnee> Data { get; set; } = new();
    
    public async Task OnGetAsync()
    {
        Data = await _service.GetDataAsync();
    }
}
```

### Debugging

- **Mode développement** : Exception pages détaillées activées
- **Logs** : Console logging pour toutes les requêtes
- **Browser DevTools** : Inspecter requêtes réseau, cache, console
- **Visual Studio** : Breakpoints, watch, immediate window

---

## 📊 Sources de données

### Blogs officiels Microsoft
- [.NET Blog](https://devblogs.microsoft.com/dotnet/feed/)
- [ASP.NET Blog](https://devblogs.microsoft.com/aspnet/feed/)
- [Visual Studio Blog](https://devblogs.microsoft.com/visualstudio/feed/)
- [C# Blog](https://devblogs.microsoft.com/dotnet/category/csharp/feed/)

### GitHub
- [Trending C# Repositories](https://api.github.com/search/repositories?q=language:csharp)
- Filtrage : Créés dans les 30 derniers jours, triés par étoiles

### Actualités IA
- Agrégation de sources diverses : OpenAI, Anthropic, Google, blogs IA
- Filtrage par mots-clés : copilot, GPT, Claude, Mistral, LLM, etc.

### YouTube
- [C# Videos RSS](https://www.youtube.com/feeds/videos.xml?channel_id=...)
- Filtrage par mots-clés C# et .NET

### Données statiques
- **LLM** : Données codées en dur dans `LLMService.cs`
- **MCP Tools** : Données codées en dur dans `MCPService.cs`
- **Useful Links** : Données codées en dur dans `Liens.cshtml.cs`
- **Quiz Questions** : Données codées en dur dans `QuestionService.cs`

---

## 🔒 Sécurité

### Mesures de sécurité implémentées

1. **XSS Protection**
   - HTML sanitization via `HtmlSanitizer`
   - Razor automatic encoding
   - CSP headers (à configurer)

2. **External Links**
   - `rel="noopener noreferrer"` sur tous les liens externes
   - Prévention de window.opener hijacking

3. **HTTPS**
   - Redirection HTTPS forcée en production
   - HSTS headers

4. **No Database**
   - Pas de risque d'injection SQL
   - Pas de stockage de données utilisateur

5. **Dependencies**
   - Packages NuGet à jour
   - Scan de vulnérabilités régulier

6. **Error Handling**
   - Pas de stack traces en production
   - Messages d'erreur génériques pour l'utilisateur

### Bonnes pratiques

- Ne jamais commiter de secrets (tokens, keys)
- Utiliser User Secrets en développement
- Utiliser des variables d'environnement en production
- Scanner le code avec des outils de sécurité (SonarQube, Snyk)

---

## 📝 License

MIT License - Voir le fichier LICENSE pour plus de détails.

---

## 👤 Auteur

**Matthieu TRACHSEL**
- Site web : [containsharp.com](https://containsharp.com)
- GitHub : [@matmout](https://github.com/matmout)
- LinkedIn : [matthieutrachsel](https://www.linkedin.com/in/matthieutrachsel)

FullStack Developer @ BRED, basé à Paris

---

## 🙏 Remerciements

- Microsoft pour l'écosystème .NET fantastique
- La communauté open-source .NET
- Bootstrap pour le framework UI
- Tous les créateurs de contenu C# et .NET

---

## 📸 Captures d'écran

> 🌐 **Le site est accessible en production sur [containsharp.com](https://containsharp.com)**

> 📖 **Documentation des pages** : Consultez [Docs/screenshots/README.md](Docs/screenshots/README.md) pour des descriptions détaillées de chaque page.

### Dashboard principal
Le dashboard affiche une simulation de console Visual Studio avec les dernières actualités, suivie de sections pour :
- **Release News** : Versions .NET, C#, ASP.NET Core
- **AI Coding Tools** : GitHub Copilot, OpenAI, Anthropic, Mistral
- **GitHub Trending** : Projets C# populaires récents
- **Official Blogs** : Articles Microsoft (.NET, ASP.NET, Visual Studio, C#)
- **WinForms News** : Actualités Windows Forms
- **C# Videos** : Dernières vidéos YouTube

**Design** : Interface sombre style Visual Studio avec console animée, cards Bootstrap, et effets hover.

---

### Liens utiles
Collection organisée de ressources essentielles avec catégories :
- Documentation officielle
- Outils de développement  
- Apprentissage et certifications
- Communauté et forums
- Packages NuGet

**Design** : Grille responsive de cards avec icônes Bootstrap.

---

### MCP Tools
Liste interactive des serveurs Model Context Protocol pour agents IA :
- Development (GitHub, GitLab, npm)
- Databases (PostgreSQL, SQLite, MySQL)
- Cloud Services (AWS, Azure, Google Cloud)
- Utilities (Filesystem, Fetch, Memory)

**Design** : Cards par catégorie avec liens GitHub.

---

### Latest LLM
Comparateur de modèles de langage avec informations sur :
- **Providers** : OpenAI, Anthropic, Google, Meta, Mistral, xAI
- **Specs** : Contexte, prix, date de sortie
- **Capacités** : Vision, function calling, streaming

**Design** : Tableau comparatif responsive avec badges.

---

### Training
Quiz interactif C# avec :
- Questions sur syntaxe, LINQ, async/await, patterns, performance
- Niveaux de difficulté progressifs
- Feedback immédiat et explications détaillées
- Suivi de score

**Design** : Interface quiz avec boutons radio et feedback coloré.

---

> 💡 **Note** : Des captures d'écran haute résolution seront ajoutées prochainement dans `Docs/screenshots/`

---

<div align="center">
  
  **Développé avec ❤️ en C# et ASP.NET Core**
  
  [⬆ Retour en haut](#containsharp---votre-dose-quotidienne-de-c)
  
</div>
