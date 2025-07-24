using System.Collections.Generic;

namespace frutaaaaa.Models
{
    public class DashboardDataDto
    {
        public decimal TotalPdscom { get; set; }
        public double TotalPdsfru { get; set; }
        public double ExportPercentage { get; set; }
        public double TotalEcart { get; set; }
        public double EcartPercentage { get; set; }
        public List<DashboardTableRowDto> TableRows { get; set; } = new List<DashboardTableRowDto>();
        public List<ChartDataDto> ReceptionByVergerChart { get; set; }
        public List<ChartDataDto> ExportByVergerChart { get; set; }

        // --- Ajoutez cette nouvelle propriété ---
        public StackedChartDataDto ClientExportChart { get; set; }
    }
}
