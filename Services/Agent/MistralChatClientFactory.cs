using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using VeilleNet.Models;

namespace VeilleNet.Services.Agent;

public interface IMistralChatClientFactory
{
    IChatClient? TryCreate();
}

public class MistralChatClientFactory : IMistralChatClientFactory
{
    private readonly MistralOptions _options;
    private IChatClient? _client;
    private readonly object _lock = new();
    private readonly ILogger<MistralChatClientFactory> _logger;

    public MistralChatClientFactory(IOptions<MistralOptions> options, ILogger<MistralChatClientFactory> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public IChatClient? TryCreate()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return null;
        }

        if (_client != null)
        {
            return _client;
        }

        lock (_lock)
        {
            if (_client != null)
            {
                return _client;
            }

            var endpoint = new Uri(_options.Endpoint.TrimEnd('/'));

            var mistralClient = new OpenAIChatClient(
                model: _options.Model,
                endpoint: endpoint,
                credential: _options.ApiKey,
                logger: _logger);

            _client = mistralClient;
            return _client;
        }
    }
}

// Helper class to create OpenAIChatClient for Mistral API
internal class OpenAIChatClient : IChatClient
{
    private readonly string _model;
    private readonly Uri _endpoint;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public OpenAIChatClient(string model, Uri endpoint, string credential, ILogger logger)
    {
        _model = model;
        _endpoint = endpoint;
        _apiKey = credential;
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _model,
            messages = messages.Select(m => new
            {
                role = m.Role.ToString().ToLower(),
                content = m.Text
            }),
            temperature = options?.Temperature ?? 0.7f,
            max_tokens = 1000 // Default value, can be configured
        };

        // Check if endpoint already ends with /, if not add it
        var baseUrl = _endpoint.ToString().TrimEnd('/');
        var requestUri = new Uri($"{baseUrl}/chat/completions");

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Mistral API request: POST {RequestUri} (model={Model}, messages={MessageCount})", requestUri, _model, messages.Count());
        }

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUri, jsonContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Mistral API error: {response.StatusCode}. Response: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonResponse = JsonDocument.Parse(responseContent);

        var responseText = jsonResponse.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;

        var chatMessage = new ChatMessage(ChatRole.Assistant, responseText);
        return new ChatResponse([chatMessage]);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Streaming not implemented");
    }

    public object? GetService(Type serviceType, object? provider = null)
    {
        return null;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}