using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.DataContext.DataSource;
using Muhasebe.Data.Helper;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.DataContext
{
    public class AppSistemDbContext : DbContext, ISistemDataSource
    {
        protected AppSistemDbContext()
        {
        }
        public AppSistemDbContext(DbContextOptions<AppSistemDbContext> options) : base(options)
        {
        }
        private void SeedUser(ModelBuilder modelBuilder)
        {
            var yonetici = new Kullanici
            {
                Id = 241341,
                Adi = "Ömer",
                AktifMi = true,
                Eposta = "korkutomer@gmail.com",
                KaydedenId = 5413300800,
                KayitTarihi = new DateTime(2025, 03, 12),
                RolId = 1,
                KullaniciAdi = "korkutomer",
                SifreHash = "AQAAAAIAAYagAAAAECnYdlrjFiWFJc+FGeGDmvR87uz20oU/Z0K4JE9ddoF2VUnmHw0idEFX8UPOb4cpzQ==",
                Soyadi = "Korkut",
                Telefon = "0 (541) 330 0800",
                ArananTerim = "korkutomer, Ömer Korkut, Yönetici"
            };
            var adminRol = new KullaniciRol
            {
                Id = 1,
                RolAdi = "Yönetici",
                Aciklama = "Sistemin tüm özelliklerine erişim yetkisi.",
                KayitTarihi = yonetici.KayitTarihi,
                KaydedenId = yonetici.KaydedenId,
            };
            modelBuilder.Entity<KullaniciRol>().HasData(adminRol);
            modelBuilder.Entity<Kullanici>().HasData(yonetici);
        }
        private void SeedInitialVersion(ModelBuilder modelBuilder)
        {
            // SistemDbVersiyon için seed
            var initialSistemDbVersion = new SistemDbVersiyon
            {
                UygulamaVersiyon = "1.0.0", // Base class property
                UygulamaSonGuncellemeTarihi = new DateTime(2025, 09, 22),
                OncekiUygulamaVersiyon = null,
                SistemDBVersiyon = "1.0.0",
                SistemDBSonGuncellemeTarihi = new DateTime(2025, 09, 22),
                OncekiSistemDbVersiyon = null
            };

            
            modelBuilder.Entity<SistemDbVersiyon>().HasData(initialSistemDbVersion);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SeedUser(modelBuilder);
            SeedInitialVersion(modelBuilder);
            modelBuilder.Entity<Kullanici>(kullanici =>
            {
                kullanici.HasMany(k => k.Hesaplar)
                .WithOne(h => h.Kullanici)
                .HasForeignKey(h => h.KullaniciId)
                .OnDelete(DeleteBehavior.NoAction);
            });
        }


        public DbSet<SistemLog> SistemLogs { get; set; }

        public DbSet<Hesap> Hesaplar { get; set; }

        public DbSet<SonSecilenKullaniciFirmaDonem> SonSecilenKullaniciFirmaDonemler { get; set; }

        public DbSet<DevirLog> DevirLogs { get; set; }

        public DbSet<DevirLogII> DevirLogII { get; set; }

        public DbSet<Kullanici> Kullanicilar { get; set; }

        public DbSet<Firma> Firmalar { get; set; }

        public DbSet<MaliDonem> MaliDonemler { get; set; }

        public DbSet<DonemDBSec> DonemDBSecim { get; set; }

        public DbSet<DbYedekAl> DbYedekAl { get; set; }

        public DbSet<AppVersiyon> AppVersiyonlar { get; set; }
        public DbSet<SistemDbVersiyon> SistemDbVersiyonlar { get; set; }

        public DbSet<ModulSec> ModulSecim { get; set; }
        public DbSet<KullaniciRol> KullaniciRoller { get; set; }
    }
}
