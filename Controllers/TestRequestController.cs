using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Requests;

namespace WebApi.Controllers;

public class TestRequestController
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
        return await _testRequestService.GetAndSaveImageAsync(newRequest);
    }
}