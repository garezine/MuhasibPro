namespace MuhasibPro.Controls
{
    public sealed partial class CustomContentDialog : ContentDialog
    {
        private string _titleText = string.Empty;

        public CustomContentDialog()
        {
            InitializeComponent();
        }

        public enum DialogIcon
        {
            None,
            Success,
            Error,
            Warning,
            Info,
            Question,
            Edit
        }

        public void SetIcon(DialogIcon icon)
        {
            // Tüm icon'larý gizle
            SuccessIcon.Visibility = Visibility.Collapsed;
            ErrorIcon.Visibility = Visibility.Collapsed;
            WarningIcon.Visibility = Visibility.Collapsed;
            InfoIcon.Visibility = Visibility.Collapsed;
            QuestionIcon.Visibility = Visibility.Collapsed;
            EditIcon.Visibility = Visibility.Collapsed;

            // Seçilen icon'u göster
            switch (icon)
            {
                case DialogIcon.Success:
                    SuccessIcon.Visibility = Visibility.Visible;
                    break;
                case DialogIcon.Error:
                    ErrorIcon.Visibility = Visibility.Visible;
                    break;
                case DialogIcon.Warning:
                    WarningIcon.Visibility = Visibility.Visible;
                    break;
                case DialogIcon.Info:
                    InfoIcon.Visibility = Visibility.Visible;
                    break;
                case DialogIcon.Question:
                    QuestionIcon.Visibility = Visibility.Visible;
                    break;
                case DialogIcon.Edit:
                    EditIcon.Visibility = Visibility.Visible;
                    break;
                case DialogIcon.None:
                default:
                    // Hiçbir icon gösterme
                    break;
            }
        }

        // Title property'sini override et
        public new string Title
        {
            get => _titleText;
            set
            {
                _titleText = value;
                if (TitleText != null)
                {
                    TitleText.Text = value;
                }
            }
        }
    }
}