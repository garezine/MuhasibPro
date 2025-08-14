using Muhasebe.Business.Common;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Models.DbModel.AppModel;

public class CalismaDonemDbModel : ObservableObject
{
    public static CalismaDonemDbModel CreateEmpty()
        => new()
        {
            Id = -1,
            IsEmpty = true
        };
    public long FirmaId { get; set; }
    public long CalismaDonemId { get; set; }
    public string DBName { get; set; }
    public string Directory { get; set; }
    public string DBPath { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public CalismaDonemModel CalismaDonem { get; set; }
    
    public bool IsNew => Id <= 0;
    public override void Merge(ObservableObject source)
    {
        if (source is CalismaDonemDbModel model)
            Merge(model);
    }
    public void Merge(CalismaDonemDbModel source)
    {
        if(source != null)
        {
            Id=source.Id;
            CalismaDonemId=source.CalismaDonemId;
            DBName=source.DBName;
            Directory=source.Directory;
            DBPath=source.DBPath;
            DatabaseType=source.DatabaseType;
            CalismaDonem  = source.CalismaDonem;

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
