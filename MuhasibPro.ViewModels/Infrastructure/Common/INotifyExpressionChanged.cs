using System.ComponentModel;

namespace MuhasibPro.ViewModels.Infrastructure.Common;

public interface INotifyExpressionChanged : INotifyPropertyChanged
{
    void NotifyPropertyChanged(string propertyName);
}
