using Muhasebe.Business.Models.SistemModel;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.Logging.SistemLogs
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
            {
                StatusReady();
            }
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
                var model = await LogService.SistemLogService.GetLogAsync(selected.Id);
                selected.Merge(model);
            }
            catch (Exception ex)
            {
                LogSistemException("SistemLogs", "Ayrıntıları Yükle", ex);
            }
        }
    }
}
