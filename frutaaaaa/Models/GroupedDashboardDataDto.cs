using System.Collections.Generic;

namespace frutaaaaa.Models
{
    public class GroupedDashboardDataDto
    {
        public List<GroupVarietyTableRowDto> TableRows { get; set; } = new List<GroupVarietyTableRowDto>();
    }
}
