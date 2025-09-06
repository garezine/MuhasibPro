using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Text.RegularExpressions;
using System.Globalization;

namespace MuhasibPro.Infrastructure.Controls;

// Önceden tanımlı format türleri
public enum PredefinedFormat
{
    None,
    PhoneNumber,        // (555) 123-4567
    TCNumber,          // 12345678901
    Currency,          // ₺1.234,56
    CurrencyUSD,       // $1,234.56
    CurrencyEUR,       // €1.234,56
    IBAN,              // TR33 0006 1005 1978 6457 8413 26
    CreditCard,        // 1234 5678 9012 3456
    PostalCode,        // 34000
    Percentage,        // %12,50
    Date,              // 01.01.2024
    Time,              // 14:30
    DateTime           // 01.01.2024 14:30
}

public class FormTextBox : TextBox, IFormControl
{
    public event EventHandler<FormVisualState> VisualStateChanged;
    public static event EventHandler<bool> GlobalValidationChanged; // Tüm kontroller için global event

    private Border _borderElement = null;
    private Control _contentElement = null;
    private Border _displayContent = null;
    private TextBlock _validationTextBlock = null;

    private bool _isInitialized = false;
    private bool _isValid = true;

    public FormTextBox()
    {
        DefaultStyleKey = typeof(FormTextBox);
        RegisterPropertyChangedCallback(TextProperty, OnTextChanged);
        BeforeTextChanging += OnBeforeTextChanging;
        
        Loaded += OnLoaded;
    }
 

    public FormVisualState VisualState { get; private set; }

    #region Properties

    #region DataType
    public TextDataType DataType
    {
        get { return (TextDataType)GetValue(DataTypeProperty); }
        set { SetValue(DataTypeProperty, value); }
    }

    public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register(nameof(DataType), typeof(TextDataType), typeof(FormTextBox), new PropertyMetadata(TextDataType.String, OnPropertyChanged));
    #endregion

    #region Format
    public string Format
    {
        get { return (string)GetValue(FormatProperty); }
        set { SetValue(FormatProperty, value); }
    }

