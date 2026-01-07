using System;
using System.Collections.Generic;

namespace frutaaaaa.Models
{
    public class DefectItem
    {
        public DefectType Type { get; set; }
        public int Quantity { get; set; }
    }

    public class DailyCheckRequest
    {
        public DateTime CheckDate { get; set; }
        public List<DefectItem> Defects { get; set; } = new List<DefectItem>();
    }
}
