using Muhasib.Domain.Enum;

namespace Muhasib.Data.DataContext
{
    public class TenantContext
    {       
        public string DatabaseName { get; set; }        
        public DatabaseType DatabaseType { get; set; }
        public DateTime LoadedAt { get; set; }        
        public string ConnectionString { get; set; }
        public bool IsLoaded => !string.IsNullOrEmpty(DatabaseName);

        public static TenantContext Empty => new TenantContext();
    }
}
