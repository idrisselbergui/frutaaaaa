namespace frutaaaaa.Models
{
    public class DashboardTableRowDto
    {
        public string VergerName { get; set; }
        public string VarieteName { get; set; } // Corrigé de GrpVarName à VarieteName
        public decimal TotalPdscom { get; set; }
        public double TotalPdsfru { get; set; }
        public double TotalEcart { get; set; }
        public DateTime? MinReceptionDate { get; set; }
        public DateTime? MaxExportDate { get; set; }
    }
}