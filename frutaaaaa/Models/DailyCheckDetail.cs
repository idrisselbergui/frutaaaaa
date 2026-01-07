using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    public enum DefectType
    {
        Rot,
        Mold,
        Soft
    }

    [Table("daily_check_detail")]
    public class DailyCheckDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("daily_check_id")]
        public int DailyCheckId { get; set; }

        [Column("defect_type")]
        public DefectType DefectType { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        // Navigation property
        [ForeignKey("DailyCheckId")]
        public virtual DailyCheck DailyCheck { get; set; }
    }
}
