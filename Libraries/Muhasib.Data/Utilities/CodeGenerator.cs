using System.Collections.Concurrent;

namespace Muhasib.Data.Utilities
{
    public static class CodeGenerator
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public static async Task<string> GenerateCodeAsync(
            Func<Task<IEnumerable<string>>> getExistingCodes,
            CodeGenerationRequest request = null,
            string customCode = null)
        {
            request ??= new CodeGenerationRequest();
            var lockKey = $"{request.Prefix}_{request.DigitLength}";

            var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                return await GenerateCodeInternalAsync(getExistingCodes, request, customCode);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static async Task<string> GenerateCodeInternalAsync(
            Func<Task<IEnumerable<string>>> getExistingCodes,
            CodeGenerationRequest request,
            string customCode)
        {
            var existingCodes = (await getExistingCodes())?.ToList() ?? new List<string>();

            // 1. ÖZEL KOD KONTROLÜ
            if (!string.IsNullOrWhiteSpace(customCode))
            {
                if (!request.AllowCustomCode)
                    throw new InvalidOperationException("Özel kod girişi desteklenmiyor");

                if (IsCodeExists(customCode, existingCodes, request.CaseSensitive))
                    throw new InvalidOperationException($"Kod zaten mevcut: {customCode}");

                if (!IsValidCodeFormat(customCode, request))
                    throw new InvalidOperationException($"Geçersiz kod formatı: {customCode}");

                return customCode.Trim();
            }

            // 2. OTOMATİK KOD ÜRETİMİ
            return GenerateAutoCode(existingCodes, request); // ✅ Async değil!
        }

        private static string GenerateAutoCode(List<string> existingCodes, CodeGenerationRequest request)
        {
            var prefixCodes = existingCodes
                .Where(code => code.StartsWith(request.Prefix, request.CaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 3. BOŞLUKLARI DOLDUR
            if (request.FillGaps && prefixCodes.Any())
            {
                var gapCode = FindFirstGap(prefixCodes, request);
                if (gapCode != null)
                    return gapCode;
            }

            // 4. YENİ NUMARA ÜRET
            return GenerateNextSequenceCode(prefixCodes, request);
        }

        private static string FindFirstGap(List<string> codes, CodeGenerationRequest request)
        {
            var numbers = codes
                .Select(code => ExtractNumber(code, request.Prefix))
                .Where(num => num.HasValue)
                .Select(num => num.Value)
                .OrderBy(num => num)
                .ToList();

            if (!numbers.Any())
                return null;

            var min = numbers.Min();
            var max = numbers.Max();

            for (long i = min; i <= max; i++)
            {
                if (!numbers.Contains(i))
                {
                    return FormatCode(request.Prefix, i, request.DigitLength);
                }
            }

            return null;
        }

        private static string GenerateNextSequenceCode(List<string> codes, CodeGenerationRequest request)
        {
            var maxNumber = codes
                .Select(code => ExtractNumber(code, request.Prefix))
                .Where(num => num.HasValue)
                .Select(num => num.Value) // ✅ .Value eklendi
                .DefaultIfEmpty(0)
                .Max();

            return FormatCode(request.Prefix, maxNumber + 1, request.DigitLength);
        }

        private static long? ExtractNumber(string code, string prefix)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(prefix))
                return null;

            if (!code.StartsWith(prefix))
                return null;

            var numberPart = code.Substring(prefix.Length);
            return long.TryParse(numberPart, out var number) ? number : null;
        }

        private static string FormatCode(string prefix, long number, int digitLength)
        {
            return $"{prefix}{number.ToString().PadLeft(digitLength, '0')}";
        }

        private static bool IsCodeExists(string code, List<string> existingCodes, bool caseSensitive)
        {
            return existingCodes.Any(ec => string.Equals(ec, code,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsValidCodeFormat(string code, CodeGenerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (!code.StartsWith(request.Prefix))
                return false;

            var numberPart = code.Substring(request.Prefix.Length);
            if (!long.TryParse(numberPart, out var number))
                return false;

            return numberPart.Length == request.DigitLength;
        }
    }

    public class CodeGenerationRequest
    {
        public string Prefix { get; set; } = "F-";
        public int DigitLength { get; set; } = 4;
        public bool AllowCustomCode { get; set; } = true;
        public bool FillGaps { get; set; } = true;
        public bool CaseSensitive { get; set; } = false;
    }
}

