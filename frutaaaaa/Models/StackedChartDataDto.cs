using System.Collections.Generic;

namespace frutaaaaa.Models
{
    public class StackedChartDataDto
    {
        public List<Dictionary<string, object>> Data { get; set; }
        public List<string> Keys { get; set; }
    }
}
