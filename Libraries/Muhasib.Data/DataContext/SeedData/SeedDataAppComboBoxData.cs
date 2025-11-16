using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.DataContext.SeedData
{
    public static class SeedDataAppComboBoxData
    {
        public static void HatirlatmaTur(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HatirlatmaTurler>()
                .HasData(
                    new HatirlatmaTurler
                    {
                        Id = 1,
                        TurAdi = "Genel",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new HatirlatmaTurler
                    {
                        Id = 2,
                        TurAdi = "Borç",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new HatirlatmaTurler
                    {
                        Id = 3,
                        TurAdi = "Alacak",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    });
        }
        public static void OdemeSekilleri(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OdemeTurler>()
                .HasData(
                    new OdemeTurler
                    {
                        Id = 1,
                        OdemeTuru = "Nakit",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new OdemeTurler
                    {
                        Id = 2,
                        OdemeTuru = "Havale",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new OdemeTurler
                    {
                        Id = 3,
                        OdemeTuru = "Kredi Kartı",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    });
        }
        public static void ParaBirimi(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParaBirimler>()
                .HasData(
                    new ParaBirimler
                    {
                        Id = 1,
                        Kisaltmasi = "TL",
                        PB = "Türk Lirası",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new ParaBirimler
                    {
                        Id = 2,
                        Kisaltmasi = "USD",
                        PB = "Amerikan Doları",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    },
                    new ParaBirimler
                    {
                        Id = 3,
                        Kisaltmasi = "EUR",
                        PB = "Euro",
                        KaydedenId = 5413300800,
                        KayitTarihi = new DateTime(2025, 3, 2)
                    });
        }
    }
}
