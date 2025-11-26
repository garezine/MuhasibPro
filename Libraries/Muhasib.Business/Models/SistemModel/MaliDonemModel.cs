using Muhasib.Business.Models.Common;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Models.SistemModel;

public class MaliDonemModel : ObservableObject
{
    public static MaliDonemModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public long FirmaId { get; set; }
    public int MaliYil { get; set; }    
    public string DBName { get; set; }
    public string Directory { get; set; }
    public string DBPath { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public FirmaModel FirmaModel { get; set; }
    public bool IsNew => Id <= 0;

    public override void Merge(ObservableObject source)
    {
        if (source is MaliDonemModel model)
            Merge(model);
    }

    public void Merge(MaliDonemModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            FirmaId = source.FirmaId;
            MaliYil = source.MaliYil;            
            DBName = source.DBName;
            Directory = source.Directory;
            DBPath = source.DBPath;
            DatabaseType = source.DatabaseType;
            FirmaModel = source.FirmaModel;

            AktifMi = source.AktifMi;
            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;

        }
    }

    public override string ToString() { return IsEmpty ? "----" : $"{MaliYil} - {FirmaModel?.KisaUnvani}"; }
}