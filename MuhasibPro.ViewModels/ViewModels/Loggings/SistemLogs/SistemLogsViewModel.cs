using Muhasib.Business.Models.SistemModel;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Loggings.SistemLogs
{
    public class SistemLogsViewModel : ViewModelBase
    {
        public SistemLogsViewModel(ICommonServices commonServices) : base(commonServices)
        {
            SistemLogList = new SistemLogListViewModel(commonServices);
            SistemLogDetails = new SistemLogDetailsViewModel(commonServices);
        }
        public SistemLogListViewModel SistemLogList { get; }
        public SistemLogDetailsViewModel SistemLogDetails { get; }
        private string Header = "Sistem Günlüğü";
        public async Task LoadAsync(SistemLogListArgs args)
        {
            await SistemLogList.LoadAsync(args);
        }
        public void Unload()
        {
            SistemLogList.Unload();
        }

        public void Subscribe()
        {
            MessageService.Subscribe<SistemLogListViewModel>(this, OnMessage);
            SistemLogList.Subscribe();
            SistemLogDetails.Subscribe();
        }
        public void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
            SistemLogList.Unsubscribe();
            SistemLogDetails.Unsubscribe();
        }

        private async void OnMessage(SistemLogListViewModel viewModel, string message, object args)
        {
            if (viewModel == SistemLogList && message == "ItemSelected")
            {
                await ContextService.RunAsync(() =>
                {
                    OnItemSelected();
                });
            }           
        }

        private async void OnItemSelected()
        {
            if (SistemLogDetails.IsEditMode)
                StatusReady();
            
            var selected = SistemLogList.SelectedItem;
            if (!SistemLogList.IsMultipleSelection)
            {
                if (selected != null && !selected.IsEmpty)
                {
                    await PopulateDetails(selected);
                }
            }
            SistemLogDetails.Item = selected;
        }

        private async Task PopulateDetails(SistemLogModel selected)
        {
            try
            {
                var model = await LogService.SistemLogService.GetSistemLogAsync(selected.Id);
                selected.Merge(model);
            }
            catch (Exception ex)
            {
                LogSistemException($"{Header}", $"{Header} Detay", ex);
            }
        }
    }
}
