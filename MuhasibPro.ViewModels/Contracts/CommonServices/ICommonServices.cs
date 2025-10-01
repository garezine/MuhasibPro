using Muhasebe.Business.Services.Abstracts.Common;

namespace MuhasibPro.ViewModels.Contracts.CommonServices;

public interface ICommonServices
{
    IContextService ContextService { get; }
    INavigationService NavigationService { get; }
    IMessageService MessageService { get; }
    IDialogService DialogService { get; }
    ILogService LogService { get; }
    INotificationService NotificationService { get; }

}
