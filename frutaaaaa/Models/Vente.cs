using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    public class Vente
    {
        public int Id { get; set; }
        public int? Numbonvente { get; set; }
        [Column("date_vente")]
        public DateTime Date { get; set; }
        public double Price { get; set; }
        [Column("poids_total")]
        public double PoidsTotal { get; set; }
        [Column("montant_total")]
        public double MontantTotal { get; set; }
        [Column("numlot")]  // optional, to specify column name
        public int? Numlot { get; set; }

        [Column("codtype")]
        public int? Codtype { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
