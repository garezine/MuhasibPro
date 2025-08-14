using System.ComponentModel;

namespace MuhasibPro.Core.Infrastructure.Tools.DependencyExpressions;

public interface INotifyExpressionChanged : INotifyPropertyChanged
{
    void NotifyPropertyChanged(string propertyName);
}
