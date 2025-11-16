using Microsoft.EntityFrameworkCore.Storage;
using Muhasib.Data.BaseRepositories.Contracts;

namespace Muhasib.Data.BaseRepositories
{
    public class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;
        private bool _isCompleted;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Transaction'ı commit eder - Tüm değişiklikleri kalıcı yapar
        /// </summary>
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
                throw new InvalidOperationException("Transaction commit işleminde hata oluştu.", ex);
            }
        }

        /// <summary>
        /// Transaction'ı geri alır - Tüm değişiklikleri iptal eder
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Transaction zaten tamamlandı.");

            try
            {
                await _transaction.RollbackAsync().ConfigureAwait(false);
                _isCompleted = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Transaction rollback işleminde hata oluştu.", ex);
            }
        }

        /// <summary>
        /// Dispose edildiğinde, commit edilmemişse otomatik rollback yapar
        /// </summary>
        public void Dispose()
        {
            if (!_isCompleted)
            {
                // Güvenlik için otomatik rollback
                _transaction.Rollback();
            }
            _transaction.Dispose();
        }
    }

}
