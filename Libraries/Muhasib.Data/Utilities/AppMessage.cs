namespace Muhasib.Data.Utilities
{
    public static class AppMessage
    {
        #region Modül İsimleri
        public static class Modules
        {
            public const string Kullanicilar = "Kullanıcılar";
            public const string Firmalar = "Firmalar";
            public const string Donemler = "Dönemler";
            public const string Veritabanlari = "Veritabanları";
            public const string Stoklar = "Stoklar";
            public const string CariHesaplar = "Cari Hesaplar";
            public const string Faturalar = "Faturalar";
            public const string Raporlar = "Raporlar";
            public const string Ayarlar = "Sistem Ayarları";
        }
        #endregion

        #region Log Durumları
        public static class LogDurum
        {
            public const string Hata = "HATA";
            public const string Bilgi = "BİLGİ";
            public const string Dikkat = "DİKKAT";
            public const string Uyari = "UYARI";
            public const string Basarili = "BAŞARILI";
            public const string Kritik = "KRİTİK";
        }
        #endregion

        #region Hata Mesajları
        public static class HataMesajlari
        {
            public const string Kayit = "Kaydetme hatası!";
            public const string Guncelle = "Güncelleme hatası!";
            public const string Sil = "Silme hatası!";
            public const string Liste = "Listeleme hatası!";
            public const string Bul = "Kayıt bulunamadı!";
            public const string VeriHata = "Veri hatası!";
            public const string YetkiHatasi = "Yetkiniz bulunmamaktadır!";
            public const string BaglantiHatasi = "Veritabanı bağlantı hatası!";
            public const string DogrulamaHatasi = "Doğrulama hatası!";

            // Parametreli hata mesajı oluşturucu
            public static string KayitBulunamadi(string varlikIsmi, object id)
            {
                return $"{varlikIsmi} (ID: {id}) bulunamadı!";
            }

            public static string ZorunluAlan(string alanAdi)
            {
                return $"{alanAdi} alanı zorunludur!";
            }
        }
        #endregion

        #region Başarı Mesajları
        public static class BasariMesajlari
        {
            public const string Kayit = "Kayıt işlemi başarılı!";
            public const string Guncelle = "Güncelleme işlemi başarılı!";
            public const string Sil = "Silme işlemi başarılı!";
            public const string Listele = "Listeleme işlemi başarılı!";
            public const string Giris = "Giriş işlemi başarılı!";
            public const string Cikis = "Çıkış işlemi başarılı!";

            public static string KayitBasarili(string varlikIsmi)
            {
                return $"{varlikIsmi} başarıyla kaydedildi!";
            }

            public static string GuncellemeBasarili(string varlikIsmi)
            {
                return $"{varlikIsmi} başarıyla güncellendi!";
            }
        }
        #endregion

        #region Doğrulama Mesajları
        public static class DogrulamaMesajlari
        {
            public const string GecersizEmail = "Geçersiz e-posta formatı!";
            public const string GecersizTelefon = "Geçersiz telefon numarası!";
            public const string GecersizTarih = "Geçersiz tarih formatı!";
            public const string SifreUyusmazlik = "Şifreler eşleşmiyor!";
            public const string SifreKarmaşıklık = "Şifre en az 8 karakter olmalı ve harf, rakam, özel karakter içermelidir!";
            public const string GecersizVergiNo = "Geçersiz vergi numarası!";
            public const string GecersizTCKN = "Geçersiz TC Kimlik Numarası!";
        }
        #endregion       

        #region Kullanıcı Mesajları
        public static class KullaniciMesajlari
        {
            public const string GirisBasari = "Giriş başarılı!";
            public const string GirisHata = "Kullanıcı adı veya şifre hatalı!";
            public const string KullaniciPasif = "Kullanıcı hesabı pasif durumda!";
            public const string SessionTimeout = "Oturum süreniz doldu, lütfen tekrar giriş yapın!";
            public const string YetkiYok = "Bu işlem için yetkiniz bulunmamaktadır!";
            public const string SifreDegistir = "Şifreniz başarıyla değiştirildi!";
            public const string SifreHatirlatma = "Şifre sıfırlama linki e-posta adresinize gönderildi!";
        }
        #endregion

        #region Rapor Mesajları
        public static class RaporMesajlari
        {
            public const string Hazirlandi = "Rapor hazırlandı!";
            public const string Hazirlaniyor = "Rapor hazırlanıyor, lütfen bekleyin...";
            public const string Hata = "Rapor oluşturulurken hata oluştu!";
            public const string VeriYok = "Rapor için yeterli veri bulunamadı!";
            public const string Indirildi = "Rapor başarıyla indirildi!";
            public const string Yazdirildi = "Rapor başarıyla yazdırıldi!";
        }
        #endregion

        #region Yardımcı Metodlar
        public static string ModulHataMesaji(string modul, string hataTuru)
        {
            return $"{modul} modülünde {hataTuru}";
        }

        public static string IslemSonucMesaji(bool basarili, string islemTuru, string varlikIsmi = null)
        {
            if (basarili)
            {
                return varlikIsmi != null ?
                    $"{varlikIsmi} {islemTuru} işlemi başarılı!" :
                    $"{islemTuru} işlemi başarılı!";
            }
            else
            {
                return varlikIsmi != null ?
                    $"{varlikIsmi} {islemTuru} işleminde hata!" :
                    $"{islemTuru} işleminde hata!";
            }
        }
        #endregion
    }
}