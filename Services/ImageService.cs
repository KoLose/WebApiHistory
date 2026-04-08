using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Models;
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
        var imagesResponse = await _supabaseClient.From<Image>().Get();

        return new OkObjectResult(new
        {
            data = new { images = imagesResponse.Models },
            status = true
        });
    }

    public async Task<IActionResult> PostImageAsync(CreateNewImage newImage)
    {
        await _supabaseClient.InitializeAsync();

        var image = new Image
        {
            Name = newImage.Name,
            ImageUrl = newImage.ImageUrl
        };

        await _supabaseClient.From<Image>().Insert(image);

        return new OkObjectResult(new
        {
            status = true
        });
    }
}
