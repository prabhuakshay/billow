using Billow.BusinessDetails;
using Billow.Customers;
using Billow.Gst;
using Billow.Tests.BusinessDetails;

namespace Billow.Tests.Customers;

public sealed class CustomerViewModelTests : IDisposable
{
    private readonly TestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    private CustomerViewModel OpenNewCustomer() => new(_database.Open, customerId: null);

    private CustomerViewModel OpenCustomer(int id) => new(_database.Open, id);

    /// <summary>Saves a new Customer with <paramref name="name"/>, and returns its Id.</summary>
    private int SaveCustomer(string name, string stateCode = "27")
    {
        var form = OpenNewCustomer();
        form.Name = name;
        form.State = GstState.Find(stateCode);
        Assert.True(form.Save());
        return form.Id!.Value;
    }

    private void SaveBusinessIn(string stateCode)
    {
        var screen = new BusinessDetailsViewModel(_database.Open, new FakeConfirmationPrompt(), new FakeLogoFilePicker())
        {
            LegalName = "Sharma General Stores",
            AddressLine1 = "12 Station Road",
            City = "Pune",
            State = GstState.Find(stateCode),
            Pin = "411001",
            RegistrationType = RegistrationType.Unregistered,
        };
        Assert.True(screen.Save());
    }

    private int CustomerCount()
    {
        using var db = _database.Open();
        return db.Customers.Count();
    }

    [Fact]
    public void ACustomerSavedWithOnlyANameReopensWithTheSameDetails()
    {
        var form = OpenNewCustomer();
        form.Name = "Ramesh Patil";

        Assert.True(form.Save());

        var reopened = OpenCustomer(form.Id!.Value);
        Assert.Equal("Ramesh Patil", reopened.Name);
        Assert.Equal("", reopened.AddressLine1);
        Assert.Equal("", reopened.AddressLine2);
        Assert.Equal("", reopened.City);
        Assert.Null(reopened.State);
        Assert.Equal("", reopened.Pin);
        Assert.Equal("", reopened.Phone);
        Assert.Equal("", reopened.Email);
    }

