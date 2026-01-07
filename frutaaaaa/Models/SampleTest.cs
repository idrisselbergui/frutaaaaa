using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    public enum SampleTestStatus
    {
        Active,
        Closed
    }

    [Table("sample_test")]
    public class SampleTest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("numrec")]
        public int Numrec { get; set; }

        [Column("coddes")]
        public short? Coddes { get; set; }

        [Column("codvar")]
        public short? Codvar { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("initial_fruit_count")]
        public int InitialFruitCount { get; set; }

        [Column("status")]
        public SampleTestStatus Status { get; set; }
    }
}
