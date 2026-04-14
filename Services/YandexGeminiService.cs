using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApi.Services;

public class YandexGeminiService
{
    private readonly string _apiKey = "AQVNz7QqYoi38pUxglCPo9YisW9v2_3voRzdsUwU";
    private readonly string _folderId = "b1gfs6kl8fehv0414o0g";
    
    private const string ModelName = "gemma-3-27b-it/latest"; 

    public async Task<string> RecognizeTextAsync(string base64ImageWithPrefix, string promptText)
    {
        var endpoint = "https://ai.api.cloud.yandex.net/v1/chat/completions";

        var requestBody = new
        {
            model = $"gpt://{_folderId}/{ModelName}",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = promptText },
                        new 
                        { 
                            type = "image_url", 
                            image_url = new { url = base64ImageWithPrefix }
                        }
                    }
                }
            },
            temperature = 0,
            max_tokens = 1000
        };

        var json = JsonSerializer.Serialize(requestBody);
        
        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.StatusCode} - {responseBody}";
            }
            
            var jsonDoc = JsonDocument.Parse(responseBody);
            string content = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
            
            string cleanJson = Regex.Replace(content, @"^```json\s*|\s*```$", "", RegexOptions.Multiline).Trim();
            
            return cleanJson;
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}