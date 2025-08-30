using System.ComponentModel.DataAnnotations;

namespace frutaaaaa.Models
{
    public class TypeEcart
    {
        [Key]
        public int codtype { get; set; }

        public string? destype { get; set; }

        public string? lier { get; set; }
    }
}
