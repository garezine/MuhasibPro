namespace Muhasebe.Domain.Interfaces.Database;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
