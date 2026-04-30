using Postgrest.Attributes;
using Postgrest.Models;

namespace WebApi.Models;

[Table("Request")]
public class Request: BaseModel
{
    [PrimaryKey("request_id", false)]
    [Column("request_id")]
    public Guid Request_id { get; set; }
    
    [Column("image_url")]
    public string ImageUrl { get; set; }
    
    [Column("excel_url")]
    public string ExcelUrl { get; set; }
    
    [Column("user_id")]
    public Guid UserId { get; set; }
}