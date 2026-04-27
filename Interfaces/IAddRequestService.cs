using Microsoft.AspNetCore.Mvc;

namespace WebApi.Interfaces;

public interface IAddRequestService
{
    Task<IActionResult> GetImage(IFormFile file);
    Task<IActionResult> Image(string url);
    Task<IActionResult> CreateExcel(string json);
    Task<IActionResult> SaveRequest(string json, string url);
}