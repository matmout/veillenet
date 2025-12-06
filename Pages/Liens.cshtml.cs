using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;

namespace VeilleNet.Pages;

public class LiensModel : PageModel
{
    public List<LinkCategory> Categories { get; set; } = new();

    public void OnGet()
    {
        Categories = new List<LinkCategory>
        {
            new LinkCategory
            {
                Name = "C# Officiel",
                Description = "Ressources officielles Microsoft pour C# et .NET",
                Icon = "bi-microsoft",
                ColorClass = "primary",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "Documentation C#",
                        Description = "Documentation officielle de C# avec guides, tutoriels et références API",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/csharp/",
                        Icon = "bi-book"
                    },
                    new UsefulLink
                    {
                        Name = ".NET Blog",
                        Description = "Blog officiel de l'équipe .NET avec les dernières annonces et nouveautés",
                        Url = "https://devblogs.microsoft.com/dotnet/",
                        Icon = "bi-newspaper"
                    },
                    new UsefulLink
                    {
                        Name = "C# Language Specification",
                        Description = "Spécification complète du langage C#",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/csharp/language-reference/language-specification/",
                        Icon = "bi-file-text"
                    },
                    new UsefulLink
                    {
                        Name = ".NET Releases",
                        Description = "Calendrier et notes de version de .NET",
                        Url = "https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core",
                        Icon = "bi-calendar-event"
                    },
                    new UsefulLink
                    {
                        Name = "GitHub .NET",
                        Description = "Repository officiel .NET sur GitHub",
                        Url = "https://github.com/dotnet",
                        Icon = "bi-github"
                    }
                }
            },
            new LinkCategory
            {
                Name = "Intelligence Artificielle",
                Description = "Outils et ressources IA pour développeurs",
                Icon = "bi-robot",
                ColorClass = "info",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "GitHub Copilot",
                        Description = "Assistant de code alimenté par l'IA pour Visual Studio et VS Code",
                        Url = "https://github.com/features/copilot",
                        Icon = "bi-code-square"
                    },
                    new UsefulLink
                    {
                        Name = "ChatGPT",
                        Description = "Assistant conversationnel d'OpenAI pour le développement et plus",
                        Url = "https://chat.openai.com/",
                        Icon = "bi-chat-dots"
                    },
                    new UsefulLink
                    {
                        Name = "Claude AI",
                        Description = "Assistant IA d'Anthropic avec capacités de raisonnement avancées",
                        Url = "https://claude.ai/",
                        Icon = "bi-lightning"
                    },
                    new UsefulLink
                    {
                        Name = "Microsoft Copilot",
                        Description = "Suite d'outils IA de Microsoft intégrée à l'écosystème Microsoft 365",
                        Url = "https://copilot.microsoft.com/",
                        Icon = "bi-stars"
                    },
                    new UsefulLink
                    {
                        Name = "Mistral AI",
                        Description = "Modèles IA open-source et performants développés en France",
                        Url = "https://mistral.ai/",
                        Icon = "bi-cpu"
                    }
                }
            },
            new LinkCategory
            {
                Name = "WinForms",
                Description = "Ressources pour le développement d'applications Windows Forms",
                Icon = "bi-window-desktop",
                ColorClass = "warning",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "Documentation WinForms",
                        Description = "Guide officiel Microsoft pour Windows Forms .NET",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/desktop/winforms/",
                        Icon = "bi-book"
                    },
                    new UsefulLink
                    {
                        Name = "WinForms Designer",
                        Description = "Documentation du concepteur visuel WinForms",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/desktop/winforms/controls/",
                        Icon = "bi-palette"
                    },
                    new UsefulLink
                    {
                        Name = "GitHub WinForms",
                        Description = "Repository officiel Windows Forms sur GitHub",
                        Url = "https://github.com/dotnet/winforms",
                        Icon = "bi-github"
                    },
                    new UsefulLink
                    {
                        Name = "WinForms Samples",
                        Description = "Exemples de code et applications WinForms",
                        Url = "https://github.com/dotnet/samples/tree/main/windowsforms",
                        Icon = "bi-code-slash"
                    }
                }
            },
            new LinkCategory
            {
                Name = "Jeux et Code",
                Description = "Plateformes pour apprendre en s'amusant",
                Icon = "bi-joystick",
                ColorClass = "success",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "CodinGame",
                        Description = "Apprenez à coder en jouant avec des défis programmation multijoueurs",
                        Url = "https://www.codingame.com/",
                        Icon = "bi-controller"
                    },
                    new UsefulLink
                    {
                        Name = "Exercism",
                        Description = "Exercices de programmation avec mentorat gratuit en C#",
                        Url = "https://exercism.org/tracks/csharp",
                        Icon = "bi-trophy"
                    },
                    new UsefulLink
                    {
                        Name = "HackerRank",
                        Description = "Défis de programmation et préparation aux entretiens techniques",
                        Url = "https://www.hackerrank.com/domains/tutorials/10-days-of-csharp",
                        Icon = "bi-award"
                    },
                    new UsefulLink
                    {
                        Name = "Advent of Code",
                        Description = "Calendrier de l'avent avec des défis de programmation quotidiens",
                        Url = "https://adventofcode.com/",
                        Icon = "bi-calendar-star"
                    }
                }
            },
            new LinkCategory
            {
                Name = "Outils C#",
                Description = "Utilitaires et générateurs de code",
                Icon = "bi-tools",
                ColorClass = "danger",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "Json2CSharp",
                        Description = "Générez des classes C# à partir de JSON",
                        Url = "https://json2csharp.com/",
                        Icon = "bi-braces"
                    },
                    new UsefulLink
                    {
                        Name = "SharpLab",
                        Description = "Compilateur C# en ligne avec visualisation IL et AST",
                        Url = "https://sharplab.io/",
                        Icon = "bi-eye"
                    },
                    new UsefulLink
                    {
                        Name = ".NET Fiddle",
                        Description = "IDE en ligne pour tester du code C# et .NET",
                        Url = "https://dotnetfiddle.net/",
                        Icon = "bi-play-circle"
                    },
                    new UsefulLink
                    {
                        Name = "NuGet",
                        Description = "Gestionnaire de packages officiel pour .NET",
                        Url = "https://www.nuget.org/",
                        Icon = "bi-box-seam"
                    },
                    new UsefulLink
                    {
                        Name = "Regex101",
                        Description = "Testeur et débogueur d'expressions régulières",
                        Url = "https://regex101.com/",
                        Icon = "bi-regex"
                    }
                }
            },
            new LinkCategory
            {
                Name = "Algorithmie",
                Description = "Entraînez-vous aux algorithmes et structures de données",
                Icon = "bi-diagram-3",
                ColorClass = "secondary",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "LeetCode",
                        Description = "Plateforme d'entraînement avec des milliers de problèmes algorithmiques",
                        Url = "https://leetcode.com/",
                        Icon = "bi-code"
                    },
                    new UsefulLink
                    {
                        Name = "AlgoExpert",
                        Description = "Formation approfondie aux algorithmes avec vidéos explicatives",
                        Url = "https://www.algoexpert.io/",
                        Icon = "bi-mortarboard"
                    },
                    new UsefulLink
                    {
                        Name = "Visualgo",
                        Description = "Visualisez les algorithmes et structures de données",
                        Url = "https://visualgo.net/",
                        Icon = "bi-graph-up"
                    },
                    new UsefulLink
                    {
                        Name = "Big-O Cheat Sheet",
                        Description = "Référence rapide des complexités algorithmiques",
                        Url = "https://www.bigocheatsheet.com/",
                        Icon = "bi-table"
                    },
                    new UsefulLink
                    {
                        Name = "Project Euler",
                        Description = "Problèmes mathématiques et algorithmiques complexes",
                        Url = "https://projecteuler.net/",
                        Icon = "bi-calculator"
                    }
                }
            }
        };
    }
}
