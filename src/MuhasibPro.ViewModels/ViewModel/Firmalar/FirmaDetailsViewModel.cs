using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Muhasebe.Business.Models.DbModel.AppModel;
using Muhasebe.Business.Services.Abstract.App;
using Muhasebe.Business.Validations.App;
using MuhasibPro.Core.Services.Common;
using MuhasibPro.Infrastructure.ViewModels.Common;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModel.Firmalar;
public class FirmaDetailsArgs
{
    public static FirmaDetailsArgs CreateDefault() => new();
    public long FirmaId
    {
        get; set;
    }
    public bool IsNew => FirmaId <= 0;
}

public class FirmaDetailsViewModel : GenericDetailsViewModel<FirmaModel>
{
    public FirmaDetailsViewModel(ICommonServices commonServices, IFilePickerService filePickerService, IFirmaService firmaService) : base(commonServices)
    {
        FilePickerService = filePickerService;
        FirmaService = firmaService;
    }
    public IFilePickerService FilePickerService
    {
        get;
    }
    public IFirmaService FirmaService
    {
        get;
    }
    public override string Title => (Item?.IsNew ?? true) ? "Yeni Firma" : TitleEdit;
    public string TitleEdit => Item == null ? "Firma" : $"{Item.KisaUnvani}";
    public override bool ItemIsNew => Item?.IsNew ?? true;
    public FirmaDetailsArgs ViewModelArgs
    {
        get; private set;
    }
    public async Task LoadAsync(FirmaDetailsArgs args)
    {
        ViewModelArgs = args ?? FirmaDetailsArgs.CreateDefault();
        if (ViewModelArgs.IsNew)
        {
            Item = new FirmaModel();
            IsEditMode = true;
        }
        else
        {
            try
            {
                var item = await FirmaService.GetByFirmaIdAsync(ViewModelArgs.FirmaId);
                Item = item.Data ?? new FirmaModel { Id = ViewModelArgs.FirmaId, IsEmpty = true };
            }
            catch (Exception ex)
            {
                LogException("Firma", "Firma Detay", ex);
            }

        }
    }
    public void Unload()
    {
        ViewModelArgs.FirmaId = Item?.Id ?? 0;
    }
    public void Subscribe()
    {
        MessageService.Subscribe<FirmaDetailsViewModel, FirmaModel>(this, OnDetailMessage);
        MessageService.Subscribe<FirmaListViewModel>(this, OnListMessage);
    }
    public void Unsubscribe()
    {
        MessageService.Unsubscribe(this);
    }
    public FirmaDetailsArgs CreateArgs()
    {
        return new FirmaDetailsArgs { FirmaId = Item?.Id ?? 0 };
    }
    private object _newPictureSource = null;
    public object NewPictureSource
    {
        get => _newPictureSource;
        set => Set(ref _newPictureSource, value);
    }

    public override void BeginEdit()
    {
        NewPictureSource = null;
        base.BeginEdit();
    }
    public ICommand EditPictureCommand => new RelayCommand(OnEditPicture);
    private async void OnEditPicture()
    {
        NewPictureSource = null;
        var result = await FilePickerService.OpenImagePickerAsync();
        if (result != null)
        {
            EditableItem.Logo = result.ImageBytes;
            EditableItem.LogoSource = result.ImageSource;
            EditableItem.LogoOnizleme = result.ImageBytes;
            EditableItem.LogoOnizlemeSource = result.ImageSource;
            NewPictureSource = result.ImageSource;
        }
        else
        {
            NewPictureSource = null;
        }
    }
    protected async override Task<bool> SaveItemAsync(FirmaModel model)
    {
        try
        {
            StartStatusMessage("Firma kaydediliyor...");
            await Task.Delay(100);
            await FirmaService.UpdateFirmaAsync(model);
            EndStatusMessage("Firma kaydedildi");
            LogInformation("Firma", "Kayıt", "Firma başarıyla kaydedildi", $"Firma {model.Id} '{model.KisaUnvani}' başarıyla kaydedildi");
            return true;
        }
        catch (Exception ex)
        {
            StatusError($"Firma kaydedilirken hata oluştu: {ex.Message}");
            LogException("Firma", "Kayıt", ex);
            return false;
        }
    }  
    protected async override Task<bool> DeleteItemAsync(FirmaModel model)
    {
        try
        {
            StartStatusMessage("Firma siliniyor...");
            await Task.Delay(100);
            await FirmaService.DeleteFirmaAsync(model);
            EndStatusMessage("Firma silindi");
            LogWarning("Firma", "Sil", "Firma silindi", $"Firma {model.Id} '{model.KisaUnvani}' silindi");
            return true;
        }
        catch (Exception ex)
        {
            StatusError($"Firma silinirken hata oluştu: {ex.Message}");
            LogException("Firma", "Sil", ex);
            return false;
        }
    }

    protected async override Task<bool> ConfirmDeleteAsync()
    {
        return await DialogService.ShowConfirmationAsync("Silme Onayı", "Firmayı silmek istediğinize emin misiniz?", "Sil", "İptal");
    }
    protected override IEnumerable<AbstractValidator<FirmaModel>> GetValidationConstraints(FirmaModel model)
    {
        yield return new FirmaValidator();
    }

    async void OnDetailMessage(FirmaDetailsViewModel sender, string message, FirmaModel changed)
    {
        var current = Item;
        if (current != null)
        {
            if (changed != null && changed.Id == current?.Id)
            {
                switch (message)
                {
                    case "ItemChanged":
                        await ContextService.RunAsync(async () =>
                        {
                            try
                            {
                                var item = await FirmaService.GetByFirmaIdAsync(current.Id);
                                item.Data = item.Data ?? new FirmaModel { Id = current.Id, IsEmpty = true };
                                current.Merge(item.Data);
                                current.NotifyChanges();
                                NotifyPropertyChanged(nameof(Title));
                                if (IsEditMode)
                                {
                                    StatusMessage("DİKKAT: Bu firma kaydı değiştirilmiş!");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogException("Firma", "Değiştirilmiş", ex);
                            }
                        });
                        break;
                    case "ItemDeleted":
                        await OnItemDeletedExternally();
                        break;
                }
            }
        }
    }
    private async Task OnItemDeletedExternally()
    {
        await ContextService.RunAsync(() =>
        {
            CancelEdit();
            IsEnabled = false;
            StatusMessage("DİKKAT: Bu firma kaydı silinmiş!");
        });
    }
    private async void OnListMessage(FirmaListViewModel sender, string message, object args)
    {
        var current = Item;
        if (current != null)
        {
            switch (message)
            {
                case "ItemsDeleted":
                    if (args is IList<FirmaModel> deletedModels)
                    {
                        if (deletedModels.Any(r => r.Id == current.Id))
                        {
                            await OnItemDeletedExternally();
                        }
                    }
                    break;
                case "ItemRangesDeleted":
                    try
                    {
                        var model = await FirmaService.GetByFirmaIdAsync(current.Id);
                        if (model == null)
                        {
                            await OnItemDeletedExternally();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException("Firma", "Firma kaydı silinmiş!", ex);
                    }
                    break;
            }
        }
    }
}
