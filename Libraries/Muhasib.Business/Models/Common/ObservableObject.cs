using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Muhasib.Business.Models.Common;

public class ObservableObject : BaseModel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsEmpty { get; set; }

    public virtual void Merge(ObservableObject source)
    {
    }

    protected bool Set<T>(ref T field, T newValue = default, [CallerMemberName] string propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, newValue))
        {
            field = newValue;
            NotifyPropertyChanged(propertyName);
            return true;
        }
        return false;
    }

    public void NotifyPropertyChanged(string propertyName)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    public void NotifyChanges()
    {
        // Notify all properties
        NotifyPropertyChanged(string.Empty);
    }

}
