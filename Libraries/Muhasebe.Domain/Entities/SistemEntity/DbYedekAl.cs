﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity;

[Table("DbYedekAl")]
public class DbYedekAl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    public long FirmaId { get; set; }
    public long DonemId { get; set; }
    public string YedeklemeAraligi { get; set; }
    public DateTime SonrakiYedekTarih { get; set; }
    public TimeSpan YedeklemeSaati { get; set; }
    public bool AktifMi { get; set; }

}
