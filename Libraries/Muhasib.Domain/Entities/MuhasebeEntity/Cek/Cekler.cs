using Muhasib.Domain.Entities.MuhasebeEntity.Banka;
using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using Muhasib.Domain.Entities.MuhasebeEntity.Kasa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cek
{
    [Table("Cekler")]
    public class Cekler : BaseEntity
    {
        [MaxLength(255)]
        public string Aciklama { get; set; }

        [MaxLength(50)]
        public string BankaAdi { get; set; }

        [MaxLength(75)]
        public string BankaIli { get; set; }

        [MaxLength(255)]
        public string BelgedekiIsim { get; set; }

        [MaxLength(25)]
        public string CariyeIsleme { get; set; }

        [MaxLength(20)]
        public string CekNo { get; set; }

        [MaxLength(20)]
        public string CekTuru { get; set; }

        [MaxLength(50)]
        public string CiroEden { get; set; }

        [MaxLength(50)]
        public string Durumu { get; set; }

        public bool Hamiline { get; set; }

        [MaxLength(20)]
        public string BankaHesapNo { get; set; }

        public DateTime IslemTarihi { get; set; }

        public long KasaId { get; set; }

        [MaxLength(50)]
        public string Subesi { get; set; }

        [MaxLength(3)]
        public string Turu { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutari { get; set; }

        public DateTime VadeTarihi { get; set; }

        //Baðlantýlý Tablolar        

        public long CariId { get; set; }

        public long BankaId { get; set; }

        public long CariHareketId { get; set; }

        public ICollection<CariHesap> Cariler { get; set; }

        public ICollection<BankaHesaplar> Bankalar { get; set; }

        public ICollection<Kasalar> Kasalar { get; set; }

        public ICollection<CekOdemeleri> CekOdemeleri { get; set; }

        public ICollection<CekTahsilatlari> CekTahsilatlari { get; set; }
    }
}