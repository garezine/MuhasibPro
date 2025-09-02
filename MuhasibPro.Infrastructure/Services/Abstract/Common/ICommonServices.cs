using Muhasebe.Business.Services.Abstract.Common;

namespace MuhasibPro.Infrastructure.Services.Abstract.Common;

public interface ICommonServices
{
    IContextService ContextService { get; }
    INavigationService NavigationService { get; }
    IMessageService MessageService { get; }
    IDialogService DialogService { get; }
    ILogService LogService { get; }
}
