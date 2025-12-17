namespace Muhasib.Business.Models.TenantModel
{
    public class TenantDeletingRequest
    {
        public long MaliDonemId { get; set; } 
        public string DatabaseName { get; set; }
        public bool DisconnectTenant { get; set; } = false;
        public bool DeletedTenantBackup { get; set; } = true;
        public bool IsDeleteDatabase { get; set; } = false;
        public string BackupFilePath { get; set; }
    }
}
