﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity
{
    [Table("AppVersiyon")]
    public class AppVersiyon
    {
        [Key]
        public string Versiyon { get; set; }
    }
}
