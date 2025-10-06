using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.Abstracts.Sistem;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;
using System.Text.RegularExpressions;

namespace Muhasebe.Business.Tools
{
    public static class KodGenerator
    {
        /// <summary>
        /// Firma kodu oluşturur (F-001, F-002...)
        /// </summary>
        public static string FirmaKoduOlustur(IEnumerable<string> mevcutKodlar, string kullaniciKodu = null)
        {            
            return KodOlustur(mevcutKodlar, kullaniciKodu, "F-", 4);
        } 
        public static async Task<List<string>> GetMevcutFirmaKodlari()
        {
            var request = new DataRequest<Firma>();
            var firmalar = Ioc.Default.GetService<IFirmaRepository>().GetQuery(request);
            return await firmalar
                .Where(f=> !string.IsNullOrWhiteSpace(f.FirmaKodu))
                .Select(f => f.FirmaKodu)
                .ToListAsync();
        }

        /// <summary>
        /// Stok kodu oluşturur (STK-00001, STK-00002...)
        /// </summary>
        public static string StokKoduOlustur(IEnumerable<string> mevcutKodlar, string kullaniciKodu = null)
        {
            return KodOlustur(mevcutKodlar, kullaniciKodu, "STK-", 5);
        }

        /// <summary>
        /// Cari kodu oluşturur (C-0001, C-0002...)
        /// </summary>
        public static string CariKoduOlustur(IEnumerable<string> mevcutKodlar, string kullaniciKodu = null)
        {
            return KodOlustur(mevcutKodlar, kullaniciKodu, "C-", 4);
        }

