using VeilleNet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add memory cache
builder.Services.AddMemoryCache();

// Add HttpClient factory
builder.Services.AddHttpClient();

// Register application services
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IBlogAggregationService, BlogAggregationService>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IReleaseNewsService, ReleaseNewsService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IAINewsService, AINewsService>();
builder.Services.AddScoped<IWinFormNewsService, WinFormNewsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Activer les fichiers statiques (nécessaire pour wwwroot)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
