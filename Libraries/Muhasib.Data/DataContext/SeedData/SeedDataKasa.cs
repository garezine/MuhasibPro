using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Entities.MuhasebeEntity.Kasa;

namespace Muhasib.Data.DataContext.SeedData;

public static class SeedDataKasa
{
    public static void KasaAdlari(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kasalar>()
            .HasData(
                new Kasalar
                {
                    Id = 22,
                    KasaAdi = "Şirket Kasası",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Kasalar
                {
                    Id = 23,
                    KasaAdi = "POS",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                });
    }


}
