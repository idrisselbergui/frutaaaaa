using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("daily_check")]
    public class DailyCheck
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("sample_test_id")]
        public int SampleTestId { get; set; }

        [Column("check_date")]
        public DateTime CheckDate { get; set; }

        [Column("pdsfru")]
        public double Pdsfru { get; set; }

        [Column("coulour1")]
        public int Couleur1 { get; set; }

        [Column("coulour2")]
        public int Couleur2 { get; set; }

        // Navigation properties
        [ForeignKey("SampleTestId")]
        public virtual SampleTest SampleTest { get; set; }

        public virtual ICollection<DailyCheckDetail> Details { get; set; } = new List<DailyCheckDetail>();
    }
}
