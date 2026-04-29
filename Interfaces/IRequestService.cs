using Microsoft.AspNetCore.Mvc;
using WebApi.Requests;

namespace WebApi.Interfaces;

public interface IRequestService
{
    public Task<IActionResult> GetAndSaveImageAsync(CreateNewRequest newRequest);
    public Task<string> SendRequest(string textRequest, string prompt);
}