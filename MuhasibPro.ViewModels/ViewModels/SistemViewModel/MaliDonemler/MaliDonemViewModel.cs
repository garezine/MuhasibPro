using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.SistemServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler
{
    public class MaliDonemViewModel : ViewModelBase
    {
        public MaliDonemViewModel(ICommonServices commonServices, IMaliDonemService maliDonemService, ITenantWorkflowService workflowService) : base(commonServices)
        {
            MaliDonemService = maliDonemService;
            WorkflowService = workflowService;
            MaliDonemList = new MaliDonemListViewModel(commonServices, maliDonemService);
            MaliDonemDetails = new MaliDonemDetailsViewModel(commonServices, maliDonemService,workflowService);
        }
        public IMaliDonemService MaliDonemService { get; }
        public ITenantWorkflowService WorkflowService { get; }
        public MaliDonemListViewModel MaliDonemList { get; set; }
        public MaliDonemDetailsViewModel MaliDonemDetails { get; set; }
        
        public async Task LoadAsync(MaliDonemDetailsArgs args)
        {
            await MaliDonemDetails.LoadAsync(args);
            long firmaID = args?.FirmaId ?? 0;
            if (firmaID > 0) 
            {
                await MaliDonemList.LoadAsync(new MaliDonemListArgs { FirmaId = args.FirmaId },silent:true);
            }
            else
            {
                await MaliDonemList.LoadAsync(new MaliDonemListArgs { IsEmpty =true}, silent:false);
            }
        }
        public void Unload()
        {
            MaliDonemDetails.CancelEdit();
            MaliDonemList.Unload();
        }

        public void Subscribe()
        {
            MessageService.Subscribe<MaliDonemDetailsViewModel,MaliDonemModel>(this, OnMessage);
            MaliDonemList.Subscribe();
            MaliDonemDetails.Subscribe();
           
        }
        public void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
            MaliDonemList.Unsubscribe();
            MaliDonemDetails.Unsubscribe();
            
        }

        private async void OnMessage(MaliDonemDetailsViewModel viewModel, string message, MaliDonemModel maliDonem)
        {
            if (viewModel == MaliDonemDetails && message == "ItemSelected")
            {
                await ContextService.RunAsync(async () =>
                {
                    await MaliDonemList.LoadAsync(new MaliDonemListArgs { FirmaId = maliDonem.FirmaId });
                });
            }
        }

      
    }

}
