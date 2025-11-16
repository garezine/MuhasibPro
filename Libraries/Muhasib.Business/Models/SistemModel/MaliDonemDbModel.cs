using Muhasib.Business.Models.Common;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Models.SistemModel;

public class MaliDonemDbModel : ObservableObject
{
    public static MaliDonemDbModel CreateEmpty()
        => new()
        {
            Id = -1,
            IsEmpty = true
        };
    public long MaliDonemId { get; set; }
    public string DBName { get; set; }
    public string Directory { get; set; }
    public string DBPath { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public MaliDonemModel MaliDonemModel { get; set; }
    public bool IsNew => Id <= 0;
    public override void Merge(ObservableObject source)
    {
        if (source is MaliDonemDbModel model)
            Merge(model);
    }
    public void Merge(MaliDonemDbModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            MaliDonemId = source.MaliDonemId;
            DBName = source.DBName;
            Directory = source.Directory;
            DBPath = source.DBPath;
            DatabaseType = source.DatabaseType;
            MaliDonemModel = source.MaliDonemModel;

            AktifMi = source.AktifMi;
            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;
        }
    }
    public override string ToString()
    {
        return IsEmpty ? "----" : DBName;
    }

}
