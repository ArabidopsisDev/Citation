using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static Citation.Utils.Api.DeepSeekApi;

namespace Citation.Utils.Api;

/// <summary>
/// Provides methods for interacting with the DeepSeek API, including sending chat completion requests and streaming
/// chat responses.
/// </summary>
/// <remarks>This class manages authentication and HTTP communication with the DeepSeek API. It is intended to be
/// used as a disposable resource; callers should dispose of instances when they are no longer needed to release
/// underlying network resources. The class is not thread-safe; if used concurrently from multiple threads, callers
/// should implement their own synchronization.</remarks>
public partial class DeepSeekApi : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the DeepSeekApi class
    /// </summary>
    /// <param name="apiKey">The API key for authentication</param>
    /// <param name="baseUrl">The base URL of the DeepSeek API (optional)</param>
    /// <param name="timeout">Request timeout in seconds (default: 30)</param>
    public DeepSeekApi(string apiKey, string baseUrl = "https://api.deepseek.com", int timeout = 3000)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

        _apiKey = apiKey;
        _baseUrl = baseUrl.TrimEnd('/');

        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Sends a chat completion request to the DeepSeek API
    /// </summary>
    /// <param name="request">The chat completion request parameters</param>
    /// <returns>A ChatCompletionResponse containing the API response</returns>
    /// <exception cref="DeepSeekApiException">Thrown when the API request fails</exception>
    public async Task<ChatCompletion?> CreateChatCompletionAsync(ChatCompletionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Messages == null || !request.Messages.Any())
            throw new ArgumentException("Messages cannot be null or empty", nameof(request.Messages));

        var url = $"{_baseUrl}/chat/completions";
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using (var response = await _httpClient.PostAsync(url, content))
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new DeepSeekApiException(
                    $"API request failed with status code {response.StatusCode}: {responseContent}",
                    response.StatusCode,
                    responseContent
                );
            }

            return JsonSerializer.Deserialize<ChatCompletion>(responseContent);
        }
    }

    /// <summary>
    /// Streams chat completion responses from the DeepSeek API
    /// </summary>
    /// <param name="request">The chat completion request parameters</param>
    /// <param name="onChunkReceived">Callback invoked when a new chunk is received</param>
    /// <returns>A task representing the streaming operation</returns>
    public async Task StreamChatCompletionAsync(ChatCompletionRequest request, Action<ChatCompletionChunk> onChunkReceived)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (onChunkReceived == null)
            throw new ArgumentNullException(nameof(onChunkReceived));

        request.Stream = true;

        var url = $"{_baseUrl}/chat/completions";
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new DeepSeekApiException(
                $"API request failed with status code {response.StatusCode}: {errorContent}",
                response.StatusCode,
                errorContent
            );
        }

        using (var stream = await response.Content.ReadAsStreamAsync())
        using (var reader = new System.IO.StreamReader(stream))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: ") && line != "data: [DONE]")
                {
                    var jsonData = line.Substring(6);
                    var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(jsonData);
                    onChunkReceived(chunk);
                }
            }
        }
    }

    /// <summary>
    /// Disposes the HttpClient instance
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents a chat completion response
/// </summary>
public class ChatCompletionResponse
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public long Created { get; set; }
    public string? Model { get; set; }
    public List<Choice>? Choices { get; set; }
    public Usage? Usage { get; set; }
}

/// <summary>
/// Represents a streaming chat completion chunk
/// </summary>
public class ChatCompletionChunk
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public long Created { get; set; }
    public string? Model { get; set; }
    public List<ChunkChoice>? Choices { get; set; }
}

/// <summary>
/// Represents a choice in a streaming chunk
/// </summary>
public class ChunkChoice
{
    public int Index { get; set; }
    public Delta? Delta { get; set; }
    public string? FinishReason { get; set; }
}

/// <summary>
/// Represents a delta in a streaming response
/// </summary>
public class Delta
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

/// <summary>
/// Represents a response containing available models
/// </summary>
public class ModelsResponse
{
    public string? Object { get; set; }
    public List<Model>? Data { get; set; }
}

/// <summary>
/// Represents an AI model
/// </summary>
public class Model
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public long Created { get; set; }
    public string? OwnedBy { get; set; }
}

/// <summary>
/// Exception thrown when the DeepSeek API returns an error
/// </summary>
public class DeepSeekApiException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }
    public string ResponseContent { get; }

    public DeepSeekApiException(string message, System.Net.HttpStatusCode statusCode, string responseContent)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}