namespace Muhasib.Business.Models.Common;

public abstract class BaseModel
{
    public long Id { get; set; }

    public DateTime KayitTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }
    public long KaydedenId { get; set; }

    public long? GuncelleyenId { get; set; }

    public bool AktifMi { get; set; } = true;
    public string BuildArananTerim { get; set; }
}
