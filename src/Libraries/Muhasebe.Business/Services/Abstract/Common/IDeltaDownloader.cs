using Muhasebe.Domain.Entities.Sistem;

namespace Muhasebe.Business.Services.Abstract.Common
{
    public interface IDeltaDownloader
    {
     
        Task<bool> DownloadDeltaUpdateAsync(DeltaUpdateInfo deltaInfo, IProgress<(long downloaded, long total, double speed)> progress = null);
    }
}
