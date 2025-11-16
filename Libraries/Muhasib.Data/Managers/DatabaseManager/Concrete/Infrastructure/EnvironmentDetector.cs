using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure
{
    public class EnvironmentDetector : IEnvironmentDetector
    {
        public bool IsDevelopment()
        {
#if DEBUG
            return true;
#else
            var currentDir = AppContext.BaseDirectory;
            return currentDir.Contains("\\bin\\") && 
                   (currentDir.Contains("Debug") || currentDir.Contains("Release"));
#endif
        }

        public bool IsProduction() => !IsDevelopment();
    }
}
