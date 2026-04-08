using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Requests;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImagesService _imagesService;

    public ImageController(IImagesService imagesService)
    {
        _imagesService = imagesService;
    }

    [HttpGet("GetAllImages")]
    public Task<IActionResult> GetAllImagesAsync()
    {
        return _imagesService.GetAllImagesAsync();
    }

    [HttpPost("PostImage")]
    public Task<IActionResult> PostImageAsync([FromBody] CreateNewImage newImage)
    {
        return _imagesService.PostImageAsync(newImage);
    }
}
