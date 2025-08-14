using System.ComponentModel;

namespace Muhasebe.Domain.Helpers
{
    public enum ResultCodes
    {
        [Description("{0} bulunamadı!")]
        HATA_Bulunamadi,

        [Description("{0} eklenemedi!")]
        HATA_Eklenemedi,

        [Description("{0} kaydedilemedi!")]
        HATA_Kaydedilemedi,

        [Description("{0} güncellenemedi!")]
        HATA_Guncellenemedi,

        [Description("{0} silinemedi!")]
        HATA_Silinemedi,

        [Description("{0} listelenemedi!")]
        HATA_Listelenemedi,

        [Description("{0} içerik bulunamadı!")]
        HATA_IcerikYok,

        [Description("{0} veritabanı hatası!")]
        HATA_VeritabaniHatasi,

        [Description("{0} bağlantı hatası!")]
        HATA_BaglantiHatasi,

        [Description("{0} bilinmeyen hata!")]
        HATA_BilinmeyenHata,

        [Description("{0} beklenmeyen hata!")]
        HATA_BeklenmeyenHata,

        [Description("{0} zaten var!")]
        HATA_ZatenVar,

        [Description("{0} boş olamaz!")]
        HATA_BosVeyaNullOlamaz,

        [Description("{0} oluşturulamadı!")]
        HATA_Olusturulamadi,

        [Description("{0} işlem görmüş!")]
        HATA_IslemGormus,

        //BAŞARILI
        [Description("{0} bulundu")]
        BASARILI_Bulundu,

        [Description("{0} eklendi")]
        BASARILI_Eklendi,

        [Description("{0} tümü eklendi")]
        BASARILI_TumuEklendi,

        [Description("{0} güncellendi")]
        BASARILI_Guncellendi,

        [Description("{0} Tümü güncellendi")]
        BASARILI_TumuGuncellendi,

        [Description("{0} tümü silindi")]
        BASARILI_TumuSilindi,

        [Description("{0} silindi")]
        BASARILI_Silindi,

        [Description("{0} listelendi")]
        BASARILI_Listelendi,

        [Description("{0} detaylar")]
        BASARILI_Detaylar,

        [Description("{0} veritabanı bağlandı")]
        BASARILI_VeritabaniBaglandi,

        [Description("{0} bağlantı kuruldu")]
        BASARILI_Baglandi,

        [Description("{0} tamamlandı")]
        BASARILI_Tamamlandi,

        [Description("{0} kaydedildi")]
        BASARILI_Kaydedildi,

        [Description("{0} tümü kaydedildi")]
        BASARILI_TumuKaydedildi,

        [Description("{0} oluşturuldu")]
        BASARILI_Olusturuldu,
    }
}
