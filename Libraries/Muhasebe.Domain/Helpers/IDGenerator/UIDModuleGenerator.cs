﻿namespace Muhasebe.Domain.Helpers.IDGenerator
{
    public static class UIDModuleGenerator
    {
        // Base sınıfın fonksiyonelliğini kullanmak için statik bir örnek
        private static readonly BaseUIDGenerator _baseGenerator = new BaseUIDGenerator();

        public static long GenerateModuleId(UIDModuleType moduleType)
        {
            // Modül türüne göre ID üretimini base sınıf üzerinden çağır
            return _baseGenerator.GenerateId((long)moduleType);
        }
    }
}
