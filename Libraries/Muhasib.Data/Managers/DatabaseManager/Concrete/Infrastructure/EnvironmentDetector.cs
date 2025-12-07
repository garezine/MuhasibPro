using Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.Infrastructure
{
    public class EnvironmentDetector : IEnvironmentDetector
    {
        public bool IsDevelopment()
        {
            // DEBUG = Geliştirme = Proje klasörü
            // RELEASE = Kullanım = AppData
#if DEBUG
            return true;
#else
    return false;
#endif
        }

        public bool IsProduction() => !IsDevelopment();
    }
}
