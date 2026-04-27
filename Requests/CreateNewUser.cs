namespace WebApi.Requests;

public class CreateNewUser
{
    public string UserName { get; set; }
    public string Mail { get; set; }
    public string Password { get; set; }
    public string Action { get; set; }
}