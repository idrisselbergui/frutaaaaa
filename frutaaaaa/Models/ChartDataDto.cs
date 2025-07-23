namespace frutaaaaa.Models
{
    public class ChartDataDto
    {
        public string Name { get; set; } // This will now be the Verger Name for the tooltip
        public string RefVer { get; set; } // We'll use this for the axis labels
        public decimal Value { get; set; }
    }
}