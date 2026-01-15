using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("vecart_d")]
    public class VecartD
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("numpal")] // Keeping numpal if user wants it, though it might be redundant if this is manual. User's SQL had it.
        public int? Numpal { get; set; }

        [Column("refver")]
        public int? Refver { get; set; }

        [Column("codgrv")] // Maps to database column 'codgrv'
        public int? Codgrv { get; set; } // Changed from Variete

        [Column("pds")]
        public double? Pds { get; set; }

        [Column("nbrcol")]
        public int? Nbrcol { get; set; }

        [Column("numvnt")]
        public int? Numvnt { get; set; }
    }
}