    public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(FormTextBox), new PropertyMetadata(null, OnPropertyChanged));
    #endregion

    #region PredefinedFormat
    public PredefinedFormat PredefinedFormat
    {
        get { return (PredefinedFormat)GetValue(PredefinedFormatProperty); }
        set { SetValue(PredefinedFormatProperty, value); }
    }

    public static readonly DependencyProperty PredefinedFormatProperty = DependencyProperty.Register(nameof(PredefinedFormat), typeof(PredefinedFormat), typeof(FormTextBox), new PropertyMetadata(PredefinedFormat.None, OnPredefinedFormatChanged));

    private static void OnPredefinedFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormTextBox;
        control.UpdatePredefinedFormat();
        control.ValidateAndFormat();
    }
    #endregion

    #region FormattedText
    public string FormattedText
    {
        get { return (string)GetValue(FormattedTextProperty); }
        set { SetValue(FormattedTextProperty, value); }
    }

    public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.Register(nameof(FormattedText), typeof(string), typeof(FormTextBox), new PropertyMetadata(null));
    #endregion

    #region IsRequired
    public bool IsRequired
    {
        get { return (bool)GetValue(IsRequiredProperty); }
        set { SetValue(IsRequiredProperty, value); }
    }

    public static readonly DependencyProperty IsRequiredProperty = DependencyProperty.Register(nameof(IsRequired), typeof(bool), typeof(FormTextBox), new PropertyMetadata(false, OnValidationPropertyChanged));
    #endregion

    #region ValidationMessage
    public string ValidationMessage
    {
        get { return (string)GetValue(ValidationMessageProperty); }
        set { SetValue(ValidationMessageProperty, value); }
    }

    public static readonly DependencyProperty ValidationMessageProperty = DependencyProperty.Register(nameof(ValidationMessage), typeof(string), typeof(FormTextBox), new PropertyMetadata(""));
    #endregion

    #region IsValid
    public bool IsValid
    {
        get { return _isValid; }
        private set
        {
            if (_isValid != value)
            {
                _isValid = value;
                UpdateValidationVisual();

                // Global validasyon durumunu bildir
                GlobalValidationChanged?.Invoke(this, value);
            }
        }
    }
    #endregion

    #region Mode
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

    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(FormEditMode), typeof(FormTextBox), new PropertyMetadata(FormEditMode.Auto, ModeChanged));
    #endregion

    #endregion

    #region Template & Initialization

    protected override void OnApplyTemplate()
    {
        _borderElement = base.GetTemplateChild("BorderElement") as Border;
        _contentElement = base.GetTemplateChild("ContentElement") as Control;
        _displayContent = base.GetTemplateChild("DisplayContent") as Border;

        // ValidationTextBlock'u template'den al veya oluştur
        _validationTextBlock = base.GetTemplateChild("ValidationTextBlock") as TextBlock;
        if (_validationTextBlock == null)
        {
            // Eğer template'de yoksa, parent'a ekle
            //CreateValidationTextBlock();
        }

        _isInitialized = true;

        UpdatePredefinedFormat();
        UpdateMode();
        UpdateVisualState();

        base.OnApplyTemplate();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_validationTextBlock == null)
        {
            //CreateValidationTextBlock();
        }
    }

    

    #endregion

    #region Validation & Formatting

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormTextBox;
        control.ValidateAndFormat();
    }

    private static void OnValidationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormTextBox;
        control.ValidateInput();
    }

    private void OnTextChanged(DependencyObject sender, DependencyProperty dp)
    {
        ValidateAndFormat();
    }

    private void ValidateAndFormat()
    {
        ApplyTextFormat();
        ValidateInput();
    }

    private void ValidateInput()
    {
        string value = Text?.Trim() ?? "";
        bool isValid = true;
        string message = "";

        // Zorunluluk kontrolü
        if (IsRequired && string.IsNullOrEmpty(value))
        {
            isValid = false;
            message = $"{Header} alanı zorunludur.";
        }
        else if (!string.IsNullOrEmpty(value))
        {
            // Format kontrolü
            var validation = ValidateFormat(value);
            isValid = validation.isValid;
            message = validation.message;
        }

        IsValid = isValid;
        ValidationMessage = message;
    }

    private (bool isValid, string message) ValidateFormat(string value)
    {
        if (PredefinedFormat == PredefinedFormat.None)
            return (true, "");

        string cleanValue = GetCleanText(value);

        return PredefinedFormat switch
        {
            PredefinedFormat.PhoneNumber => ValidatePhone(cleanValue),
            PredefinedFormat.TCNumber => ValidateTC(cleanValue),
            PredefinedFormat.Currency or PredefinedFormat.CurrencyUSD or PredefinedFormat.CurrencyEUR => ValidateCurrency(cleanValue),
            PredefinedFormat.IBAN => ValidateIBAN(value),
            PredefinedFormat.CreditCard => ValidateCreditCard(cleanValue),
            PredefinedFormat.PostalCode => ValidatePostalCode(cleanValue),
            PredefinedFormat.Date => ValidateDate(cleanValue),
            _ => (true, "")
        };
    }

    #region Specific Validators

    private (bool isValid, string message) ValidatePhone(string digits)
    {
        // Uzunluk kontrolü
        if (digits.Length < 10)
            return (false, "Geçersiz telefon numarası: Numara en az 10 haneli olmalıdır.");

        // 0 ile başlama kontrolü
        if (digits.StartsWith("0"))
            return (false, "Geçersiz format: Numara 0 ile başlamamalıdır.");

        // Format kontrolü (isteğe bağlı)
        // Eğer belirli bir formatta olmasını istiyorsanız bu kısmı ekleyebilirsiniz
        // Örnek: (123) 456 7890
        if (digits.Length == 10)
        {
            // Eğer 10 haneli bir numara ise formatını kontrol edelim
            try
            {
                var formatted = string.Format("({0}) {1} {2}", digits.Substring(0, 3), digits.Substring(3, 3), digits.Substring(6));
                return (true, formatted); // İsterseniz sadece true döndürüp message'ı boş bırakabilirsiniz
            }
            catch
            {
                return (false, "Geçersiz telefon numarası: Format hatası.");
            }
        }

        return (true, "");
    }

    private (bool isValid, string message) ValidateTC(string digits)
    {
        if (digits.Length != 11)
            return (false, "TC Kimlik No 11 haneli olmalıdır.");

        if (digits[0] == '0')
            return (false, "TC Kimlik No 0 ile başlayamaz.");

        if (!IsValidTCAlgorithm(digits))
            return (false, "Geçersiz TC Kimlik No.");

        return (true, "");
    }

    private (bool isValid, string message) ValidateCurrency(string digits)
    {
        if (!Decimal.TryParse(digits, out decimal amount))
            return (false, "Geçerli bir tutar giriniz.");

        if (amount < 0)
            return (false, "Tutar negatif olamaz.");

        return (true, "");
    }

    private (bool isValid, string message) ValidateIBAN(string value)
    {
        string cleanIban = Regex.Replace(value.ToUpper(), @"[^A-Z0-9]", "");

        if (!cleanIban.StartsWith("TR"))
            return (false, "IBAN TR ile başlamalıdır.");

        if (cleanIban.Length != 26)
            return (false, "IBAN 26 karakter olmalıdır.");

        return (true, "");
    }

    private (bool isValid, string message) ValidateCreditCard(string digits)
    {
        if (digits.Length < 13 || digits.Length > 19)
            return (false, "Kredi kartı numarası 13-19 haneli olmalıdır.");

        return (true, "");
    }

    private (bool isValid, string message) ValidatePostalCode(string digits)
    {
        if (digits.Length != 5)
            return (false, "Posta kodu 5 haneli olmalıdır.");

        if (!int.TryParse(digits, out int code) || code < 1000 || code > 81999)
            return (false, "Geçerli bir posta kodu giriniz.");

        return (true, "");
    }

    private (bool isValid, string message) ValidateDate(string digits)
    {
        if (digits.Length != 8)
            return (false, "Tarih GGAAYYYY formatında olmalıdır.");

        string day = digits.Substring(0, 2);
        string month = digits.Substring(2, 2);
        string year = digits.Substring(4, 4);

        if (!DateTime.TryParse($"{day}.{month}.{year}", out _))
            return (false, "Geçersiz tarih.");

        return (true, "");
    }

    private bool IsValidTCAlgorithm(string tc)
    {
        if (tc.Length != 11) return false;

        int[] digits = tc.Select(c => int.Parse(c.ToString())).ToArray();

        int oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int evenSum = digits[1] + digits[3] + digits[5] + digits[7];

        int checkDigit1 = (oddSum * 7 - evenSum) % 10;
        int checkDigit2 = (oddSum + evenSum + checkDigit1) % 10;

        return checkDigit1 == digits[9] && checkDigit2 == digits[10];
    }

    #endregion

    private void UpdateValidationVisual()
    {
        if (!_isInitialized) return;

        if (!IsValid)
        {
            // Hata durumu
            if (_borderElement != null)
            {
                _borderElement.BorderBrush = new SolidColorBrush(Colors.Red);
                _borderElement.BorderThickness = new Thickness(2);
            }

            // Validasyon mesajını göster
            if (_validationTextBlock != null)
            {
                _validationTextBlock.Text = ValidationMessage;
                _validationTextBlock.Visibility = Visibility.Visible;
            }
        }
        else
        {
            // Normal durum
            if (_borderElement != null)
            {
                _borderElement.BorderBrush = new SolidColorBrush(Colors.Gray);
                _borderElement.BorderThickness = new Thickness(1);
            }

            // Validasyon mesajını gizle
            if (_validationTextBlock != null)
            {
                _validationTextBlock.Visibility = Visibility.Collapsed;
            }
        }
    }

    #endregion

    #region Text Formatting

    private void UpdatePredefinedFormat()
    {
        if (PredefinedFormat == PredefinedFormat.None) return;

        switch (PredefinedFormat)
        {
            case PredefinedFormat.PhoneNumber:
                DataType = TextDataType.String;
                MaxLength = 10;
                break;
            case PredefinedFormat.TCNumber:
                DataType = TextDataType.Integer;
                MaxLength = 11;
                break;
            case PredefinedFormat.Currency:
            case PredefinedFormat.CurrencyUSD:
            case PredefinedFormat.CurrencyEUR:
                DataType = TextDataType.Decimal;
                Format = "N2";
                break;
            case PredefinedFormat.IBAN:
                DataType = TextDataType.String;
                MaxLength = 32;
                break;
            case PredefinedFormat.CreditCard:
                DataType = TextDataType.String;
                MaxLength = 19;
                break;
            case PredefinedFormat.PostalCode:
                DataType = TextDataType.Integer;
                MaxLength = 5;
                break;
            case PredefinedFormat.Date:
                DataType = TextDataType.String;
                MaxLength = 10;
                break;
        }
    }

    private void ApplyTextFormat()
    {
        if (PredefinedFormat != PredefinedFormat.None)
        {
            ApplyPredefinedFormat();
        }
        else
        {
            ApplyCustomFormat();
        }
    }

    private void ApplyPredefinedFormat()
    {
        string cleanText = GetCleanText(Text);

        FormattedText = PredefinedFormat switch
        {
            PredefinedFormat.PhoneNumber => FormatPhoneNumber(cleanText),
            PredefinedFormat.TCNumber => cleanText,
            PredefinedFormat.Currency => FormatCurrency(cleanText, "tr-TR"),
            PredefinedFormat.CurrencyUSD => FormatCurrency(cleanText, "en-US"),
            PredefinedFormat.CurrencyEUR => FormatCurrency(cleanText, "de-DE"),
            PredefinedFormat.IBAN => FormatIBAN(cleanText),
            PredefinedFormat.CreditCard => FormatCreditCard(cleanText),
            PredefinedFormat.PostalCode => cleanText,
            PredefinedFormat.Date => FormatDate(cleanText),
            _ => Text
        };
    }

    private void ApplyCustomFormat()
    {
        FormattedText = DataType switch
        {
            TextDataType.Integer when Int64.TryParse(Text, out Int64 n) => n.ToString(Format),
            TextDataType.Decimal when Decimal.TryParse(Text, out decimal m) => m.ToString(Format),
            TextDataType.Double when Double.TryParse(Text, out double d) => d.ToString(Format),
            _ => Text
        };
    }

    #region Format Methods

    private string GetCleanText(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return Regex.Replace(text, @"[^\d]", "");
    }

    private string FormatPhoneNumber(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return "";

        return digits.Length switch
        {
            <= 3 => $"({digits}",
            <= 6 => $"({digits.Substring(0, 3)}) {digits.Substring(3)}",
            <= 10 => $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6)}",
            _ => $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}"
        };
    }

    private string FormatCurrency(string digits, string culture)
    {
        if (!Decimal.TryParse(digits, out decimal amount))
            return culture switch
            {
                "tr-TR" => "₺0,00",
                "en-US" => "$0.00",
                "de-DE" => "€0,00",
                _ => "0"
            };

        return amount.ToString("C2", new CultureInfo(culture));
    }

    private string FormatIBAN(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return "";

        string formatted = "";
        for (int i = 0; i < digits.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                formatted += " ";
            formatted += digits[i];
        }

        if (!formatted.StartsWith("TR") && digits.Length >= 2)
            formatted = "TR" + formatted;

        return formatted;
    }

    private string FormatCreditCard(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return "";

        string formatted = "";
        for (int i = 0; i < digits.Length && i < 16; i++)
        {
            if (i > 0 && i % 4 == 0)
                formatted += " ";
            formatted += digits[i];
        }
        return formatted;
    }

    private string FormatDate(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return "";

        return digits.Length switch
        {
            <= 2 => digits,
            <= 4 => $"{digits.Substring(0, 2)}.{digits.Substring(2)}",
            <= 8 => $"{digits.Substring(0, 2)}.{digits.Substring(2, 2)}.{digits.Substring(4)}",
            _ => $"{digits.Substring(0, 2)}.{digits.Substring(2, 2)}.{digits.Substring(4, 4)}"
        };
    }

    #endregion

    #endregion

    #region Focus & Input Handling

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        if (PredefinedFormat != PredefinedFormat.None)
        {
            Text = GetCleanText(Text);
        }
        else
        {
            switch (DataType)
            {
                case TextDataType.Integer:
                    Int64.TryParse(Text, out Int64 n);
                    Text = n == 0 ? "" : n.ToString();
                    break;
                case TextDataType.Decimal:
                    Decimal.TryParse(Text, out decimal m);
                    Text = m == 0 ? "" : m.ToString();
                    break;
                case TextDataType.Double:
                    Double.TryParse(Text, out double d);
                    Text = d == 0 ? "" : d.ToString();
                    break;
            }
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

        ValidateInput();
        base.OnLostFocus(e);
    }

    private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        string str = args.NewText;
        if (String.IsNullOrEmpty(str) || str == "-")
        {
            return;
        }

        if (PredefinedFormat != PredefinedFormat.None)
        {
            switch (PredefinedFormat)
            {
                case PredefinedFormat.PhoneNumber:
                case PredefinedFormat.TCNumber:
                case PredefinedFormat.PostalCode:
                case PredefinedFormat.CreditCard:
                    args.Cancel = !Regex.IsMatch(str, @"^\d*$");
                    break;
                case PredefinedFormat.IBAN:
                    args.Cancel = !Regex.IsMatch(str, @"^[A-Za-z0-9]*$");
                    break;
                case PredefinedFormat.Currency:
                case PredefinedFormat.CurrencyUSD:
                case PredefinedFormat.CurrencyEUR:
                    args.Cancel = !Decimal.TryParse(GetCleanText(str), out _);
                    break;
            }
        }
        else
        {
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
            }
        }
    }

    #endregion

    #region Visual State Management

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
            UpdateValidationVisual();

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

    #endregion

    #region Focus on First Error

    public static void FocusFirstInvalidControl(DependencyObject parent)
    {
        var invalidControl = FindFirstInvalidControl(parent);
        if (invalidControl != null)
        {
            invalidControl.Focus(FocusState.Programmatic);
        }
    }

    private static FormTextBox FindFirstInvalidControl(DependencyObject parent)
    {
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is FormTextBox formTextBox && !formTextBox.IsValid)
            {
                return formTextBox;
            }

            var result = FindFirstInvalidControl(child);
            if (result != null)
                return result;
        }

        return null;
    }

    #endregion

    private readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    private readonly Brush OpaqueBrush = new SolidColorBrush(Colors.Transparent);
}
