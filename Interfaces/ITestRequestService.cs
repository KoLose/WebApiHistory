using Microsoft.AspNetCore.Mvc;
using WebApi.Requests;

namespace WebApi.Interfaces;

public interface ITestRequestService
{
    public Task<IActionResult> PostRequestAsync(CreateNewRequest newRequest);
    public Task<IActionResult> GetRequestsAsync(Guid userId);
}