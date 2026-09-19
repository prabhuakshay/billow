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
    /// value can't be saved, or null. Save checks every field listed here.
    /// </summary>
    private static readonly Dictionary<string, Func<CustomerViewModel, string?>> _rules = new()
    {
        [nameof(Name)] = form => IsBlank(form.Name) ? "Enter the name." : null,
        [nameof(Pin)] = form => IsBlank(form.Pin) ? null : Billow.Pin.Check(form.Pin).Error,
        [nameof(Phone)] = form => IsBlank(form.Phone) ? null : Billow.Phone.Check(form.Phone).Error,
        [nameof(Email)] = form => IsBlank(form.Email) ? null : Billow.Email.Check(form.Email).Error,
    };

    private readonly Func<BillowDbContext> _openDatabase;
    private readonly Dictionary<string, string> _errors = [];

    private int? _id;
    private string _name = "";
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

    public string Name
    {
        get => _name;
        set => SetAndCheck(ref _name, value);
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

    /// <summary>A new Customer's State starts as the Business's State (none if no Business is saved yet).</summary>
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

        customer.Name = Name.Trim();
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

        _name = customer?.Name ?? "";
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
