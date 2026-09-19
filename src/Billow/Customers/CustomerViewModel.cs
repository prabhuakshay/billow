using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Billow.Data;
using Billow.Gst;

namespace Billow.Customers;

/// <summary>
/// The Customer form: every field of one new or existing Customer, its error, and Save and
/// Cancel. A field is checked each time it changes, and every field is checked on Save.
/// </summary>
public sealed class CustomerViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    /// <summary>
    /// The rule for each field that can have an error: it gives the reason the field's current
    /// value can't be saved, or null. Save checks every field listed here. A B2B Customer (one
    /// with a GSTIN) needs its Legal Name, address, State and PIN; a B2C Customer only a name.
    /// </summary>
    private static readonly Dictionary<string, Func<CustomerViewModel, string?>> _rules = new()
    {
        [nameof(Name)] = form => !IsBlank(form.Name) ? null
            : form.IsB2B ? "Enter the Legal Name."
            : "Enter the name.",
        [nameof(AddressLine1)] = form =>
            form.IsB2B && IsBlank(form.AddressLine1) ? "Enter the first line of the address." : null,
        [nameof(City)] = form => form.IsB2B && IsBlank(form.City) ? "Enter the city." : null,
        [nameof(State)] = form => form.IsB2B && form.State is null ? "Choose the State." : null,
        [nameof(Pin)] = form => IsBlank(form.Pin) && !form.IsB2B ? null : Billow.Pin.Check(form.Pin).Error,
        [nameof(Phone)] = form => IsBlank(form.Phone) ? null : Billow.Phone.Check(form.Phone).Error,
        [nameof(Email)] = form => IsBlank(form.Email) ? null : Billow.Email.Check(form.Email).Error,
        [nameof(Gstin)] = form => IsBlank(form.Gstin) ? null : form.GstinError(),
    };

    /// <summary>The fields whose rule depends on whether the Customer is B2B.</summary>
    private static readonly string[] _fieldsCheckedDifferentlyForB2B =
        [nameof(Name), nameof(AddressLine1), nameof(City), nameof(State), nameof(Pin)];

    private readonly Func<BillowDbContext> _openDatabase;
    private readonly Dictionary<string, string> _errors = [];

    private int? _id;
    private string _gstin = "";
    private string _name = "";
    private string _tradeName = "";
    private string _addressLine1 = "";
    private string _addressLine2 = "";
    private string _city = "";
    private GstState? _state;
    private string _pin = "";
    private string _phone = "";
    private string _email = "";

    /// <summary>Opens the Customer with <paramref name="customerId"/>, or a new Customer if it is null.</summary>
    public CustomerViewModel(Func<BillowDbContext> openDatabase, int? customerId)
    {
        _openDatabase = openDatabase;
        _id = customerId;
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

    /// <summary>Asks the window to close. The argument is true if the Customer was saved.</summary>
    public event EventHandler<bool>? CloseRequested;

    public bool HasErrors => _errors.Count > 0;

    /// <summary>The saved Customer's Id, or null until a new Customer is first saved.</summary>
    public int? Id => _id;

    public string Title => _id is null ? "New Customer" : "Edit Customer";

    public IReadOnlyList<GstState> States { get; } = GstState.All;

    /// <summary>Saves, then closes the form; while there are errors, only shows them.</summary>
    public ICommand SaveCommand { get; }

    /// <summary>Puts back the saved details, then closes the form.</summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Makes the Customer a B2B Customer. While it is valid, the State is filled in from it and
    /// locked. Clearing it makes the Customer a B2C Customer again.
    /// </summary>
    public string Gstin
    {
        get => _gstin;
        set
        {
            var wasB2B = IsB2B;
            SetAndCheck(ref _gstin, value);
            FillInFromGstin();

            if (IsB2B != wasB2B)
            {
                OnPropertyChanged(nameof(IsB2B));

                // Recheck only fields already showing an error, so starting to type a GSTIN
                // doesn't flag the fields a B2B Customer needs before the user reaches them.
                foreach (var propertyName in _fieldsCheckedDifferentlyForB2B.Where(_errors.ContainsKey).ToList())
                {
                    Check(propertyName);
                }
            }
        }
    }

    /// <summary>Whether a GSTIN has been entered, so the Customer is a B2B Customer.</summary>
    public bool IsB2B => !IsBlank(Gstin);

    /// <summary>Whether the State comes from a valid GSTIN, so can't be changed.</summary>
    public bool IsStateLocked => ValidGstin is not null;

    /// <summary>The Legal Name of a B2B Customer, or the plain name of a B2C Customer.</summary>
    public string Name
    {
        get => _name;
        set => SetAndCheck(ref _name, value);
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

    /// <summary>
    /// A new Customer's State starts as the Business's State (none if no Business is saved yet).
    /// Filled in from the GSTIN, and can't be changed, while the GSTIN is valid.
    /// </summary>
    public GstState? State
    {
        get => _state;
        set
        {
            if (!IsStateLocked)
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

    public string Phone
    {
        get => _phone;
        set => SetAndCheck(ref _phone, value);
    }

    public string Email
    {
        get => _email;
        set => SetAndCheck(ref _email, value);
    }

    /// <summary>Stores the Customer, adding it if it is new. False if nothing was saved.</summary>
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
        var customer = _id is { } id ? db.Customers.Single(c => c.Id == id) : null;
        if (customer is null)
        {
            customer = new Customer();
            db.Customers.Add(customer);
        }

        customer.Gstin = ValidGstin?.Value;
        customer.Name = Name.Trim();
        customer.TradeName = NullIfBlank(TradeName);
        customer.AddressLine1 = NullIfBlank(AddressLine1);
        customer.AddressLine2 = NullIfBlank(AddressLine2);
        customer.City = NullIfBlank(City);
        customer.StateCode = State?.Code;
        customer.Pin = IsBlank(Pin) ? null : Billow.Pin.Check(Pin).Value!.Value;
        customer.Phone = IsBlank(Phone) ? null : Billow.Phone.Check(Phone).Value!.Value;
        customer.Email = IsBlank(Email) ? null : Billow.Email.Check(Email).Value!.Value;

        db.SaveChanges();

        if (_id is null)
        {
            _id = customer.Id;
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Title));
        }

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

    /// <summary>Shows the saved Customer (a new, empty one if it isn't saved yet), with no errors.</summary>
    private void Load()
    {
        using var db = _openDatabase();
        var customer = _id is { } id ? db.Customers.Single(c => c.Id == id) : null;
        var stateCode = customer is null
            ? db.Businesses.Select(b => b.StateCode).SingleOrDefault()
            : customer.StateCode;

        _gstin = customer?.Gstin ?? "";
        _name = customer?.Name ?? "";
        _tradeName = customer?.TradeName ?? "";
        _addressLine1 = customer?.AddressLine1 ?? "";
        _addressLine2 = customer?.AddressLine2 ?? "";
        _city = customer?.City ?? "";
        _state = stateCode is null ? null : GstState.Find(stateCode);
        _pin = customer?.Pin ?? "";
        _phone = customer?.Phone ?? "";
        _email = customer?.Email ?? "";

        var fieldsWithErrors = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var field in fieldsWithErrors)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(field));
        }

        // An empty name means every property changed.
        OnPropertyChanged("");
    }

    /// <summary>The GSTIN, if it is valid; otherwise null.</summary>
    private Gst.Gstin? ValidGstin => Gst.Gstin.Check(Gstin).Value;

    /// <summary>Why the entered GSTIN can't be saved: it is invalid, or another Customer has it.</summary>
    private string? GstinError()
    {
        var check = Gst.Gstin.Check(Gstin);
        if (check.Value is not { } gstin)
        {
            return check.Error;
        }

        using var db = _openDatabase();
        var owner = db.Customers
            .Where(c => c.Gstin == gstin.Value && c.Id != _id)
            .Select(c => c.Name)
            .FirstOrDefault();
        return owner is null ? null : $"This GSTIN already belongs to {owner}.";
    }

    /// <summary>Fills in the State from a valid GSTIN, locking it; unlocks it otherwise.</summary>
    private void FillInFromGstin()
    {
        if (ValidGstin is { } gstin)
        {
            SetAndCheck(ref _state, GstState.Find(gstin.StateCode), nameof(State));
        }

        OnPropertyChanged(nameof(IsStateLocked));
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
