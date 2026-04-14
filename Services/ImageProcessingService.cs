using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Text;

namespace WebApi.Services;

public class ImageProcessingService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly YandexGeminiService _yandexService;

    public ImageProcessingService(Supabase.Client supabaseClient, YandexGeminiService yandexService)
    {
        _supabaseClient = supabaseClient;
        _yandexService = yandexService;
    }

    public async Task<IActionResult> ProcessAndSaveImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return new BadRequestObjectResult(new { status = false, message = "Файл не передан" });

            await _supabaseClient.InitializeAsync();
            
            var ext = Path.GetExtension(file.FileName);
            var path = $"images/{Guid.NewGuid()}{ext}";
            
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            await _supabaseClient.Storage.From("Storage").Upload(fileBytes, path);

            var url = $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/Storage/{path}";
            
            string base64WithPrefix = $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}";
            
            string prompt = "Напиши одно слово: УСПЕХ"; 
            
            string aiResponse = await _yandexService.RecognizeTextAsync(base64WithPrefix, prompt);
            
            Random rnd = new Random();
            int id = rnd.Next(0, 1_000_000);
                
            var image = new Image
            {
                Id = id,
                ImageUrl = url,
                ExcelUrl = aiResponse
            };

            await _supabaseClient.From<Image>().Insert(image);

            return new OkObjectResult(new { status = true });
        }
        catch (Exception e)
        {   
            return new ObjectResult(new 
            { 
                status = false,
                error = e.Message, 
                inner = e.InnerException?.Message,
                stack = e.StackTrace?.Split('\n').FirstOrDefault() 
            }) 
            { 
                StatusCode = 500 
            };
        }
    }
}