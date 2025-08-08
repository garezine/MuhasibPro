using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Muhasebe.Business.Common;
using MuhasibPro.Core.Infrastructure.Common;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Services.Common;
using System.Windows.Input;

namespace MuhasibPro.Infrastructure.ViewModels.Common;

public abstract class GenericDetailsViewModel<TModel> : ViewModelBase where TModel : ObservableObject, new()
{
    protected GenericDetailsViewModel(ICommonServices commonServices) : base(commonServices)
    {
       
    }

    public bool IsDataAvailable => _item != null;
    public bool IsDataUnavailable => !IsDataAvailable;

    public bool CanGoBack => !IsMainWindow && NavigationService.CanGoBack;

    private TModel _item = null;
    public TModel Item
    {
        get => _item;
        set
        {
            if (Set(ref _item, value))
            {
                EditableItem = _item;
                IsEnabled = (!_item?.IsEmpty) ?? false;
                NotifyPropertyChanged(nameof(IsDataAvailable));
                NotifyPropertyChanged(nameof(IsDataUnavailable));
                NotifyPropertyChanged(nameof(Title));
            }
        }
    }

    private TModel _editableItem = null;
    public TModel EditableItem
    {
        get => _editableItem;
        set => Set(ref _editableItem, value);
    }

    private bool _isEditMode = false;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => Set(ref _isEditMode, value);
    }

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => Set(ref _isEnabled, value);
    }
    private bool _canSave = true;

    public bool CanSave
    {
        get => _canSave;
        set
        {
            if (_canSave != value)
            {
                _canSave = value;
                Set(ref _canSave, value);
                OnCanSaveChanged();
            }
        }
    }
    private void OnValidationChanged(object sender, bool areAllValid)
    {
        CanSave = areAllValid;
    }
    protected virtual bool OnSaveAttempt()
    {
        if (!ValidationHelper.AreAllControlsValid())
        {
            // İlk hatalı alana focus yap
            ValidationHelper.FocusFirstInvalidControl();

            // Hata mesajlarını göster (isteğe bağlı)
            var errors = ValidationHelper.GetValidationErrors();
            if (errors.Any())
            {
                ShowValidationErrors(errors);
            }

            return false;
        }

        return true;
    }
    protected virtual void ShowValidationErrors(List<string> errors)
    {
        var message = "Lütfen aşağıdaki hataları düzeltin:\n\n" + string.Join("\n", errors);
        ShowErrorMessage(message);
    }
    private void ShowErrorMessage(string message)
    {
        // ContentDialog, Notification, vs. kullanabilirsiniz
        // Örnek: await ShowMessageDialogAsync("Hata", message);
    }
    protected virtual void OnCanSaveChanged()
    {
        ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
    }
    public ICommand BackCommand => new RelayCommand(OnBack);
    virtual protected void OnBack()
    {
        StatusReady();
        if (NavigationService.CanGoBack)
        {
            NavigationService.GoBack();
        }
    }

    public ICommand EditCommand => new RelayCommand(OnEdit);
    virtual protected void OnEdit()
    {
        StatusReady();
        BeginEdit();
        MessageService.Send(this, "BeginEdit", Item);
    }
    virtual public void BeginEdit()
    {
        if (!IsEditMode)
        {
            IsEditMode = true;
            // Create a copy for edit
            var editableItem = new TModel();
            editableItem.Merge(Item);
            EditableItem = editableItem;
        }
    }

    public ICommand CancelCommand => new RelayCommand(OnCancel);
    virtual protected void OnCancel()
    {
        StatusReady();
        CancelEdit();
        MessageService.Send(this, "CancelEdit", Item);
    }
    virtual public void CancelEdit()
    {
        if (ItemIsNew)
        {
            // We were creating a new item: cancel means exit
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.CloseViewAsync();
            }
            return;
        }

        // We were editing an existing item: just cancel edition
        if (IsEditMode)
        {
            EditableItem = Item;
        }
        IsEditMode = false;
    }

    public ICommand SaveCommand => new RelayCommand(OnSave);
    virtual protected async void OnSave()
    {
        StatusReady();
        if (!OnSaveAttempt())
        {
            return; // Validasyon hatası varsa kaydetme
        }
        var result = Validate(EditableItem);
        if (result.IsOk)
        {
            await SaveAsync();
        }
        else
        {
            await DialogService.ShowMessageAsync(result.Message, $"{result.Description}");
        }
    }
    virtual public async Task SaveAsync()
    {
        IsEnabled = false;
        bool isNew = ItemIsNew;
        if (await SaveItemAsync(EditableItem))
        {           
            StatusBar.SetSaveStatus(true);
            Item.Merge(EditableItem);
            Item.NotifyChanges();
            NotifyPropertyChanged(nameof(Title));
            EditableItem = Item;

            if (isNew)
            {
                MessageService.Send(this, "NewItemSaved", Item);
            }
            else
            {
                MessageService.Send(this, "ItemChanged", Item);
            }
            IsEditMode = false;

            NotifyPropertyChanged(nameof(ItemIsNew));
        }
        IsEnabled = true;       
        StatusBar.SetSaveStatus(false);        
    }

    public ICommand DeleteCommand => new RelayCommand(OnDelete);
    virtual protected async void OnDelete()
    {
        StatusReady();
        if (await ConfirmDeleteAsync())
        {            
            await DeleteAsync();
        }
       
    }
    virtual public async Task DeleteAsync()
    {
        var model = Item;
        if (model != null)
        {
            IsEnabled = false;
            if (await DeleteItemAsync(model))
            {
                StatusBar.ShowProgress("İşlem yapılıyor...");
                MessageService.Send(this, "ItemDeleted", model);
            }
            else
            {
                IsEnabled = true;
            }
        }
        StatusBar.HideProgress();
    }

    public virtual Result Validate(TModel model)
    {        
        foreach(var constraint in GetValidationConstraints(model))
        {
            var result = constraint.Validate(model);
            if(!result.IsValid)
            {
                return Result.Error("Doğrulama Hatası", string.Join("\n", result.Errors.Select(e => e.ErrorMessage)));
            }
        }
        return Result.Ok();
    }

    protected virtual IEnumerable<AbstractValidator<TModel>> GetValidationConstraints(TModel model) => Enumerable.Empty<AbstractValidator<TModel>>(
        );

    public abstract bool ItemIsNew { get; }

    protected abstract Task<bool> SaveItemAsync(TModel model);

    protected abstract Task<bool> DeleteItemAsync(TModel model);

    protected abstract Task<bool> ConfirmDeleteAsync();
}

