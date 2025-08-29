using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Interfaces.Provider
{
    public interface IDatabaseProviderFactory
    {
        IDatabaseProvider Create(DatabaseType dbType);
    }
}
