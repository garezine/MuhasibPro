namespace Muhasib.Business.Models.TenantModel
{
    public class TenantCreationRequest
    {
        public long FirmaId { get; set; }
        public int MaliYil { get; set; }
        public bool AutoCreateDatabase { get; set; } = true;
        public bool RunMigrations { get; set; } = true;
        public bool CreateInitialBackup { get; set; } = false;
    }
}
