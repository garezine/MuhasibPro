namespace Muhasib.Data.BaseRepositories.Contracts;

public interface IUnitOfWork<TContext> : IDisposable where TContext : class
{
    Task<ITransaction> BeginTransactionAsync();
    Task<int> CommitAsync();
    TContext Context { get; }
}
