using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Muhasib.Domain.Entities.MuhasebeEntity.Cari
{
    [Table("CariFaturaBilgiler")]
    public class CariFaturaBilgi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long CariId { get; set; }

        [MaxLength(150)]
        public string Adres { get; set; }

        [MaxLength(150)]
        public string AdresTanim { get; set; }

        [MaxLength(150)]
        public string FaturaUnvani { get; set; }

        [MaxLength(150)]
        public string FaturaUnvani2 { get; set; }

        [MaxLength(25)]
        public long? IlId { get; set; }

        [MaxLength(25)]
        public string Ilcesi { get; set; }

        [MaxLength(50)]
        public string VergiDairesi { get; set; }

        [MaxLength(11)]
        public string VergiNo { get; set; }

        public CariHesap Cari { get; set; }
        public Iller Il { get; set; }


    }
}