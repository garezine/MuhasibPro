using Microsoft.EntityFrameworkCore;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.DataContext.DataSource
{
    public interface ISistemDataSource
    {
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
