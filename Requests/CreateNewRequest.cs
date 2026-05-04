namespace WebApi.Requests;

public class CreateNewRequest
{
    public IFormFile FileUser { get; set; }
    public Guid UserId { get; set; }
}