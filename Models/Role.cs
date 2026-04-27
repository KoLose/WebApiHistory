using Postgrest.Attributes;
using Postgrest.Models;

namespace WebApi.Models;

[Table("Role")]
public class Role: BaseModel
{
    [PrimaryKey("role_id", false)]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("role_name")]
    public string RoleName { get; set; }
}