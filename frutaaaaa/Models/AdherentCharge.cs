using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("adherent_charges")]
    public class AdherentCharge
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("refadh")]
        public int Refadh { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("idcharge")]
        public int Idcharge { get; set; }

        [Column("montant")]
        public double Montant { get; set; }
    }
}
