using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

public class ImageAIController: ControllerBase
{
    private readonly ImageProcessingService _processingService;
    
    public ImageAIController(ImageProcessingService processingService)
    {
        _processingService = processingService;
    }
    
    [HttpPost("ProcessImage")]
    public async Task<IActionResult> ProcessImage(IFormFile file)
    {
        return await _processingService.ProcessAndSaveImageAsync(file);
    }
}