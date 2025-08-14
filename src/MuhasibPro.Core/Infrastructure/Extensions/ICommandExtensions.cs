using System.Windows.Input;

namespace MuhasibPro.Core.Infrastructure.Extensions;

static public class ICommandExtensions
{
    static public void TryExecute(this ICommand command, object parameter = null)
    {
        if (command != null)
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
