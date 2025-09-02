namespace Muhasebe.Data.Database.Interfaces.Services
{
    public interface IDatabaseSelectionService
    {
        /// <summary>
        /// Belirtilen Firma ve Dönem için veritabanı bağlantısını seçer ve etkinleştirir.
        /// </summary>
        /// <param name="fId">Seçilen Firma ID.</param>
        /// <param name="dId">Seçilen Dönem ID.</param>
        /// <returns>İşlem başarılı olursa true, aksi takdirde false döner veya hata fırlatır.</returns>
        /// <exception cref="KeyNotFoundException">Firma veya Dönem için veritabanı kaydı bulunamazsa.</exception>
        /// <exception cref="InvalidOperationException">Veritabanı konfigürasyonu sırasında bir hata oluşursa.</exception>
        Task<bool> SelectDatabaseAsync(long fId, long dId);

        /// <summary>
        /// Mevcut veritabanı seçimini sıfırlar (varsayılan duruma getirir).
        /// Örneğin, kullanıcı çıkış yaptığında veya Firma/Dönem değiştirmeden önce kullanılır.
        /// </summary>
        void ResetSelection();

        /// <summary>
        /// Şu anda bir veritabanının seçili olup olmadığını kontrol eder.
        /// </summary>
        /// <returns>Bir veritabanı seçiliyse true, değilse false.</returns>
        bool IsDatabaseSelected();
    }
}
