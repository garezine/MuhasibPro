namespace Muhasib.Business.Models.TenantModel
{
    public class TenantDetailsModel
    {
        public long MaliDonemId { get; set; }
        public long FirmaId { get; set; }
        public string FirmaKodu { get; set; }
        public string FirmaUnvani { get; set; }
        public int MaliYil { get; set; }
        public string DatabaseName { get; set; }
        public string Directory { get; set; }
        public string DatabasePath { get; set; }        
        public DateTime? LastBackupDate { get; set; }
        
        public int PendingMigrations { get; set; }
        public bool IsHealthy { get; set; }
    }
}
