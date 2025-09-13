using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("trait")]
    public class Trait
    {
        [Key]
        [Column("ref")]
        public int Ref { get; set; }

        [Column("nomcom")]
        public string? Nomcom { get; set; }

        [Column("matieractive")]
        public string? Matieractive { get; set; }

        [Column("dar")]
        public int? Dar { get; set; }

        [Column("dos")]
        public double? Dos { get; set; }

        [Column("unité")]
        public string? Unite { get; set; }
    }
}
