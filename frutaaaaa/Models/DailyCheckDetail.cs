using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace frutaaaaa.Models
{
    [Table("daily_check_detail")]
    public class DailyCheckDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("daily_check_id")]
        public int DailyCheckId { get; set; }

        [Column("defect_id")]
        public int DefectId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        // Navigation property - ignored during serialization to prevent circular reference
        [ForeignKey("DailyCheckId")]
        [JsonIgnore]
        public virtual DailyCheck DailyCheck { get; set; }
    }
}
