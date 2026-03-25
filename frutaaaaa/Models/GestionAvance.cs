using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("gestionavances")]
    public class GestionAvance
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("refadh")]
        public int? Refadh { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }
        [Column("annee")]
        public int? Annee { get; set; }

        [Column("mois")]
        public int? Mois { get; set; }

        [Column("ttdecompte")]
        public double? Ttdecompte { get; set; }

        [Column("ttcharges")]
        public double? Ttcharges { get; set; }

        [Column("tgExport")]
        public double? TgExport { get; set; }

        [Column("prix_esteme_mois")]
        public double? PrixEstemeMois { get; set; }

        [Column("decaompte_esteme")]
        public double? DecaompteEsteme { get; set; }

        [Column("s1")]
        public double? S1 { get; set; }

        [Column("s2")]
        public double? S2 { get; set; }

        [Column("s3")]
        public double? S3 { get; set; }

        [Column("s4")]
        public double? S4 { get; set; }

        [Column("s5")]
        public double? S5 { get; set; }

        [Column("real_t_s1")]
        public double? RealTS1 { get; set; }

        [Column("real_t_s2")]
        public double? RealTS2 { get; set; }

        [Column("real_t_s3")]
        public double? RealTS3 { get; set; }

        [Column("real_t_s4")]
        public double? RealTS4 { get; set; }

        [Column("real_t_s5")]
        public double? RealTS5 { get; set; }

        [Column("real_dec_s1")]
        public double? RealDecS1 { get; set; }

        [Column("real_dec_s2")]
        public double? RealDecS2 { get; set; }

        [Column("real_dec_s3")]
        public double? RealDecS3 { get; set; }

        [Column("real_dec_s4")]
        public double? RealDecS4 { get; set; }

        [Column("real_dec_s5")]
        public double? RealDecS5 { get; set; }

        [Column("montant")]
        public double? Montant { get; set; }

        // Navigation property — variety-level detail rows
        public virtual ICollection<GestionAvanceDetail> Details { get; set; }
    }
}
