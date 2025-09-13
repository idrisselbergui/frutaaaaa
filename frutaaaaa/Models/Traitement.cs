using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("traitement")]
    public class Traitement
    {
        [Key]
        [Column("numtrait")]
        public int Numtrait { get; set; }

        [Column("refver")]
        public int? Refver { get; set; }

        [Column("ref")]
        public int? Ref { get; set; }

        [Column("codgrp")]
        public int? Codgrp { get; set; }
        [Column("codvar")]
        public int? Codvar { get; set; }

        [Column("dateappli")]
        public DateTime? Dateappli { get; set; }

        [Column("dateprecolte")]
        public DateTime? Dateprecolte { get; set; }
    }
}
