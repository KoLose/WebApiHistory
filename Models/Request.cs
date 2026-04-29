using Postgrest.Attributes;
using Postgrest.Models;

namespace WebApi.Models;

[Table("image")]
public class Request: BaseModel
{
    [PrimaryKey("request_id", false)]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("image_url")]
    public string ImageUrl { get; set; }
    
    [Column("excel_url")]
    public string ExcelUrl { get; set; }
    
    [Column("user_id")]
    public Guid UserId { get; set; }
}