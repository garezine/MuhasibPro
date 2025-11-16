namespace Muhasib.Data.BaseRepositories.Contracts;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
