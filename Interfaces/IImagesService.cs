using Microsoft.AspNetCore.Mvc;
using WebApi.Requests;

namespace WebApi.Interfaces;

public interface IImagesService
{
    Task<IActionResult> GetAllImagesAsync();
    Task<IActionResult> PostImageAsync(CreateNewImage request);
}
