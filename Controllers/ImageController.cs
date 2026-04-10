using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Requests;

namespace WebApi.Controllers;

public class ImageController
{
    private readonly IImagesService _imagesService;

    public ImageController(IImagesService imagesService)
    {
        _imagesService = imagesService;
    }

    [HttpGet]
    [Route("GetAllImages")]
    public Task<IActionResult> GetAllImagesAsync()
    {
        return _imagesService.GetAllImagesAsync();
    }

    [HttpPost]
    [Route("PostImage")]
    public Task<IActionResult> PostImageAsync([FromForm] CreateNewImage newImage)
    {
        return _imagesService.PostImageAsync(newImage);
    }
}
