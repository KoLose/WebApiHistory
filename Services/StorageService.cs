using System.Net.Http.Headers;

namespace WebApi.Services;

public static class StorageService
{
    // Вставь сюда свои данные из Supabase Dashboard -> Settings -> API
    private const string SupabaseUrl = "https://usumwizzaswjmiucesxo.supabase.co";
    private const string AnonKey = "sb_publishable_i18hag_-4MHiTIIucrSrlw_qCLq3lFf";

    public static async Task<string> UploadFileAsync(byte[] data, string filePath, string contentType)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(5);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseUrl}/storage/v1/object/storage/{filePath}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AnonKey);
        request.Content = new ByteArrayContent(data);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await client.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Storage Upload Failed: {error}");
        }

        return $"{SupabaseUrl}/storage/v1/object/public/storage/{filePath}";
    }
}