using System;
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

        // Navigation property
        [ForeignKey("SampleTestId")]
        public virtual SampleTest SampleTest { get; set; }

        // Calculated property
        public int DayNumber => (CheckDate.Date - SampleTest?.StartDate.Date)?.Days + 1 ?? 0;
    }
}
