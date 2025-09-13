using System.ComponentModel;

namespace MuhasibPro.Tools.DependencyExpressions;

public interface INotifyExpressionChanged : INotifyPropertyChanged
{
    void NotifyPropertyChanged(string propertyName);
}
