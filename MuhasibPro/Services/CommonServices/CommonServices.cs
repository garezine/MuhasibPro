using Muhasebe.Business.Services.Abstracts.Common;
using MuhasibPro.Contracts.CommonServices;

namespace MuhasibPro.Services.CommonServices;

public class CommonServices : ICommonServices
{
    public CommonServices(
        IContextService contextService,
        INavigationService navigationService,
        IMessageService messageService,
        IDialogService dialogService,
        ILogService logService,
        INotificationService notificationService)
    {
        ContextService = contextService;
        NavigationService = navigationService;
        MessageService = messageService;
        DialogService = dialogService;
        LogService = logService;
        NotificationService = notificationService;
    }

    public IContextService ContextService { get; }

    public INavigationService NavigationService { get; }

    public IMessageService MessageService { get; }

    public IDialogService DialogService { get; }

    public ILogService LogService { get; }
    public INotificationService NotificationService { get; }


}
