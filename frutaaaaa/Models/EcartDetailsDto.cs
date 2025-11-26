using System;

namespace frutaaaaa.Models
{
    public class EcartDetailsDto
    {
        public string? VergerName { get; set; }
        public string? VarieteName { get; set; }
        public string? EcartType { get; set; }
        public decimal TotalPdsfru { get; set; }
        public int TotalNbrcai { get; set; }
        public DateTime? MinEcartDate { get; set; }
        public DateTime? MaxEcartDate { get; set; }

        // Add these for date calculations
        public int? VergerId { get; set; }
        public int? VarieteId { get; set; }
        public int? EcartTypeId { get; set; }
    }
}
