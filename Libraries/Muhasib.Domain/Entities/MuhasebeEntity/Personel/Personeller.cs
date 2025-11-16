using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Personel
{
    [Table("Personeller")]
    public class Personeller : BaseEntity
    {
        [MaxLength(75)]
        public string Adi { get; set; }

        [MaxLength(75)]
        public string Soyadi { get; set; }
        public long BolumId { get; set; }
        public long GorevId { get; set; }
        public DateTime IseGirisTarih { get; set; }

        [MaxLength(50)]
        public string SicilNo { get; set; }

        public string CalismaDurum { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal NetMaas { get; set; }
        public short AvansOran { get; set; }

        [MaxLength(17)]
        public string CepTelefon { get; set; }

        [MaxLength(17)]
        public string EvTelefon { get; set; }

        [MaxLength(255)]
        public string Adres { get; set; }

        public byte[] Resim { get; set; }

        public byte[] ResimOnizle { get; set; }

        //Kimlik Bilgileri
        [MaxLength(11)]
        public int kb_TCKimlikNo { get; set; }

        public DateTime kb_DogumTarihi { get; set; }

        public string kb_SeriNo { get; set; }

        public DateTime kb_SonGecerlilikTarihi { get; set; }

        [MaxLength(25)]
        public string kb_Cinsiyet { get; set; }

        [MaxLength(5)]
        public string kb_Uyrugu { get; set; }

        [MaxLength(50)]
        public string kb_AnaAdi { get; set; }

        [MaxLength(50)]
        public string kb_BabaAdi { get; set; }

        [MaxLength(50)]
        public string kb_VerenMakam { get; set; }

        public PersonelGorev Gorev { get; set; }

        public PersonelBolum Bolum { get; set; }

        public ICollection<PersonelHareket> PersonelHareketler { get; set; }
    }
}
