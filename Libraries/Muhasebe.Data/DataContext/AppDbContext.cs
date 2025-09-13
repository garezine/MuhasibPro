using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.DataContext.DataSource;
using Muhasebe.Data.DataContext.SeedData;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.MuhasebeEntity.Banka;
using Muhasebe.Domain.Entities.MuhasebeEntity.Cari;
using Muhasebe.Domain.Entities.MuhasebeEntity.Fatura_Irsaliye;
using Muhasebe.Domain.Entities.MuhasebeEntity.Kasa;
using Muhasebe.Domain.Entities.MuhasebeEntity.Stok;

namespace Muhasebe.Data.DataContext;

public class AppDbContext : DbContext, IAppDataSource
{
    protected AppDbContext() { }
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        #region İlişkiler        

        // Faturalar - Irsaliyeler İlişkisi
        modelBuilder.Entity<Faturalar>(fatura =>
        {
            // Faturalar ile Irsaliyeler arasında 1-N ilişki
            fatura.HasMany(f => f.Irsaliyeler)
                .WithOne(i => i.Fatura)
                .HasForeignKey(i => i.FaturaId)
                .OnDelete(DeleteBehavior.ClientSetNull); // Fatura silinirse Irsaliye.FaturaId NULL olur

            // Faturalar ile CariHareket arasında 1-N ilişki
            fatura.HasMany(f => f.CariHareketler)
                .WithOne(ch => ch.Fatura)
                .HasForeignKey(ch => ch.FaturaId)
                .OnDelete(DeleteBehavior.NoAction); // Fatura silinirse CariHareket.FaturaId korunur
        });
        // Irsaliyeler - Faturalar İlişkisi
        modelBuilder.Entity<Irsaliyeler>(entity =>
        {
            // Fatura ile ilişki
            entity.HasOne(i => i.Fatura)
                  .WithMany(f => f.Irsaliyeler)
                  .HasForeignKey(i => i.FaturaId)
                  .OnDelete(DeleteBehavior.NoAction); // 👈 Cascade Delete'i kapat

            // CariHesap ile ilişki
            entity.HasOne(i => i.CariHesap)
                  .WithMany()
                  .HasForeignKey(i => i.CariHesapId)
                  .OnDelete(DeleteBehavior.Restrict);
        });


        // CariHareket - CariHesap İlişkisi
        modelBuilder.Entity<CariHareketler>(cariHareket =>
        {
            // CariHareket ile CariHesap arasında N-1 ilişki
            cariHareket.HasOne(ch => ch.Cari)
                .WithMany()
                .HasForeignKey(ch => ch.CariId)
                .OnDelete(DeleteBehavior.Restrict); // CariHesap silinemez (ilişkili hareketler varsa)
        });

        // FaturaKalemler - Faturalar İlişkisi (Opsiyonel)
        modelBuilder.Entity<FaturaKalemler>(faturaKalem =>
        {
            faturaKalem.HasOne(fk => fk.Fatura)
                .WithMany(f => f.FaturaKalemler)
                .HasForeignKey(fk => fk.FaturaId)
                .OnDelete(DeleteBehavior.Cascade); // Fatura silinirse Kalemler de silinir
        });

        // IrsaliyeKalemler - Irsaliyeler İlişkisi (Opsiyonel)
        modelBuilder.Entity<IrsaliyeKalemler>(irsaliyeKalem =>
        {
            irsaliyeKalem.HasOne(ik => ik.Irsaliye)
                .WithMany(i => i.IrsaliyeKalemler)
                .HasForeignKey(ik => ik.IrsaliyeId)
                .OnDelete(DeleteBehavior.Cascade); // Irsaliye silinirse Kalemler de silinir
        });
        #endregion

        #region SeedData
        SeedDataAppComboBoxData.HatirlatmaTur(modelBuilder);
        SeedDataAppComboBoxData.OdemeSekilleri(modelBuilder);
        SeedDataAppComboBoxData.ParaBirimi(modelBuilder);
        SeedDatailler.IlListesi(modelBuilder);
        SeedDataBanka.Bankalar(modelBuilder);

        SeedDataStok.StokBirim(modelBuilder);
        SeedDataStok.StokGrup(modelBuilder);
        SeedDataCari.CariGruplar(modelBuilder);
        SeedDataKasa.KasaAdlari(modelBuilder);
        #endregion

        //SeedDataPersonel.PersonelBolum(modelBuilder);
        //SeedDataPersonel.PersonelGorev(modelBuilder);
        //SeedDataSenet.SenetMahkemeSec(modelBuilder);
        #region HasNoKey
        modelBuilder.Entity<Ayarlar>().HasNoKey();
        modelBuilder.Entity<BelgeNumara>().HasNoKey();
        modelBuilder.Entity<VarsayilanDegerler>().HasNoKey();
        base.OnModelCreating(modelBuilder);
        #endregion


        base.OnModelCreating(modelBuilder);

    }

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

    //public DbSet<CekBankalar> CekBankalar { get; set; }

    //public DbSet<CekCirolari> CekCirolari { get; set; }

    //public DbSet<CekHesaplari> CekHesaplari { get; set; }

    //public DbSet<Cekler> Cekler { get; set; }

    //public DbSet<CekOdemeleri> CekOdemeleri { get; set; }

    //public DbSet<CekTahsilatlari> CekTahsilatlari { get; set; }


    //public DbSet<PersonelBolum> PersonelBolumler { get; set; }

    //public DbSet<PersonelGorev> PersonelGorevler { get; set; }

    //public DbSet<PersonelHareket> PersonelHareketler { get; set; }

    //public DbSet<PersonelKart> PersonelKartlar { get; set; }

    //public DbSet<PersonelMizan> PersonelMizanlar { get; set; }

    //public DbSet<PersonelTopluMaasTahakkuk> PersonelTopluMaasTahakkuklar { get; set; }

    //public DbSet<PersonelTopluOdeme> PersonelTopluOdemeler { get; set; }

    //public DbSet<SenetCirolari> SenetCirolari { get; set; }

    //public DbSet<Senetler> Senetler { get; set; }

    //public DbSet<SenetMahkemeler> SenetMahkemeler { get; set; }

    //public DbSet<SenetOdemeleri> SenetOdemeleri { get; set; }

    //public DbSet<SenetTahsilatlari> SenetTahsilatlari { get; set; }

    //public DbSet<SiparisDetay> SenetarisDetaylar { get; set; }

    //public DbSet<Siparisler> Siparisler { get; set; }

    //public DbSet<SiparisNotSablonlari> SiparisNotSablonlari { get; set; }


    //public DbSet<GecikenTaksitler> GecikenTaksitler { get; set; }

    //public DbSet<Taksitler> Taksitler { get; set; }

    //public DbSet<TaksitliAlis> TaksitliAlislar { get; set; }

    //public DbSet<TaksitliSatis> TaksitliSatislar { get; set; }

    //public DbSet<TaksitSenet> TaksitSenetler { get; set; }

    //public DbSet<TeklifDetay> TeklifDetaylar { get; set; }

    //public DbSet<Teklifler> Teklifler { get; set; }

    //public DbSet<TeklifNotSablonlari> TeklifNotSablonlari { get; set; }
}
