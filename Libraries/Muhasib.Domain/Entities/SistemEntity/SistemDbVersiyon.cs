using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity
{
    [Table("SistemDbVersiyonlar")]
    public class SistemDbVersiyon : AppVersiyon
    {
        public string MevcutDbVersiyon { get; set; }
        public DateTime SistemDBSonGuncellemeTarihi { get; set; }
        public string? OncekiSistemDbVersiyon { get; set; }
    }
}

