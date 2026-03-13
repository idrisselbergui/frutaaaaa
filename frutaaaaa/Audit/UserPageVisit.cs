using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Audit
{
    [Table("user_page_visits")]
    public class UserPageVisit
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("session_id")]
        public long SessionId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("username")]
        [MaxLength(100)]
        public string? Username { get; set; }

        [Column("page_path")]
        [MaxLength(255)]
        public string? PagePath { get; set; }

        [Column("entered_at")]
        public DateTime EnteredAt { get; set; }

        [Column("left_at")]
        public DateTime? LeftAt { get; set; }

        [Column("time_spent")]
        public int? TimeSpent { get; set; } // seconds

        [Column("tenant_database")]
        [MaxLength(100)]
        public string? TenantDatabase { get; set; }

        // Navigation property
        [ForeignKey("SessionId")]
        public UserSession? Session { get; set; }
    }
}
