using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.DataContext.SeedData;

public static class SeedDatailler
{
    public static void IlListesi(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Iller>()
            .HasData(
                new Iller { Id = 1, IlAdi = "Adana", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 2,
                    IlAdi = "Adıyaman",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 3,
                    IlAdi = "Afyonkarahisar",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 4, IlAdi = "Ağrı", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 5, IlAdi = "Amasya", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 6, IlAdi = "Ankara", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 7,
                    IlAdi = "Antalya",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 8, IlAdi = "Artvin", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 9, IlAdi = "Aydın", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 10,
                    IlAdi = "Balıkesir",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 11,
                    IlAdi = "Bilecik",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 12,
                    IlAdi = "Bingöl",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller
                {
                    Id = 13,
                    IlAdi = "Bitlis",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 14, IlAdi = "Bolu", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller
                {
                    Id = 15,
                    IlAdi = "Burdur",
                    KaydedenId = 24134175366,
                    KayitTarihi = new DateTime(2025, 3, 2)
                },
                new Iller { Id = 16, IlAdi = "Bursa", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 17, IlAdi = "Çanakkale", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 18, IlAdi = "Çankırı", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 19, IlAdi = "Çorum", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 20, IlAdi = "Denizli", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 21, IlAdi = "Diyarbakır", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 22, IlAdi = "Edirne", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 23, IlAdi = "Elazığ", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 24, IlAdi = "Erzincan", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 25, IlAdi = "Erzurum", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 26, IlAdi = "Eskişehir", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 27, IlAdi = "Gaziantep", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 28, IlAdi = "Giresun", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 29, IlAdi = "Gümüşhane", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 30, IlAdi = "Hakkari", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 31, IlAdi = "Hatay", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 32, IlAdi = "Isparta", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 33, IlAdi = "Mersin", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 34, IlAdi = "İstanbul", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 35, IlAdi = "İzmir", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 36, IlAdi = "Kars", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 37, IlAdi = "Kastamonu", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 38, IlAdi = "Kayseri", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 39, IlAdi = "Kırklareli", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 40, IlAdi = "Kırşehir", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 41, IlAdi = "Kocaeli", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 42, IlAdi = "Konya", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 43, IlAdi = "Kütahya", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 44, IlAdi = "Malatya", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 45, IlAdi = "Manisa", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 46, IlAdi = "Kahramanmaraş", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 47, IlAdi = "Mardin", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 48, IlAdi = "Muğla", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 49, IlAdi = "Muş", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 50, IlAdi = "Nevşehir", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 51, IlAdi = "Niğde", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 52, IlAdi = "Ordu", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 53, IlAdi = "Rize", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 54, IlAdi = "Sakarya", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 55, IlAdi = "Samsun", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 56, IlAdi = "Siirt", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 57, IlAdi = "Sinop", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 58, IlAdi = "Sivas", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 59, IlAdi = "Tekirdağ", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 60, IlAdi = "Tokat", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 61, IlAdi = "Trabzon", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 62, IlAdi = "Tunceli", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 63, IlAdi = "Şanlıurfa", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 64, IlAdi = "Uşak", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 65, IlAdi = "Van", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 66, IlAdi = "Yozgat", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 67, IlAdi = "Zonguldak", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 68, IlAdi = "Aksaray", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 69, IlAdi = "Bayburt", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 70, IlAdi = "Karaman", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 71, IlAdi = "Kırıkkale", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 72, IlAdi = "Batman", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 73, IlAdi = "Şırnak", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 74, IlAdi = "Bartın", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 75, IlAdi = "Ardahan", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 76, IlAdi = "Iğdır", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 77, IlAdi = "Yalova", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 78, IlAdi = "Karabük", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 79, IlAdi = "Kilis", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 80, IlAdi = "Osmaniye", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) },
                new Iller { Id = 81, IlAdi = "Düzce", KaydedenId = 24134175366, KayitTarihi = new DateTime(2025, 3, 2) });
    }
}
