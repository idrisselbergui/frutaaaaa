  using System;

namespace frutaaaaa.Models
{
    public class EcartGroupDetailsDto
    {
        public string VergerName { get; set; }
        public string GroupVarieteName { get; set; }
        public string EcartType { get; set; }
        public int? GroupId { get; set; }
        public int? VergerId { get; set; }
        public int? EcartTypeId { get; set; }
        public double TotalPdsfru { get; set; }
        public int TotalNbrcai { get; set; }
        public DateTime? MinEcartDate { get; set; }
        public DateTime? MaxEcartDate { get; set; }
    }
}
