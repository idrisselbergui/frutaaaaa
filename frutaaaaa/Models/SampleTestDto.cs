using System;

namespace frutaaaaa.Models
{
    public class SampleTestDto
    {
        public int Id { get; set; }
        public int Numpal { get; set; }
        public short? Coddes { get; set; }
        public short? Codvar { get; set; }
        public DateTime StartDate { get; set; }
        public string VergerName { get; set; }
        public int InitialFruitCount { get; set; }
        public decimal? Pdsfru { get; set; }
        public int? Couleur1 { get; set; }
        public int? Couleur2 { get; set; }
        public SampleTestStatus Status { get; set; }
        public bool IsCheckedToday { get; set; }
    }

    public class CreateSampleTestRequest
    {
        public int Numpal { get; set; }
        public short? Coddes { get; set; }
        public short? Codvar { get; set; }
        public DateTime StartDate { get; set; }
        public int InitialFruitCount { get; set; }
        public decimal? Pdsfru { get; set; }
        public int? Couleur1 { get; set; }
        public int? Couleur2 { get; set; }
        public SampleTestStatus Status { get; set; } = SampleTestStatus.Active;
    }

    public class DefectHistoryDto
    {
        public int DefectId { get; set; }
        public int Quantity { get; set; }
    }

    public class DailyCheckHistoryDto
    {
        public int Id { get; set; }
        public DateTime CheckDate { get; set; }
        public double Pdsfru { get; set; }
        public int Couleur1 { get; set; }
        public int Couleur2 { get; set; }
        public List<DefectHistoryDto> Defects { get; set; }
    }

    public class SampleHistoryDto
    {
        public SampleTestDto Sample { get; set; }
        public List<DailyCheckHistoryDto> DailyChecks { get; set; }
    }
}
