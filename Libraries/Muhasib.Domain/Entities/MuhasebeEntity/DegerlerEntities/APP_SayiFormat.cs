using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("APP_SayiFormat")]
    public class APP_SayiFormat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [MaxLength(50)]
        public string FaturaFormatAltToplam { get; set; }

        [MaxLength(50)]
        public string FaturaFormatBirimFiyati { get; set; }

        [MaxLength(50)]
        public string FaturaFormatTutari { get; set; }

        [MaxLength(50)]
        public string FaturaYuvarlamaAltToplam { get; set; }

        [MaxLength(50)]
        public string FaturaYuvarlamaBirimFiyati { get; set; }

        [MaxLength(50)]
        public string FaturaYuvarlamaTutari { get; set; }

        [MaxLength(50)]
        public string GenelTutarYuvarlama { get; set; }

        [MaxLength(50)]
        public string ParaBirimi { get; set; }

        [MaxLength(50)]
        public string ParaFormati { get; set; }

        [MaxLength(50)]
        public string SayiYuvarlama { get; set; }
    }
}