using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MuhasibPro.Controls;

public class FormCalendarDatePicker : CalendarDatePicker, IFormControl
{
    public event EventHandler<FormVisualState> VisualStateChanged;
    private Border _backgroundBorder = null;
    private TextBlock _errorTextBlock = null; // ← BUNU EKLEYİN

    private bool _isInitialized = false;

    public FormCalendarDatePicker()
    {
        DefaultStyleKey = typeof(FormCalendarDatePicker);
    }

    public FormVisualState VisualState
    {
        get; private set;
    }

    #region Mode*
    public FormEditMode Mode
    {
        get
        {
            return (FormEditMode)GetValue(ModeProperty);
        }
        set
        {
            SetValue(ModeProperty, value);
        }
    }

    private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormCalendarDatePicker;
        control.UpdateMode();
        control.UpdateVisualState();
    }

    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(FormEditMode), typeof(FormCalendarDatePicker), new PropertyMetadata(FormEditMode.Auto, ModeChanged));
    #endregion

    protected override void OnApplyTemplate()
    {
        _backgroundBorder = base.GetTemplateChild("Background") as Border;
        _errorTextBlock = base.GetTemplateChild("ErrorTextBlock") as TextBlock; // ← EKLEYİN
        _isInitialized = true;

        UpdateMode();
        UpdateVisualState();

        base.OnApplyTemplate();
    }

    protected override void OnTapped(TappedRoutedEventArgs e)
    {
        if (Mode == FormEditMode.Auto)
        {
            SetVisualState(FormVisualState.Focused);
        }

        base.OnTapped(e);
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
                    _backgroundBorder.Opacity = 0.40;
                    _backgroundBorder.Background = TransparentBrush;
                    break;
                case FormVisualState.Ready:
                    _backgroundBorder.Opacity = 1.0;
                    _backgroundBorder.Background = OpaqueBrush;
                    break;
                case FormVisualState.Focused:
                    _backgroundBorder.Opacity = 1.0;
                    _backgroundBorder.Background = OpaqueBrush;
                    break;
                case FormVisualState.Disabled:
                    _backgroundBorder.Opacity = 0.40;
                    _backgroundBorder.Background = TransparentBrush;
                    IsEnabled = false;
                    Opacity = 0.75;
                    break;
            }
        }
    }

    private readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    private readonly Brush OpaqueBrush = new SolidColorBrush(Colors.White);

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
            typeof(FormCalendarDatePicker),
            new PropertyMetadata(null, OnErrorMessageChanged));

    public bool HasError
    {
        get { return (bool)GetValue(HasErrorProperty); }
        set { SetValue(HasErrorProperty, value); }
    }

    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(FormCalendarDatePicker),
            new PropertyMetadata(false, OnHasErrorChanged));

    private static void OnErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormCalendarDatePicker;
        var errorMessage = e.NewValue as string;
        control.HasError = !string.IsNullOrEmpty(errorMessage);
        control.UpdateValidationState();
    }

    private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FormCalendarDatePicker;
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
        if (_isInitialized && _backgroundBorder != null)
        {
            if (HasError && !string.IsNullOrEmpty(ErrorMessage))
            {
                // Koyu temaya uygun kırmızı border
                _backgroundBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 211, 47, 47));
                _backgroundBorder.BorderThickness = new Thickness(2);

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
                _backgroundBorder.BorderBrush = (Brush)GetValue(BorderBrushProperty);
                _backgroundBorder.BorderThickness = (Thickness)GetValue(BorderThicknessProperty);

                // Hata mesajını gizle
                if (_errorTextBlock != null)
                {
                    _errorTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }
        #endregion
    }
}
