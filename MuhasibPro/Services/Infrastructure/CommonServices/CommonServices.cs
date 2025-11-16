using Muhasib.Business.Services.Contracts.LogServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;

namespace MuhasibPro.Services.Infrastructure.CommonServices;
public class CommonServices : ICommonServices
{
    public CommonServices(
        IContextService contextService,
        INavigationService navigationService,
        IMessageService messageService,
        IDialogService dialogService,
        ILogService logService,
        INotificationService notificationService,
        ISettingsService settingsService,
        IStatusBarService statusBarService)
    {
        ContextService = contextService;
        NavigationService = navigationService;
        MessageService = messageService;
        DialogService = dialogService;
        LogService = logService;
        NotificationService = notificationService;
        SettingsService = settingsService;
        StatusBarService = statusBarService;
    }
    public IContextService ContextService { get; }
    public INavigationService NavigationService { get; }
    public IMessageService MessageService { get; }
    public IDialogService DialogService { get; }
    public ILogService LogService { get; }
    public INotificationService NotificationService { get; }
    public ISettingsService SettingsService { get; }
    public IStatusBarService StatusBarService { get; }
}
