using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Banka;

[Table("BankaHareketler")]
public class BankaHareket : BaseEntity
{
    public long BankaId { get; set; }

    public DateTime IslemTarihi { get; set; }

    [MaxLength(50)]
    public string IslemTipi { get; set; }

    [MaxLength(50)]
    public string HesapSekli { get; set; }

    [MaxLength(50)]
    public string BelgeNo { get; set; }

    [MaxLength(150)]
    public string Aciklama { get; set; }

    public bool GC { get; set; }

    [Column(TypeName = "decimal (18,2)")]
    public decimal IslemTutari { get; set; }

    public BankaHesaplar Banka { get; set; }

}
