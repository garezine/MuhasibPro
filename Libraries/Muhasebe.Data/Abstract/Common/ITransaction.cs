namespace Muhasebe.Data.Abstract.Common;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
