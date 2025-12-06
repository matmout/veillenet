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
                Name = "Official C#",
                Description = "Microsoft official resources for C# and .NET",
                Icon = "bi-microsoft",
                ColorClass = "primary",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "C# Documentation",
                        Description = "Official C# docs with guides, tutorials, and API references",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/csharp/",
                        Icon = "bi-book"
                    },
                    new UsefulLink
                    {
                        Name = ".NET Blog",
                        Description = "Official .NET team blog with the latest announcements and news",
                        Url = "https://devblogs.microsoft.com/dotnet/",
                        Icon = "bi-newspaper"
                    },
                    new UsefulLink
                    {
                        Name = "C# Language Specification",
                        Description = "Full C# language specification",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/csharp/language-reference/language-specification/",
                        Icon = "bi-file-text"
                    },
                    new UsefulLink
                    {
                        Name = ".NET Releases",
                        Description = "Release schedule and notes for .NET",
                        Url = "https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core",
                        Icon = "bi-calendar-event"
                    },
                    new UsefulLink
                    {
                        Name = "GitHub .NET",
                        Description = "Official .NET GitHub organization",
                        Url = "https://github.com/dotnet",
                        Icon = "bi-github"
                    }
                }
            },
            new LinkCategory
            {
                Name = "Artificial Intelligence",
                Description = "AI tools and resources for developers",
                Icon = "bi-robot",
                ColorClass = "info",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "GitHub Copilot",
                        Description = "AI-powered coding assistant for Visual Studio and VS Code",
                        Url = "https://github.com/features/copilot",
                        Icon = "bi-code-square"
                    },
                    new UsefulLink
                    {
                        Name = "ChatGPT",
                        Description = "OpenAI conversational assistant for development and more",
                        Url = "https://chat.openai.com/",
                        Icon = "bi-chat-dots"
                    },
                    new UsefulLink
                    {
                        Name = "Claude AI",
                        Description = "Anthropic AI assistant with advanced reasoning capabilities",
                        Url = "https://claude.ai/",
                        Icon = "bi-lightning"
                    },
                    new UsefulLink
                    {
                        Name = "Microsoft Copilot",
                        Description = "Microsoft's AI tools suite integrated with Microsoft 365",
                        Url = "https://copilot.microsoft.com/",
                        Icon = "bi-stars"
                    },
                    new UsefulLink
                    {
                        Name = "Mistral AI",
                        Description = "Open-source, high-performance AI models developed in France",
                        Url = "https://mistral.ai/",
                        Icon = "bi-cpu"
                    }
                }
            },
            new LinkCategory
            {
                Name = "WinForms",
                Description = "Resources for building Windows Forms applications",
                Icon = "bi-window-desktop",
                ColorClass = "warning",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "WinForms Documentation",
                        Description = "Microsoft official guide for Windows Forms .NET",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/desktop/winforms/",
                        Icon = "bi-book"
                    },
                    new UsefulLink
                    {
                        Name = "WinForms Designer",
                        Description = "Documentation for the WinForms visual designer",
                        Url = "https://learn.microsoft.com/fr-fr/dotnet/desktop/winforms/controls/",
                        Icon = "bi-palette"
                    },
                    new UsefulLink
                    {
                        Name = "GitHub WinForms",
                        Description = "Official Windows Forms GitHub repository",
                        Url = "https://github.com/dotnet/winforms",
                        Icon = "bi-github"
                    },
                    new UsefulLink
                    {
                        Name = "WinForms Samples",
                        Description = "WinForms code samples and applications",
                        Url = "https://github.com/dotnet/samples/tree/main/windowsforms",
                        Icon = "bi-code-slash"
                    }
                }
            },
            new LinkCategory
            {
                Name = "Coding & Games",
                Description = "Platforms to learn while having fun",
                Icon = "bi-joystick",
                ColorClass = "success",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "CodinGame",
                        Description = "Learn to code by playing with multiplayer programming challenges",
                        Url = "https://www.codingame.com/",
                        Icon = "bi-controller"
                    },
                    new UsefulLink
                    {
                        Name = "Exercism",
                        Description = "Programming exercises with free mentoring in C#",
                        Url = "https://exercism.org/tracks/csharp",
                        Icon = "bi-trophy"
                    },
                    new UsefulLink
                    {
                        Name = "HackerRank",
                        Description = "Programming challenges and technical interview prep",
                        Url = "https://www.hackerrank.com/domains/tutorials/10-days-of-csharp",
                        Icon = "bi-award"
                    },
                    new UsefulLink
                    {
                        Name = "Advent of Code",
                        Description = "Advent calendar with daily coding challenges",
                        Url = "https://adventofcode.com/",
                        Icon = "bi-calendar-star"
                    }
                }
            },
            new LinkCategory
            {
                Name = "C# Tools",
                Description = "Utilities and code generators",
                Icon = "bi-tools",
                ColorClass = "danger",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "Json2CSharp",
                        Description = "Generate C# classes from JSON",
                        Url = "https://json2csharp.com/",
                        Icon = "bi-braces"
                    },
                    new UsefulLink
                    {
                        Name = "SharpLab",
                        Description = "Online C# compiler with IL and AST visualization",
                        Url = "https://sharplab.io/",
                        Icon = "bi-eye"
                    },
                    new UsefulLink
                    {
                        Name = ".NET Fiddle",
                        Description = "Online IDE to test C# and .NET code",
                        Url = "https://dotnetfiddle.net/",
                        Icon = "bi-play-circle"
                    },
                    new UsefulLink
                    {
                        Name = "NuGet",
                        Description = "Official package manager for .NET",
                        Url = "https://www.nuget.org/",
                        Icon = "bi-box-seam"
                    },
                    new UsefulLink
                    {
                        Name = "Regex101",
                        Description = "Regex tester and debugger",
                        Url = "https://regex101.com/",
                        Icon = "bi-regex"
                    }
                }
            },
            new LinkCategory
            {
                Name = "Algorithms",
                Description = "Practice algorithms and data structures",
                Icon = "bi-diagram-3",
                ColorClass = "secondary",
                Links = new List<UsefulLink>
                {
                    new UsefulLink
                    {
                        Name = "LeetCode",
                        Description = "Training platform with thousands of algorithmic problems",
                        Url = "https://leetcode.com/",
                        Icon = "bi-code"
                    },
                    new UsefulLink
                    {
                        Name = "AlgoExpert",
                        Description = "Deep algorithm training with explanatory videos",
                        Url = "https://www.algoexpert.io/",
                        Icon = "bi-mortarboard"
                    },
                    new UsefulLink
                    {
                        Name = "Visualgo",
                        Description = "Visualize algorithms and data structures",
                        Url = "https://visualgo.net/",
                        Icon = "bi-graph-up"
                    },
                    new UsefulLink
                    {
                        Name = "Big-O Cheat Sheet",
                        Description = "Quick reference for algorithmic complexities",
                        Url = "https://www.bigocheatsheet.com/",
                        Icon = "bi-table"
                    },
                    new UsefulLink
                    {
                        Name = "Project Euler",
                        Description = "Challenging mathematical and algorithmic problems",
                        Url = "https://projecteuler.net/",
                        Icon = "bi-calculator"
                    }
                }
            }
        };
    }
}
