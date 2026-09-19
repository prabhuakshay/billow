using System.Windows.Input;

namespace Billow;

/// <summary>A command that runs <paramref name="execute"/> and can always be executed.</summary>
public sealed class RelayCommand(Action execute) : ICommand
{
    // Never raised: the command can always be executed.
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute();
}
