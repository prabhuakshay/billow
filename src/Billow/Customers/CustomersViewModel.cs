using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Billow.Data;
using Billow.Gst;

namespace Billow.Customers;

/// <summary>
/// The Customers list: the active Customers sorted by name, with a search, Add and Edit. Add and
/// Edit open the Customer form, and the list is reloaded once it closes. The selected Customer can
/// be deactivated, which keeps its record but hides it from the list, activated again, or deleted
/// once the user confirms.
/// </summary>
public sealed class CustomersViewModel : INotifyPropertyChanged
{
    private readonly Func<BillowDbContext> _openDatabase;
    private readonly ICustomerFormOpener _formOpener;
    private readonly IConfirmationPrompt _confirmationPrompt;

    private List<CustomerListItem> _allCustomers = [];
    private CustomerListItem? _selectedCustomer;
    private string _searchText = "";
    private bool _showInactive;

    public CustomersViewModel(
        Func<BillowDbContext> openDatabase,
        ICustomerFormOpener formOpener,
        IConfirmationPrompt confirmationPrompt)
    {
        _openDatabase = openDatabase;
        _formOpener = formOpener;
        _confirmationPrompt = confirmationPrompt;
        AddCommand = new RelayCommand(() => OpenForm(customerId: null));
        EditCommand = new RelayCommand(() =>
        {
            if (SelectedCustomer is { } customer)
            {
                OpenForm(customer.Id);
            }
        });
        DeactivateCommand = new RelayCommand(() => SetSelectedActive(false));
        ActivateCommand = new RelayCommand(() => SetSelectedActive(true));
        DeleteCommand = new RelayCommand(DeleteSelected);
        Load(selectId: null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The Customers matching the search, sorted by name: the active ones, and the inactive ones too
    /// while <see cref="ShowInactive"/> is on.
    /// </summary>
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

    /// <summary>Whether inactive Customers are listed alongside the active ones.</summary>
    public bool ShowInactive
    {
        get => _showInactive;
        set
        {
            if (_showInactive != value)
            {
                _showInactive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowInactive)));
                Load(selectId: SelectedCustomer?.Id);
            }
        }
    }

    /// <summary>Opens the form on a new Customer. Once it is saved, clears the search and selects it.</summary>
    public ICommand AddCommand { get; }

    /// <summary>Opens the form on the selected Customer. Does nothing if none is selected.</summary>
    public ICommand EditCommand { get; }

    /// <summary>
    /// Marks the selected Customer inactive, keeping its record. Does nothing if none is selected.
    /// </summary>
    public ICommand DeactivateCommand { get; }

    /// <summary>Marks the selected Customer active again. Does nothing if none is selected.</summary>
    public ICommand ActivateCommand { get; }

    /// <summary>
    /// Deletes the selected Customer for good once the user confirms. Does nothing if none is
    /// selected.
    /// </summary>
    public ICommand DeleteCommand { get; }

    private void OpenForm(int? customerId)
    {
        var form = new CustomerViewModel(_openDatabase, customerId);
        if (_formOpener.Open(form))
        {
            // A new Customer may not match the search, so clear the search to show and select it.
            if (customerId is null)
            {
                SearchText = "";
            }

            Load(selectId: form.Id);
        }
    }

    private void SetSelectedActive(bool isActive)
    {
        if (SelectedCustomer is not { } selected)
        {
            return;
        }

        using (var db = _openDatabase())
        {
            if (db.Customers.Find(selected.Id) is { } customer)
            {
                customer.IsActive = isActive;
                db.SaveChanges();
            }
        }

        Load(selectId: selected.Id);
    }

    private void DeleteSelected()
    {
        if (SelectedCustomer is not { } selected
            || !_confirmationPrompt.Confirm(
                $"Delete the Customer \"{selected.Name}\"? This can't be undone.\n\n"
                + "To keep their record but take them off the list, Deactivate them instead."))
        {
            return;
        }

        using (var db = _openDatabase())
        {
            if (db.Customers.Find(selected.Id) is { } customer)
            {
                db.Customers.Remove(customer);
                db.SaveChanges();
            }
        }

        Load(selectId: null);
    }

    private void Load(int? selectId)
    {
        using var db = _openDatabase();
        _allCustomers = [.. db.Customers
            .Where(c => ShowInactive || c.IsActive)
            .AsEnumerable()
            .OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(c => new CustomerListItem(
                c.Id,
                c.Name,
                c.TradeName ?? "",
                c.Gstin ?? "",
                c.City ?? "",
                c.StateCode is null ? "" : GstState.Find(c.StateCode)?.Name ?? "",
                c.Phone ?? "",
                c.IsActive))];

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
