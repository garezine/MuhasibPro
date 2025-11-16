using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MuhasibPro.Controls;

// Önceden tanımlı format türleri
public partial class FormTextBox : TextBox, IFormControl
{
    public event EventHandler<FormVisualState> VisualStateChanged;

    private Border _borderElement = null;
    private Control _contentElement = null;
    private Border _displayContent = null;
    private bool _isFormattingPhone = false;

    private TextBlock _errorTextBlock = null; // ← BUNU EKLEYİN

    private bool _isInitialized = false;

    public FormTextBox()
    {
        DefaultStyleKey = typeof(FormTextBox);
        RegisterPropertyChangedCallback(TextProperty, OnTextChanged);
        BeforeTextChanging += OnBeforeTextChanging;
    }

    public FormVisualState VisualState { get; private set; }

    #region DataType
    public TextDataType DataType
    {
        get { return (TextDataType)GetValue(DataTypeProperty); }
        set { SetValue(DataTypeProperty, value); }
    }

    public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register(
        nameof(DataType),
        typeof(TextDataType),
        typeof(FormTextBox),
        new PropertyMetadata(TextDataType.String, OnPropertyChanged));
    #endregion

    #region Format
    public string Format { get { return (string)GetValue(FormatProperty); } set { SetValue(FormatProperty, value); } }

