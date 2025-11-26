namespace frutaaaaa.Models
{
    public class GroupVarietyTableRowDto
    {
        public string VergerName { get; set; }
        public string GroupVarieteName { get; set; }
        public double TotalPdsfru { get; set; }
        public decimal TotalPdscom { get; set; }
        public decimal TotalEcart { get; set; }
        public int? VergerId { get; set; }      // nullable
        public int? GroupId { get; set; }
        public DateTime? MinReceptionDate { get; set; }
        public DateTime? MaxExportDate { get; set; }

    }
}
