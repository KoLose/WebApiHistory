namespace WebApi.Requests;

public class PatchAvatarRequest
{
    public Guid UserId { get; set; }
    public IFormFile AvatarFile { get; set; }
}