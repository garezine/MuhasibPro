using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Entities.MuhasebeEntity.Personel;

namespace Muhasib.Data.DataContext.SeedData;

public static class SeedDataPersonel
{
    public static void PersonelBolumler(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonelBolum>()
            .HasData(
                new PersonelBolum { Id = 1, BolumAdi = "Yönetim" },
                new PersonelBolum { Id = 2, BolumAdi = "Muhasebe" },
                new PersonelBolum { Id = 3, BolumAdi = "Satın Alma" },
                new PersonelBolum { Id = 4, BolumAdi = "Depo" },
                new PersonelBolum { Id = 5, BolumAdi = "Üretim" },
                new PersonelBolum { Id = 6, BolumAdi = "Pazarlama" },
                new PersonelBolum { Id = 7, BolumAdi = "İnsan Kaynakları" },
                new PersonelBolum { Id = 8, BolumAdi = "Bilgi İşlem" },
                new PersonelBolum { Id = 9, BolumAdi = "Kalite Kontrol" },
                new PersonelBolum { Id = 10, BolumAdi = "Müşteri Hizmetleri" });
    }
    public static void PersonelGorev(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonelGorev>()
            .HasData(
                new PersonelGorev { Id = 1, GorevAdi = "Genel" },
                new PersonelGorev { Id = 2, GorevAdi = "Görevli" },
                new PersonelGorev { Id = 3, GorevAdi = "Müdür" },
                new PersonelGorev { Id = 4, GorevAdi = "Şef" },
                new PersonelGorev { Id = 5, GorevAdi = "İşçi" },
                new PersonelGorev { Id = 6, GorevAdi = "Uzman" },
                new PersonelGorev { Id = 7, GorevAdi = "Usta" },
                new PersonelGorev { Id = 8, GorevAdi = "Ustabaşı" },
                new PersonelGorev { Id = 9, GorevAdi = "Sekreter" },
                new PersonelGorev { Id = 10, GorevAdi = "Mühendis" });
    }
}

