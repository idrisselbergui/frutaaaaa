using System.Collections.Generic;

namespace frutaaaaa.Models
{
    public class DashboardDataDto
    {
        public decimal TotalPdscom { get; set; }
        public double TotalPdsfru { get; set; }
        public List<DashboardTableRowDto> TableRows { get; set; } = new List<DashboardTableRowDto>();
    }
}