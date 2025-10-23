﻿namespace frutaaaaa.Models
{
    public class EcartDirect
    {
        public int Numpal { get; set; }
        public int? Refver { get; set; }
        public int? Codvar { get; set; }
        public DateTime? Dtepal { get; set; }
        public int? Numbl { get; set; }
        public double? Pdsfru { get; set; }

        // New field
        public int? Codtype { get; set; }
        public TypeEcart? TypeEcart { get; set; }
    }
}
