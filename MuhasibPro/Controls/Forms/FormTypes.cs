namespace MuhasibPro.Controls;

public interface IFormControl
{
    event EventHandler<FormVisualState> VisualStateChanged;

    FormEditMode Mode
    {
        get;
    }
    FormVisualState VisualState
    {
        get;
    }

    bool IsEnabled
    {
        get;
    }

    bool Focus(FocusState value);

    void SetVisualState(FormVisualState visualState);
}

public enum TextDataType
{
    String,
    Integer,
    Decimal,
    Double
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

