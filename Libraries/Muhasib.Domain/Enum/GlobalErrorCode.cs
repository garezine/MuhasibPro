using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muhasib.Domain.Enum
{
    public enum GlobalErrorCode
    {
        // Genel Sistem Hataları: 0x00001 - 0x00FFF
        GeneralError = 0x00001,
        
        // Veritabanı Hataları: 0x01000 - 0x01FFF
        DatabaseConnection = 0x01001,
        DatabaseTimeout = 0x01002,
        DatabaseConstraint = 0x01003,
        DatabaseDeadlock = 0x01004,
        
        // Ağ ve İletişim Hataları: 0x02000 - 0x02FFF
        NetworkError = 0x02001,
        ServiceUnavailable = 0x02002,
        RequestTimeout = 0x02003,
        DnsResolutionFailed = 0x02004,
        
        // Dosya Sistemi Hataları: 0x03000 - 0x03FFF
        FileSystemError = 0x03001,
        FileNotFound = 0x03002,
        AccessDenied = 0x03003,
        DiskFull = 0x03004,
        PathTooLong = 0x03005,
        
        // Kimlik Doğrulama ve Yetkilendirme: 0x04000 - 0x04FFF
        AuthenticationFailed = 0x04001,
        AuthorizationDenied = 0x04002,
        SessionExpired = 0x04003,
        InvalidToken = 0x04004,
        
        // Harici Servis Hataları: 0x05000 - 0x05FFF
        ExternalServiceError = 0x05001,
        ApiLimitExceeded = 0x05002,
        InvalidApiResponse = 0x05003,
        ServiceMaintenance = 0x05004,
        
        // Sistem Kaynak Hataları: 0x06000 - 0x06FFF
        MemoryAllocationFailed = 0x06001,
        ResourceExhausted = 0x06002,
        OutOfMemory = 0x06003,
        
        // Yazıcı ve Çıktı Hataları: 0x07000 - 0x07FFF
        PrinterError = 0x07001,
        PrintSpoolerError = 0x07002,
        DocumentGenerationFailed = 0x07003,

            // EntityFramework & SQL Hataları: 0x08000 - 0x08FFF
        SqlConnectionFailed = 0x08001,
        SqlTimeout = 0x08002,
        SqlDeadlock = 0x08003,
        SqlConstraintViolation = 0x08004,
        SqlUniqueConstraint = 0x08005,      // 2627
        SqlForeignKeyViolation = 0x08006,   // 547
        SqlDuplicateKey = 0x08007,          // 2601
        SqlArithmeticOverflow = 0x08008,    // 8115
        SqlStringTruncation = 0x08009,      // 8152
        SqlCannotInsertNull = 0x0800A,      // 515
        SqlNetworkError = 0x0800B           // -1, -2, 53, 121
    }
}
