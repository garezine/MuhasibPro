namespace MuhasibPro.Controls;

public sealed partial class FormField : UserControl
{
    public FormField()
    {
        this.InitializeComponent();
    }

    #region Header
    public string Header
    {
        get { return (string)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(FormField), new PropertyMetadata(""));
    #endregion

    #region Text
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(FormField), new PropertyMetadata(""));
    #endregion

    #region PredefinedFormat
    public PredefinedFormat PredefinedFormat
    {
        get { return (PredefinedFormat)GetValue(PredefinedFormatProperty); }
        set { SetValue(PredefinedFormatProperty, value); }
    }

    public static readonly DependencyProperty PredefinedFormatProperty =
        DependencyProperty.Register("PredefinedFormat", typeof(PredefinedFormat), typeof(FormField), new PropertyMetadata(PredefinedFormat.None));
    #endregion

    #region DataType
    public TextDataType DataType
    {
        get { return (TextDataType)GetValue(DataTypeProperty); }
        set { SetValue(DataTypeProperty, value); }
    }

    public static readonly DependencyProperty DataTypeProperty =
        DependencyProperty.Register("DataType", typeof(TextDataType), typeof(FormField), new PropertyMetadata(TextDataType.String));
    #endregion

    #region Format
    public string Format
    {
        get { return (string)GetValue(FormatProperty); }
        set { SetValue(FormatProperty, value); }
    }

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register("Format", typeof(string), typeof(FormField), new PropertyMetadata(""));
    #endregion

    #region IsRequired
    public bool IsRequired
    {
        get { return (bool)GetValue(IsRequiredProperty); }
        set { SetValue(IsRequiredProperty, value); }
    }

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register("IsRequired", typeof(bool), typeof(FormField), new PropertyMetadata(false));
    #endregion

    #region IsValidationEnabled
    public bool IsValidationEnabled
    {
        get { return (bool)GetValue(IsValidationEnabledProperty); }
        set { SetValue(IsValidationEnabledProperty, value); }
    }

    public static readonly DependencyProperty IsValidationEnabledProperty =
        DependencyProperty.Register("IsValidationEnabled", typeof(bool), typeof(FormField), new PropertyMetadata(true));
    #endregion

    #region Mode
    public FormEditMode Mode
    {
        get { return (FormEditMode)GetValue(ModeProperty); }
        set { SetValue(ModeProperty, value); }
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register("Mode", typeof(FormEditMode), typeof(FormField), new PropertyMetadata(FormEditMode.Auto));
    #endregion

    #region MaxLength
    public int MaxLength
    {
        get { return (int)GetValue(MaxLengthProperty); }
        set { SetValue(MaxLengthProperty, value); }
    }

    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register("MaxLength", typeof(int), typeof(FormField), new PropertyMetadata(0));
    #endregion

    #region PlaceholderText
    public string PlaceholderText
    {
        get { return (string)GetValue(PlaceholderTextProperty); }
        set { SetValue(PlaceholderTextProperty, value); }
    }

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register("PlaceholderText", typeof(string), typeof(FormField), new PropertyMetadata(""));
    #endregion

    #region HelpMessage
    public string HelpMessage
    {
        get { return (string)GetValue(HelpMessageProperty); }
        set { SetValue(HelpMessageProperty, value); }
    }

    public static readonly DependencyProperty HelpMessageProperty =
        DependencyProperty.Register("HelpMessage", typeof(string), typeof(FormField), new PropertyMetadata(""));
    #endregion

    #region ShowValidationIcon
    public bool IsShowValidationIcon
    {
        get { return (bool)GetValue(IsShowValidationIconProperty); }
        set { SetValue(IsShowValidationIconProperty, value); }
    }

    public static readonly DependencyProperty IsShowValidationIconProperty =
        DependencyProperty.Register("ShowValidationIcon", typeof(bool), typeof(FormField), new PropertyMetadata(true));
    #endregion

    // Helper methods for visibility binding
    public Visibility ShowValidationIcon(bool isValid)
    {
        return (!isValid && IsShowValidationIcon) ? Visibility.Visible : Visibility.Collapsed;
    }

    public Visibility ShowValidationMessage(bool isValid, string errorMessage)
    {
        return (!isValid && !string.IsNullOrEmpty(errorMessage)) ? Visibility.Visible : Visibility.Collapsed;
    }

    public Visibility ShowHelpText(string helpMessage)
    {
        return !string.IsNullOrEmpty(helpMessage) ? Visibility.Visible : Visibility.Collapsed;
    }

    // Public properties to access inner TextBox
    public FormTextBox TextBoxControl => InnerTextBox;
    public bool IsValid => InnerTextBox.IsValid;
    public string ValidationErrorMessage => InnerTextBox.ValidationMessage;
}