    [Fact]
    public void EveryDetailIsSavedTrimmedAndShownWhenReopened()
    {
        var form = OpenNewCustomer();
        form.Name = " Ramesh Patil ";
        form.AddressLine1 = " 4 MG Road ";
        form.AddressLine2 = " Opp. Post Office ";
        form.City = " Bengaluru ";
        form.State = GstState.Find("29");
        form.Pin = " 560001 ";
        form.Phone = " 98765 43210 ";
        form.Email = " ramesh@gmail.com ";

        Assert.True(form.Save());

        var reopened = OpenCustomer(form.Id!.Value);
        Assert.Equal("Ramesh Patil", reopened.Name);
        Assert.Equal("4 MG Road", reopened.AddressLine1);
        Assert.Equal("Opp. Post Office", reopened.AddressLine2);
        Assert.Equal("Bengaluru", reopened.City);
        Assert.Equal(GstState.Find("29"), reopened.State);
        Assert.Equal("560001", reopened.Pin);
        Assert.Equal("98765 43210", reopened.Phone);
        Assert.Equal("ramesh@gmail.com", reopened.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankNameIsAnErrorAndNothingIsSaved(string name)
    {
        var form = OpenNewCustomer();
        form.City = "Pune";
        form.Name = name;

        Assert.False(form.Save());

        Assert.Equal("Enter the name.", form.ErrorFor(nameof(form.Name)));
        Assert.True(form.HasErrors);
        Assert.Null(form.Id);
        Assert.Equal(0, CustomerCount());
    }

    [Fact]
    public void OnlyTheNameIsRequired()
    {
        var form = OpenNewCustomer();

        Assert.False(form.Save());

        Assert.Equal("Enter the name.", form.ErrorFor(nameof(form.Name)));
        Assert.Null(form.ErrorFor(nameof(form.AddressLine1)));
        Assert.Null(form.ErrorFor(nameof(form.AddressLine2)));
        Assert.Null(form.ErrorFor(nameof(form.City)));
        Assert.Null(form.ErrorFor(nameof(form.State)));
        Assert.Null(form.ErrorFor(nameof(form.Pin)));
        Assert.Null(form.ErrorFor(nameof(form.Phone)));
        Assert.Null(form.ErrorFor(nameof(form.Email)));
    }

    [Fact]
    public void TheNamesErrorClearsOnceItIsFilledIn()
    {
        var form = OpenNewCustomer();
        form.Save();

        form.Name = "Ramesh Patil";

        Assert.Null(form.ErrorFor(nameof(form.Name)));
        Assert.False(form.HasErrors);
    }

    [Theory]
    [InlineData(nameof(CustomerViewModel.Pin), "41100", "A PIN is exactly 6 digits.")]
    [InlineData(nameof(CustomerViewModel.Phone), "98765",
        "A phone number is 8 to 13 digits, with spaces, hyphens, brackets or a leading +, such as 98765 43210.")]
    [InlineData(nameof(CustomerViewModel.Email), "ramesh@gmail",
        "An email address is a name, then @, then a domain, such as sharma@gmail.com.")]
    public void AnInvalidOptionalFieldIsAnErrorAndNothingIsSaved(string field, string value, string error)
    {
        var form = OpenNewCustomer();
        form.Name = "Ramesh Patil";

        SetText(form, field, value);

        Assert.Equal(error, form.ErrorFor(field));
        Assert.False(form.Save());
        Assert.Equal(0, CustomerCount());
    }

    [Theory]
    [InlineData(nameof(CustomerViewModel.Pin))]
    [InlineData(nameof(CustomerViewModel.Phone))]
    [InlineData(nameof(CustomerViewModel.Email))]
    public void AnOptionalFieldLeftBlankIsFine(string field)
    {
        var form = OpenNewCustomer();
        form.Name = "Ramesh Patil";
        SetText(form, field, "x");

        SetText(form, field, "   ");

        Assert.Null(form.ErrorFor(field));
        Assert.True(form.Save());
    }

    [Fact]
    public void ANewCustomersStateDefaultsToTheBusinesssState()
    {
        SaveBusinessIn("27");

        var form = OpenNewCustomer();

        Assert.Equal(GstState.Find("27"), form.State);
    }

    [Fact]
    public void ANewCustomerHasNoStateWhenNoBusinessHasBeenSaved()
    {
        var form = OpenNewCustomer();

        Assert.Null(form.State);
    }

    [Fact]
    public void ANewCustomersStateCanBeChanged()
    {
        SaveBusinessIn("27");
        var form = OpenNewCustomer();
        form.Name = "Ramesh Patil";

        form.State = GstState.Find("29");

        Assert.True(form.Save());
        Assert.Equal(GstState.Find("29"), OpenCustomer(form.Id!.Value).State);
    }

    [Fact]
    public void AnExistingCustomerKeepsItsOwnStateRatherThanTheBusinesss()
    {
        var id = SaveCustomer("Ramesh Patil", stateCode: "29");
        SaveBusinessIn("27");

        Assert.Equal(GstState.Find("29"), OpenCustomer(id).State);
    }

    [Fact]
    public void EditingACustomerAndSavingUpdatesIt()
    {
        var id = SaveCustomer("Ramesh Patil");
        var form = OpenCustomer(id);

        form.Name = "Ramesh S. Patil";
        form.City = "Nashik";

        Assert.True(form.Save());
        var reopened = OpenCustomer(id);
        Assert.Equal("Ramesh S. Patil", reopened.Name);
        Assert.Equal("Nashik", reopened.City);
        Assert.Equal(1, CustomerCount());
    }

    [Fact]
    public void SavingANewCustomerTwiceKeepsOneCustomer()
    {
        var form = OpenNewCustomer();
        form.Name = "Ramesh Patil";
        Assert.True(form.Save());

        form.City = "Pune";
        Assert.True(form.Save());

        Assert.Equal(1, CustomerCount());
        Assert.Equal("Pune", OpenCustomer(form.Id!.Value).City);
    }

    [Fact]
    public void CancelDiscardsUnsavedChangesAndClosesTheFormUnsaved()
    {
        var id = SaveCustomer("Ramesh Patil");
        var form = OpenCustomer(id);
        bool? saved = null;
        form.CloseRequested += (_, wasSaved) => saved = wasSaved;
        form.Name = "";
        form.City = "Nashik";

        form.CancelCommand.Execute(null);

        Assert.False(saved);
        Assert.Equal("Ramesh Patil", form.Name);
        Assert.Equal("", form.City);
        Assert.Null(form.ErrorFor(nameof(form.Name)));
        Assert.Equal("Ramesh Patil", OpenCustomer(id).Name);
    }

    [Fact]
    public void SaveCommandClosesTheFormOnlyOnceSaved()
    {
        var form = OpenNewCustomer();
        bool? saved = null;
        form.CloseRequested += (_, wasSaved) => saved = wasSaved;

        form.SaveCommand.Execute(null);
        Assert.Null(saved);

        form.Name = "Ramesh Patil";
        form.SaveCommand.Execute(null);
        Assert.True(saved);
    }

    [Fact]
    public void TheFormIsTitledForANewOrAnExistingCustomer()
    {
        var id = SaveCustomer("Ramesh Patil");

        Assert.Equal("New Customer", OpenNewCustomer().Title);
        Assert.Equal("Edit Customer", OpenCustomer(id).Title);
    }

    private static void SetText(CustomerViewModel form, string field, string value)
    {
        switch (field)
        {
            case nameof(form.Pin):
                form.Pin = value;
                break;
            case nameof(form.Phone):
                form.Phone = value;
                break;
            case nameof(form.Email):
                form.Email = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(field), field, null);
        }
    }
}
