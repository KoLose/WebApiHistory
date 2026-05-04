namespace WebApi.Models.Response;

public class UserResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Mail { get; set; }
    public string AvatarUrl { get; set; }
    public string RoleName { get; set; }
}