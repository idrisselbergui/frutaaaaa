using System;
using System.ComponentModel.DataAnnotations;

namespace frutaaaaa.Models
{
    public class Entreprise
    {
        [Key]
        public string refent { get; set; }

        public string? noment { get; set; }

        public short? refsta1 { get; set; }

        public short? refsta2 { get; set; }

        public string? type { get; set; }

        public string? domaine { get; set; }

        public string? rueent { get; set; }

        public string? vilent { get; set; }

        public string? payent { get; set; }

        public string? cpent { get; set; }

        public string? telent { get; set; }

        public string? faxent { get; set; }

        public string? repent { get; set; }

        public string? logent { get; set; }

        public decimal? tarpal { get; set; }

        public string? lier { get; set; }

        public string? typtrs { get; set; }

        public string? camp { get; set; }

        public char? typsta { get; set; }

        public uint? numde { get; set; }

        public uint? numau { get; set; }

        public DateTime? dtDebut { get; set; }

        public DateTime? dtFin { get; set; }

        public double? pmoyOuv { get; set; }

        public double? pmoyPerm { get; set; }
    }
}