    public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
        nameof(Format),
        typeof(string),
        typeof(FormTextBox),
        new PropertyMetadata(null, OnPropertyChanged));
    #endregion

    #region FormattedText
    public string FormattedText
    {
        get { return (string)GetValue(FormattedTextProperty); }
        set { SetValue(FormattedTextProperty, value); }
    }

    public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.Register(
        nameof(FormattedText),
        typeof(string),
        typeof(FormTextBox),
        new PropertyMetadata(null, OnPropertyChanged));
    #endregion

    #region Mode*
    public FormEditMode Mode
    {
        get { return (FormEditMode)GetValue(ModeProperty); }
        set { SetValue(ModeProperty, value); }
    }

    private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormTextBox;
        control.UpdateMode();
        control.UpdateVisualState();
    }

    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
        nameof(Mode),
        typeof(FormEditMode),
        typeof(FormTextBox),
        new PropertyMetadata(FormEditMode.Auto, ModeChanged));
    #endregion

    protected override void OnApplyTemplate()
    {
        _borderElement = base.GetTemplateChild("BorderElement") as Border;
        _contentElement = base.GetTemplateChild("ContentElement") as Control;
        _displayContent = base.GetTemplateChild("DisplayContent") as Border;

        _errorTextBlock = base.GetTemplateChild("ErrorTextBlock") as TextBlock; // ← EKLEYİN
        _isInitialized = true;

        UpdateMode();
        UpdateVisualState();
        //Validation özelliklerini uygula
        UpdateValidationState();
        base.OnApplyTemplate();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormTextBox;
        control.ApplyTextFormat();
    }

    private void OnTextChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (DataType == TextDataType.Phone && !_isFormattingPhone)
        {
            _isFormattingPhone = true;

            // Mevcut cursor pozisyonunu kaydet
            int cursorPosition = SelectionStart;

            // Sadece rakamları al
            string digitsOnly = new string(Text.Where(char.IsDigit).ToArray());

            // Formatla
            string formatted = FormatPhoneNumber(digitsOnly);

            if (formatted != Text)
            {
                // Text'i güncelle
                Text = formatted;

                // Cursor pozisyonunu hesapla
                // Cursor'dan önceki rakam sayısını bul
                int digitsBeforeCursor = Text.Substring(0, Math.Min(cursorPosition, Text.Length))
                                             .Count(char.IsDigit);

                // Yeni cursor pozisyonunu hesapla
                int newCursorPosition = 0;
                int digitCount = 0;

                for (int i = 0; i < formatted.Length; i++)
                {
                    if (char.IsDigit(formatted[i]))
                    {
                        digitCount++;
                        if (digitCount >= digitsBeforeCursor)
                        {
                            newCursorPosition = i + 1;
                            break;
                        }
                    }
                }

                SelectionStart = Math.Min(newCursorPosition, formatted.Length);
            }

            _isFormattingPhone = false;
        }
        ApplyTextFormat();
        // Text değişince hatayı temizle
        if (HasError)
        {
            HasError = false;
            ErrorMessage = null;
        }
    }

    private void ApplyTextFormat()
    {
        switch (DataType)
        {
            case TextDataType.Integer:
                Int64.TryParse(Text, out Int64 n);
                FormattedText = n.ToString(Format);
                break;
            case TextDataType.Decimal:
                Decimal.TryParse(Text, out decimal m);
                FormattedText = m.ToString(Format);
                break;
            case TextDataType.Double:
                Double.TryParse(Text, out double d);
                FormattedText = d.ToString(Format);
                break;
            case TextDataType.Phone:  // ← BURAYI EKLEYİN
                FormattedText = FormatPhoneNumber(Text);
                break;
            case TextDataType.String:
            default:
                FormattedText = Text;
                break;
        }
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        switch (DataType)
        {
            case TextDataType.Integer:
                Int64.TryParse(Text, out Int64 n);
                Text = n == 0 ? string.Empty : n.ToString();
                break;
            case TextDataType.Decimal:
                Decimal.TryParse(Text, out decimal m);
                Text = m == 0 ? string.Empty : m.ToString();
                break;
            case TextDataType.Double:
                Double.TryParse(Text, out double d);
                Text = d == 0 ? string.Empty : d.ToString();
                break;
            case TextDataType.String:

            default:
                break;
        }

        if (Mode == FormEditMode.Auto)
        {
            SetVisualState(FormVisualState.Focused);
        }

        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        if (VisualState == FormVisualState.Focused)
        {
            SetVisualState(FormVisualState.Ready);
        }

        switch (DataType)
        {
            case TextDataType.Integer:
                if (!Int64.TryParse(Text, out Int64 n))
                {
                    Text = "0";
                }
                break;
            case TextDataType.Decimal:
                if (!Decimal.TryParse(Text, out decimal m))
                {
                    Text = "0";
                }
                break;
            case TextDataType.Double:
                if (!Double.TryParse(Text, out double d))
                {
                    Text = "0";
                }
                break;
            case TextDataType.String:

            default:
                break;
        }

        base.OnLostFocus(e);
    }

    private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        string str = args.NewText;
        if (String.IsNullOrEmpty(str) || str == "-")
        {
            return;
        }

        switch (DataType)
        {
            case TextDataType.Integer:
                args.Cancel = !Int64.TryParse(str, out Int64 n);
                break;
            case TextDataType.Decimal:
                args.Cancel = !Decimal.TryParse(str, out decimal m);
                break;
            case TextDataType.Double:
                args.Cancel = !Double.TryParse(str, out double d);
                break;
            case TextDataType.Phone:
                string digitsOnly = new string(str.Where(char.IsDigit).ToArray());
                args.Cancel = digitsOnly.Length > 11; // Maksimum 11 hane
                break;
        }
    }

    private void UpdateMode()
    {
        switch (Mode)
        {
            case FormEditMode.Auto:
                VisualState = FormVisualState.Idle;
                break;
            case FormEditMode.ReadWrite:
                VisualState = FormVisualState.Ready;
                break;
            case FormEditMode.ReadOnly:
                VisualState = FormVisualState.Disabled;
                break;
        }
    }

    public void SetVisualState(FormVisualState visualState)
    {
        if (Mode == FormEditMode.ReadOnly)
        {
            visualState = FormVisualState.Disabled;
        }

        if (visualState != VisualState)
        {
            VisualState = visualState;
            UpdateVisualState();
            VisualStateChanged?.Invoke(this, visualState);
        }
    }

    private void UpdateVisualState()
    {
        if (_isInitialized)
        {
            switch (VisualState)
            {
                case FormVisualState.Idle:
                    _borderElement.Opacity = 0.40;
                    _contentElement.Visibility = Visibility.Collapsed;
                    _displayContent.Background = TransparentBrush;
                    _displayContent.Visibility = Visibility.Visible;
                    break;
                case FormVisualState.Ready:
                    _borderElement.Opacity = 1.0;
                    _contentElement.Visibility = Visibility.Collapsed;
                    _displayContent.Background = OpaqueBrush;
                    _displayContent.Visibility = Visibility.Visible;
                    break;
                case FormVisualState.Focused:
                    _borderElement.Opacity = 1.0;
                    _contentElement.Visibility = Visibility.Visible;
                    _displayContent.Visibility = Visibility.Collapsed;
                    break;
                case FormVisualState.Disabled:
                    _borderElement.Opacity = 0.40;
                    _contentElement.Visibility = Visibility.Visible;
                    _displayContent.Visibility = Visibility.Collapsed;
                    IsEnabled = false;
                    Opacity = 0.75;
                    break;
            }
        }
    }

    private readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    private readonly Brush OpaqueBrush = new SolidColorBrush(Colors.Transparent);

    #region Validation Properties
    public string ErrorMessage
    {
        get { return (string)GetValue(ErrorMessageProperty); }
        set { SetValue(ErrorMessageProperty, value); }
    }

    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
        nameof(ErrorMessage),
        typeof(string),
        typeof(FormTextBox),
        new PropertyMetadata(null, OnErrorMessageChanged));

    public bool HasError { get { return (bool)GetValue(HasErrorProperty); } set { SetValue(HasErrorProperty, value); } }

    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(
        nameof(HasError),
        typeof(bool),
        typeof(FormTextBox),
        new PropertyMetadata(false, OnHasErrorChanged));

    private static void OnErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormTextBox;
        var errorMessage = e.NewValue as string;
        control.HasError = !string.IsNullOrEmpty(errorMessage);
        control.UpdateValidationState();
    }

    private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormTextBox;
        control.UpdateValidationState();
    }

    public void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    public void SetError(string errorMessage)
    {
        ErrorMessage = errorMessage;
        HasError = true;
    }

    private void UpdateValidationState()
    {
        if (_isInitialized && _borderElement != null)
        {
            if (HasError && !string.IsNullOrEmpty(ErrorMessage))
            {
                // Koyu temaya uygun kırmızı border
                _borderElement.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 211, 47, 47));
                _borderElement.BorderThickness = new Thickness(2);

                // Hata mesajını göster
                if (_errorTextBlock != null)
                {
                    _errorTextBlock.Text = ErrorMessage;
                    _errorTextBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Normal border'a dön
                _borderElement.BorderBrush = (Brush)GetValue(BorderBrushProperty);
                _borderElement.BorderThickness = (Thickness)GetValue(BorderThicknessProperty);

                // Hata mesajını gizle
                if (_errorTextBlock != null)
                {
                    _errorTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
    #endregion

    #region Mask
    private string FormatPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return string.Empty;

        // Sadece rakamları al
        string digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Maksimum 11 hane
        if (digits.Length > 11)
            digits = digits.Substring(0, 11);

        // Formatla: 0 (532) 123 45 67
        if (digits.Length == 0)
            return string.Empty;
        if (digits.Length <= 1)
            return digits;
        if (digits.Length <= 4)
            return $"{digits[0]} ({digits.Substring(1)})";
        if (digits.Length <= 7)
            return $"{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4)}";
        if (digits.Length <= 9)
            return $"{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 3)} {digits.Substring(7)}";

        return $"{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 3)} {digits.Substring(7, 2)} {digits.Substring(9)}";
    }
    #endregion
}

