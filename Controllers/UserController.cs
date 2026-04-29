using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Requests;

namespace WebApi.Controllers;

public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Route("GetUser")]
    public async Task<IActionResult> GetUserAsync()
    {
        return await _userService.GetUserAsync();
    }

    [HttpPost]
    [Route("PostUser")]
    public Task<IActionResult> PostUserAsync([FromBody] CreateNewUser user)
    {
        return _userService.PostUserAsync(user);
    }

    [HttpPatch]
    [Route("PatchUserRole")]
    public async Task<IActionResult> PatchUserAsync([FromBody] UpdateUser updateUser)
    {

        return await _userService.PatchUserAsync(updateUser);
    }

    [HttpDelete]
    [Route("DeleteUserRole")]
    public async Task<IActionResult> DeleteUserAsync([FromBody] DeleteUser deleteUser)
    {
        return await _userService.DeleteUserAsync(deleteUser);
    }
}