using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("gestionavance_details")]
    public class GestionAvanceDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("gestion_avance_id")]
        public int GestionAvanceId { get; set; }

        [Column("codgrv")]
        public int CodGrv { get; set; }

        [Column("nom_grv")]
        public string NomGrv { get; set; }

        [Column("prix_estime")] public double PrixEstime { get; set; }

        [Column("ts1")] public double TS1 { get; set; }
        [Column("ts2")] public double TS2 { get; set; }
        [Column("ts3")] public double TS3 { get; set; }
        [Column("ts4")] public double TS4 { get; set; }
        [Column("ts5")] public double TS5 { get; set; }

        [Column("decs1")] public double DecS1 { get; set; }
        [Column("decs2")] public double DecS2 { get; set; }
        [Column("decs3")] public double DecS3 { get; set; }
        [Column("decs4")] public double DecS4 { get; set; }
        [Column("decs5")] public double DecS5 { get; set; }
    }
}
