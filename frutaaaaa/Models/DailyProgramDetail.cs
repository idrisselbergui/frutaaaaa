namespace frutaaaaa.Models
{
    public class DailyProgramDetail
    {
        // Composite Primary Key will be configured in the DbContext
        public int NumProg { get; set; }
        public int Codvar { get; set; }
        public int Nbrpal { get; set; }
        public int Nbrcoli { get; set; }
        public int Valide { get; set; }
    }
}