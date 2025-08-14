using Muhasebe.Business.Helpers;
using Muhasebe.Domain.Entities.Sistem;
using System.Collections.Concurrent;

namespace Muhasebe.Business.Services.Abstract.Common
{
    public interface IDeltaAnalyzer
    {
        Task<bool> CanApplyDeltaUpdate(DeltaUpdateInfo deltaInfo);
        Task<List<string>> GetChangedFilesAsync(string fromVersion, string toVersion);
        Task<ConcurrentDictionary<string, string>> GetCurrentFileHashesAsync();
        DeltaUpdateInfo ParseDeltaInfo(string releaseBody, GitHubAsset[] assets);
        Task<string[]> AnalyzeChangedFilesAsync(DeltaUpdateInfo deltaInfo);
    }
}
