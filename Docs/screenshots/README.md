# Captures d'écran - ContainSharp

Ce dossier contient les captures d'écran de l'application ContainSharp.

## Pages principales

### Dashboard (/)
**Description** : Page d'accueil principale avec simulation de console Visual Studio et agrégation de toutes les sources.

**Fonctionnalités visibles** :
- Header avec logo ContainSharp et navigation
- Console de sortie animée (Visual Studio style) affichant les dernières actualités
- Section "Release News" avec les dernières versions .NET/C#/ASP.NET
- Section "AI Coding Tools" avec actualités GitHub Copilot, OpenAI, Anthropic
- Section "GitHub Trending" avec projets C# récents populaires
- Section "Official .NET Blogs" avec articles des blogs Microsoft
- Section "WinForms News" avec actualités Windows Forms
- Section "Latest C# Videos" avec vidéos YouTube

**Design** :
- Thème sombre inspiré de Visual Studio
- Cards avec effets hover
- Layout responsive Bootstrap 5
- Animations CSS pour la console

---

### Liens utiles (/Liens)
**Description** : Collection organisée de ressources essentielles pour développeurs C#.

**Catégories** :
- Documentation officielle (MSDN, docs.microsoft.com)
- Outils de développement (Visual Studio, VS Code, Rider)
- Apprentissage (Microsoft Learn, Pluralsight)
- Communauté (.NET Foundation, Stack Overflow)
- Packages et libraries (NuGet)

**Design** :
- Layout en grille responsive
- Cards avec icônes et descriptions
- Liens externes avec `target="_blank"`
- Hover effects sur les cards

---

### MCP Tools (/MCP)
**Description** : Liste des serveurs Model Context Protocol pour agents IA.

**Catégories** :
- Development (GitHub, GitLab, npm, Docker)
- Databases (PostgreSQL, SQLite, MySQL, MongoDB)
- Cloud Services (AWS, Azure, Google Cloud)
- Utilities (Filesystem, Fetch, Memory, Time)
- AI Tools (Brave Search, Puppeteer)

**Fonctionnalités** :
- Cards avec nom, description, lien GitHub
- Icônes pour chaque catégorie
- Layout en grille responsive
- Filtres par catégorie

---

### Latest LLM (/LatestLLM)
**Description** : Comparateur et suivi des derniers modèles de langage.

**Providers** :
- OpenAI (GPT-4, GPT-4 Turbo, o1)
- Anthropic (Claude 3.5 Sonnet, Claude 3 Opus)
- Google (Gemini 2.0 Flash, Gemini 1.5 Pro)
- Meta (Llama 3.3 70B)
- Mistral (Mistral Large 2)
- xAI (Grok 2)

**Informations affichées** :
- Nom du modèle
- Provider
- Date de sortie
- Taille du contexte
- Prix (input/output)
- Capacités (vision, function calling)

**Design** :
- Tableau comparatif responsive
- Cards pour chaque provider
- Badges pour les capacités
- Tri et filtrage

---

### Training (/Training)
**Description** : Quiz interactif pour tester ses connaissances en C#.

**Fonctionnalités** :
- Questions à choix multiples
- Sujets variés : syntaxe, LINQ, async/await, patterns, performance
- Niveaux de difficulté : débutant, intermédiaire, expert
- Feedback immédiat avec explications
- Score et progression

**Design** :
- Interface style quiz avec boutons radio
- Feedback coloré (vert = correct, rouge = incorrect)
- Progression visuelle
- Bouton "Next Question"

---

### About (/About)
**Description** : Page à propos avec informations sur l'auteur et le projet.

**Contenu** :
- Photo de profil
- Biographie
- Technologies utilisées
- Liens LinkedIn et GitHub
- Motivation du projet

**Design** :
- Layout 2 colonnes (photo + texte)
- Cards pour les profils sociaux
- Responsive mobile

---

## Comment prendre des captures d'écran

### Méthode 1 : Site en production
1. Visiter https://containsharp.com
2. Naviguer vers la page souhaitée
3. Utiliser l'outil de capture d'écran (F12 → Device toolbar pour mobile)
4. Sauvegarder dans `Docs/screenshots/`

### Méthode 2 : Local
1. Lancer l'application : `dotnet run`
2. Ouvrir http://localhost:5000
3. Capturer les screenshots
4. Sauvegarder avec noms descriptifs

### Méthode 3 : Navigateur headless
```bash
# Avec Playwright
npx playwright screenshot https://containsharp.com dashboard.png

# Avec Puppeteer
node screenshot.js
```

## Nomenclature des fichiers

Format : `page-description-size.png`

Exemples :
- `dashboard-desktop-1920x1080.png`
- `dashboard-mobile-375x667.png`
- `liens-desktop-1920x1080.png`
- `mcp-desktop-1920x1080.png`
- `llm-desktop-1920x1080.png`
- `training-desktop-1920x1080.png`
- `about-desktop-1920x1080.png`

## Tailles recommandées

- **Desktop** : 1920x1080 (Full HD)
- **Tablet** : 768x1024 (iPad)
- **Mobile** : 375x667 (iPhone SE)

## Optimisation

Après avoir pris les screenshots :
1. Convertir en WebP pour réduire la taille
2. Garder une version PNG pour compatibilité
3. Optimiser avec des outils comme squoosh.app

```bash
# Convertir PNG en WebP
cwebp -q 85 dashboard-desktop-1920x1080.png -o dashboard-desktop-1920x1080.webp
```

## Note

Les captures d'écran doivent être ajoutées au repository pour enrichir le README.
En attendant, le README contient des descriptions textuelles détaillées de chaque page.

Le site en production est accessible sur **https://containsharp.com** pour visualiser l'application complète.
