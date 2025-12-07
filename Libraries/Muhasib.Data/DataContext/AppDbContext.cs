using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.DataContext
{
    public class AppDbContext : DbContext
    {
        protected AppDbContext()
        {
        }
        public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
       
        public DbSet<TenantDatabaseVersiyon> TenantDatabaseVersions { get; set; }

        public DbSet<AppLog> AppLogs { get; set; }
       
    }
}
