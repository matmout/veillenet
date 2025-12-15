using VeilleNet.Services;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.DataProtection;
using Amazon.SimpleEmail;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;
using VeilleNet.Services.Agent;
using Amazon.Runtime;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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

// Add HttpClient factory
builder.Services.AddHttpClient();

// Register application services
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<IMCPService, MCPService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IBlogAggregationService, BlogAggregationService>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IReleaseNewsService, ReleaseNewsService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IAINewsService, AINewsService>();
builder.Services.AddScoped<IWinFormNewsService, WinFormNewsService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<ILLMService, LLMService>();
builder.Services.AddSingleton<IQuestionService, QuestionService>();

// AI summarization (Mistral via OpenAI-compatible endpoint)
builder.Services.Configure<VeilleNet.Models.MistralOptions>(builder.Configuration.GetSection("Mistral"));
builder.Services.AddSingleton<IMistralChatClientFactory, MistralChatClientFactory>();
builder.Services.AddScoped<IAiSummarizationService, AiSummarizationService>();

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

// Configure AWS options
// builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
// builder.Services.AddAWSService<IAmazonSimpleEmailService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add background service for daily AI summarization
builder.Services.AddHostedService<AiSummarizationBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Use response compression early in the pipeline
app.UseResponseCompression();

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
app.MapRazorPages();

await app.RunAsync();