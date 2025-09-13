using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace frutaaaaa.Models
{
    [Table("reception")]
    public class Reception
    {
        [Key]
        [Column("numrec")]
        public int Numrec { get; set; }

        [Column("refsta")]
        public short? Refsta { get; set; }

        [Column("codvar")]
        public short? Codvar { get; set; }

        [Column("varrec")]
        public short? Varrec { get; set; }

        [Column("refver")]
        public short? Refver { get; set; }

        [Column("numveh")]
        public string? Numveh { get; set; }

        [Column("numcai")]
        public int? Numcai { get; set; }

        [Column("numtrs")]
        public int? Numtrs { get; set; }

        [Column("numbl")]
        public int? Numbl { get; set; }

        [Column("dterec")]
        public DateTime? Dterec { get; set; }

        [Column("herrec")]
        public string? Herrec { get; set; }

        [Column("dtecue")]
        public DateTime? Dtecue { get; set; }

        [Column("hercue")]
        public string? Hercue { get; set; }

        [Column("typcai")]
        public short? Typcai { get; set; }

        [Column("codtyp")]
        public string? Codtyp { get; set; }

        [Column("nbrcai")]
        public int? Nbrcai { get; set; }

        [Column("nbrpal")]
        public int? Nbrpal { get; set; }

        [Column("pdspes")]
        public decimal? Pdspes { get; set; }

        [Column("brurec")]
        public decimal? Brurec { get; set; }

        [Column("tarrec")]
        public decimal? Tarrec { get; set; }

        [Column("netrec")]
        public decimal? Netrec { get; set; }

        [Column("moycai")]
        public decimal? Moycai { get; set; }

        [Column("coment")]
        public string? Coment { get; set; }

        [Column("certif")]
        public string? Certif { get; set; }

        [Column("codmaj")]
        public string? Codmaj { get; set; }

        [Column("numbou")]
        public uint? Numbou { get; set; }

        [Column("codaff")]
        public ushort? Codaff { get; set; }

        [Column("pdsech")]
        public decimal? Pdsech { get; set; }

        [Column("nbrfru")]
        public ushort? Nbrfru { get; set; }

        [Column("tmprec")]
        public decimal? Tmprec { get; set; }

        [Column("numr")]
        public uint? Numr { get; set; }

        [Column("numpes")]
        public int? Numpes { get; set; }

        [Column("numord")]
        public int? Numord { get; set; }

        [Column("numcaisor")]
        public int? Numcaisor { get; set; }

        [Column("nbrcailiv")]
        public int? Nbrcailiv { get; set; }

        [Column("nbrpalliv")]
        public int? Nbrpalliv { get; set; }

        [Column("codtrj")]
        public int? Codtrj { get; set; }

        [Column("listpar")]
        public string? Listpar { get; set; }

        [Column("comagri")]
        public string? Comagri { get; set; }

        [Column("affect")]
        public short? Affect { get; set; }

        [Column("typetrns")]
        public string? Typetrns { get; set; }

        [Column("mttrans")]
        public double? Mttrans { get; set; }

        [Column("dttare")]
        public DateTime? Dttare { get; set; }

        [Column("pese")]
        public string? Pese { get; set; }

        [Column("pdsNet")]
        public double? PdsNet { get; set; }

        [Column("numtrait")]
        public int? Numtrait { get; set; }
    }
}
