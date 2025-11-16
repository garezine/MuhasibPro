using Muhasib.Business.Models.Common;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Models.MuhasebeModel
{
    public class AppLogModel : ObservableObject
    {
        public static AppLogModel CreateEmpty() => new() { Id = -1, IsEmpty = true };

        public bool IsRead { get; set; }
        public string User { get; set; }
        public LogType Type { get; set; }
        public string Source { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }

    }

}
