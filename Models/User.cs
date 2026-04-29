using Postgrest.Attributes;
using Postgrest.Models;

namespace WebApi.Models;

[Table("User")]
public class User: BaseModel
{
    [PrimaryKey("user_id", false)]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("user_name")]
    public string UserName { get; set; }
    
    [Column("mail")]
    public string Mail { get; set; }
    
    [Column("password")]
    public string Password { get; set; }
    
    [Column("role_id")]
    public int RoleId { get; set; }
}