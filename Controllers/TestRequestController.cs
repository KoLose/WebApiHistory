using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Requests;

namespace WebApi.Controllers;

public class TestRequestController: ControllerBase
{
    private readonly ITestRequestService _testRequestService;
    
    public TestRequestController(ITestRequestService testRequestService)
    {
        _testRequestService = testRequestService;
    }

    [HttpPost]
    [Route("PostTestRequest")]
    public async Task<IActionResult> TestPostRequest([FromForm] CreateNewRequest newRequest)
    {
        return await _testRequestService.PostRequestAsync(newRequest);
    }
    [HttpGet("GetAllImages")]
    public async Task<IActionResult> GetAllImages([FromQuery] Guid userId)
    {
        return await _testRequestService.GetRequestsAsync(userId);
    }
}