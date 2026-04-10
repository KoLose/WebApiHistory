using Postgrest.Attributes;
using Postgrest.Models;

namespace WebApi.Models;

[Table("image")]
public class Image : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }
}