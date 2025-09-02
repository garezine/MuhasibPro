using System.ComponentModel;

namespace MuhasibPro.Infrastructure.Infrastructure.Tools.DependencyExpressions;

public interface INotifyExpressionChanged : INotifyPropertyChanged
{
    void NotifyPropertyChanged(string propertyName);
}
