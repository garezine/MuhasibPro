using Microsoft.EntityFrameworkCore.Storage;
using Muhasib.Data.BaseRepositories.Contracts;

namespace Muhasib.Data.BaseRepositories
{
    public class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;
        private readonly Action _onDispose; // <-- YENİ EKLENEN: Bittiğinde çalışacak kod
        private bool _isCompleted;

        // Constructor güncellendi
        public EfTransaction(IDbContextTransaction transaction, Action onDispose)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
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
                throw new InvalidOperationException("Transaction commit işleminde hata oluştu.", ex);
            }
        }

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

        public void Dispose()
        {
            if (!_isCompleted)
            {
                try
                {
                    _transaction.Rollback();
                }
                catch
                {
                    // Rollback hatası yutulabilir veya loglanabilir
                }
            }

            _transaction.Dispose();

            // BURASI KRİTİK: UnitOfWork'e haber veriyoruz
            _onDispose.Invoke();
        }
    }
}