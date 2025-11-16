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

        public DbSet<AppLog> AppLogs { get; set; }
        public DbSet<MuhasebeVersiyon> MuhasebeVersiyonlar { get; set; }
    }
}
