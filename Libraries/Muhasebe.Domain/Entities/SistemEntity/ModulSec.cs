using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity;

public class ModulSec
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long KullaniciId { get; set; }

    public bool mdl_cari_y1 { get; set; }

    public bool mdl_cari_y2 { get; set; }

    public bool mdl_cari_y3 { get; set; }

    public bool mdl_cari_y4 { get; set; }

    public bool mdl_kasa_y1 { get; set; }

    public bool mdl_kasa_y2 { get; set; }

    public bool mdl_kasa_y3 { get; set; }

    public bool mdl_kasa_y4 { get; set; }

    public bool mdl_cek_y1 { get; set; }

    public bool mdl_cek_y2 { get; set; }

    public bool mdl_cek_y3 { get; set; }

    public bool mdl_cek_y4 { get; set; }

    public bool mdl_senet_y1 { get; set; }

    public bool mdl_senet_y2 { get; set; }

    public bool mdl_senet_y3 { get; set; }

    public bool mdl_senet_y4 { get; set; }

    public bool mdl_personel_y1 { get; set; }

    public bool mdl_personel_y2 { get; set; }

    public bool mdl_personel_y3 { get; set; }

    public bool mdl_personel_y4 { get; set; }

    public bool mdl_banka_y1 { get; set; }

    public bool mdl_banka_y2 { get; set; }

    public bool mdl_banka_y3 { get; set; }

    public bool mdl_banka_y4 { get; set; }

    public bool mdl_stok_y1 { get; set; }

    public bool mdl_stok_y2 { get; set; }

    public bool mdl_stok_y3 { get; set; }

    public bool mdl_stok_y4 { get; set; }

    public bool mdl_teklif_y1 { get; set; }

    public bool mdl_teklif_y2 { get; set; }

    public bool mdl_teklif_y3 { get; set; }

    public bool mdl_teklif_y4 { get; set; }

    public bool mdl_siparis_y1 { get; set; }

    public bool mdl_siparis_y2 { get; set; }

    public bool mdl_siparis_y3 { get; set; }

    public bool mdl_siparis_y4 { get; set; }

    public bool mdl_ajanda_y1 { get; set; }

    public bool mdl_ajanda_y2 { get; set; }

    public bool mdl_ajanda_y3 { get; set; }

    public bool mdl_ajanda_y4 { get; set; }

    public bool mdl_istatistikler_y1 { get; set; }

    public bool mdl_istatistikler_y2 { get; set; }

    public bool mdl_istatistikler_y3 { get; set; }

    public bool mdl_istatistikler_y4 { get; set; }

    public bool mdl_raporlar_y1 { get; set; }

    public bool mdl_raporlar_y2 { get; set; }

    public bool mdl_raporlar_y3 { get; set; }

    public bool mdl_raporlar_y4 { get; set; }

    public bool mdl_ayarlar_y1 { get; set; }

    public bool mdl_ayarlar_y2 { get; set; }

    public bool mdl_ayarlar_y3 { get; set; }

    public bool mdl_ayarlar_y4 { get; set; }

    public bool mdl_hatirlatma_y1 { get; set; }

    public bool mdl_hatirlatma_y2 { get; set; }

    public bool mdl_hatirlatma_y3 { get; set; }

    public bool mdl_hatirlatma_y4 { get; set; }
}
