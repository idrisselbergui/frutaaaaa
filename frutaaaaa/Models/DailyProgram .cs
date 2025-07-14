using System.ComponentModel.DataAnnotations;

namespace frutaaaaa.Models
{
    public class DailyProgram
    {
        [Key]
        public int Id { get; set; }
        public int NumProg { get; set; }
        public int Coddes { get; set; }
        public int Refexp { get; set; }
        public string PO { get; set; }
        public DateTime Havday { get; set; }
        public DateTime Dteprog { get; set; }
        public string Lot { get; set; }

        // Navigation property for the details
        public virtual ICollection<DailyProgramDetail> Details { get; set; } = new List<DailyProgramDetail>();
    }
}