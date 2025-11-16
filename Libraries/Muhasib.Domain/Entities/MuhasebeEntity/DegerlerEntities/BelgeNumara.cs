using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("BelgeNumaralar")]
    public class BelgeNumara
    {
        public int? Alis { get; set; }

        public int? Kasa { get; set; }

        public int? Odeme { get; set; }

        public int? PersonelMaas { get; set; }

        public int? PersonelTahakkuk { get; set; }

        public int? Satis { get; set; }

        public int? SiparisAlinan { get; set; }

        public int? SiparisVerilen { get; set; }

        public int? StokHareket { get; set; }

        public int? Tahsilat { get; set; }

        public int? TaksitOdeme { get; set; }

        public int? TaksitTahsilat { get; set; }

        public int? TaksitliAlis { get; set; }

        public int? TaksitliSatis { get; set; }

        public int? TeklifAlinan { get; set; }

        public int? TeklifVerilen { get; set; }
    }
}