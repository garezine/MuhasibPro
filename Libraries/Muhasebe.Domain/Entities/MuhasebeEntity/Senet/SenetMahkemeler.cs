using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.MuhasebeEntity.Senet
{
    [Table("SenetMahkemeleri")]
    public class SenetMahkemeler : BaseEntity
    {
        [MaxLength(150)]
        public string Mahkeme { get; set; }
    }
}
