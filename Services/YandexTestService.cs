using System.Text;
using System.Text.Json;

namespace WebApi.Services;

public class YandexTestService
{
    public async Task<string> SendTestRequestAsync()
    {
        string apiKey = "AQVNz7QqYoi38pUxglCPo9YisW9v2_3voRzdsUwU";
        string folderId = "b1gfs6kl8fehv0414o0g";
        string promptId = "fvtnganr509g1btptrev";

        var requestBody = new
        {
            prompt = new
            {
                id = promptId
            },
            input = "some message"
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        
        using var httpClient = new HttpClient();
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://ai.api.cloud.yandex.net/v1/responses");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        request.Headers.Add("Authorization", $"Api-Key {apiKey}");
        request.Headers.Add("OpenAI-Project", folderId);

        try 
        {
            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("output", out var output) &&
                    output.GetArrayLength() > 0 &&
                    output[0].TryGetProperty("content", out var contentArray) &&
                    contentArray.GetArrayLength() > 0 &&
                    contentArray[0].TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? "Empty text";
                }
                return "Error: Text field not found in response";
            }
            else
            {
                return $"Error: {response.StatusCode} - {responseBody}";
            }
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}