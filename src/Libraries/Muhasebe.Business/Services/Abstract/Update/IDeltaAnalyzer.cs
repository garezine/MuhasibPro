using Muhasebe.Business.Models;
using System.Collections.Concurrent;

namespace Muhasebe.Business.Services.Abstract.Update
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
