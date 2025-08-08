using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Muhasebe.Data.DataContext.DesignTimeFactory;

public class DesignTimeSistemDbContextFactory : IDesignTimeDbContextFactory<AppSistemDbContext>
{
    public AppSistemDbContext CreateDbContext(string[] args)
    {
        var configuration = ConfigurationDb.GetConfiguration();
        var dbPath = Path.Combine("Databases");

        // Eğer klasör yoksa oluştur
        if (!Directory.Exists(dbPath))
        {
            Directory.CreateDirectory(dbPath);
            Console.WriteLine($"Klasör oluşturuldu: {dbPath}");
        }

        // SQLite bağlantı dizesini ayarla


        // DbContext ayarlarını yap ve geri döndür
        // DbContextOptions oluştur
        var optionsBuilder = new DbContextOptionsBuilder<AppSistemDbContext>();
        var connectionString = configuration.GetConnectionString("SistemConnection");
        Console.WriteLine($"Bağlantı Dizgisi: {connectionString}");
        optionsBuilder.UseSqlite(connectionString);

        // DbContext örneği oluştur ve döndür
        return new AppSistemDbContext(optionsBuilder.Options);
    }



}


