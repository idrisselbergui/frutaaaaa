using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("charges")]
    public class Charge
    {
        [Key]
        [Column("idcharge")]
        public int Idcharge { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [Column("typecharge")]
        public string Typecharge { get; set; }
    }
}
