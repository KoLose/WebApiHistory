using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebApi.Services;

public class YandexService
{
    private readonly string _folderId;
    private readonly string _apiKey;
    
    private const string ModelName = "yandexgpt/latest"; 

    public YandexService(string folderId, string apiKey)
    {
        _folderId = folderId;
        _apiKey = apiKey;
    }

    public async Task<string> SendRequestAsync(string prompt = "Hi")
    {
        var modelUri = $"gpt://{_folderId}/{ModelName}";
        
        var endpoint = "https://llm.api.cloud.yandex.net/foundationModels/v1/completion";

        var requestBody = new
        {
            modelUri = modelUri,
            completionOptions = new { stream = false, temperature = 0, maxTokens = 6000 },
            messages = new[] 
            { 
                new { role = "user", text = prompt } 
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Api-Key", _apiKey);

        try
        {
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"ERROR {response.StatusCode}: {content}";
            }

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement
                      .GetProperty("result")
                      .GetProperty("alternatives")[0]
                      .GetProperty("message")
                      .GetProperty("text")
                      .GetString() ?? "No text";
        }
        catch (Exception ex)
        {
            return $"EXCEPTION: {ex.Message}";
        }
    }
}