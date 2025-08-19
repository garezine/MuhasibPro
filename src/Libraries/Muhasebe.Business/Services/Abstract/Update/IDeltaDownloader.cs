using Muhasebe.Business.Models;

namespace Muhasebe.Business.Services.Abstract.Update
{
    public interface IDeltaDownloader
    {
     
        Task<bool> DownloadDeltaUpdateAsync(DeltaUpdateInfo deltaInfo, IProgress<(long downloaded, long total, double speed)> progress = null);
    }
}
