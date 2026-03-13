using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Audit
{
    [Table("user_sessions")]
    public class UserSession
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("username")]
        [MaxLength(100)]
        public string? Username { get; set; }

        [Column("tenant_database")]
        [MaxLength(100)]
        public string? TenantDatabase { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("country")]
        [MaxLength(100)]
        public string? Country { get; set; }

        [Column("city")]
        [MaxLength(100)]
        public string? City { get; set; }

        [Column("machine_name")]
        [MaxLength(255)]
        public string? MachineName { get; set; }

        [Column("browser")]
        [MaxLength(255)]
        public string? Browser { get; set; }

        [Column("os")]
        [MaxLength(255)]
        public string? Os { get; set; }

        [Column("login_at")]
        public DateTime LoginAt { get; set; }

        [Column("logout_at")]
        public DateTime? LogoutAt { get; set; }

        [Column("session_duration")]
        public int? SessionDuration { get; set; } // seconds

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, LOGGED_OUT, TAB_CLOSED, EXPIRED

        [Column("failed_attempts")]
        public int FailedAttempts { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
