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
        string phone = "")
    {
        _formOpener.UseForm = form =>
        {
            form.Name = name;
            form.City = city;
            form.State = stateCode is null ? null : GstState.Find(stateCode);
            form.Phone = phone;
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
}
