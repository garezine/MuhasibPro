using Muhasib.Domain.Enum;

namespace Muhasib.Data.DataContext
{
    public class TenantContext
    {
        public long MaliDonemId { get; set; }
        public string DatabaseName { get; set; }
        public string DatabasePath { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public DateTime LoadedAt { get; set; }
        public string ConnectionString { get; set; }
        public bool IsLoaded => !string.IsNullOrEmpty(DatabaseName);

        public static TenantContext Empty => new TenantContext();
    }
}
