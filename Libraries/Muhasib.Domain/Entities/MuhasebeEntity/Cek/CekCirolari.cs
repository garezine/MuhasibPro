using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cek
{
    [Table("CekCirolari")]
    public class CekCirolari : BaseEntity
    {
        public string Aciklama { get; set; }

        public long CariId { get; set; }

        public long CekId { get; set; }

        public decimal CekTutari { get; set; }

        public long CariHareketId { get; set; }

        public DateTime Tarih { get; set; }

        public ICollection<Cekler> Cekler { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }

        public ICollection<CariHareketler> CariHareketler { get; set; }
    }
}