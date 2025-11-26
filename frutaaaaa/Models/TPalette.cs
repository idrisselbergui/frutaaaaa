using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("tpalette")]
    public class TPalette
    {
        [Key]
        [Column("codtyp")]
        [MaxLength(20)]
        public string codtyp { get; set; } = string.Empty;

        [Column("modtrp")]
        [MaxLength(2)]
        public string? modtrp { get; set; }

        [Column("natpal")]
        [MaxLength(2)]
        public string? natpal { get; set; }

        [Column("codmar")]
        public int? codmar { get; set; }

        [Column("codvar")]
        public short? codvar { get; set; }

        [Column("codgrp")]
        public int? codgrp { get; set; }

        [Column("indice")]
        [MaxLength(1)]
        public string? indice { get; set; }

        [Column("pdcomc", TypeName = "decimal(16,2)")]
        public decimal? pdcomc { get; set; }

        [Column("pdproc", TypeName = "decimal(16,2)")]
        public decimal? pdproc { get; set; }

        [Column("pdnetc", TypeName = "decimal(16,2)")]
        public decimal? pdnetc { get; set; }

        [Column("prxemb", TypeName = "decimal(16,4)")]
        public decimal? prxemb { get; set; }

        [Column("prxcon", TypeName = "decimal(16,4)")]
        public decimal? prxcon { get; set; }

        [Column("prxpal", TypeName = "decimal(16,4)")]
        public decimal? prxpal { get; set; }

        [Column("tarcol", TypeName = "decimal(16,2)")]
        public decimal? tarcol { get; set; }

        [Column("nomemb")]
        [MaxLength(40)]
        public string? nomemb { get; set; }

        [Column("pdsfil", TypeName = "decimal(16,2)")]
        public decimal? pdsfil { get; set; }

        [Column("nbrfil")]
        public int? nbrfil { get; set; }

        [Column("tarpal", TypeName = "decimal(16,2)")]
        public decimal? tarpal { get; set; }

        [Column("lier")]
        [MaxLength(1)]
        public string? lier { get; set; }

        [Column("codebar")]
        [MaxLength(25)]
        public string? codebar { get; set; }

        [Column("nbrbar")]
        public int? nbrbar { get; set; }

        [Column("pdsbar")]
        public double? pdsbar { get; set; }

        [Column("codbarq")]
        public int? codbarq { get; set; }
    }
}