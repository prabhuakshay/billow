using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Billow.Data;
using Billow.Gst;

namespace Billow.Customers;

/// <summary>
/// The Customers list: the active Customers sorted by name, with a search, Add and Edit. Add and
/// Edit open the Customer form, and the list is reloaded once it closes.
/// </summary>
public sealed class CustomersViewModel : INotifyPropertyChanged
{
    private readonly Func<BillowDbContext> _openDatabase;
    private readonly ICustomerFormOpener _formOpener;

    private List<CustomerListItem> _allCustomers = [];
    private CustomerListItem? _selectedCustomer;
    private string _searchText = "";

    public CustomersViewModel(Func<BillowDbContext> openDatabase, ICustomerFormOpener formOpener)
    {
        _openDatabase = openDatabase;
        _formOpener = formOpener;
        AddCommand = new RelayCommand(() => OpenForm(customerId: null));
        EditCommand = new RelayCommand(() =>
        {
            if (SelectedCustomer is { } customer)
            {
                OpenForm(customer.Id);
            }
        });
        Load(selectId: null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The active Customers matching the search, sorted by name.</summary>
    public ObservableCollection<CustomerListItem> Customers { get; } = [];

    public CustomerListItem? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (_selectedCustomer != value)
            {
                _selectedCustomer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCustomer)));
            }
        }
    }

    /// <summary>
    /// Narrows the list to Customers whose name, Trade Name, phone or GSTIN contains this text,
    /// ignoring case. Empty shows every active Customer.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                ShowMatches(selectId: SelectedCustomer?.Id);
            }
        }
    }

    /// <summary>Opens the form on a new Customer, and selects it once saved.</summary>
    public ICommand AddCommand { get; }

    /// <summary>Opens the form on the selected Customer. Does nothing if none is selected.</summary>
    public ICommand EditCommand { get; }

    private void OpenForm(int? customerId)
    {
        var form = new CustomerViewModel(_openDatabase, customerId);
        if (_formOpener.Open(form))
        {
            Load(selectId: form.Id);
        }
    }

    private void Load(int? selectId)
    {
        using var db = _openDatabase();
        _allCustomers = [.. db.Customers
            .Where(c => c.IsActive)
            .AsEnumerable()
            .OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(c => new CustomerListItem(
                c.Id,
                c.Name,
                c.TradeName ?? "",
                c.Gstin ?? "",
                c.City ?? "",
                c.StateCode is null ? "" : GstState.Find(c.StateCode)?.Name ?? "",
                c.Phone ?? ""))];

        ShowMatches(selectId);
    }

    /// <summary>Fills the list with the Customers matching the search, keeping the selection if it still matches.</summary>
    private void ShowMatches(int? selectId)
    {
        var search = SearchText.Trim();

        Customers.Clear();
        foreach (var customer in _allCustomers.Where(c => c.Matches(search)))
        {
            Customers.Add(customer);
        }

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == selectId);
    }
}
