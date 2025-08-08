namespace Muhasebe.Domain.Helpers
{
    public static class AppMessage
    {
        public static class Modules
        {
            public const string Kullanicis = "Kullanıcılar";
            public const string Firmas = "Firmalar";
            public const string Donems = "Dönemler";
            public const string Database = "Veritabanları";
            public const string Stoks = "Stoklar";
        }
        public static class LogDurum
        {
            public const string Hata = "HATA";
            public const string Bilgi = "BİLGİ";
            public const string Dikkat = "DİKKAT";
        }
        public static class HataDurum
        {
            public const string Kayit = "Kaydetme hatası!";
            public const string Guncelle = "Güncelleme hatası!";
            public const string Sil = "Silme hatası!";
            public const string Liste = "Listeleme hatası!";
            public const string Bul = "Kayıt bulunamadı!";
            public const string VeriHata = "Veri hatası!";
        }
        public static class DatabaseName
        {
            public const string DbFolder = "Databases";
            public const string SistemDbName = "Sistem.db";

        }

    }
}
