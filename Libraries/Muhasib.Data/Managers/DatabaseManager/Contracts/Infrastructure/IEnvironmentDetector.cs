namespace Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure
{
    public interface IEnvironmentDetector
    {
        bool IsDevelopment();
        bool IsProduction();
    }
}
