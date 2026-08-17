using VeilleNet.Services;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.DataProtection;
using Amazon.SimpleEmail;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;
using VeilleNet.Services.Agent;
using VeilleNet.Services.Tools;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using VeilleNet.Data;
using Quartz;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

// Ensure environment variables (e.g. Railway `Database__ConnectionString`) override appsettings.
builder.Configuration.AddEnvironmentVariables();

// Logging: be verbose only in Development
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);

    // EF Core: hide SQL + command details
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);

    // HttpClient: very noisy at Info (request start/stop)
    builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

    // Framework noise
    builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Error);
    builder.Logging.AddFilter("Microsoft.AspNetCore.HttpsPolicy", LogLevel.Error);

    // Default for Microsoft
    builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
}

// Add services to the container.
builder.Services.AddRazorPages();

// Persist DataProtection keys to the filesystem to maintain a stable key ring across restarts/deploys
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
    .SetApplicationName("VeilleNet");

// Ensure the keys directory exists on startup
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "keys"));

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add response compression (Brotli and Gzip)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/javascript",
        "application/json",
        "application/xml",
        "text/css",
        "text/html",
        "text/json",
        "text/plain",
        "text/xml",
        "image/svg+xml"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Add memory cache
builder.Services.AddMemoryCache();

// Configure Database
builder.Services.Configure<VeilleNet.Models.DatabaseOptions>(
    builder.Configuration.GetSection(VeilleNet.Models.DatabaseOptions.SectionName));

builder.Services.Configure<VeilleNet.Models.RedditOptions>(
    builder.Configuration.GetSection(VeilleNet.Models.RedditOptions.SectionName));

builder.Services.AddDbContextFactory<ApplicationDbContext>((serviceProvider, options) =>
{
    var dbOptions = serviceProvider.GetRequiredService<IOptions<VeilleNet.Models.DatabaseOptions>>().Value;

    options.UseNpgsql(dbOptions.ConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: dbOptions.MaxRetryCount,
            maxRetryDelay: TimeSpan.FromSeconds(dbOptions.MaxRetryDelay),
            errorCodesToAdd: null);

        npgsqlOptions.CommandTimeout(dbOptions.CommandTimeout);
    });

    if (dbOptions.EnableSensitiveDataLogging) options.EnableSensitiveDataLogging();
    if (dbOptions.EnableDetailedErrors) options.EnableDetailedErrors();
});

// Add HttpClient factory with retry/backoff resilience for external news API calls
builder.Services.AddHttpClient(string.Empty).AddStandardResilienceHandler();

// Register application services
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<IMCPService, MCPService>();
builder.Services.AddSingleton<ISkillsService, SkillsService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IBlogAggregationService, BlogAggregationService>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IReleaseNewsService, ReleaseNewsService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IAINewsService, AINewsService>();
builder.Services.AddScoped<IWinFormNewsService, WinFormNewsService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IStackOverflowService, StackOverflowService>();
builder.Services.AddScoped<IRedditService, RedditService>();
builder.Services.AddScoped<ILLMService, LLMService>();
builder.Services.AddSingleton<IQuestionService, QuestionService>();
builder.Services.AddSingleton<IFrameworkVersionService, FrameworkVersionService>();
builder.Services.AddScoped<INewsHistoryService, NewsHistoryService>();
builder.Services.AddScoped<INewsDeduplicationService, NewsDeduplicationService>();

// Data services
builder.Services.AddScoped<VeilleNet.Services.Data.NewsRepository>();
builder.Services.AddScoped<VeilleNet.Services.Data.INewsRepository>(sp => sp.GetRequiredService<VeilleNet.Services.Data.NewsRepository>());
builder.Services.AddScoped<VeilleNet.Services.Data.IArticleRepository>(sp => sp.GetRequiredService<VeilleNet.Services.Data.NewsRepository>());
builder.Services.AddScoped<VeilleNet.Services.Data.IAiSummaryRepository>(sp => sp.GetRequiredService<VeilleNet.Services.Data.NewsRepository>());
builder.Services.AddScoped<VeilleNet.Services.Data.ISubscriberRepository>(sp => sp.GetRequiredService<VeilleNet.Services.Data.NewsRepository>());
builder.Services.AddScoped<VeilleNet.Services.Data.INewsletterRepository>(sp => sp.GetRequiredService<VeilleNet.Services.Data.NewsRepository>());

