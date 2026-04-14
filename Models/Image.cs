using Postgrest.Attributes;
using Postgrest.Models;

namespace WebApi.Models;

[Table("image")]
public class Image : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    public int Id { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }
    
    [Column("excel_url")]
    public string ExcelUrl { get; set; }
    
}