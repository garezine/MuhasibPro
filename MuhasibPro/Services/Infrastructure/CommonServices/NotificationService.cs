using Microsoft.UI.Xaml.Media.Animation;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;

namespace MuhasibPro.Services.Infrastructure.CommonServices
{
    public static class NotificationServiceExtensions
    {
        public static void InitializeNotificationService(this INotificationService notificationService, InfoBar infoBar)
        {
            if (notificationService is NotificationService service)
            {
                service.Initialize(infoBar);
            }
        }
    }

    public class NotificationService : INotificationService
    {
        private InfoBar _infoBar;

        public void Initialize(InfoBar infoBar)
        {
            _infoBar = infoBar;

            // Açılış/kapanış animasyonları
            if (_infoBar != null)
            {
                var storyboard = new Storyboard();
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(fadeIn, _infoBar);
                Storyboard.SetTargetProperty(fadeIn, "Opacity");
                storyboard.Children.Add(fadeIn);

                _infoBar.Loaded += (s, e) => storyboard.Begin();
            }
        }

        public void ShowError(string title, string message, int autoCloseDuration = 5000)
        {
            ShowNotification(title, message, InfoBarSeverity.Error, autoCloseDuration);
        }

        public void ShowWarning(string title, string message, int autoCloseDuration = 5000)
        {
            ShowNotification(title, message, InfoBarSeverity.Warning, autoCloseDuration);
        }

        public void ShowSuccess(string title, string message, int autoCloseDuration = 3000)
        {
            ShowNotification(title, message, InfoBarSeverity.Success, autoCloseDuration);
        }
        public void ShowInfo(string title, string message, int autoCloseDuration = 4000)
        {
            ShowNotification(title, message, InfoBarSeverity.Informational, autoCloseDuration);
        }

        private void ShowNotification(string title, string message, InfoBarSeverity severity, int autoCloseDuration)
        {
            if (_infoBar == null) return;

            _infoBar.DispatcherQueue.TryEnqueue(() =>
            {
                _infoBar.Title = title;
                _infoBar.Message = message;
                _infoBar.Severity = severity;

                // Yumuşak açılış
                _infoBar.Opacity = 0;
                _infoBar.IsOpen = true;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var storyboard = new Storyboard();
                Storyboard.SetTarget(fadeIn, _infoBar);
                Storyboard.SetTargetProperty(fadeIn, "Opacity");
                storyboard.Children.Add(fadeIn);
                storyboard.Begin();

                // Otomatik kapanma
                if (autoCloseDuration > 0)
                {
                    Task.Delay(autoCloseDuration).ContinueWith(_ =>
                    {
                        CloseWithAnimation();
                    });
                }
            });
        }

        public void Close()
        {
            CloseWithAnimation();
        }

        private void CloseWithAnimation()
        {
            if (_infoBar == null) return;

            _infoBar.DispatcherQueue.TryEnqueue(() =>
            {
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                var storyboard = new Storyboard();
                Storyboard.SetTarget(fadeOut, _infoBar);
                Storyboard.SetTargetProperty(fadeOut, "Opacity");
                storyboard.Children.Add(fadeOut);

                storyboard.Completed += (s, e) =>
                {
                    _infoBar.IsOpen = false;
                    _infoBar.Opacity = 1; // Sonraki açılış için sıfırla
                };

                storyboard.Begin();
            });
        }
    }
}
