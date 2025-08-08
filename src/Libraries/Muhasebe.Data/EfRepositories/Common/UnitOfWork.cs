using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Data.EfRepositories.Common
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly IServiceProvider _serviceProvider;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(TContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }
        public TContext Context => _context;

        // Özel Repository'leri dinamik olarak çözümle
        public TRepository GetRepository<TRepository>() where TRepository : class
        {
            // Eğer repository bir generic tür ise çözümle
            if (typeof(TRepository).IsGenericType && typeof(TRepository).GetGenericTypeDefinition() == typeof(IGenericRepository<>))
            {
                var entityType = typeof(TRepository).GetGenericArguments()[0]; // Generic tür parametresini al
                var repoType = typeof(GenericRepository<>).MakeGenericType(entityType); // Generic türü oluştur

                // DbContext'i kullanarak GenericRepository örneği oluştur
                return (TRepository)Activator.CreateInstance(repoType, _context);
            }

            // Standart türler için çözümle
            var repo = _serviceProvider.GetService<TRepository>();
            if (repo == null)
                throw new InvalidOperationException($"Repository {typeof(TRepository).Name} kayıtlı değil.");

            return repo;
        }

        public async Task<ITransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("Zaten aktif bir transaction mevcut");

            _currentTransaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
            return new EfTransaction(_currentTransaction);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}