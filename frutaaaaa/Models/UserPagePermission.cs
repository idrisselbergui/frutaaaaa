using frutaaaaa.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ... other using ...

[Table("user_page_permissions")]
public class UserPagePermission
{
    // Properties as before...
    [Key]  // Remove if using HasKey in OnModelCreating
    [Column("user_id", Order = 0)]
    public int UserId { get; set; }

    [Key]  // Remove if using HasKey
    [Column("page_name", Order = 1)]
    [StringLength(100)]
    public string PageName { get; set; } = string.Empty;

    [Column("allowed")]
    public bool Allowed { get; set; } = false;

    public User? User { get; set; }
}
