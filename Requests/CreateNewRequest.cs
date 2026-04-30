namespace WebApi.Requests;

public class CreateNewRequest
{
    public IFormFile File { get; set; }
    public Guid UserId { get; set; }
}