// AI summarization (Mistral via OpenAI-compatible endpoint)
builder.Services.Configure<VeilleNet.Models.MistralOptions>(builder.Configuration.GetSection("Mistral"));
builder.Services.AddSingleton<IMistralChatClientFactory, MistralChatClientFactory>();
builder.Services.AddScoped<IAiSummarizationService, AiSummarizationService>();
builder.Services.AddScoped<IDailyBriefingService, DailyBriefingService>();

// Email service (Amazon SES)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IAmazonSimpleEmailService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<EmailSettings>>().Value;
    var credentials = new BasicAWSCredentials(settings.AwsAccessKey, settings.AwsSecretKey);
    var config = new AmazonSimpleEmailServiceConfig
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(settings.AwsRegion)
    };

    return new AmazonSimpleEmailServiceClient(credentials, config);
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJobExecutionLogger, JobExecutionLogger>();

// Quartz jobs
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var aiSummaryJobKey = new JobKey("AiSummaryGenerationJob");
    q.AddJob<AiSummaryGenerationJob>(opts => opts.WithIdentity(aiSummaryJobKey));
    q.AddTrigger(opts => opts
        .ForJob(aiSummaryJobKey)
        .WithIdentity("AiSummaryGenerationJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromMinutes(10))
            .RepeatForever()));

    var newsletterJobKey = new JobKey("NewsletterSendJob");
    q.AddJob<NewsletterSendJob>(opts => opts.WithIdentity(newsletterJobKey));
    q.AddTrigger(opts => opts
        .ForJob(newsletterJobKey)
        .WithIdentity("NewsletterSendJob-trigger")
        // Tous les jours à 17h00 (timezone locale du serveur)
        .WithCronSchedule("0 0 17 ? * *"));

    var dailyBriefingJobKey = new JobKey("DailyBriefingJob");
    q.AddJob<DailyBriefingJob>(opts => opts.WithIdentity(dailyBriefingJobKey));
    q.AddTrigger(opts => opts
        .ForJob(dailyBriefingJobKey)
        .WithIdentity("DailyBriefingJob-trigger")
        // Every day at 07:00 UTC to pre-generate the daily briefing
        .WithCronSchedule("0 0 7 ? * *"));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

// Add controllers for API endpoints
builder.Services.AddControllers();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbOptions = services.GetRequiredService<IOptions<VeilleNet.Models.DatabaseOptions>>().Value;

        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Checking database connection...");
            logger.LogInformation("Connection string: {ConnectionString}",
                new Npgsql.NpgsqlConnectionStringBuilder(dbOptions.ConnectionString) { Password = "****" }.ToString());
        }

        // Test raw connection first (noisy) only in Development
        var canConnect = true;
        if (app.Environment.IsDevelopment())
        {
            canConnect = await VeilleNet.Tools.PostgresConnectionTest.TestConnectionAsync(dbOptions.ConnectionString);

            if (!canConnect)
            {
                logger.LogWarning("Raw Npgsql connection test failed!");
            }
        }

        // Test if we can connect via EF Core
        canConnect = await context.Database.CanConnectAsync();
        if (canConnect)
        {
            if (app.Environment.IsDevelopment())
            {
                logger.LogInformation("Database connection successful!");
            }

            // Apply pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully!");
            }
        }
        else
        {
            logger.LogWarning("Cannot connect to database. Application will continue but database features will not work.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database. Application will continue but database features may not work.");
    }
}

// Configure the HTTP request pipeline.
// Use response compression early in the pipeline
app.UseResponseCompression();

app.Use(async (context, next) =>
{
    if (string.Equals(context.Request.Host.Host, "containsharp.com", StringComparison.OrdinalIgnoreCase))
    {
        var target = $"https://www.containsharp.com{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(target, permanent: true);
        return;
    }

    await next();
});

// Configure status code pages for 404
app.UseStatusCodePagesWithReExecute("/Error404");

//if (!app.Environment.IsDevelopment())
//{
//      app.UseExceptionHandler("/Error404");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    // app.UseHsts();
//}

// Static files with caching headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets for 1 year in production
        if (!app.Environment.IsDevelopment())
        {
            const int durationInSeconds = 60 * 60 * 24 * 30; // 30 days
            ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={durationInSeconds}";
            ctx.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
        }
    }
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

await app.RunAsync();
