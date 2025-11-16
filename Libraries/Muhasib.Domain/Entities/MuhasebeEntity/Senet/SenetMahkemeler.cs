using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Senet
{
    [Table("SenetMahkemeleri")]
    public class SenetMahkemeler : BaseEntity
    {
        [MaxLength(150)]
        public string Mahkeme { get; set; }
    }
}
