using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("prix_estimatifs")]
    public class PrixEstimatif
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("annee")]
        public int Annee { get; set; }

        [Column("mois")]
        public int Mois { get; set; }

        [Column("codgrv")]
        public int CodGrv { get; set; }

        [Column("prix_estime")]
        public double PrixEstime { get; set; }
    }
}
