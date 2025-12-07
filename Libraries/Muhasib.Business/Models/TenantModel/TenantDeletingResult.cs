namespace Muhasib.Business.Models.TenantModel
{
    public class TenantDeletingResult
    {
        public string DatabaseName { get; set; }
        public long MaliDonemId { get; set; }
        public bool DatabaseDeleted { get; set; }       
        public string Message { get; set; }
    }
}
