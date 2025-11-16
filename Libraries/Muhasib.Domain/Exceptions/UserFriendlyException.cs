using Muhasib.Domain.Enum;

namespace Muhasib.Domain.Exceptions
{
    public class UserFriendlyException : Exception
    {
        public GlobalErrorCode GlobalErrorCode { get; }
        public string UserTitle { get; }
        public DateTime Timestamp { get; }
        public string TechnicalDetails { get; }
        public bool ShouldLog { get; }

        public UserFriendlyException(GlobalErrorCode errorCode, string userMessage = null, Exception innerException = null)
            : base(userMessage ?? GetDefaultUserMessage(errorCode), innerException)
        {
            GlobalErrorCode = errorCode;
            UserTitle = GetDefaultTitle(errorCode);
            Timestamp = DateTime.Now;
            TechnicalDetails = innerException?.Message;
            ShouldLog = true;
        }

        public UserFriendlyException(GlobalErrorCode errorCode, string userMessage, string technicalDetails, Exception innerException = null)
            : base(userMessage ?? GetDefaultUserMessage(errorCode), innerException)
        {
            GlobalErrorCode = errorCode;
            UserTitle = GetDefaultTitle(errorCode);
            Timestamp = DateTime.Now;
            TechnicalDetails = technicalDetails;
            ShouldLog = true;
        }

        public UserFriendlyException(GlobalErrorCode errorCode, string userTitle, string userMessage, string technicalDetails = null, Exception innerException = null)
            : base(userMessage, innerException)
        {
            GlobalErrorCode = errorCode;
            UserTitle = userTitle;
            Timestamp = DateTime.Now;
            TechnicalDetails = technicalDetails;
            ShouldLog = true;
        }

        private static string GetDefaultUserMessage(GlobalErrorCode errorCode)
        {
            return errorCode switch
            {
                // Veritabanı Hataları
                GlobalErrorCode.DatabaseConnection => "Veritabanına bağlanılamıyor. Lütfen daha sonra tekrar deneyin.",
                GlobalErrorCode.DatabaseTimeout => "Veritabanı işlemi zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin.",
                GlobalErrorCode.DatabaseConstraint => "Veritabanı kısıt ihlali oluştu. Lütfen sistem yöneticinize başvurun.",

                // Ağ Hataları
                GlobalErrorCode.NetworkError => "Ağ bağlantısı kesildi. Lütfen internet bağlantınızı kontrol edin.",
                GlobalErrorCode.ServiceUnavailable => "Servis geçici olarak kullanılamıyor. Lütfen daha sonra tekrar deneyin.",

                // Dosya Sistemi
                GlobalErrorCode.FileSystemError => "Dosya sistemi hatası oluştu. Lütfen disk alanını kontrol edin.",
                GlobalErrorCode.DiskFull => "Disk alanı doldu. Lütfen disk alanını temizleyin.",

                // Kimlik Doğrulama
                GlobalErrorCode.AuthenticationFailed => "Kimlik doğrulama başarısız. Lütfen tekrar giriş yapın.",
                GlobalErrorCode.SessionExpired => "Oturum süreniz doldu. Lütfen tekrar giriş yapın.",

                // Harici Servisler
                GlobalErrorCode.ExternalServiceError => "Harici serviste geçici bir hata oluştu. Lütfen daha sonra tekrar deneyin.",

                _ => "Sistem hatası oluştu. Lütfen daha sonra tekrar deneyin."
            };
        }

        private static string GetDefaultTitle(GlobalErrorCode errorCode)
        {
            return errorCode switch
            {
                GlobalErrorCode.DatabaseConnection or GlobalErrorCode.DatabaseTimeout or GlobalErrorCode.DatabaseConstraint => "Veritabanı Hatası",
                GlobalErrorCode.NetworkError or GlobalErrorCode.ServiceUnavailable => "Ağ Hatası",
                GlobalErrorCode.FileSystemError or GlobalErrorCode.DiskFull => "Dosya Sistemi Hatası",
                GlobalErrorCode.AuthenticationFailed or GlobalErrorCode.SessionExpired => "Kimlik Doğrulama Hatası",
                GlobalErrorCode.ExternalServiceError => "Harici Servis Hatası",
                _ => "Sistem Hatası"
            };
        }

        public override string ToString()
        {
            return $"UserFriendlyException [Code: 0x{GlobalErrorCode:X5}, Title: {UserTitle}, Message: {Message}, Time: {Timestamp:yyyy-MM-dd HH:mm:ss}, Technical: {TechnicalDetails}]";
        }
    }
}
