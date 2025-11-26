using FluentValidation;
using Muhasib.Business.Models.Common;
using Muhasib.Domain.Enum;
using Muhasib.Domain.Models;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.Infrastructure.ViewModels;

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
            if(Set(ref _item, value))
            {
                EditableItem = _item;
                IsEnabled = !_item?.IsEmpty ?? false;
                NotifyPropertyChanged(nameof(IsDataAvailable));
                NotifyPropertyChanged(nameof(IsDataUnavailable));
                NotifyPropertyChanged(nameof(Title));
            }
        }
    }

    private TModel _editableItem = null;

    public TModel EditableItem { get => _editableItem; set => Set(ref _editableItem, value); }

    private bool _isEditMode = false;

    public bool IsEditMode { get => _isEditMode; set => Set(ref _isEditMode, value); }

    private bool _isEnabled = true;

    public bool IsEnabled { get => _isEnabled; set => Set(ref _isEnabled, value); }

    public ICommand BackCommand => new RelayCommand(OnBack);

    virtual protected void OnBack()
    {
        StatusReady();
        if(NavigationService.CanGoBack)
        {
            NavigationService.GoBack();
        }
    }

    public ICommand EditCommand => new RelayCommand(OnEdit);

    virtual protected void OnEdit()
    {
        StatusActionMessage($"", StatusMessageType.Edit, autoHide:-1);
        BeginEdit();
        MessageService.Send(this, "BeginEdit", Item);
    }

    virtual public void BeginEdit()
    {
        if(!IsEditMode)
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
        ValidationErrors = new(); // ← Hataları temizle
        if(ItemIsNew)
        {
            // We were creating a new item: cancel means exit
            if(NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            } else
            {
                NavigationService.CloseViewAsync();
            }
            return;
        }

        // We were editing an existing item: just cancel edition
        if(IsEditMode)
        {
            EditableItem = Item;
        }
        IsEditMode = false;
    }

    public ICommand SaveCommand => new RelayCommand(OnSave);

    virtual protected async void OnSave()
    {
        StatusReady();

        var validationResult = ValidateModel(EditableItem);
        ValidationErrors = validationResult.Errors;

        if(validationResult.IsValid)
        {
            await SaveAsync();
        } else
        {
            var errorCount = validationResult.Errors.Sum(e => e.Value.Count);
            NotificationService?.ShowError(
            $"Doğrulama Hatası ({errorCount} adet)",
            $"Lütfen hatalı alanları düzeltip tekrar deneyin");
        }
    }

    virtual public async Task SaveAsync()
    {
        await ExecuteActionAsync(
            action: async () =>
            {
                IsEnabled = false;
                bool isNew = ItemIsNew;

                if (await SaveItemAsync(EditableItem))
                {
                    Item.Merge(EditableItem);
                    Item.NotifyChanges();
                    NotifyPropertyChanged(nameof(Title));
                    EditableItem = Item;

                    if (isNew)
                    {
                        MessageService.Send(this, "NewItemSaved", Item);
                        NotificationService?.ShowSuccess("Başarılı", "Yeni kayıt başarıyla oluşturuldu.");
                    }
                    else
                    {
                        MessageService.Send(this, "ItemChanged", Item);
                        NotificationService?.ShowSuccess("Başarılı", "Değişiklikler başarıyla kaydedildi.");
                    }

                    IsEditMode = false;
                    NotifyPropertyChanged(nameof(ItemIsNew));
                }

                IsEnabled = true;
            },
            startMessage: $"Kaydediliyor",
            startMessageType: StatusMessageType.Saving, // 💾
            successMessage: $"Kaydedildi"       // ✅
        );
    }
    public ICommand DeleteCommand => new RelayCommand(OnDelete);

    virtual protected async void OnDelete()
    {
        StatusReady();
        if(await ConfirmDeleteAsync())
        {
            await DeleteAsync();
        }
    }

    virtual public async Task DeleteAsync()
    {
        if (Item == null) return;

        await ExecuteActionAsync(
            action: async () =>
            {
                IsEnabled = false;
                if (await DeleteItemAsync(Item))
                {
                    MessageService.Send(this, "ItemDeleted", Item);
                }
                IsEnabled = true;
            },
            startMessage: $"{Title} siliniyor",
            startMessageType: StatusMessageType.Deleting, // 🗑
            successMessage: $"{Title} silindi"            // ✅
        );
    }

    private Dictionary<string, List<string>> _validationErrors = new();

    public Dictionary<string, List<string>> ValidationErrors
    {
        get => _validationErrors;
        set => Set(ref _validationErrors, value);
    }

    public virtual ValidationResult ValidateModel(TModel model)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach(var constraint in GetValidationConstraints(model))
        {
            var result = constraint.Validate(model);
            if(!result.IsValid)
            {
                foreach(var error in result.Errors)
                {
                    var propertyName = error.PropertyName;

                    if(!errors.ContainsKey(propertyName))
                    {
                        errors[propertyName] = new List<string>();
                    }

                    errors[propertyName].Add(error.ErrorMessage);
                }
            }
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    public virtual Result Validate(TModel model)
    {
        var validationResult = ValidateModel(model);

        if(!validationResult.IsValid)
        {
            var allErrors = string.Join("\n", validationResult.Errors.SelectMany(e => e.Value));

            return Result.Error("Doğrulama Hatası", allErrors);
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

