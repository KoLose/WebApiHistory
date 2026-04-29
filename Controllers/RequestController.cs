using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Requests;

namespace WebApi.Controllers;

public class RequestController: ControllerBase
{
    private readonly IRequestService _requestService;
    
    public RequestController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    [Route("PostRequest")]
    public async Task<IActionResult> PostRequest(CreateNewRequest request)
    {
        return await _requestService.GetAndSaveImageAsync(request);
    }
    
    
}