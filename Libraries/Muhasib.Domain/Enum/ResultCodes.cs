using System.ComponentModel;

namespace Muhasib.Domain.Enum
{
    public enum ResultCodes
    {
        // HATA MESAJLARI
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

        [Description("{0} zaten mevcut!")]
        HATA_ZatenVar,

        [Description("{0} alanı boş olamaz!")]
        HATA_BosOlamaz,

        [Description("{0} oluşturulamadı!")]
        HATA_Olusturulamadi,

        [Description("{0} işlem görmüş, değiştirilemez!")]
        HATA_IslemGormus,

        [Description("{0} yetkisiz erişim!")]
        HATA_YetkisizErisim,

        [Description("{0} geçersiz format!")]
        HATA_GecersizFormat,

        [Description("{0} zaman aşımı hatası!")]
        HATA_ZamanAsimi,

        // BAŞARILI MESAJLARI
        [Description("{0} başarıyla bulundu")]
        BASARILI_Bulundu,

        [Description("{0} başarıyla eklendi")]
        BASARILI_Eklendi,

        [Description("{0} başarıyla güncellendi")]
        BASARILI_Guncellendi,

        [Description("{0} başarıyla silindi")]
        BASARILI_Silindi,

        [Description("{0} başarıyla listelendi")]
        BASARILI_Listelendi,

        [Description("{0} detayları getirildi")]
        BASARILI_Detaylar,

        [Description("{0} başarıyla kaydedildi")]
        BASARILI_Kaydedildi,

        [Description("{0} başarıyla oluşturuldu")]
        BASARILI_Olusturuldu,

        [Description("{0} başarıyla tamamlandı")]
        BASARILI_Tamamlandi,

        [Description("Tüm {0} başarıyla eklendi")]
        BASARILI_TumuEklendi,

        [Description("Tüm {0} başarıyla güncellendi")]
        BASARILI_TumuGuncellendi,

        [Description("Tüm {0} başarıyla silindi")]
        BASARILI_TumuSilindi,

        [Description("Tüm {0} başarıyla kaydedildi")]
        BASARILI_TumuKaydedildi,

        [Description("Veritabanı bağlantısı başarılı")]
        BASARILI_VeritabaniBaglandi,

        [Description("Bağlantı başarıyla kuruldu")]
        BASARILI_Baglandi,

        // BİLGİ MESAJLARI
        [Description("{0} başarıyla alındı")]
        BILGI_Alindi,

        [Description("{0} doğrulandı")]
        BILGI_Dogrulandi,

        [Description("{0} hazır")]
        BILGI_Hazir,

        // UYARI MESAJLARI
        [Description("{0} uyarısı: İşlem dikkatle yapılmalı")]
        UYARI_IslemUyarisi,

        [Description("{0} limiti yaklaştı")]
        UYARI_LimitYaklasti,

        [Description("{0} süresi dolmak üzere")]
        UYARI_SuresiDoluyor
    }
}
