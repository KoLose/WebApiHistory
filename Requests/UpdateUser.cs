namespace WebApi.Requests;

public class UpdateUser
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Mail { get; set; }
    public string Password { get; set; }
}