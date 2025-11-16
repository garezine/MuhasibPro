using Muhasib.Business.Models.Common;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Models.SistemModel;

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
