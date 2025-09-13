namespace Muhasebe.Data.Abstracts.Common;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
