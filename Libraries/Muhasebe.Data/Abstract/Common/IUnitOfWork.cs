namespace Muhasebe.Data.Abstract.Common;

public interface IUnitOfWork<TContext> : IDisposable where TContext : class
{
    TRepository GetRepository<TRepository>() where TRepository : class;

    Task<int> CommitAsync();

    Task<ITransaction> BeginTransactionAsync();
    TContext Context { get; }
}
