using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Audit
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("username")]
        [MaxLength(100)]
        public string? Username { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("machine_name")]
        [MaxLength(255)]
        public string? MachineName { get; set; }

        [Column("browser")]
        [MaxLength(255)]
        public string? Browser { get; set; }

        [Column("os")]
        [MaxLength(255)]
        public string? Os { get; set; }

        [Column("action_type")]
        [MaxLength(10)]
        public string ActionType { get; set; } = null!; // "INSERT", "UPDATE", or "DELETE"

        [Column("entity_name")]
        [MaxLength(100)]
        public string EntityName { get; set; } = null!;

        [Column("entity_id")]
        [MaxLength(100)]
        public string? EntityId { get; set; }

        [Column("endpoint")]
        [MaxLength(500)]
        public string? Endpoint { get; set; }

        [Column("http_method")]
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        [Column("old_values")]
        public string? OldValues { get; set; } // JSON

        [Column("new_values")]
        public string? NewValues { get; set; } // JSON

        [Column("tenant_database")]
        [MaxLength(100)]
        public string? TenantDatabase { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
