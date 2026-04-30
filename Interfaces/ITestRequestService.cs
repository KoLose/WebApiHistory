using Microsoft.AspNetCore.Mvc;
using WebApi.Requests;

namespace WebApi.Interfaces;

public interface ITestRequestService
{
    public Task<IActionResult> GetAndSaveImageAsync(CreateNewRequest newRequest);
}