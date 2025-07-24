namespace frutaaaaa.Models
{
    public class ChartDataDto
    {
        public string Name { get; set; } // The full name for the tooltip (e.g., nomver)
        public string RefVer { get; set; } // The ID for the axis label (e.g., refver)
        public decimal Value { get; set; } // The calculated total for the bar height
    }
}
