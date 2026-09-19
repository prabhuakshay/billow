using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Billow.Data;
using Billow.Gst;

namespace Billow.BusinessDetails;

/// <summary>
/// The Business Details screen: every field, its error, and Save and Cancel. It edits the one
/// Business record, loading it when created. A field is checked each time it changes, and every
/// field is checked on Save.
/// </summary>
public sealed class BusinessDetailsViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    /// <summary>
    /// The rule for each field that can have an error: it gives the reason the field's current
    /// value can't be saved, or null. Save checks every field listed here.
    /// </summary>
    private static readonly Dictionary<string, Func<BusinessDetailsViewModel, string?>> _rules = new()
    {
        [nameof(LegalName)] = screen => IsBlank(screen.LegalName) ? "Enter the Legal Name." : null,
        [nameof(AddressLine1)] = screen => IsBlank(screen.AddressLine1) ? "Enter the first line of the address." : null,
        [nameof(City)] = screen => IsBlank(screen.City) ? "Enter the city." : null,
        [nameof(State)] = screen => screen.State is null ? "Choose the State." : null,
        [nameof(Pin)] = screen => screen.Pin.Trim() switch
        {
            "" => "Enter the 6-digit PIN.",
            { Length: 6 } pin when pin.All(char.IsAsciiDigit) => null,
            _ => "A PIN is exactly 6 digits.",
        },
        [nameof(Pan)] = screen => IsBlank(screen.Pan) ? null : Gst.Pan.Check(screen.Pan).Error,
    };

    private readonly Func<BillowDbContext> _openDatabase;
    private readonly Dictionary<string, string> _errors = [];

    private string _legalName = "";
    private string _tradeName = "";
    private string _addressLine1 = "";
    private string _addressLine2 = "";
    private string _city = "";
    private GstState? _state;
    private string _pin = "";
    private RegistrationType _registrationType = RegistrationType.Unregistered;
    private string _pan = "";

    public BusinessDetailsViewModel(
        Func<BillowDbContext> openDatabase,
        IConfirmationPrompt confirmationPrompt,
        ILogoFilePicker logoFilePicker)
    {
        _openDatabase = openDatabase;
        SaveCommand = new RelayCommand(() =>
        {
            if (Save())
            {
                CloseRequested?.Invoke(this, true);
            }
        });
        CancelCommand = new RelayCommand(Cancel);
        Load();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>Asks the window to close. The argument is true if the details were saved.</summary>
    public event EventHandler<bool>? CloseRequested;

    public bool HasErrors => _errors.Count > 0;

    public IReadOnlyList<GstState> States { get; } = GstState.All;

    /// <summary>The Registration Types that can be chosen. Regular and Composition come later.</summary>
    public IReadOnlyList<RegistrationType> RegistrationTypes { get; } = [RegistrationType.Unregistered];

    /// <summary>Saves, then closes the screen; while there are errors, only shows them.</summary>
    public ICommand SaveCommand { get; }

    /// <summary>Puts back the saved details, then closes the screen.</summary>
    public ICommand CancelCommand { get; }

    public string LegalName
    {
        get => _legalName;
        set => SetAndCheck(ref _legalName, value);
    }

    public string TradeName
    {
        get => _tradeName;
        set => SetAndCheck(ref _tradeName, value);
    }

    public string AddressLine1
    {
        get => _addressLine1;
        set => SetAndCheck(ref _addressLine1, value);
    }

    public string AddressLine2
    {
        get => _addressLine2;
        set => SetAndCheck(ref _addressLine2, value);
    }

    public string City
    {
        get => _city;
        set => SetAndCheck(ref _city, value);
    }

    public GstState? State
    {
        get => _state;
        set => SetAndCheck(ref _state, value);
    }

    public string Pin
    {
        get => _pin;
        set => SetAndCheck(ref _pin, value);
    }

    public RegistrationType RegistrationType
    {
        get => _registrationType;
        set => SetAndCheck(ref _registrationType, value);
    }

    public string Pan
    {
        get => _pan;
        set => SetAndCheck(ref _pan, value);
    }

    /// <summary>Stores the details as the one Business. False if nothing was saved.</summary>
    public bool Save()
    {
        foreach (var field in _rules.Keys)
        {
            Check(field);
        }

        if (HasErrors)
        {
            return false;
        }

        using var db = _openDatabase();
        var business = db.Businesses.SingleOrDefault();
        if (business is null)
        {
            business = new Business();
            db.Businesses.Add(business);
        }

        business.LegalName = LegalName.Trim();
        business.TradeName = NullIfBlank(TradeName);
        business.AddressLine1 = AddressLine1.Trim();
        business.AddressLine2 = NullIfBlank(AddressLine2);
        business.City = City.Trim();
        business.StateCode = State!.Code;
        business.Pin = Pin.Trim();
        business.RegistrationType = RegistrationType;
        business.Pan = IsBlank(Pan) ? null : Gst.Pan.Check(Pan).Value!.Value;

        db.SaveChanges();
        return true;
    }

    /// <summary>The error to show next to <paramref name="propertyName"/>, or null if it has none.</summary>
    public string? ErrorFor(string propertyName) => _errors.GetValueOrDefault(propertyName);

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName) =>
        ErrorFor(propertyName ?? "") is { } error ? new[] { error } : Array.Empty<string>();

    private void Check(string propertyName)
    {
        var error = _rules.TryGetValue(propertyName, out var rule) ? rule(this) : null;
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
        OnPropertyChanged(nameof(HasErrors));
    }

    private void Cancel()
    {
        Load();
        CloseRequested?.Invoke(this, false);
    }

    /// <summary>Shows the saved Business (an empty form if there is none yet), with no errors.</summary>
    private void Load()
    {
        using var db = _openDatabase();
        var business = db.Businesses.SingleOrDefault();

        _legalName = business?.LegalName ?? "";
        _tradeName = business?.TradeName ?? "";
        _addressLine1 = business?.AddressLine1 ?? "";
        _addressLine2 = business?.AddressLine2 ?? "";
        _city = business?.City ?? "";
        _state = business is null ? null : GstState.Find(business.StateCode);
        _pin = business?.Pin ?? "";
        _registrationType = business?.RegistrationType ?? RegistrationType.Unregistered;
        _pan = business?.Pan ?? "";

        var fieldsWithErrors = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var field in fieldsWithErrors)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(field));
        }

        // An empty name means every property changed.
        OnPropertyChanged("");
    }

    private static bool IsBlank(string value) => string.IsNullOrWhiteSpace(value);

    private static string? NullIfBlank(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void SetAndCheck<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
        Check(propertyName);
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
