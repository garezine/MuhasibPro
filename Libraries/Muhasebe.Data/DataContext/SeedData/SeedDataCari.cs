using Microsoft.EntityFrameworkCore;
using Muhasebe.Domain.Entities.MuhasebeEntity.Cari;

namespace Muhasebe.Data.DataContext.SeedData;

public static class SeedDataCari
{
    public static void CariGruplar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CariGrup>()
            .HasData(
                new CariGrup
                {
                    Id = 1,
                    GrupAdi = "Genel",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new CariGrup
                {
                    Id = 2,
                    GrupAdi = "Tedarikçi",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new CariGrup
                {
                    Id = 3,
                    GrupAdi = "Müşteri",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                });
    }
}
