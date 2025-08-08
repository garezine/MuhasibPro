using Microsoft.EntityFrameworkCore.Storage;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Data.EfRepositories.Common
{
    public class EfTransaction : ITransaction, IDisposable
    {
        private readonly IDbContextTransaction _transaction;
        private bool _isCompleted;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public async Task CommitAsync()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Transaction zaten tamamlandı.");

            try
            {
                await _transaction.CommitAsync().ConfigureAwait(false);
                _isCompleted = true;
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                throw new InvalidOperationException("Transaction commit işleminde hata oluştu.", ex);
            }
        }

        public async Task RollbackAsync()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Transaction zaten tamamlandı veya geri alındı.");

            try
            {
                await _transaction.RollbackAsync().ConfigureAwait(false);
                _isCompleted = true;
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                throw new InvalidOperationException("Transaction rollback işleminde hata oluştu.", ex);
            }
        }

        public void Dispose()
        {
            if (!_isCompleted)
            {
                // Güvenlik için rollback yapılabilir
                _transaction.Rollback();
            }
            _transaction.Dispose();
        }
    }

}
