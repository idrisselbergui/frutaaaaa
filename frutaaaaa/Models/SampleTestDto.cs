using System;

namespace frutaaaaa.Models
{
    public class SampleTestDto
    {
        public int Id { get; set; }
        public int Numrec { get; set; }
        public short? Coddes { get; set; }
        public short? Codvar { get; set; }
        public DateTime StartDate { get; set; }
        public int InitialFruitCount { get; set; }
        public SampleTestStatus Status { get; set; }
        public bool IsCheckedToday { get; set; }
    }

    public class CreateSampleTestRequest
    {
        public int Numrec { get; set; }
        public short? Coddes { get; set; }
        public short? Codvar { get; set; }
        public DateTime StartDate { get; set; }
        public int InitialFruitCount { get; set; }
        public SampleTestStatus Status { get; set; } = SampleTestStatus.Active;
    }
}
