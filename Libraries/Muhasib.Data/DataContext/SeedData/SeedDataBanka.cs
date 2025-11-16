using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Entities.MuhasebeEntity.Banka;

namespace Muhasib.Data.DataContext.SeedData;

public static class SeedDataBanka
{
    public static void Bankalar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankaListesi>()
            .HasData(
                new BankaListesi
                {
                    Id = 1,
                    BankaAdi = "Ziraat Bankası",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 2,
                    BankaAdi = "Vakıfbank",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 3,
                    BankaAdi = "Halkbank",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 4,
                    BankaAdi = "Garanti Bankası",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 5,
                    BankaAdi = "Akbank",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 6,
                    BankaAdi = "İş Bankası",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 7,
                    BankaAdi = "Yapı Kredi",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 8,
                    BankaAdi = "QNB Finansbank",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 9,
                    BankaAdi = "Denizbank",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new BankaListesi
                {
                    Id = 10,
                    BankaAdi = "TEB",
                    KaydedenId = 5413300800,
                    KayitTarihi = new DateTime(2025, 3, 2)
                });
    }
}
