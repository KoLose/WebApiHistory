using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Models;
using WebApi.Models.Response;
using WebApi.Requests;

namespace WebApi.Services;

public class ImageService : IImagesService
{
    private readonly Supabase.Client _supabaseClient;

    public ImageService(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<IActionResult> GetAllImagesAsync()
    {
        await _supabaseClient.InitializeAsync();
        var imageResponse = await _supabaseClient.From<Image>().Get();

        var data = imageResponse.Models.Select(img => new ImageResponse
        {
            ImageUrl = img.ImageUrl,
            ExcelUrl = img.ExcelUrl
        }).ToList();
        
        return new OkObjectResult(new
        {
            data = new { image = data },
            status = true
        });
    }

    public async Task<IActionResult> PostImageAsync(CreateNewImage newImage)
    {
        try
        {
            if (newImage.File == null || newImage.File.Length == 0)
                return new BadRequestObjectResult(new { status = false, message = "Файл не передан" });

            await _supabaseClient.InitializeAsync();

            var ext = Path.GetExtension(newImage.File.FileName);
            var path = $"images/{Guid.NewGuid()}{ext}";
            using var stream = newImage.File.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            await _supabaseClient.Storage.From("Storage").Upload(memoryStream.ToArray(), path);

            var url = $"https://bccvmwlqehhsbldanwao.supabase.co/storage/v1/object/public/Storage/{path}";

            Random rnd = new Random();
            int id = rnd.Next(0, 1_000_000);
                
            var image = new Image
            {
                Id = id,
                ImageUrl = url
            };

            await _supabaseClient.From<Image>().Insert(image);

            return new OkObjectResult(new
            {
                status = true
            });
        }
        catch (Exception e)
        {   
            return new ObjectResult(new 
            { 
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
