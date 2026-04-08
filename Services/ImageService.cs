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
            Id = img.Id,
            Name = img.Name,
            ImageUrl = img.ImageUrl
        }).ToList();
        
        return new OkObjectResult(new
        {
            data = new { image = data },
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
