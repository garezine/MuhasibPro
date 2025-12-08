using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.DataContext
{
    public class SistemDbContext : DbContext
    {
        protected SistemDbContext()
        {
        }
        public SistemDbContext(DbContextOptions<SistemDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SeedUser(modelBuilder);
            SeedInitialVersion(modelBuilder);
            modelBuilder.Entity<Kullanici>(
                kullanici =>
                {
                    kullanici.HasMany(k => k.Hesaplar)
                        .WithOne(h => h.Kullanici)
                        .HasForeignKey(h => h.KullaniciId)
                        .OnDelete(DeleteBehavior.NoAction);
                });
            modelBuilder.Entity<MaliDonem>(entity =>
            {
                entity.HasOne(m => m.Firma)
                      .WithMany(f => f.MaliDonemler) //eğer Firma'da collection varsa
                      .HasForeignKey(m => m.FirmaId)
                      .OnDelete(DeleteBehavior.Restrict); // Veya Cascade, SetNull

                // Diğer konfigürasyonlar...
            });
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
            modelBuilder.Entity<Kullanici>().HasData(yonetici);
            modelBuilder.Entity<KullaniciRol>().HasData(adminRol);
        }
        private void SeedInitialVersion(ModelBuilder modelBuilder)
        {
            // SistemDbVersiyon için seed
            var initialSistemDbVersion = new AppDbVersion
            {
                DatabaseName = "Sistem.db",
                CurrentAppVersion = "1.0.0", // Base class property
                CurrentAppVersionLastUpdate = new DateTime(2025, 09, 22),
                PreviousAppVersiyon = null,
                CurrentDatabaseVersion = "1.0.0",
                CurrentDatabaseLastUpdate = new DateTime(2025, 09, 22),
                PreviousDatabaseVersion = null
            };
            modelBuilder.Entity<AppDbVersion>().HasData(initialSistemDbVersion);
        }
        public DbSet<SistemLog> SistemLogs { get; set; }
        public DbSet<AppVersion> AppVersiyonlar { get; set; }
        public DbSet<AppDbVersion> AppDbVersiyonlar { get; set; }
        public DbSet<Hesap> Hesaplar { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<KullaniciRol> KullaniciRoller { get; set; }
        public DbSet<Firma> Firmalar { get; set; }
        public DbSet<MaliDonem> MaliDonemler { get; set; }
        
    }
}
