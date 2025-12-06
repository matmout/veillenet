using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;

namespace VeilleNet.Pages;

public class MCPModel : PageModel
{
    public List<MCPCategory> Categories { get; set; } = new();

    public void OnGet()
    {
        Categories = new List<MCPCategory>
        {
            new MCPCategory
            {
                Name = "AI & Code Agents",
                Description = "MCP servers for AI-powered coding assistance and development",
                Icon = "bi-robot",
                ColorClass = "info",
                Tools = new List<MCPTool>
                {
                    new MCPTool
                    {
                        Name = "GitHub MCP Server",
                        Description = "Access GitHub repositories, issues, pull requests, and workflows directly from your AI assistant",
                        Category = "AI & Code Agents",
                        Icon = "bi-github",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/github"
                    },
                    new MCPTool
                    {
                        Name = "Playwright MCP Server",
                        Description = "Browser automation and web testing capabilities through MCP",
                        Category = "AI & Code Agents",
                        Icon = "bi-browser-chrome",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/playwright"
                    },
                    new MCPTool
                    {
                        Name = "Memory MCP Server",
                        Description = "Knowledge graph-based persistent memory system for AI assistants",
                        Category = "AI & Code Agents",
                        Icon = "bi-diagram-3",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/memory"
                    },
                    new MCPTool
                    {
                        Name = "Fetch MCP Server",
                        Description = "Web content fetching and processing for AI context",
                        Category = "AI & Code Agents",
                        Icon = "bi-download",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/fetch"
                    },
                    new MCPTool
                    {
                        Name = "Filesystem MCP Server",
                        Description = "Secure file system operations with configurable access controls",
                        Category = "AI & Code Agents",
                        Icon = "bi-folder",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/filesystem"
                    }
                }
            },
            new MCPCategory
            {
                Name = "Database Connections",
                Description = "MCP servers for database querying and management",
                Icon = "bi-database",
                ColorClass = "primary",
                Tools = new List<MCPTool>
                {
                    new MCPTool
                    {
                        Name = "SQLite MCP Server",
                        Description = "Query and manage SQLite databases with built-in safety features",
                        Category = "Database Connections",
                        Icon = "bi-database-check",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/sqlite"
                    },
                    new MCPTool
                    {
                        Name = "PostgreSQL MCP Server",
                        Description = "Connect to PostgreSQL databases with secure query execution",
                        Category = "Database Connections",
                        Icon = "bi-database-gear",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/postgres"
                    },
                    new MCPTool
                    {
                        Name = "MySQL MCP Server",
                        Description = "MySQL database integration for data operations through MCP",
                        Category = "Database Connections",
                        Icon = "bi-database-fill",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/mysql"
                    },
                    new MCPTool
                    {
                        Name = "MongoDB MCP Server",
                        Description = "NoSQL database access for MongoDB collections and documents",
                        Category = "Database Connections",
                        Icon = "bi-database-add",
                        GuideUrl = "https://github.com/tommoor/mcp-server-mongodb"
                    }
                }
            },
            new MCPCategory
            {
                Name = "Deployment & DevOps",
                Description = "MCP servers for application deployment and infrastructure management",
                Icon = "bi-cloud-upload",
                ColorClass = "success",
                Tools = new List<MCPTool>
                {
                    new MCPTool
                    {
                        Name = "Docker MCP Server",
                        Description = "Manage Docker containers, images, and orchestration",
                        Category = "Deployment & DevOps",
                        Icon = "bi-box-seam",
                        GuideUrl = "https://github.com/ckreiling/mcp-server-docker"
                    },
                    new MCPTool
                    {
                        Name = "Kubernetes MCP Server",
                        Description = "Kubernetes cluster management and deployment automation",
                        Category = "Deployment & DevOps",
                        Icon = "bi-boxes",
                        GuideUrl = "https://github.com/Flux159/mcp-server-kubernetes"
                    },
                    new MCPTool
                    {
                        Name = "AWS MCP Server",
                        Description = "AWS resource management and deployment through MCP",
                        Category = "Deployment & DevOps",
                        Icon = "bi-cloud",
                        GuideUrl = "https://github.com/rishikavikondala/mcp-server-aws"
                    },
                    new MCPTool
                    {
                        Name = "Git MCP Server",
                        Description = "Version control operations and Git workflow automation",
                        Category = "Deployment & DevOps",
                        Icon = "bi-git",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/git"
                    },
                    new MCPTool
                    {
                        Name = "Sentry MCP Server",
                        Description = "Error tracking and monitoring integration",
                        Category = "Deployment & DevOps",
                        Icon = "bi-bug",
                        GuideUrl = "https://github.com/modelcontextprotocol/servers/tree/main/src/sentry"
                    }
                }
            }
        };
    }
}
