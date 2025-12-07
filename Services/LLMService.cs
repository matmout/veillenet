using VeilleNet.Models;

namespace VeilleNet.Services;

public interface ILLMService
{
    Task<List<LLM>> GetLatestLLMsAsync();
    Task<List<LLM>> GetTopLLMsAsync(int count);
}

public class LLMService : ILLMService
{
    private readonly ICacheService _cacheService;
    private const string CacheKey = "LatestLLMs";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    public LLMService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<LLM>> GetLatestLLMsAsync()
    {
        var cachedLLMs = _cacheService.Get<List<LLM>>(CacheKey);
        if (cachedLLMs != null)
        {
            return cachedLLMs;
        }

        var llms = new List<LLM>
        {
            new LLM
            {
                Name = "Grok-2",
                Description = "Deuxième génération du modèle Grok de xAI, orienté conversation, code et accès temps réel au web via X (Twitter).",
                Link = "https://x.ai",
                Author = "xAI",
                DateRelease = new DateTime(2024, 12, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GitHub Copilot (chat & IDE)",
                Description = "Assistant de développement basé sur des modèles OpenAI (dont GPT‑4o et dérivés), optimisé par GitHub pour le code, les revues et la génération contextuelle.",
                Link = "https://docs.github.com/copilot",
                Author = "GitHub / Microsoft",
                DateRelease = new DateTime(2024, 10, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "OpenAI o3",
                Description = "Modèle de raisonnement optimisé pour les tâches complexes, logique et résolution de problèmes (raisonnement avancé).",
                Link = "https://platform.openai.com/docs/models#reasoning",
                Author = "OpenAI",
                DateRelease = new DateTime(2024, 9, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Perplexity pplx-llama-3.1-sonar-large-online",
                Description = "Grand modèle Perplexity basé sur Llama 3.1, spécialisé dans la recherche en ligne et les réponses avec citations.",
                Link = "https://docs.perplexity.ai/docs/model-cards",
                Author = "Perplexity",
                DateRelease = new DateTime(2024, 8, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Perplexity pplx-llama-3.1-sonar-small-online",
                Description = "Version plus légère et rapide du modèle sonar-online de Perplexity, adaptée aux usages temps réel.",
                Link = "https://docs.perplexity.ai/docs/model-cards",
                Author = "Perplexity",
                DateRelease = new DateTime(2024, 8, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude 3.5 Haiku",
                Description = "Version rapide et légère de Claude 3.5, optimisée pour la latence et le coût.",
                Link = "https://docs.anthropic.com/claude/docs/models",
                Author = "Anthropic",
                DateRelease = new DateTime(2024, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "OpenAI GPT-4.1",
                Description = "Modèle généraliste avancé, texte et code, successeur de GPT‑4/GPT‑4 Turbo (multimodal via API et produits OpenAI).",
                Link = "https://platform.openai.com/docs/models",
                Author = "OpenAI",
                DateRelease = new DateTime(2024, 6, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude 3.5 Sonnet",
                Description = "Modèle Claude 3.5 de taille intermédiaire, bon équilibre vitesse/qualité, très fort en raisonnement et code.",
                Link = "https://docs.anthropic.com/claude/docs/models",
                Author = "Anthropic",
                DateRelease = new DateTime(2024, 6, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "OpenAI GPT-4o",
                Description = "Modèle multimodal temps réel (texte, image, audio) optimisé pour l'interactivité et le coût, base de nombreux produits OpenAI.",
                Link = "https://platform.openai.com/docs/models#gpt-4o",
                Author = "OpenAI",
                DateRelease = new DateTime(2024, 5, 1),
                ScoreIA = "N/A"
            }
        };

        // Sort by release date descending (newest first)
        llms = llms.OrderByDescending(l => l.DateRelease).ToList();

        _cacheService.Set(CacheKey, llms, CacheExpiration);
        return await Task.FromResult(llms);
    }

    public async Task<List<LLM>> GetTopLLMsAsync(int count)
    {
        var allLLMs = await GetLatestLLMsAsync();
        return allLLMs.Take(count).ToList();
    }
}
