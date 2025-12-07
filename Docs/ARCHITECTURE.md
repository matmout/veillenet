# Architecture de ContainSharp

## Vue d'ensemble

ContainSharp est une application ASP.NET Core basée sur le pattern **Razor Pages** avec une architecture en couches propre (Clean Architecture simplifiée).

## Diagramme d'architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         NAVIGATEUR                               │
│  (Chrome, Firefox, Safari, Edge)                                │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP/HTTPS
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                          │
│                      (Razor Pages)                               │
│                                                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │ Index.cshtml│  │ Liens.cshtml│  │  MCP.cshtml │            │
│  │     +       │  │     +       │  │     +       │  ...        │
│  │ .cshtml.cs  │  │ .cshtml.cs  │  │ .cshtml.cs  │            │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘            │
│         │                │                 │                    │
└─────────┼────────────────┼─────────────────┼────────────────────┘
          │                │                 │
          └────────────────┴─────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DEPENDENCY INJECTION                          │
│                      (Built-in DI)                               │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      BUSINESS LAYER                              │
│                        (Services)                                │
│                                                                  │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │ BlogAggregation  │  │   GitHubService  │                    │
│  │    Service       │  │                  │                    │
│  └────────┬─────────┘  └────────┬─────────┘                    │
│           │                     │                               │
│  ┌────────┴─────────┐  ┌────────┴─────────┐                    │
│  │   AINewsService  │  │  ReleaseNews     │                    │
│  │                  │  │    Service       │  ...                │
│  └────────┬─────────┘  └────────┬─────────┘                    │
│           │                     │                               │
│           └─────────┬───────────┘                               │
│                     │                                           │
│            ┌────────▼─────────┐                                │
│            │   CacheService   │                                │
│            │  (IMemoryCache)  │                                │
│            └──────────────────┘                                │
│                                                                  │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                   HTTP CLIENT FACTORY                            │
│                  (Connection Pooling)                            │
└────────────────────────────┬────────────────────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   RSS Feeds  │    │  GitHub API  │    │  YouTube RSS │
│              │    │              │    │              │
│ • .NET Blog  │    │ • Trending   │    │ • C# Videos  │
│ • ASP.NET    │    │ • Releases   │    │              │
│ • C# Blog    │    │              │    │              │
│ • VS Blog    │    │              │    │              │
└──────────────┘    └──────────────┘    └──────────────┘
```

## Flux de données

### 1. Requête utilisateur
```
User → Browser → ASP.NET Core → Razor Page (.cshtml.cs)
```

### 2. Récupération des données
```
Page → Service (DI) → CacheService.Get(key)
                            │
                            ├─ Cache Hit → Return cached data
                            │
                            └─ Cache Miss → HttpClient.GetAsync()
                                               │
                                               ├─ API Response → Parse → Cache.Set()
                                               │
                                               └─ Return data
