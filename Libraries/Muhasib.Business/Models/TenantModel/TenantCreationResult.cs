namespace Muhasib.Business.Models.TenantModel
{
    public class TenantCreationResult
    {
        public long MaliDonemId { get; set; }
        public long MaliDonemDbId { get; set; }
        public string DatabaseName { get; set; }
        public bool DatabaseCreated { get; set; }
        public bool MigrationsRun { get; set; }
        public string Message { get; set; }
    }
}
