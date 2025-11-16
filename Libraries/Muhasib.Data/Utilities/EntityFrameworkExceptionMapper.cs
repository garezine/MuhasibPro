using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Muhasib.Domain.Enum;
using Muhasib.Domain.Exceptions;

namespace Muhasib.Data.Utilities
{
    public static class EntityFrameworkExceptionMapper
    {
        public static UserFriendlyException ToUserFriendly(this Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            var (errorCode, userMessage) = sqlEx.Number switch
            {
                // Connection errors
                -1 or 2 or 53 => (GlobalErrorCode.SqlConnectionFailed, "Veritabanı sunucusuna bağlanılamıyor"),
                -2 => (GlobalErrorCode.SqlTimeout, "Veritabanı işlemi zaman aşımına uğradı"),

                // Constraint violations
                547 => (GlobalErrorCode.SqlForeignKeyViolation, "Bu kayıt başka tablolarda kullanıldığı için işlem yapılamaz"),
                2627 => (GlobalErrorCode.SqlUniqueConstraint, "Bu kayıt zaten sistemde mevcut"),
                2601 => (GlobalErrorCode.SqlDuplicateKey, "Benzersiz anahtar ihlali - kayıt zaten var"),

                // Data errors
                515 => (GlobalErrorCode.SqlCannotInsertNull, "Zorunlu alanlarda eksik veri var"),
                8115 => (GlobalErrorCode.SqlArithmeticOverflow, "Sayısal değer izin verilen aralığı aşıyor"),
                8152 => (GlobalErrorCode.SqlStringTruncation, "Metin alanı izin verilen uzunluğu aşıyor"),

                // Deadlock
                1205 => (GlobalErrorCode.SqlDeadlock, "Veritabanı kilidi hatası - lütfen tekrar deneyin"),

                // Default
                _ => (GlobalErrorCode.SqlConstraintViolation, "Veritabanı işlemi sırasında hata oluştu")
            };

            return new UserFriendlyException(errorCode, userMessage, $"SQL Error {sqlEx.Number}: {sqlEx.Message}", sqlEx);
        }

        public static UserFriendlyException ToUserFriendly(this DbUpdateException dbEx)
        {
            // Inner exception'dan SqlException'ı bul
            var sqlEx = dbEx.InnerException as Microsoft.Data.SqlClient.SqlException
                       ?? dbEx.InnerException?.InnerException as Microsoft.Data.SqlClient.SqlException;

            if (sqlEx != null)
            {
                return sqlEx.ToUserFriendly();
            }

            // SqlException değilse generic hata
            return new UserFriendlyException(GlobalErrorCode.DatabaseConstraint,
                "Veritabanı güncelleme hatası oluştu",
                dbEx.InnerException?.Message ?? dbEx.Message,
                dbEx);
        }

        public static UserFriendlyException ToUserFriendly(this DbUpdateConcurrencyException concEx)
        {
            return new UserFriendlyException(GlobalErrorCode.DatabaseConstraint,
                "Kayıt başka bir kullanıcı tarafından değiştirildi",
                "Veri eşzamanlılık ihlali - lütfen sayfayı yenileyip tekrar deneyin",
                concEx);
        }
    }
}
