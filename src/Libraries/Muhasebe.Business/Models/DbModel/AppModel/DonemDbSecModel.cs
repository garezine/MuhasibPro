using Muhasebe.Business.Common;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Models.DbModel.AppModel;

public class DonemDbSecModel : ObservableObject
{
    public static DonemDbSecModel CreateEmpty()
        => new()
        {
            Id = -1,
            IsEmpty = true
        };
    public long FirmaId { get; set; }
    public long MaliDonemId { get; set; }
    public string DBName { get; set; }
    public string Directory { get; set; }
    public string DBPath { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public MaliDonemModel MaliDonem { get; set; }
    
    public bool IsNew => Id <= 0;
    public override void Merge(ObservableObject source)
    {
        if (source is DonemDbSecModel model)
            Merge(model);
    }
    public void Merge(DonemDbSecModel source)
    {
        if(source != null)
        {
            Id=source.Id;
            MaliDonemId=source.MaliDonemId;
            DBName=source.DBName;
            Directory=source.Directory;
            DBPath=source.DBPath;
            DatabaseType=source.DatabaseType;
            MaliDonem  = source.MaliDonem;

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
