using Muhasebe.Business.Common;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Models.SistemModel;

public class SistemLogModel : ObservableObject
{
    public static SistemLogModel CreateEmpty() => new() { Id = -1, IsEmpty = true };


    public bool IsRead { get; set; }
    public string User { get; set; }

    public LogType Type { get; set; }
    public string Source { get; set; }
    public string Action { get; set; }
    public string Message { get; set; }
    public string Description { get; set; }
}
