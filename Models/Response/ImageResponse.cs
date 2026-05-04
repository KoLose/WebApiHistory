
namespace WebApi.Models.Response;

public class ImageResponse
{
    public Guid UserId { get; set; }
    public string ImageUrl { get; set; }
    public string ExcelUrl { get; set; }
}