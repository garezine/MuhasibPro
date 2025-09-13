using Microsoft.EntityFrameworkCore;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.MuhasebeEntity.Banka;
using Muhasebe.Domain.Entities.MuhasebeEntity.Cari;
using Muhasebe.Domain.Entities.MuhasebeEntity.Fatura_Irsaliye;
using Muhasebe.Domain.Entities.MuhasebeEntity.Kasa;
using Muhasebe.Domain.Entities.MuhasebeEntity.Stok;

namespace Muhasebe.Data.DataContext.DataSource
{
    public interface IAppDataSource
    {
        #region Uygulama Tabloları    
        public DbSet<AppLog> AppLogs { get; set; }
        public DbSet<Ajanda> Ajandalar { get; set; }

        public DbSet<Ayarlar> Ayarlar { get; set; }

        public DbSet<BelgeNumara> BelgeNumaralar { get; set; }

        public DbSet<Hatirlatmalar> Hatirlatmalar { get; set; }

        public DbSet<HatirlatmaTurler> HatirlatmaTurler { get; set; }

        public DbSet<Iller> Iller { get; set; }

        public DbSet<Notlar> Notlar { get; set; }

        public DbSet<OdemeTurler> OdemeTurler { get; set; }

        public DbSet<APP_FormPozisyon> APP_FormPozisyon { get; set; }

        public DbSet<APP_GuidCode> APP_GuidCode { get; set; }

        public DbSet<APP_SayiFormat> APP_SayiFormat { get; set; }

        public DbSet<ParaBirimler> ParaBirimler { get; set; }

        public DbSet<TeslimatSablonlar> TeslimatSablonlari { get; set; }

        public DbSet<VarsayilanDegerler> VarsayilanDegerler { get; set; }
        #endregion

        #region Banka Modul Tabloları
        public DbSet<BankaHareket> BankaHareketler { get; set; }

        public DbSet<BankaHesaplar> BankaHesapKartlar { get; set; }

        public DbSet<BankaListesi> BankaListesi { get; set; }
        #endregion

        #region Cari Modul Tabloları
        public DbSet<CariGrup> CariGruplar { get; set; }

        public DbSet<CariHesap> CariHesaplar { get; set; }

        public DbSet<CariHesapDetay> CariHesapDetaylar { get; set; }

        public DbSet<CariBankaHesap> CariBankaHesaplar { get; set; }

        public DbSet<CariFaturaBilgi> CariFaturaBilgiler { get; set; }

        public DbSet<CariBakiyeler> CariBakiyeler { get; set; }

        public DbSet<CariHareketler> CariHareketler { get; set; }
        #endregion

        #region Fatura - Irsaliyeler Modul Tabloları
        public DbSet<Faturalar> Faturalar { get; set; }

        public DbSet<FaturaKalemler> FaturaKalemler { get; set; }

        public DbSet<Irsaliyeler> Irsaliyeler { get; set; }

        public DbSet<IrsaliyeKalemler> IrsaliyeKalemler { get; set; }
        #endregion

        #region Stok Modul Tabloları
        public DbSet<Barkod> Barkodlar { get; set; }

        public DbSet<BarkodYazdir> BarkodYazdir { get; set; }

        //public DbSet<IstatistikKarZarar> IstatistikKarZararlar { get; set; }   

        public DbSet<StokBirimler> StokBirimler { get; set; }

        public DbSet<StokGruplar> StokGruplar { get; set; }

        public DbSet<StokBakiyeler> StokBakiyeler { get; set; }

        public DbSet<StokHareketler> StokHareketler { get; set; }

        public DbSet<Stoklar> Stoklar { get; set; }
        #endregion

        #region Kasa Modul Tabloları
        public DbSet<KasaHareket> KasaHareketler { get; set; }

        public DbSet<KasaDurum> KasaDurumlar { get; set; }

        public DbSet<Kasalar> Kasalar { get; set; }
        #endregion
    }
}
