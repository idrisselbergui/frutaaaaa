namespace frutaaaaa.Models
{
    public class DashboardDataDto
    {
        public decimal TotalPdscom { get; set; }
        public double TotalPdsfru { get; set; }
        public double ExportPercentage { get; set; } // Add this line
        public double TotalEcart { get; set; }
        public double EcartPercentage { get; set; }
        public List<DashboardTableRowDto> TableRows { get; set; } = new List<DashboardTableRowDto>();
    }
}