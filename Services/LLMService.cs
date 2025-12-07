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
                Name = "Mistral Large 3",
                Description = "A state-of-the-art, open-weight, general-purpose model.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 12, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Mistral Medium 3.1",
                Description = "Our frontier-class multimodal model released for general availability.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 8, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Mistral Small 3.2",
                Description = "An update to our previous small model, optimized for efficiency.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 6, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Ministral 3 14B",
                Description = "A powerful model offering best-in-class text generation capabilities.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 12, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Ministral 3 8B",
                Description = "A powerful and efficient model offering best-in-class performance for its size.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 12, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Ministral 3 3B",
                Description = "A tiny and efficient model offering best-in-class performance on edge devices.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 12, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Magistral Medium 1.2",
                Description = "Our frontier-class multimodal reasoning model.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 9, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Magistral Small 1.2",
                Description = "Our small multimodal reasoning model.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 9, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Mistral OCR",
                Description = "Our OCR service powering our Document AI capabilities.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 5, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Voxtral Mini Transcribe",
                Description = "An efficient audio input model, fine-tuned for transcription tasks.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Codestral",
                Description = "Our cutting-edge language model for coding tasks.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 8, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Devstral Medium 1.0",
                Description = "An enterprise grade text model, that excels at development tasks.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Voxtral Mini",
                Description = "A mini version of our first audio input model.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Voxtral Small",
                Description = "Our first model with audio input capabilities for general use.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Devstral Small 1.1",
                Description = "An update to our open source model that specializes in development.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Mistral Moderation",
                Description = "Our moderation service that enables our safety capabilities.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2024, 11, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Codestral Embed",
                Description = "Our state-of-the-art semantic model for extracting embeddings from code.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 5, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Mistral Medium 3",
                Description = "Our frontier-class multimodal model released for general availability.",
                Link = "https://mistral.ai/",
                Author = "Mistral AI",
                DateRelease = new DateTime(2025, 5, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude Opus 4.5",
                Description = "Anthropic's most powerful model (4.5 series), excelling in complex tasks, reasoning, and creativity.",
                Link = "https://docs.anthropic.com/claude/docs/models",
                Author = "Anthropic",
                DateRelease = new DateTime(2025, 12, 1), // Conjectured from context
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude Sonnet 4.5",
                Description = "Performance/cost balance of the 4.5 series, ideal for enterprise and scaling.",
                Link = "https://docs.anthropic.com/claude/docs/models",
                Author = "Anthropic",
                DateRelease = new DateTime(2025, 12, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Gemini 3 Pro",
                Description = "New generation of Google Gemini, natively multimodal with increased reasoning capabilities.",
                Link = "https://deepmind.google/technologies/gemini/",
                Author = "Google",
                DateRelease = new DateTime(2025, 11, 15),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Kimi K2 Thinking",
                Description = "Advanced Moonshot AI model with reinforced 'Thinking' capabilities.",
                Link = "https://moonshot.cn/",
                Author = "Moonshot AI",
                DateRelease = new DateTime(2025, 11, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GPT 5.1",
                Description = "Iterative improvement of GPT-5, offering better reliability and more nuanced responses.",
                Link = "https://openai.com/",
                Author = "OpenAI",
                DateRelease = new DateTime(2025, 10, 15),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GPT-5",
                Description = "OpenAI's next major leap, promising general intelligence closer to human level.",
                Link = "https://openai.com/",
                Author = "OpenAI",
                DateRelease = new DateTime(2025, 9, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude Opus 4.1",
                Description = "Intermediate update to the Claude 4 Opus series.",
                Link = "https://docs.anthropic.com/",
                Author = "Anthropic",
                DateRelease = new DateTime(2025, 8, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GPT oss 20b",
                Description = "Performant mid-sized open-source model (20B), optimized for self-hosting.",
                Link = "https://huggingface.co/",
                Author = "Open Source",
                DateRelease = new DateTime(2025, 7, 15),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GPT oss 120b",
                Description = "Large open-source model (120B) rivaling proprietary models in reasoning.",
                Link = "https://huggingface.co/",
                Author = "Open Source",
                DateRelease = new DateTime(2025, 7, 15),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Grok 4",
                Description = "Fourth iteration of xAI, deeply integrated with real-time data.",
                Link = "https://x.ai/",
                Author = "xAI",
                DateRelease = new DateTime(2025, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude 4 Opus",
                Description = "Flagship model of generation 4, pushing boundaries of context and understanding.",
                Link = "https://docs.anthropic.com/",
                Author = "Anthropic",
                DateRelease = new DateTime(2025, 6, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude 4 Sonnet",
                Description = "The 'workhorse' of series 4, efficient and versatile.",
                Link = "https://docs.anthropic.com/",
                Author = "Anthropic",
                DateRelease = new DateTime(2025, 6, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Gemini 2.5 Flash",
                Description = "Ultra-fast and economical model from Google, ideal for high-frequency applications.",
                Link = "https://deepmind.google/technologies/gemini/flash/",
                Author = "Google",
                DateRelease = new DateTime(2025, 5, 15),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "OpenAI o4-mini",
                Description = "Small, performant reasoning model, economical successor.",
                Link = "https://openai.com/",
                Author = "OpenAI",
                DateRelease = new DateTime(2025, 5, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GPT-4.5 nano",
                Description = "Very compact model for embedded or mobile applications.",
                Link = "https://openai.com/",
                Author = "OpenAI",
                DateRelease = new DateTime(2025, 4, 15),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GPT-4.1 mini",
                Description = "Miniaturized and optimized version of the GPT-4.1 branch.",
                Link = "https://openai.com/",
                Author = "OpenAI",
                DateRelease = new DateTime(2025, 4, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Grok-2",
                Description = "Second generation xAI Grok model, oriented towards conversation, code, and real-time web access via X (Twitter).",
                Link = "https://x.ai",
                Author = "xAI",
                DateRelease = new DateTime(2024, 12, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "GitHub Copilot (chat & IDE)",
                Description = "Development assistant based on OpenAI models (including GPT-4o), optimized by GitHub for code, reviews, and context generation.",
                Link = "https://docs.github.com/copilot",
                Author = "GitHub / Microsoft",
                DateRelease = new DateTime(2024, 10, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "OpenAI o3",
                Description = "Reasoning model optimized for complex tasks, logic, and problem solving (advanced reasoning).",
                Link = "https://platform.openai.com/docs/models#reasoning",
                Author = "OpenAI",
                DateRelease = new DateTime(2024, 9, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Perplexity pplx-llama-3.1-sonar-large-online",
                Description = "Large Perplexity model based on Llama 3.1, specialized in online search and cited answers.",
                Link = "https://docs.perplexity.ai/docs/model-cards",
                Author = "Perplexity",
                DateRelease = new DateTime(2024, 8, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Perplexity pplx-llama-3.1-sonar-small-online",
                Description = "Lighter and faster version of Perplexity sonar-online, adapted for real-time usage.",
                Link = "https://docs.perplexity.ai/docs/model-cards",
                Author = "Perplexity",
                DateRelease = new DateTime(2024, 8, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude 3.5 Haiku",
                Description = "Fast and light version of Claude 3.5, optimized for latency and cost.",
                Link = "https://docs.anthropic.com/claude/docs/models",
                Author = "Anthropic",
                DateRelease = new DateTime(2024, 7, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "OpenAI GPT-4.1",
                Description = "Advanced generalist model, text and code, successor to GPT-4/Turbo.",
                Link = "https://platform.openai.com/docs/models",
                Author = "OpenAI",
                DateRelease = new DateTime(2024, 6, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "Claude 3.5 Sonnet",
                Description = "Intermediate Claude 3.5 model, great speed/quality balance, strong in reasoning and code.",
                Link = "https://docs.anthropic.com/claude/docs/models",
                Author = "Anthropic",
                DateRelease = new DateTime(2024, 6, 1),
                ScoreIA = "N/A"
            },
            new LLM
            {
                Name = "OpenAI GPT-4o",
                Description = "Real-time multimodal model (text, image, audio) optimized for interactivity and cost.",
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
