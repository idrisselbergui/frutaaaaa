using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    public class EcartD
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int numpre { get; set; }

        public int numpal { get; set; }

        public int numver { get; set; }

        public int refver { get; set; }

        public int codtype { get; set; }

        public int nbrcai { get; set; }

        public decimal pdsfru { get; set; }

        public int palnum { get; set; }

        public string typemb { get; set; }
    }
}
