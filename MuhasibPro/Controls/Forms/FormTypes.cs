namespace MuhasibPro.Controls;


public interface IFormControl
{
    event EventHandler<FormVisualState> VisualStateChanged;

    FormEditMode Mode { get; }

    FormVisualState VisualState { get; }

    bool IsEnabled { get; }

    bool Focus(FocusState value);

    void SetVisualState(FormVisualState visualState);

    // Validation için yeni metodlar
    string ErrorMessage { get; set; }
    bool HasError { get; set; }

    void ClearError();
    void SetError(string errorMessage);
}

public enum TextDataType
{
    String,
    Integer,
    Decimal,
    Double,
    Phone,
}

public enum FormEditMode
{
    Auto,
    ReadOnly,
    ReadWrite
}

public enum FormVisualState
{
    Idle,
    Ready,
    Focused,
    Disabled
}

