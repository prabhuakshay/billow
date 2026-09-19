using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Billow.BusinessDetails;

/// <summary>
/// One Additional Registration row on the Business Details screen. It moves and removes itself
/// within the screen's list of rows, which it is given. A row needs both a label and a number,
/// or neither: an empty row isn't saved. It is checked each time it changes.
/// </summary>
public sealed class AdditionalRegistrationViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly ObservableCollection<AdditionalRegistrationViewModel> _rows;
    private readonly Dictionary<string, string> _errors = [];
    private string _label = "";
    private string _number = "";

    public AdditionalRegistrationViewModel(ObservableCollection<AdditionalRegistrationViewModel> rows)
    {
        _rows = rows;
        MoveUpCommand = new RelayCommand(() => Move(-1));
        MoveDownCommand = new RelayCommand(() => Move(1));
        RemoveCommand = new RelayCommand(() => _rows.Remove(this));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => _errors.Count > 0;

    /// <summary>Whether both the label and the number are blank.</summary>
    public bool IsEmpty => IsBlank(Label) && IsBlank(Number);

    /// <summary>Swaps the row with the one above; does nothing for the first row.</summary>
    public ICommand MoveUpCommand { get; }

    /// <summary>Swaps the row with the one below; does nothing for the last row.</summary>
    public ICommand MoveDownCommand { get; }

    public ICommand RemoveCommand { get; }

    public string Label
    {
        get => _label;
        set => SetAndCheck(ref _label, value);
    }

    public string Number
    {
        get => _number;
        set => SetAndCheck(ref _number, value);
    }

    /// <summary>The error to show next to <paramref name="propertyName"/>, or null if it has none.</summary>
    public string? ErrorFor(string propertyName) => _errors.GetValueOrDefault(propertyName);

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName) =>
        ErrorFor(propertyName ?? "") is { } error ? new[] { error } : Array.Empty<string>();

    /// <summary>Works out the label's and the number's errors afresh.</summary>
    public void Check()
    {
        SetError(nameof(Label), IsBlank(Label) && !IsBlank(Number) ? "Enter the label." : null);
        SetError(nameof(Number), IsBlank(Number) && !IsBlank(Label) ? "Enter the number." : null);
    }

    private static bool IsBlank(string value) => string.IsNullOrWhiteSpace(value);

    private void SetError(string propertyName, string? error)
    {
        if (error == ErrorFor(propertyName))
        {
            return;
        }

        if (error is null)
        {
            _errors.Remove(propertyName);
        }
        else
        {
            _errors[propertyName] = error;
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
    }

    private void Move(int offset)
    {
        var from = _rows.IndexOf(this);
        var to = from + offset;
        if (to >= 0 && to < _rows.Count)
        {
            _rows.Move(from, to);
        }
    }

    private void SetAndCheck(ref string field, string value, [CallerMemberName] string propertyName = "")
    {
        if (field == value)
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        Check();
    }
}
