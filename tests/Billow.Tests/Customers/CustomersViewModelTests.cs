using Billow.Customers;
using Billow.Gst;

namespace Billow.Tests.Customers;

public sealed class CustomersViewModelTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeCustomerFormOpener _formOpener = new();

    public void Dispose() => _database.Dispose();

    private CustomersViewModel OpenList() => new(_database.Open, _formOpener);

    /// <summary>Adds a Customer through the list's Add, as the user would.</summary>
    private void AddCustomer(
        CustomersViewModel list,
        string name,
        string city = "",
        string? stateCode = null,
        string phone = "",
        string tradeName = "")
    {
        _formOpener.UseForm = form =>
        {
            form.Name = name;
            form.TradeName = tradeName;
            form.City = city;
            form.State = stateCode is null ? null : GstState.Find(stateCode);
            form.Phone = phone;
            form.SaveCommand.Execute(null);
        };
        list.AddCommand.Execute(null);
    }

    /// <summary>Adds a B2B Customer in Karnataka through the list's Add.</summary>
    private void AddB2BCustomer(CustomersViewModel list, string name, string gstin)
    {
        _formOpener.UseForm = form =>
        {
            form.Name = name;
            form.Gstin = gstin;
            form.AddressLine1 = "7 MG Road";
            form.City = "Bengaluru";
            form.Pin = "560001";
            form.SaveCommand.Execute(null);
        };
        list.AddCommand.Execute(null);
    }

    private static string[] NamesIn(CustomersViewModel list) => [.. list.Customers.Select(c => c.Name)];

    [Fact]
    public void TheListIsEmptyBeforeAnyCustomerIsAdded()
    {
        Assert.Empty(OpenList().Customers);
    }

    [Fact]
    public void ACustomerAddedWithOnlyANameAppearsInTheListAndIsSelected()
    {
        var list = OpenList();

        AddCustomer(list, "Ramesh Patil");

        var row = Assert.Single(list.Customers);
        Assert.Equal("Ramesh Patil", row.Name);
        Assert.Equal("", row.Gstin);
        Assert.Equal("", row.City);
        Assert.Equal("", row.State);
        Assert.Equal("", row.Phone);
        Assert.Same(row, list.SelectedCustomer);
        Assert.Equal(["Ramesh Patil"], NamesIn(OpenList()));
    }

    [Fact]
    public void TheListShowsEachCustomersNameGstinCityStateAndPhone()
    {
        var list = OpenList();

        AddCustomer(list, "Ramesh Patil", city: "Bengaluru", stateCode: "29", phone: "98765 43210");

        var row = Assert.Single(OpenList().Customers);
        Assert.Equal("Ramesh Patil", row.Name);
        Assert.Equal("", row.Gstin);
        Assert.Equal("Bengaluru", row.City);
        Assert.Equal("Karnataka", row.State);
        Assert.Equal("98765 43210", row.Phone);
    }

    [Fact]
    public void TheListShowsAB2BCustomersGstin()
    {
        var list = OpenList();
        _formOpener.UseForm = form =>
        {
            form.Name = "Umesh Traders";
            form.Gstin = "29AAACB2894G1ZJ";
            form.AddressLine1 = "7 MG Road";
            form.City = "Bengaluru";
            form.Pin = "560001";
            form.SaveCommand.Execute(null);
        };

        list.AddCommand.Execute(null);

        var row = Assert.Single(OpenList().Customers);
        Assert.Equal("29AAACB2894G1ZJ", row.Gstin);
        Assert.Equal("Karnataka", row.State);
    }

    [Fact]
    public void TheListIsSortedByNameIgnoringCase()
    {
        var list = OpenList();

        AddCustomer(list, "suresh Kumar");
        AddCustomer(list, "Anita Desai");
        AddCustomer(list, "Ramesh Patil");

        Assert.Equal(["Anita Desai", "Ramesh Patil", "suresh Kumar"], NamesIn(list));
        Assert.Equal(["Anita Desai", "Ramesh Patil", "suresh Kumar"], NamesIn(OpenList()));
    }

    [Fact]
    public void InactiveCustomersAreLeftOutOfTheList()
    {
        using (var db = _database.Open())
        {
            db.Customers.Add(new Customer { Name = "Ramesh Patil" });
            db.Customers.Add(new Customer { Name = "Anita Desai", IsActive = false });
            db.SaveChanges();
        }

        Assert.Equal(["Ramesh Patil"], NamesIn(OpenList()));
    }

    [Fact]
    public void AddingACustomerThenCancellingAddsNothing()
    {
        var list = OpenList();
        _formOpener.UseForm = form =>
        {
            form.Name = "Ramesh Patil";
            form.CancelCommand.Execute(null);
        };

        list.AddCommand.Execute(null);

        Assert.Empty(list.Customers);
        Assert.Empty(OpenList().Customers);
    }

    [Fact]
    public void EditOpensTheSelectedCustomerAndSavingUpdatesTheList()
    {
        var list = OpenList();
        AddCustomer(list, "Anita Desai", city: "Pune");
        AddCustomer(list, "Ramesh Patil", city: "Pune");
        list.SelectedCustomer = list.Customers[0];
        _formOpener.UseForm = form =>
        {
            Assert.Equal("Edit Customer", form.Title);
            Assert.Equal("Anita Desai", form.Name);
            form.Name = "Zoya Desai";
            form.City = "Nashik";
            form.SaveCommand.Execute(null);
        };

        list.EditCommand.Execute(null);

        Assert.Equal(["Ramesh Patil", "Zoya Desai"], NamesIn(list));
        Assert.Equal("Nashik", list.Customers[1].City);
        Assert.Same(list.Customers[1], list.SelectedCustomer);
        Assert.Equal(["Ramesh Patil", "Zoya Desai"], NamesIn(OpenList()));
    }

    [Fact]
    public void EditingACustomerThenCancellingLeavesItUnchanged()
    {
        var list = OpenList();
        AddCustomer(list, "Ramesh Patil", city: "Pune");
        _formOpener.UseForm = form =>
        {
            form.Name = "Ramesh S. Patil";
            form.City = "Nashik";
            form.CancelCommand.Execute(null);
        };

        list.EditCommand.Execute(null);

        var row = Assert.Single(list.Customers);
        Assert.Equal("Ramesh Patil", row.Name);
        Assert.Equal("Pune", row.City);
        var reopened = Assert.Single(OpenList().Customers);
        Assert.Equal("Ramesh Patil", reopened.Name);
        Assert.Equal("Pune", reopened.City);
    }

    [Fact]
    public void EditDoesNothingWhenNoCustomerIsSelected()
    {
        var list = OpenList();
        AddCustomer(list, "Ramesh Patil");
        list.SelectedCustomer = null;
        _formOpener.Opened.Clear();

        list.EditCommand.Execute(null);

        Assert.Empty(_formOpener.Opened);
    }

    [Fact]
    public void SearchingByPartOfANameFindsTheCustomerIgnoringCase()
    {
        var list = OpenList();
        AddCustomer(list, "Anita Desai");
        AddCustomer(list, "Ramesh Patil");

        list.SearchText = "PAT";

        Assert.Equal(["Ramesh Patil"], NamesIn(list));
    }

    [Fact]
    public void SearchingByPartOfATradeNameFindsTheCustomer()
    {
        var list = OpenList();
        AddCustomer(list, "Anita Desai", tradeName: "Desai Sweets");
        AddCustomer(list, "Ramesh Patil", tradeName: "Patil Hardware");

        list.SearchText = "hardware";

        Assert.Equal(["Ramesh Patil"], NamesIn(list));
    }

    [Fact]
    public void SearchingByPartOfAPhoneNumberFindsTheCustomer()
    {
        var list = OpenList();
        AddCustomer(list, "Anita Desai", phone: "91234 56789");
        AddCustomer(list, "Ramesh Patil", phone: "98765 43210");

        list.SearchText = "43210";

        Assert.Equal(["Ramesh Patil"], NamesIn(list));
    }

    [Fact]
    public void SearchingByPartOfAGstinInAnyCaseFindsTheCustomer()
    {
        var list = OpenList();
        AddCustomer(list, "Anita Desai");
        AddB2BCustomer(list, "Umesh Traders", "29AAACB2894G1ZJ");

        list.SearchText = "aaacb2894g";

        Assert.Equal(["Umesh Traders"], NamesIn(list));
    }

    [Fact]
    public void ClearingTheSearchBringsBackTheFullListSortedByName()
    {
        var list = OpenList();
        AddCustomer(list, "suresh Kumar");
        AddCustomer(list, "Anita Desai");
        AddCustomer(list, "Ramesh Patil");
        list.SearchText = "ramesh";

        list.SearchText = "";

        Assert.Equal(["Anita Desai", "Ramesh Patil", "suresh Kumar"], NamesIn(list));
    }

    [Fact]
    public void ASearchMatchingNoCustomerEmptiesTheList()
    {
        var list = OpenList();
        AddCustomer(list, "Ramesh Patil");

        list.SearchText = "zzz";

        Assert.Empty(list.Customers);
        Assert.Null(list.SelectedCustomer);
    }

    [Fact]
    public void TheSearchStillAppliesAfterACustomerIsAdded()
    {
        var list = OpenList();
        AddCustomer(list, "Ramesh Patil");
        list.SearchText = "patil";

        AddCustomer(list, "Anita Desai");

        Assert.Equal(["Ramesh Patil"], NamesIn(list));
    }

    [Fact]
    public void SpacesAroundTheSearchTextAreIgnored()
    {
        var list = OpenList();
        AddCustomer(list, "Anita Desai");
        AddCustomer(list, "Ramesh Patil");

        list.SearchText = "  patil ";

        Assert.Equal(["Ramesh Patil"], NamesIn(list));
    }

    [Fact]
    public void TheSelectedCustomerStaysSelectedWhileItMatchesTheSearch()
    {
        var list = OpenList();
        AddCustomer(list, "Anita Desai");
        AddCustomer(list, "Ramesh Patil");
        list.SelectedCustomer = list.Customers[1];

        list.SearchText = "pat";

        Assert.Equal("Ramesh Patil", list.SelectedCustomer?.Name);
    }
}
