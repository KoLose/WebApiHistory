using Microsoft.AspNetCore.Mvc;
using WebApi.Requests;

namespace WebApi.Interfaces;

public interface IUserService
{
    Task<IActionResult> GetUserAsync();
    Task<IActionResult> PostUserAsync(CreateNewUser newUser);
    Task<IActionResult> PatchUserAsync(UpdateUser updateUser);
    Task<IActionResult> DeleteUserAsync(DeleteUser deleteUser);
}