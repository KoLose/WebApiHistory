using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Requests;
using WebApi.Services;

namespace WebApi.Controllers;

public class ImageController: ControllerBase    
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
    
    [HttpGet("test-yandex")]
    public async Task<IActionResult> TestYandex([FromServices] YandexService service)
    {
        var result = await service.SendRequestAsync();
        return Ok(new { answer = result });
    }
}
