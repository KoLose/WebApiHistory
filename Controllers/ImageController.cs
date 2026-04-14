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

    [HttpGet("test2-yandex")]
    public async Task<IActionResult> Test2Yandex([FromServices] YandexTestService service)
    {
        var result = await service.SendTestRequestAsync();
        return Ok(new { answer = result });
    }
    
    [HttpPost("recognize-text")]
    public async Task<IActionResult> RecognizeText(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не передан");

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();
            
            string mimeType = "image/jpeg";
            string base64String = Convert.ToBase64String(bytes);
            string base64WithPrefix = $"data:{mimeType};base64,{base64String}";
            
            string prompt = "Распознай текст на изображении и верни его в формате JSON как: ";
            
            var service = new YandexGeminiService(); 
            string result = await service.RecognizeTextAsync(base64WithPrefix, prompt);

            return Ok(new { data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