```

### 3. Rendu de la page
```
Service → Page Model → Razor View (.cshtml) → HTML → Browser
```

## Pattern de cache

Tous les services suivent le même pattern :

```
┌─────────────────────────────────────────────────────────┐
│ Service.GetDataAsync()                                  │
│                                                         │
│  1. Check Cache                                         │
│     └─ if exists → return                              │
│                                                         │
│  2. Fetch from External API                            │
│     └─ HTTP Request → Parse Response                   │
│                                                         │
│  3. Store in Cache (with expiration)                   │
│                                                         │
│  4. Return Data                                        │
│                                                         │
│  Error Handling: try-catch → return empty list         │
└─────────────────────────────────────────────────────────┘
```

## Couches de l'application

### 1. Présentation (Pages/)
- **Responsabilité** : Affichage et interaction utilisateur
- **Technologies** : Razor Pages, HTML, CSS, JavaScript
- **Pattern** : MVVM (Model-View-ViewModel)

### 2. Logique métier (Services/)
- **Responsabilité** : Récupération et traitement des données
- **Pattern** : Repository pattern (sans DB)
- **Caching** : In-memory pour toutes les données

### 3. Modèles (Models/)
- **Responsabilité** : Structures de données
- **Type** : POCOs (Plain Old CLR Objects)
- **Validation** : Data annotations

### 4. Infrastructure (Program.cs)
- **Responsabilité** : Configuration et DI
- **Services** : Enregistrement des dépendances
- **Middleware** : Pipeline de requêtes

## Stratégie de cache

| Service | Clé cache | Durée | Raison |
|---------|-----------|-------|--------|
| BlogAggregation | "BlogPosts" | 1h | Articles mis à jour plusieurs fois/jour |
| GitHubService | "GitHubTrending" | 6h | Trending change lentement |
| ReleaseNews | "ReleaseNews" | 24h | Releases peu fréquentes |
| AINews | "AINews" | 1h | Actualités IA dynamiques |
| WinFormNews | "WinFormNews" | 1h | Blog actif |
| VideoService | "CSharpVideos" | 1h | Nouvelles vidéos régulières |
| LLMService | "LLMData" | 24h | Données quasi-statiques |
| MCPService | "MCPTools" | 24h | Liste stable |

## Sécurité

### Couches de sécurité

```
┌─────────────────────────────────────────────────────────┐
│ 1. HTTPS Redirection                                    │
│    └─ Force HTTPS en production                        │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│ 2. Response Compression                                 │
│    └─ Gzip/Brotli pour réduire la taille              │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│ 3. Session Middleware                                   │
│    └─ Gestion des sessions utilisateur                │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│ 4. Authorization Middleware                             │
│    └─ Vérification des autorisations                  │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│ 5. HTML Sanitization                                    │
│    └─ HtmlSanitizer pour contenu externe              │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│ 6. Razor Auto-Encoding                                  │
│    └─ Protection XSS automatique                      │
└─────────────────────────────────────────────────────────┘
```

## Performance

### Optimisations appliquées

1. **Caching agressif**
   - In-memory cache pour toutes les données API
   - Pas de requêtes DB (pas de DB)
   - Response caching pour assets statiques

2. **Async/Await partout**
   - Toutes les I/O sont non-bloquantes
   - `Task.WhenAll()` pour parallélisation

3. **HTTP Connection Pooling**
   - `IHttpClientFactory` réutilise les connexions
   - Évite la création/destruction de sockets

4. **Response Compression**
   - Gzip/Brotli activés
   - Réduction de 60-80% de la taille

5. **Static Files Optimization**
   - Cache-Control headers
   - ETag pour validation
   - Lazy loading des images

## Scalabilité

### Horizontal Scaling

L'application est **stateless** (pas de session serveur stockée) :
- ✅ Peut être déployée sur plusieurs instances
- ✅ Load balancer devant les instances
- ✅ Chaque instance a son propre cache mémoire
- ⚠️ Cache non partagé entre instances (acceptable pour ce cas d'usage)

### Vertical Scaling

- Cache en mémoire augmente avec la RAM
- Plus de CPU = plus de threads pour async/await
- Pas de limite inhérente à l'application

## Monitoring et observabilité

### Logs
```csharp
// Built-in logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});
```

### Métriques (à ajouter)
- Request count
- Response times
- Cache hit ratio
- Error rates

### Tracing (à ajouter)
- OpenTelemetry
- Application Insights (Azure)
- Jaeger

## Évolutions futures

### Court terme
- [ ] Ajouter des screenshots au README
- [ ] Implémenter la newsletter fonctionnelle
- [ ] Ajouter des tests unitaires (xUnit)
- [ ] CI/CD automatisé (GitHub Actions)

### Moyen terme
- [ ] API REST pour les données
- [ ] WebSockets pour updates en temps réel
- [ ] Progressive Web App (PWA)
- [ ] Dark/Light theme toggle

### Long terme
- [ ] Cache distribué (Redis)
- [ ] Authentification OAuth
- [ ] Personnalisation par utilisateur
- [ ] Analytics et tracking
