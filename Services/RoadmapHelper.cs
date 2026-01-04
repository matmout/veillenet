using VeilleNet.Models;

namespace VeilleNet.Services;

public static class RoadmapHelper
{
    public static List<RoadmapItem> GetCSharpLearningPath() => new()
    {
        // STEP 1: C# & Object-Oriented Programming (merged fundamentals + OOP)
        new RoadmapItem
        {
            Step = 1,
            Title = "C# & Object-Oriented Programming",
            Description = "Language fundamentals and core object-oriented concepts you need to start building C# applications.",
            Category = "Foundation",
            Type = RoadmapItemType.Foundation,
            Prerequisites = new List<int>(),
            Children = new List<RoadmapItem>
            {
                new() { Title = "Variables & Data Types", Description = "Built-in types, var/const, casting, nullable types", Type = RoadmapItemType.Foundation },
                new() { Title = "Control Flow & Operators", Description = "if/else, switch, loops (for/while/foreach), arithmetic and logical operators", Type = RoadmapItemType.Foundation },
                new() { Title = "Methods & Parameters", Description = "Method declaration, ref/out/in parameters, optional params, overloading", Type = RoadmapItemType.Foundation },
                new() { Title = "Strings & Text Manipulation", Description = "Interpolation, StringBuilder, formatting, basic parsing", Type = RoadmapItemType.Foundation },
                new() { Title = "Collections Basics", Description = "Array, List<T>, Dictionary<TKey,TValue>, basic iteration", Type = RoadmapItemType.Foundation },
                new() { Title = "Enums & Simple Types", Description = "Enums, flags, simple value types", Type = RoadmapItemType.Foundation },
                new() { Title = "Classes & OOP Fundamentals", Description = "Classes, properties, constructors, encapsulation, inheritance, interfaces (core OOP concepts)", Type = RoadmapItemType.Foundation },
                new() { Title = "Generics & Equality", Description = "Generic types, constraints, equality (Equals/GetHashCode, IEquatable<T>)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 2: .NET Platform & Tooling (requires Step 1)
        new RoadmapItem
        {
            Step = 2,
            Title = ".NET Platform & Tooling",
            Description = "How .NET runs your code and how to work efficiently",
            Category = "Foundation",
            Type = RoadmapItemType.Foundation,
            Prerequisites = new List<int> { 1 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "dotnet CLI", Description = "build/test/run/publish, SDK-style projects", Type = RoadmapItemType.Foundation },
                new() { Title = "Runtime (CLR)", Description = "JIT, GC, managed vs unmanaged", Type = RoadmapItemType.Foundation },
                new() { Title = "NuGet", Description = "Dependencies, versioning, central package management (optional)", Type = RoadmapItemType.Foundation },
                new() { Title = "Git", Description = "Branching, PRs, code reviews", Type = RoadmapItemType.Foundation },
                new() { Title = "Project Structure", Description = "Solution, projects, namespaces, analyzers", Type = RoadmapItemType.Foundation },
                new() { Title = "Debugging", Description = "Breakpoints, watches, dumps basics", Type = RoadmapItemType.Foundation }
            }
        },

        // STEP 3: Error Handling & Diagnostics (requires Step 1)
        new RoadmapItem
        {
            Step = 3,
            Title = "Error Handling & Diagnostics",
            Description = "Exceptions, logging, and observability",
            Category = "Advanced",
            Type = RoadmapItemType.Advanced,
            Prerequisites = new List<int> { 1 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Exception Handling", Description = "try/catch/finally, best practices", Type = RoadmapItemType.Advanced },
                new() { Title = "Custom Exceptions", Description = "Domain-specific exceptions", Type = RoadmapItemType.Advanced },
                new() { Title = "Microsoft.Extensions.Logging", Description = "ILogger abstractions, providers, scopes", Type = RoadmapItemType.Advanced },
                new() { Title = "Serilog", Description = "Structured logs, sinks (console/file/seq), enrichment", Type = RoadmapItemType.Advanced },
                new() { Title = "log4net", Description = "Classic .NET logging (optional)", Type = RoadmapItemType.Advanced },
                new() { Title = "Tracing & Metrics", Description = "OpenTelemetry basics (optional)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 4: Collections, LINQ & Data Transformations (requires Step 1)
        new RoadmapItem
        {
            Step = 4,
            Title = "Collections & LINQ",
            Description = "Working effectively with data in memory",
            Category = "Advanced",
            Type = RoadmapItemType.Advanced,
            Prerequisites = new List<int> { 1 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Collections Deep Dive", Description = "HashSet, Queue/Stack, LinkedList, concurrent collections", Type = RoadmapItemType.Advanced },
                new() { Title = "LINQ", Description = "Deferred execution, projection, grouping, joins", Type = RoadmapItemType.Advanced },
                new() { Title = "IEnumerable vs IQueryable", Description = "In-memory vs provider queries", Type = RoadmapItemType.Advanced },
                new() { Title = "Streams", Description = "Stream, File I/O, async streams (overview)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 5: Async & Concurrency (requires Step 4)
        new RoadmapItem
        {
            Step = 5,
            Title = "Async & Concurrency",
            Description = "Asynchronous programming, threading and synchronization",
            Category = "Advanced",
            Type = RoadmapItemType.Advanced,
            Prerequisites = new List<int> { 4 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Task & async/await", Description = "CancellationToken, Task.WhenAll, timeouts", Type = RoadmapItemType.Advanced },
                new() { Title = "Async Streams", Description = "IAsyncEnumerable, await foreach", Type = RoadmapItemType.Advanced },
                new() { Title = "Threading Basics", Description = "ThreadPool, SynchronizationContext", Type = RoadmapItemType.Advanced },
                new() { Title = "Synchronization", Description = "lock, SemaphoreSlim, Concurrent* collections", Type = RoadmapItemType.Advanced },
                new() { Title = "Parallelism", Description = "Parallel.ForEach, PLINQ (optional)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 6: Memory, Performance & Interop (requires Steps 2,4)
        new RoadmapItem
        {
            Step = 6,
            Title = "Memory & Performance",
            Description = "Write efficient code and understand allocations",
            Category = "Advanced",
            Type = RoadmapItemType.Advanced,
            Prerequisites = new List<int> { 2, 4 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Allocation Awareness", Description = "Boxing, closures, string allocations", Type = RoadmapItemType.Advanced },
                new() { Title = "Span<T>/Memory<T>", Description = "High-performance memory access", Type = RoadmapItemType.Advanced },
                new() { Title = "Profiling", Description = "dotnet-trace, dotnet-counters, profilers", Type = RoadmapItemType.Advanced },
                new() { Title = "Interop", Description = "IDisposable, SafeHandle, P/Invoke basics (optional)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 7: Testing & Quality (requires Steps 1,3)
        new RoadmapItem
        {
            Step = 7,
            Title = "Testing & Quality",
            Description = "Build confidence and maintainability",
            Category = "Foundation",
            Type = RoadmapItemType.Foundation,
            Prerequisites = new List<int> { 1, 3 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Unit Testing", Description = "xUnit/NUnit/MSTest, assertions", Type = RoadmapItemType.Foundation },
                new() { Title = "Mocking", Description = "Moq/NSubstitute, fakes, test doubles", Type = RoadmapItemType.Advanced },
                new() { Title = "Integration Testing", Description = "TestServer/WebApplicationFactory (optional)", Type = RoadmapItemType.Advanced },
                new() { Title = "Static Analysis", Description = "Analyzers, StyleCop, nullable warnings", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 8: Data & Persistence (requires Steps 4,5)
        new RoadmapItem
        {
            Step = 8,
            Title = "Data & Persistence",
            Description = "SQL basics, ORMs, migrations, caching",
            Category = "Specialization",
            Type = RoadmapItemType.Specialization,
            Prerequisites = new List<int> { 4, 5 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "SQL Fundamentals", Description = "SELECT/JOIN/INDEX/STORED PROCEDURE/TRIGGER, transactions basics", Type = RoadmapItemType.Foundation },
                new() { Title = "Entity Framework Core", Description = "DbContext, tracking, migrations, performance", Type = RoadmapItemType.Specialization },
                new() { Title = "Dapper", Description = "Micro-ORM for hot paths (optional)", Type = RoadmapItemType.Specialization },
                new() { Title = "SQL Server", Description = "Microsoft SQL Server, T-SQL, SqlClient", Type = RoadmapItemType.Specialization },
                new() { Title = "PostgreSQL", Description = "Postgres, Npgsql driver, JSONB basics", Type = RoadmapItemType.Specialization },
                new() { Title = "Oracle", Description = "Oracle DB, ODP.NET / Oracle.ManagedDataAccess", Type = RoadmapItemType.Specialization }
            }
        },

        // STEP 9: Caching
        new RoadmapItem
        {
            Step = 9,
            Title = "Caching",
            Description = "In-memory + distributed caching patterns and tools",
            Category = "Specialization",
            Type = RoadmapItemType.Specialization,
            Prerequisites = new List<int> { 8 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "In-memory cache", Description = "IMemoryCache, cache-aside, TTL, eviction", Type = RoadmapItemType.Specialization },
                new() { Title = "Distributed cache", Description = "IDistributedCache, serialization, key design", Type = RoadmapItemType.Specialization },
                new() { Title = "Redis", Description = "Most common distributed cache (strings, hashes, pub/sub)", Type = RoadmapItemType.Specialization },
                new() { Title = "Memcached", Description = "Simple, widely-used distributed cache (key/value)", Type = RoadmapItemType.Specialization },
                new() { Title = "NCache", Description = ".NET-focused distributed cache (often used in enterprise)", Type = RoadmapItemType.Specialization },
                new() { Title = "Cache invalidation", Description = "Versioned keys, stampede protection, refresh-ahead", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 10: ASP.NET Core Web (requires Steps 5,7,8)
        new RoadmapItem
        {
            Step = 10,
            Title = "ASP.NET Core Web",
            Description = "Build production-grade web apps/APIs",
            Category = "Specialization",
            Type = RoadmapItemType.Specialization,
            Prerequisites = new List<int> { 5, 7, 8 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "HTTP & REST", Description = "Status codes, headers, idempotency", Type = RoadmapItemType.Specialization },
                new() { Title = "Razor Pages", Description = "Page model, handlers, routing", Type = RoadmapItemType.Specialization },
                new() { Title = "Minimal APIs", Description = "Endpoints, filters (optional)", Type = RoadmapItemType.Specialization },
                new() { Title = "Web API", Description = "Controllers, model binding, validation", Type = RoadmapItemType.Specialization },
                new() { Title = "Middleware", Description = "Pipeline, exception handling middleware", Type = RoadmapItemType.Specialization },
                new() { Title = "Dependency Injection", Description = "Service registration & lifetimes (Scoped, Transient, Singleton), options pattern and resolution", Type = RoadmapItemType.Specialization },
                new() { Title = ".NET Aspire", Description = "Guided mini-projects to build small end-to-end .NET apps and practice architecture, DI, testing, performance and deployment.", Type = RoadmapItemType.Specialization },
                new() { Title = "Swagger / OpenAPI", Description = "Auto-generate API docs and interactive UI", Type = RoadmapItemType.Specialization },
                new() { Title = "GraphQL", Description = "Flexible query API alternative to REST", Type = RoadmapItemType.Specialization },
                new() { Title = "Ocelot (API Gateway)", Description = "Lightweight API gateway for microservices routing", Type = RoadmapItemType.Specialization },
                new() { Title = ".NET MAUI", Description = "Cross-platform native apps for mobile & desktop", Type = RoadmapItemType.Specialization },
            }
        },

        // STEP 11: Security (requires Step 10)
        new RoadmapItem
        {
            Step = 11,
            Title = "Security",
            Description = "Protect apps and data",
            Category = "Advanced",
            Type = RoadmapItemType.Advanced,
            Prerequisites = new List<int> { 10 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Authentication", Description = "Cookies, JWT, OpenID Connect concepts", Type = RoadmapItemType.Advanced },
                new() { Title = "Authorization", Description = "Policies, roles, claims", Type = RoadmapItemType.Advanced },
                new() { Title = "Web Security", Description = "XSS, CSRF, CORS, headers", Type = RoadmapItemType.Advanced },
                new() { Title = "Secrets Management", Description = "User secrets, env vars, vaults (optional)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 12: Real-Time Communication (requires Step 10)
        new RoadmapItem
        {
            Step = 12,
            Title = "Real-Time Communication",
            Description = "Push, streaming and messaging in distributed systems",
            Category = "Specialization",
            Type = RoadmapItemType.Specialization,
            Prerequisites = new List<int> { 10 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "WebSockets", Description = "Persistent connections, basic realtime patterns", Type = RoadmapItemType.Specialization },
                new() { Title = "SignalR", Description = "Hubs, groups, scaling out, backplanes", Type = RoadmapItemType.Specialization },
                new() { Title = "RabbitMQ", Description = "Message broker, pub/sub, queues, background processing", Type = RoadmapItemType.Specialization }
            }
        },

        // STEP 13: Design & Architecture (requires Steps 1,7,9)
        new RoadmapItem
        {
            Step = 13,
            Title = "Design & Architecture",
            Description = "Patterns and maintainable architectures",
            Category = "Advanced",
            Type = RoadmapItemType.Advanced,
            Prerequisites = new List<int> { 1, 7, 9 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "SOLID", Description = "Core OO design principles", Type = RoadmapItemType.Advanced },
                new() { Title = "Common Patterns", Description = "Factory, Strategy, Adapter, Decorator", Type = RoadmapItemType.Advanced },
                new() { Title = "Clean Architecture", Description = "Layering, boundaries, DTOs", Type = RoadmapItemType.Advanced },
                new() { Title = "CQRS/Eventing", Description = "Intro to CQRS, messages (optional)", Type = RoadmapItemType.Advanced },
                new() { Title = "Object Mapping", Description = "Converting one type to another (AutoMapper)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 14: Modern C# Language Features (optional track, requires Step 1)
        new RoadmapItem
        {
            Step = 14,
            Title = "Modern C# (C# 10-12+)",
            Description = "Use modern language features effectively",
            Category = "Advanced",
            Type = RoadmapItemType.Advanced,
            Prerequisites = new List<int> { 1 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Records", Description = "Value-like types, immutable models", Type = RoadmapItemType.Advanced },
                new() { Title = "Pattern Matching", Description = "switch expressions, property patterns", Type = RoadmapItemType.Advanced },
                new() { Title = "Nullable Reference Types", Description = "Avoid null bugs", Type = RoadmapItemType.Advanced },
                new() { Title = "Source Generators", Description = "Compile-time code generation (optional)", Type = RoadmapItemType.Advanced }
            }
        },

        // STEP 15: Dev Workflow & Delivery (optional, requires Step 2)
        new RoadmapItem
        {
            Step = 15,
            Title = "Dev Workflow & Delivery",
            Description = "Ship software reliably",
            Category = "Foundation",
            Type = RoadmapItemType.Foundation,
            Prerequisites = new List<int> { 2 },
            Children = new List<RoadmapItem>
            {
                new() { Title = "Build & CI", Description = "dotnet test, pipelines, GitHub Actions", Type = RoadmapItemType.Foundation },
                new() { Title = "Docker", Description = "Images, containers, compose (optional)", Type = RoadmapItemType.Foundation },
                new() { Title = "Deployment", Description = "Hosting basics, configs, environments (optional)", Type = RoadmapItemType.Foundation },
                new() { Title = "SonarQube", Description = "Continuous code quality and static analysis", Type = RoadmapItemType.Foundation },
                new() { Title = "Exception Monitoring", Description = "Production error tracking (Sentry, App Insights)", Type = RoadmapItemType.Foundation }
            }
        }
    };
}
