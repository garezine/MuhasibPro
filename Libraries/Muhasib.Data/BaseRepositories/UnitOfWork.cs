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

        public TContext Context => _context;

        public async Task<ITransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                // Mevcut transaction varsa onu dönebilir veya hata fırlatabilirsiniz.
                // Genelde nested transaction EF Core'da desteklenmez (Savepoint hariç),
                // bu yüzden hata fırlatmak daha güvenlidir.
                throw new InvalidOperationException("Zaten aktif bir transaction mevcut.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();

            // EfTransaction wrapper'ına (kendi yazdığınız sınıf) context ve transaction'ı veriyoruz.
            // Önemli: Transaction bittiğinde _currentTransaction null yapılmalı.
            return new EfTransaction(_currentTransaction, () => _currentTransaction = null);
        }

        /// <summary>
        /// Değişiklikleri veritabanına yazar.
        /// İsimlendirme SaveChangesAsync olarak değiştirildi çünkü yaptığı iş bu.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch
            {
                // Hata durumunda transaction bütünlüğünü korumak için rollback yapılabilir
                // Ancak bu kararı çağıran katmana (service layer) bırakmak bazen daha iyidir.
                // Eğer burada rollback yapacaksanız:
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
                throw;
            }
        }

        public void Dispose()
        {
            // Sadece transaction'ı dispose ediyoruz. 
            // Context DI container tarafından dispose edilecektir.
            _currentTransaction?.Dispose();
            _currentTransaction = null;

            // GC.SuppressFinalize(this); // Eğer finalizer yoksa buna gerek yok ama eklenebilir.
        }
    }
}