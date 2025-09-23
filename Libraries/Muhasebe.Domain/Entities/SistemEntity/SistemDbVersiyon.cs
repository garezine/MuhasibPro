using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity
{
    [Table("SistemDbVersiyonlar")]
    public class SistemDbVersiyon : AppVersiyon
    {
        public string SistemDBVersiyon { get; set; }
        public DateTime SistemDBSonGuncellemeTarihi { get; set; }
        public string? OncekiSistemDbVersiyon { get; set; }
    }
}

