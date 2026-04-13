using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

public class YandexTestController : ControllerBase
{
    private readonly YandexTestService _service;
    
    public YandexTestController(YandexTestService service)
    {
        _service = service;
    }
    
    [HttpGet("test2-yandex")]
    public async Task<IActionResult> Test2Yandex([FromServices] YandexTestService service)
    {
        var result = await _service.SendTestRequestAsync();
        return Ok(new { data = result });
    }
}