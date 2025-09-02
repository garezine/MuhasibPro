using Microsoft.EntityFrameworkCore;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Entities.Uygulama;

namespace Muhasebe.Data.DataContext;

public class AppSistemDbContext : DbContext
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
            Rol = "Yönetici",
            KullaniciAdi = "korkutomer",            
            SifreHash = "AQAAAAIAAYagAAAAECnYdlrjFiWFJc+FGeGDmvR87uz20oU/Z0K4JE9ddoF2VUnmHw0idEFX8UPOb4cpzQ==",
            Soyadi = "Korkut",
            Telefon = "0 (541) 330 0800",
            ArananTerim = "korkutomer, Ömer Korkut, Yönetici"

        };
        modelBuilder.Entity<Kullanici>().HasData(yonetici);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        SeedUser(modelBuilder);
    }
    public DbSet<AppLog> AppLogs { get; set; }   

    public DbSet<Hesap> Hesaplar { get; set; }

    public DbSet<Degerler> Degerler { get; set; }

    public DbSet<DevirLog> DevirLogs { get; set; }

    public DbSet<DevirLogII> DevirLogII { get; set; }

    public DbSet<Kullanici> Kullanicilar { get; set; }

    public DbSet<Firma> Firmalar { get; set; }

    public DbSet<MaliDonem> MaliDonemler { get; set; }

    public DbSet<DonemDBSec> DonemDBSecim { get; set; }

    public DbSet<DbYedekAl> DbYedekAl { get; set; }
    public DbSet<AppVersiyon> AppVersiyonlar  { get; set; }

    public DbSet<ModulSec> ModulSecim { get; set; }
}
