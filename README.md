# VeilleNet - Dashboard de Veille .NET

Dashboard de veille technologique pour l'écosystème .NET développé en ASP.NET Core avec C#.

## Problème

Rester à jour sur l'écosystème .NET peut être difficile avec la multitude de sources d'information disponibles.

## Solution

VeilleNet centralise toutes les informations importantes de l'écosystème .NET en un seul endroit :

### Fonctionnalités

- ✅ **Agrégation des blogs .NET officiels** : Derniers articles des blogs Microsoft (.NET, ASP.NET, Visual Studio, C#)
- ✅ **Nouveautés des releases** : Informations sur les dernières versions de .NET, C#, et ASP.NET Core
- ✅ **Tendances GitHub** : Projets C# les plus populaires et récents sur GitHub
- ✅ **Newsletter personnalisée** : Inscription avec sélection de sujets d'intérêt

### Technologies

- **Framework** : ASP.NET Core (Razor Pages) avec .NET 10
- **Langage** : C# avec nullable reference types activés
- **Architecture** : Clean architecture avec services et dependency injection
- **Cache** : Cache serveur en mémoire (MemoryCache) - pas de base de données
- **UI** : Bootstrap 5 avec Bootstrap Icons
- **APIs** : 
  - RSS/Atom feeds pour les blogs
  - GitHub API pour les projets tendance

## Structure du Projet

```
VeilleNet/
├── Models/                    # Modèles de données
│   ├── BlogPost.cs           # Articles de blog
│   ├── GitHubProject.cs      # Projets GitHub
│   ├── ReleaseNews.cs        # Nouvelles des releases
│   └── NewsletterSubscription.cs
├── Services/                  # Couche service
│   ├── CacheService.cs       # Service de cache
│   ├── BlogAggregationService.cs
│   ├── GitHubService.cs
│   ├── ReleaseNewsService.cs
│   └── NewsletterService.cs
├── Pages/                     # Pages Razor
│   ├── Index.cshtml          # Dashboard principal
│   ├── Newsletter.cshtml     # Page d'inscription newsletter
│   └── Shared/               # Layout et composants partagés
└── wwwroot/                   # Ressources statiques

```

## Installation et Lancement

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Instructions

1. Cloner le repository :
```bash
git clone https://github.com/matmout/veillenet.git
cd veillenet
```

2. Restaurer les dépendances :
```bash
dotnet restore
```

3. Lancer l'application :
```bash
dotnet run
```

4. Ouvrir un navigateur à l'adresse : `https://localhost:5001` ou `http://localhost:5000`

## Configuration

L'application utilise le cache en mémoire avec les durées suivantes :
- Articles de blog : 1 heure
- Projets GitHub : 6 heures
- Nouvelles des releases : 24 heures

Ces durées peuvent être modifiées dans les services correspondants.

## Fonctionnement du Cache

Le cache serveur en mémoire permet de :
- Réduire les appels aux APIs externes
- Améliorer les performances de l'application
- Éviter les limitations de taux des APIs

Les données sont mises en cache automatiquement lors de la première requête et rafraîchies après l'expiration.

## Développement

### Build
```bash
dotnet build
```

### Exécution en mode développement
```bash
dotnet watch run
```

## License

MIT
