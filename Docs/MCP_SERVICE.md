# MCP Service

Service centralisé pour gérer la liste des outils MCP (Model Context Protocol) disponibles.

## Structure

Le service `MCPService` gère trois catégories principales d'outils MCP :

### 1. AI & Code Agents
Serveurs MCP pour l'assistance au développement assistée par IA :
- GitHub MCP Server (officiel)
- Playwright MCP Server (officiel)
- Memory MCP Server (officiel)
- Fetch MCP Server (officiel)
- Filesystem MCP Server (officiel)

### 2. Database Connections
Serveurs MCP pour les connexions et la gestion de bases de données :
- SQLite MCP Server (officiel)
- PostgreSQL MCP Server (officiel)
- MySQL MCP Server
- MongoDB MCP Server
- **Elasticsearch MCP Server (officiel Elastic)**
- **Oracle Database MCP Server (officiel Oracle)**

### 3. Deployment & DevOps
Serveurs MCP pour le déploiement et la gestion d'infrastructure :
- Docker MCP Server
- Kubernetes MCP Server
- AWS MCP Server
- Git MCP Server (officiel)
- Sentry MCP Server (officiel)

## Utilisation

### Dans une Razor Page

```csharp
public class MCPModel : PageModel
{
    private readonly IMCPService _mcpService;

    public List<MCPCategory> Categories { get; set; } = new();

    public MCPModel(IMCPService mcpService)
    {
        _mcpService = mcpService;
    }

    public void OnGet()
    {
        // Récupérer toutes les catégories
        Categories = _mcpService.GetMCPCategories();
        
        // Ou récupérer tous les outils
        var allTools = _mcpService.GetAllTools();
        
        // Ou filtrer par catégorie
        var dbTools = _mcpService.GetToolsByCategory("Database Connections");
    }
}
```

### Enregistrement du service

Le service est enregistré comme Singleton dans `Program.cs` :

```csharp
builder.Services.AddSingleton<IMCPService, MCPService>();
```

## Méthodes disponibles

### `GetMCPCategories()`
Retourne la liste complète des catégories avec leurs outils.

**Retour** : `List<MCPCategory>`

### `GetAllTools()`
Retourne une liste plate de tous les outils MCP disponibles.

**Retour** : `List<MCPTool>`

### `GetToolsByCategory(string category)`
Filtre les outils par catégorie.

**Paramètres** :
- `category` : Nom de la catégorie (insensible à la casse)

**Retour** : `List<MCPTool>`

## Modèles

### MCPTool
```csharp
public class MCPTool
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string GuideUrl { get; set; }
    public string Icon { get; set; }
}
```

### MCPCategory
```csharp
public class MCPCategory
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string ColorClass { get; set; }
    public List<MCPTool> Tools { get; set; }
}
```

## Ajout de nouveaux outils

Pour ajouter de nouveaux outils MCP, modifiez la méthode `InitializeCategories()` dans `Services/MCPService.cs` :

```csharp
new MCPTool
{
    Name = "Nom du serveur MCP",
    Description = "Description détaillée",
    Category = "Catégorie appropriée",
    Icon = "bi-icon-name",
    GuideUrl = "https://github.com/..."
}
```

## Sources officielles privilégiées

Le service privilégie les implémentations officielles :
- ? Serveurs du repo officiel `modelcontextprotocol/servers`
- ? Serveurs maintenus par les éditeurs (Elastic, Oracle, etc.)
- ?? Serveurs communautaires de confiance

## Notes

- Le service est thread-safe (Singleton)
- Les données sont chargées au démarrage de l'application
- Aucune dépendance externe (pas de cache, pas d'API)
- Parfait pour les petites listes statiques

## Évolutions futures possibles

- [ ] Charger les outils depuis un fichier JSON
- [ ] Ajouter un système de tags/filtres
- [ ] Intégrer des statistiques (popularité, dernière mise à jour)
- [ ] Ajouter un système de votes/reviews
