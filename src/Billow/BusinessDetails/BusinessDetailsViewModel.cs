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
        [nameof(RegistrationType)] = screen =>
            screen.RegistrationType is null ? "Choose the Registration Type." : null,
        [nameof(Gstin)] = screen => !screen.IsGstinApplicable ? null
            : IsBlank(screen.Gstin) ? "Enter the GSTIN."
            : Gst.Gstin.Check(screen.Gstin).Error,
        [nameof(Pan)] = screen => IsBlank(screen.Pan) ? null : Gst.Pan.Check(screen.Pan).Error,
    };

    /// <summary>Why a change of Registration Type or GSTIN is safe: see ADR-0001.</summary>
    private const string ChangeAppliesToNewBillsOnly =
        "Changing the Registration Type or GSTIN applies to new Bills only. Bills already issued "
        + "keep the details they were printed with.\n\nSave the change?";

    private readonly Func<BillowDbContext> _openDatabase;
    private readonly IConfirmationPrompt _confirmationPrompt;
    private readonly Dictionary<string, string> _errors = [];

    private string _legalName = "";
    private string _tradeName = "";
    private string _addressLine1 = "";
    private string _addressLine2 = "";
    private string _city = "";
    private GstState? _state;
    private string _pin = "";
    private RegistrationType? _registrationType;
    private string _gstin = "";
    private string _pan = "";

    public BusinessDetailsViewModel(
        Func<BillowDbContext> openDatabase,
        IConfirmationPrompt confirmationPrompt,
        ILogoFilePicker logoFilePicker)
    {
        _openDatabase = openDatabase;
        _confirmationPrompt = confirmationPrompt;
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

    public IReadOnlyList<RegistrationType> RegistrationTypes { get; } =
    [
        BusinessDetails.RegistrationType.Regular,
        BusinessDetails.RegistrationType.Composition,
        BusinessDetails.RegistrationType.Unregistered,
    ];

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

    /// <summary>Filled in from the GSTIN, and can't be changed, while the GSTIN is valid.</summary>
    public GstState? State
    {
        get => _state;
        set
        {
            if (!AreStateAndPanLocked)
            {
                SetAndCheck(ref _state, value);
            }
        }
    }

    public string Pin
    {
        get => _pin;
        set => SetAndCheck(ref _pin, value);
    }

    /// <summary>Null until one is chosen: a new Business has none.</summary>
    public RegistrationType? RegistrationType
    {
        get => _registrationType;
        set
        {
            SetAndCheck(ref _registrationType, value);
            OnPropertyChanged(nameof(IsGstinApplicable));

            // An Unregistered Business has no GSTIN; the PAN keeps its value and unlocks.
            if (!IsGstinApplicable)
            {
                SetAndCheck(ref _gstin, "", nameof(Gstin));
            }

            Check(nameof(Gstin));
            FillInFromGstin();
        }
    }

    /// <summary>Whether the Business has a GSTIN: true for Regular and Composition.</summary>
    public bool IsGstinApplicable =>
        RegistrationType is BusinessDetails.RegistrationType.Regular or BusinessDetails.RegistrationType.Composition;

    public string Gstin
    {
        get => _gstin;
        set
        {
            SetAndCheck(ref _gstin, value);
            FillInFromGstin();
        }
    }

    /// <summary>
    /// Whether State and PAN come from the GSTIN, so can't be edited: true while a Regular or
    /// Composition Business has a valid GSTIN.
    /// </summary>
    public bool AreStateAndPanLocked => ValidGstin is not null;

    /// <summary>Filled in from the GSTIN, and can't be changed, while the GSTIN is valid.</summary>
    public string Pan
    {
        get => _pan;
        set
        {
            if (!AreStateAndPanLocked)
            {
                SetAndCheck(ref _pan, value);
            }
        }
    }

    /// <summary>The GSTIN, if it applies and is valid; otherwise null.</summary>
    private Gst.Gstin? ValidGstin => IsGstinApplicable ? Gst.Gstin.Check(Gstin).Value : null;

    /// <summary>Whether the one Business has been saved yet. Until it has, Billow can't bill.</summary>
    public static bool HasSavedBusiness(Func<BillowDbContext> openDatabase)
    {
        using var db = openDatabase();
        return db.Businesses.Any();
    }

    /// <summary>
    /// Stores the details as the one Business. Changing the Registration Type or GSTIN of a saved
    /// Business needs confirming first. False if nothing was saved.
    /// </summary>
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
        var gstin = ValidGstin?.Value;
        if (business is not null
            && (business.RegistrationType != RegistrationType || business.Gstin != gstin)
            && !_confirmationPrompt.Confirm(ChangeAppliesToNewBillsOnly))
        {
            return false;
        }

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
        business.RegistrationType = RegistrationType!.Value;
        business.Gstin = gstin;
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
        _registrationType = business?.RegistrationType;
        _gstin = business?.Gstin ?? "";
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

    /// <summary>Fills in State and PAN from a valid GSTIN, locking them; unlocks them otherwise.</summary>
    private void FillInFromGstin()
    {
        if (ValidGstin is { } gstin)
        {
            SetAndCheck(ref _state, GstState.Find(gstin.StateCode), nameof(State));
            SetAndCheck(ref _pan, gstin.Pan.Value, nameof(Pan));
        }

        OnPropertyChanged(nameof(AreStateAndPanLocked));
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