        /// <summary>
        /// Genel kod oluşturucu
        /// </summary>
        public static string KodOlustur(
            IEnumerable<string> mevcutKodlar,
            string kullaniciKodu = null,
            string varsayilanPrefix = "K-",
            int basamakSayisi = 3)
        {
            var kodListesi = mevcutKodlar?.Where(k => !string.IsNullOrWhiteSpace(k)).ToList()
                           ?? new List<string>();

            // Kullanıcı kod girdiyse
            if (!string.IsNullOrWhiteSpace(kullaniciKodu))
            {
                // Kod zaten varsa arttır
                if (kodListesi.Contains(kullaniciKodu, StringComparer.OrdinalIgnoreCase))
                {
                    return KoduArttir(kullaniciKodu);
                }
                // Yoksa olduğu gibi kullan
                return kullaniciKodu;
            }

            // Otomatik üret
            if (!kodListesi.Any())
            {
                return $"{varsayilanPrefix}{1.ToString().PadLeft(basamakSayisi, '0')}";
            }

            // Prefix'e göre filtrele
            var ayniPrefixKodlar = kodListesi
                .Where(k => k.StartsWith(varsayilanPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!ayniPrefixKodlar.Any())
            {
                return $"{varsayilanPrefix}{1.ToString().PadLeft(basamakSayisi, '0')}";
            }

            // En büyük numarayı bul
            var enBuyukNumara = ayniPrefixKodlar
                .Select(k => NumarayiAl(k))
                .Where(n => n > 0)
                .DefaultIfEmpty(0)
                .Max();

            var yeniNumara = enBuyukNumara + 1;
            return $"{varsayilanPrefix}{yeniNumara.ToString().PadLeft(basamakSayisi, '0')}";
        }

        /// <summary>
        /// Kodu bir arttırır (B-001 -> B-002)
        /// </summary>
        public static string KoduArttir(string kod)
        {
            if (string.IsNullOrWhiteSpace(kod))
                throw new ArgumentException("Kod boş olamaz");

            // Son sayıyı bul
            var match = Regex.Match(kod, @"^(.*?)(\d+)([^\d]*)$");

            if (!match.Success)
                throw new ArgumentException($"Geçersiz kod formatı: {kod}");

            var prefix = match.Groups[1].Value;
            var numara = match.Groups[2].Value;
            var suffix = match.Groups[3].Value;

            var yeniNumara = int.Parse(numara) + 1;
            var yeniNumaraStr = yeniNumara.ToString().PadLeft(numara.Length, '0');

            return $"{prefix}{yeniNumaraStr}{suffix}";
        }

        /// <summary>
        /// Eksik kodları bulur (silinmiş olabilecek)
        /// </summary>
        public static List<string> EksikKodlariBul(IEnumerable<string> mevcutKodlar)
        {
            var kodlar = mevcutKodlar?.ToList() ?? new List<string>();
            if (!kodlar.Any()) return new List<string>();

            var numaralar = kodlar
                .Select(k => NumarayiAl(k))
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            if (!numaralar.Any()) return new List<string>();

            var eksikler = new List<string>();
            var min = numaralar.Min();
            var max = numaralar.Max();

            // İlk kodun formatını al
            var ornekKod = kodlar.First();
            var prefix = Regex.Match(ornekKod, @"^(.*?)(\d+)").Groups[1].Value;
            var basamak = Regex.Match(ornekKod, @"(\d+)").Groups[1].Value.Length;

            for (int i = min; i < max; i++)
            {
                if (!numaralar.Contains(i))
                {
                    eksikler.Add($"{prefix}{i.ToString().PadLeft(basamak, '0')}");
                }
            }

            return eksikler;
        }

        /// <summary>
        /// Kodun benzersiz olup olmadığını kontrol eder
        /// </summary>
        public static bool KodBenzersizMi(string kod, IEnumerable<string> mevcutKodlar)
        {
            if (string.IsNullOrWhiteSpace(kod)) return false;

            var kodlar = mevcutKodlar?.ToList() ?? new List<string>();
            return !kodlar.Contains(kod, StringComparer.OrdinalIgnoreCase);
        }

        #region Private Methods

        private static int NumarayiAl(string kod)
        {
            var match = Regex.Match(kod, @"(\d+)");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        #endregion
    }
}

// ============================================
// KULLANIM ÖRNEKLERİ
// ============================================

/*

// 1. Firma Kodu
var mevcutFirmaKodlari = new List<string> { "F-001", "F-002", "F-005" };

// Otomatik
var yeniKod = KodGenerator.FirmaKoduOlustur(mevcutFirmaKodlari);
// Sonuç: F-006

// Kullanıcı "B-001" yazdı
var yeniKod2 = KodGenerator.FirmaKoduOlustur(mevcutFirmaKodlari, "B-001");
// Sonuç: B-001 (benzersiz olduğu için)

// Kullanıcı "F-002" yazdı
var yeniKod3 = KodGenerator.FirmaKoduOlustur(mevcutFirmaKodlari, "F-002");
// Sonuç: F-003 (zaten var, arttırıldı)


// 2. Stok Kodu
var mevcutStokKodlari = new List<string> { "STK-00001", "STK-00002" };
var stokKodu = KodGenerator.StokKoduOlustur(mevcutStokKodlari);
// Sonuç: STK-00003


// 3. Cari Kodu
var mevcutCariKodlari = new List<string> { "C-0001", "C-0002" };
var cariKodu = KodGenerator.CariKoduOlustur(mevcutCariKodlari);
// Sonuç: C-0003


// 4. Özel Format
var mevcutKodlar = new List<string> { "MÜŞ-100", "MÜŞ-101" };
var ozelKod = KodGenerator.KodOlustur(mevcutKodlar, null, "MÜŞ-", 3);
// Sonuç: MÜŞ-102


// 5. Kod Arttırma
var arttir = KodGenerator.KoduArttir("B-001");
// Sonuç: B-002


// 6. Eksik Kodları Bulma
var mevcutler = new List<string> { "F-001", "F-002", "F-005", "F-006" };
var eksikler = KodGenerator.EksikKodlariBul(mevcutler);
// Sonuç: ["F-003", "F-004"]


// 7. Benzersizlik Kontrolü
var benzersizMi = KodGenerator.KodBenzersizMi("F-999", mevcutFirmaKodlari);
// Sonuç: true

*/

