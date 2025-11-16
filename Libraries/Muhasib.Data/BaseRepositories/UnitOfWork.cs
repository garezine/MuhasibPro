using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Muhasib.Data.BaseRepositories.Contracts;

namespace Muhasib.Data.BaseRepositories
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Context'e erişim sağlar (gerektiğinde)
        /// </summary>
        public TContext Context => _context;

        /// <summary>
        /// Transaction başlatır
        /// Aynı anda sadece bir transaction olabilir
        /// </summary>
        /// <returns>ITransaction - using bloğu ile kullanılmalı</returns>
        public async Task<ITransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("Zaten aktif bir transaction mevcut");

            _currentTransaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
            return new EfTransaction(_currentTransaction);
        }

        /// <summary>
        /// Tüm değişiklikleri database'e kaydeder (SaveChanges)
        /// Transaction içindeyse sadece SaveChanges yapar
        /// Transaction commit'i ayrıca yapılmalıdır
        /// </summary>
        /// <returns>Etkilenen kayıt sayısı</returns>
        public async Task<int> CommitAsync()
        {
            try
            {
                return await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch
            {
                // Hata durumunda transaction varsa rollback
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync().ConfigureAwait(false);
                }
                throw;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}

