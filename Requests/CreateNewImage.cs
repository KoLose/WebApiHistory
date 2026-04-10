namespace WebApi.Requests;

public class CreateNewImage
{
    public string Name { get; set; }
    public IFormFile File { get; set; }
}
