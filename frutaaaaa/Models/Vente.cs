using System;

namespace frutaaaaa.Models
{
    public class Vente
    {
        public int Id { get; set; }
        public int? Numbonvente { get; set; }
        public DateTime Date { get; set; }
        public double Price { get; set; }
        public double PoidsTotal { get; set; }
        public double MontantTotal { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
