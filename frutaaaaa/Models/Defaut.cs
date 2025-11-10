using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("defaut")]
    public class Defaut
    {
        public short Coddef { get; set; }
        public string Intdef { get; set; }
        public string Famdef { get; set; }
    }
}
