using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("adherent")]
    public class Adherent
    {
        [Key]
        [Column("refadh")]
        public int Refadh { get; set; }

        [Column("nomadh")]
        public string Nomadh { get; set; }

        [Column("cinadh")]
        public string Cinadh { get; set; }

        [Column("Type")]
        public string Type { get; set; }
    }
}
