# GEMINI.md - Project Context: ContainSharp (VeilleNet)

This file provides essential context for Gemini to understand and assist with the ContainSharp project.

## Project Overview
ContainSharp (also referred to as VeilleNet in the codebase) is a **technology watch dashboard** for the .NET and C# ecosystem. It aggregates news from official Microsoft blogs, GitHub (trending projects), YouTube, and various AI news sources.

### Key Features
- **News Aggregation:** RSS feeds from .NET, ASP.NET, Visual Studio, and C# blogs.
- **AI-Powered Summarization:** Automatically generates expert-level summaries of news articles using **Mistral** (via `Microsoft.Extensions.AI`).
- **Trend Analysis:** Identifies dominant daily themes across multiple news sources.
- **Daily Newsletter:** Automated email newsletter sent to subscribers via **AWS SES**.
- **Interactive Training:** A C# quiz for developers.
- **MCP Directory:** A list of Model Context Protocol (MCP) servers for AI agents.

## Technical Stack
- **Framework:** ASP.NET Core 10.0 (Razor Pages)
- **Language:** C# 12.0
- **Database:** PostgreSQL with Entity Framework Core
- **Background Tasks:** Quartz.NET for scheduled jobs
- **AI Integration:** Mistral AI (using OpenAI-compatible provider)
- **Email:** Amazon Simple Email Service (SES)
- **Frontend:** Bootstrap 5.3, Vanilla JavaScript, CSS for "Terminal/Visual Studio" aesthetic

## Core Architecture
The project follows a simplified **Clean Architecture** with a Service-Repository pattern.

- **`Models/`**: POCOs for domain data and configuration options.
- **`Services/`**: Business logic, API clients (GitHub, RSS), and AI services.
- **`Data/`**: `ApplicationDbContext` and EF Core Migrations.
- **`Pages/`**: Razor Pages for the UI.
- **`Services/Agent/`**: Contains AI-specific logic like `AiSummarizationService.cs` and background jobs.
- **`Services/Data/`**: Repository layer for database operations.

## Development & Operations

### Building and Running
- **Build:** `dotnet build`
- **Run:** `dotnet run`
- **Docker:** `docker-compose up -d` (uses multi-stage build in `Dockerfile`)
- **Database Migrations:** `dotnet ef database update`

### Key Configuration (appsettings.json / User Secrets)
- `DatabaseOptions:ConnectionString`: PostgreSQL connection string.
- `Mistral:ApiKey`: API key for Mistral AI.
- `GitHub:Token`: (Optional) For higher rate limits on GitHub API.
- `EmailSettings`: AWS SES credentials and configuration.

### Scheduled Jobs (Quartz)
- **`AiSummaryGenerationJob`**: Runs every 10 minutes to fetch new articles and generate AI summaries.
- **`NewsletterSendJob`**: Sends the daily newsletter every day at 17:00 (5 PM).

## Development Conventions
- **Asynchronous Everywhere:** All I/O operations (HTTP, DB) must use `async/await`.
- **Null Safety:** Nullable reference types are enabled.
- **Caching:** Extensive use of `IMemoryCache` to minimize external API calls.
- **Resilience:** Try-catch blocks in services ensure the dashboard remains functional even if some sources fail.
- **Clean UI:** Mimic the Visual Studio / Terminal aesthetic using the custom CSS in `wwwroot/css/site.css`.

## Project History & Context
Originally conceived as a "no-database" static aggregator, it has evolved into a full data-driven application with AI summaries and newsletter management. The database is now a central component for persistence.
