using System;
using System.Collections.Generic;

namespace frutaaaaa.Models
{
    public class DefectItem
    {
        public int DefectId { get; set; }
        public int Quantity { get; set; }
    }

    public class DailyCheckRequest
    {
        public DateTime CheckDate { get; set; }
        public double? Pdsfru { get; set; }
        public int? Couleur1 { get; set; }
        public int? Couleur2 { get; set; }
        public List<DefectItem> Defects { get; set; } = new List<DefectItem>();
    }
}
