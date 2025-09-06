namespace Muhasebe.Business.Common;

public abstract class BaseModel
{
    public long Id { get; set; }

    public DateTimeOffset KayitTarihi { get; set; }

    public DateTimeOffset? GuncellemeTarihi { get; set; }
    public long KaydedenId { get; set; }

    public long? GuncelleyenId { get; set; }



    public bool AktifMi { get; set; } = true;    
}
