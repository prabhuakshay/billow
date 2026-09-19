using Billow.BusinessDetails;
using Billow.Gst;

namespace Billow.Tests.BusinessDetails;

public sealed class BusinessDetailsViewModelTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeConfirmationPrompt _confirmationPrompt = new();

    public void Dispose() => _database.Dispose();

    private BusinessDetailsViewModel OpenScreen() =>
        new(_database.Open, _confirmationPrompt, new FakeLogoFilePicker());

    private BusinessDetailsViewModel OpenScreenWithValidDetails()
    {
        var screen = OpenScreen();
        screen.LegalName = "Sharma General Stores";
        screen.AddressLine1 = "12 Station Road";
        screen.City = "Pune";
        screen.State = GstState.Find("27");
        screen.Pin = "411001";
        screen.RegistrationType = RegistrationType.Unregistered;
        return screen;
    }

    /// <summary>Saves a Business with a GSTIN, then opens the screen on it.</summary>
    private BusinessDetailsViewModel OpenScreenOnSavedBusinessWithGstin(
        RegistrationType registrationType = RegistrationType.Regular)
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = registrationType;
        screen.Gstin = "27AAPFU0939F1ZV";
        Assert.True(screen.Save());
        return OpenScreen();
    }

    [Fact]
    public void SavedDetailsAreShownWhenTheScreenIsReopened()
    {
        var screen = OpenScreen();
        screen.LegalName = "Sharma General Stores Pvt Ltd";
        screen.TradeName = "Sharma Kirana";
        screen.AddressLine1 = "12 Station Road";
        screen.AddressLine2 = "Near Bus Stand";
        screen.City = "Pune";
        screen.State = GstState.Find("27");
        screen.Pin = "411001";
        screen.RegistrationType = RegistrationType.Unregistered;
        screen.Pan = "ABCPK1234L";

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal("Sharma General Stores Pvt Ltd", reopened.LegalName);
        Assert.Equal("Sharma Kirana", reopened.TradeName);
        Assert.Equal("12 Station Road", reopened.AddressLine1);
        Assert.Equal("Near Bus Stand", reopened.AddressLine2);
        Assert.Equal("Pune", reopened.City);
        Assert.Equal(GstState.Find("27"), reopened.State);
        Assert.Equal("411001", reopened.Pin);
        Assert.Equal(RegistrationType.Unregistered, reopened.RegistrationType);
        Assert.Equal("ABCPK1234L", reopened.Pan);
    }

    [Fact]
    public void AnEmptyFormShowsAnErrorForEachRequiredFieldAndIsNotSaved()
    {
        var screen = OpenScreen();

        Assert.False(screen.Save());

        Assert.Equal("Enter the Legal Name.", screen.ErrorFor(nameof(screen.LegalName)));
        Assert.Equal("Enter the first line of the address.", screen.ErrorFor(nameof(screen.AddressLine1)));
        Assert.Equal("Enter the city.", screen.ErrorFor(nameof(screen.City)));
        Assert.Equal("Choose the State.", screen.ErrorFor(nameof(screen.State)));
        Assert.Equal("Enter the 6-digit PIN.", screen.ErrorFor(nameof(screen.Pin)));
        Assert.Equal("Choose the Registration Type.", screen.ErrorFor(nameof(screen.RegistrationType)));
        Assert.Null(screen.ErrorFor(nameof(screen.TradeName)));
        Assert.Null(screen.ErrorFor(nameof(screen.AddressLine2)));
        Assert.Null(screen.ErrorFor(nameof(screen.Pan)));
        Assert.True(screen.HasErrors);

        Assert.Equal("", OpenScreen().LegalName);
    }

    [Theory]
    [InlineData(nameof(BusinessDetailsViewModel.LegalName), "Enter the Legal Name.")]
    [InlineData(nameof(BusinessDetailsViewModel.AddressLine1), "Enter the first line of the address.")]
    [InlineData(nameof(BusinessDetailsViewModel.City), "Enter the city.")]
    [InlineData(nameof(BusinessDetailsViewModel.Pin), "Enter the 6-digit PIN.")]
    public void ABlankRequiredFieldIsAnErrorAndBlocksSave(string field, string error)
    {
        var screen = OpenScreenWithValidDetails();

        // Spaces alone don't count as an entry.
        SetText(screen, field, "   ");

        Assert.Equal(error, screen.ErrorFor(field));
        Assert.False(screen.Save());
        Assert.Equal("", OpenScreen().LegalName);
    }

    [Fact]
    public void AFieldsErrorClearsOnceItIsFilledIn()
    {
        var screen = OpenScreen();
        screen.Save();

        screen.LegalName = "Sharma General Stores";
        screen.State = GstState.Find("27");

        Assert.Null(screen.ErrorFor(nameof(screen.LegalName)));
        Assert.Null(screen.ErrorFor(nameof(screen.State)));
        Assert.Equal("Enter the city.", screen.ErrorFor(nameof(screen.City)));
    }

    [Fact]
    public void AFormWithEveryRequiredFieldFilledInHasNoErrorsAndSaves()
    {
        var screen = OpenScreenWithValidDetails();

        Assert.True(screen.Save());
        Assert.False(screen.HasErrors);
        Assert.Equal("Sharma General Stores", OpenScreen().LegalName);
    }

    [Theory]
    [InlineData("41100")]
    [InlineData("4110011")]
    [InlineData("41100A")]
    [InlineData("411 001")]
    [InlineData("४११००१")] // Devanagari digits
    public void APinThatIsNotExactly6DigitsIsAnErrorAndBlocksSave(string pin)
    {
        var screen = OpenScreenWithValidDetails();

        screen.Pin = pin;

        Assert.Equal("A PIN is exactly 6 digits.", screen.ErrorFor(nameof(screen.Pin)));
        Assert.False(screen.Save());
    }

    [Fact]
    public void APinWithSpacesAroundItIsSavedWithoutThem()
    {
        var screen = OpenScreenWithValidDetails();
        screen.Pin = " 411001 ";

        Assert.True(screen.Save());
        Assert.Equal("411001", OpenScreen().Pin);
    }

    [Theory]
    [InlineData("ABCPK1234", "A PAN has 10 characters; this has 9.")]
    [InlineData("ABCPK12345", "A PAN is 5 letters, then 4 digits, then 1 letter.")]
    [InlineData("ABCDK1234L", "The 4th letter of a PAN must be one of P, C, H, F, A, T, B, L, J or G.")]
    public void AnEnteredPanIsCheckedAndABadOneBlocksSave(string pan, string error)
    {
        var screen = OpenScreenWithValidDetails();

        screen.Pan = pan;

        Assert.Equal(error, screen.ErrorFor(nameof(screen.Pan)));
        Assert.False(screen.Save());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void PanIsOptional(string pan)
    {
        var screen = OpenScreenWithValidDetails();

        screen.Pan = pan;

        Assert.Null(screen.ErrorFor(nameof(screen.Pan)));
        Assert.True(screen.Save());
        Assert.Equal("", OpenScreen().Pan);
    }

    [Fact]
    public void AnEnteredPanIsSavedUppercasedWithoutSurroundingSpaces()
    {
        var screen = OpenScreenWithValidDetails();
        screen.Pan = " abcpk1234l ";

        Assert.True(screen.Save());
        Assert.Equal("ABCPK1234L", OpenScreen().Pan);
    }

    [Theory]
    [InlineData(RegistrationType.Regular)]
    [InlineData(RegistrationType.Composition)]
    public void ARegularOrCompositionBusinessNeedsAGstin(RegistrationType registrationType)
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = registrationType;

        Assert.True(screen.IsGstinApplicable);
        Assert.False(screen.Save());
        Assert.Equal("Enter the GSTIN.", screen.ErrorFor(nameof(screen.Gstin)));
        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Theory]
    [InlineData("27AAPFU0939F1Z", "A GSTIN has 15 characters; this has 14.")]
    [InlineData("27AAPFU0939F1ZX", "The last character of this GSTIN does not match the rest; check it for a typing mistake.")]
    [InlineData("27AAPDU0939F1ZV", "Characters 3–12 of a GSTIN are its PAN. The 4th letter of a PAN must be one of P, C, H, F, A, T, B, L, J or G.")]
    public void AnInvalidGstinShowsWhyAndBlocksSave(string gstin, string error)
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = RegistrationType.Regular;

        screen.Gstin = gstin;

        Assert.Equal(error, screen.ErrorFor(nameof(screen.Gstin)));
        Assert.False(screen.Save());
        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Theory]
    [InlineData(RegistrationType.Regular)]
    [InlineData(RegistrationType.Composition)]
    public void ARegularOrCompositionBusinessIsSavedWithItsGstinNormalised(RegistrationType registrationType)
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = registrationType;
        screen.Gstin = " 27aapfu0939f1zv ";

        Assert.Null(screen.ErrorFor(nameof(screen.Gstin)));
        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal(registrationType, reopened.RegistrationType);
        Assert.Equal("27AAPFU0939F1ZV", reopened.Gstin);
    }

    [Fact]
    public void AValidGstinFillsInAndLocksStateAndPan()
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = RegistrationType.Regular;
        Assert.False(screen.AreStateAndPanLocked);

        screen.Gstin = "29aaacb2894g1zj";

        Assert.True(screen.AreStateAndPanLocked);
        Assert.Equal(GstState.Find("29"), screen.State);
        Assert.Equal("AAACB2894G", screen.Pan);
        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.True(reopened.AreStateAndPanLocked);
        Assert.Equal(GstState.Find("29"), reopened.State);
        Assert.Equal("AAACB2894G", reopened.Pan);
    }

    [Fact]
    public void LockedStateAndPanCannotBeChanged()
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = RegistrationType.Regular;
        screen.Gstin = "29AAACB2894G1ZJ";

        screen.State = GstState.Find("07");
        screen.Pan = "ABCPK1234L";

        Assert.Equal(GstState.Find("29"), screen.State);
        Assert.Equal("AAACB2894G", screen.Pan);
    }

    [Fact]
    public void StateAndPanUnlockWhenTheGstinStopsBeingValid()
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = RegistrationType.Regular;
        screen.Gstin = "29AAACB2894G1ZJ";

        screen.Gstin = "29AAACB2894G1Z";

        Assert.False(screen.AreStateAndPanLocked);
        Assert.Equal(GstState.Find("29"), screen.State);
        Assert.Equal("AAACB2894G", screen.Pan);

        screen.State = GstState.Find("07");
        Assert.Equal(GstState.Find("07"), screen.State);
    }

    [Fact]
    public void SwitchingToUnregisteredClearsTheGstinKeepsThePanAndUnlocksIt()
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = RegistrationType.Composition;
        screen.Gstin = "29AAACB2894G1ZJ";

        screen.RegistrationType = RegistrationType.Unregistered;

        Assert.Equal("", screen.Gstin);
        Assert.False(screen.IsGstinApplicable);
        Assert.False(screen.AreStateAndPanLocked);
        Assert.Equal("AAACB2894G", screen.Pan);
        Assert.Equal(GstState.Find("29"), screen.State);

        screen.Pan = "ABCPK1234L";
        Assert.Equal("ABCPK1234L", screen.Pan);
    }

    [Fact]
    public void SwitchingToUnregisteredClearsAGstinError()
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = RegistrationType.Regular;
        screen.Gstin = "27AAPFU0939F1ZX";

        screen.RegistrationType = RegistrationType.Unregistered;

        Assert.Null(screen.ErrorFor(nameof(screen.Gstin)));
        Assert.True(screen.Save());
    }

    [Fact]
    public void SavingABusinessForTheFirstTimeDoesNotAskForConfirmation()
    {
        var screen = OpenScreenWithValidDetails();
        screen.RegistrationType = RegistrationType.Regular;
        screen.Gstin = "27AAPFU0939F1ZV";

        Assert.True(screen.Save());
        Assert.Empty(_confirmationPrompt.Questions);
    }

    [Fact]
    public void ChangingTheRegistrationTypeOfASavedBusinessAsksForConfirmation()
    {
        OpenScreenWithValidDetails().Save();
        var screen = OpenScreen();
        screen.RegistrationType = RegistrationType.Regular;
        screen.Gstin = "27AAPFU0939F1ZV";

        Assert.True(screen.Save());

        Assert.Equal([ChangeAppliesToNewBillsOnly], _confirmationPrompt.Questions);
        Assert.Equal(RegistrationType.Regular, OpenScreen().RegistrationType);
    }

    [Fact]
    public void SwitchingBetweenRegularAndCompositionAsksForConfirmation()
    {
        var screen = OpenScreenOnSavedBusinessWithGstin(RegistrationType.Composition);

        screen.RegistrationType = RegistrationType.Regular;

        Assert.True(screen.Save());
        Assert.Equal([ChangeAppliesToNewBillsOnly], _confirmationPrompt.Questions);
    }

    [Fact]
    public void ChangingTheGstinOfASavedBusinessAsksForConfirmation()
    {
        var screen = OpenScreenOnSavedBusinessWithGstin();

        screen.Gstin = "29AAACB2894G1ZJ";

        Assert.True(screen.Save());
        Assert.Equal([ChangeAppliesToNewBillsOnly], _confirmationPrompt.Questions);
        Assert.Equal("29AAACB2894G1ZJ", OpenScreen().Gstin);
    }

    [Fact]
    public void DecliningTheConfirmationSavesNothingAndKeepsTheScreenOpen()
    {
        OpenScreenWithValidDetails().Save();
        var screen = OpenScreen();
        var closed = false;
        screen.CloseRequested += (_, _) => closed = true;
        screen.City = "Mumbai";
        screen.RegistrationType = RegistrationType.Regular;
        screen.Gstin = "27AAPFU0939F1ZV";
        _confirmationPrompt.Answer = false;

        screen.SaveCommand.Execute(null);

        Assert.False(closed);
        Assert.Single(_confirmationPrompt.Questions);
        var reopened = OpenScreen();
        Assert.Equal(RegistrationType.Unregistered, reopened.RegistrationType);
        Assert.Equal("", reopened.Gstin);
        Assert.Equal("Pune", reopened.City);
    }

    [Fact]
    public void OtherChangesToASavedBusinessDoNotAskForConfirmation()
    {
        var screen = OpenScreenOnSavedBusinessWithGstin();

        screen.City = "Mumbai";
        screen.Gstin = " 27aapfu0939f1zv "; // the same GSTIN, typed differently

        Assert.True(screen.Save());
        Assert.Empty(_confirmationPrompt.Questions);
        Assert.Equal("Mumbai", OpenScreen().City);
    }

    [Fact]
    public void CancelPutsBackTheSavedRegistrationTypeAndGstin()
    {
        var screen = OpenScreenOnSavedBusinessWithGstin();
        screen.RegistrationType = RegistrationType.Unregistered;

        screen.CancelCommand.Execute(null);

        Assert.Equal(RegistrationType.Regular, screen.RegistrationType);
        Assert.Equal("27AAPFU0939F1ZV", screen.Gstin);
        Assert.True(screen.AreStateAndPanLocked);
        Assert.Equal("AAPFU0939F", screen.Pan);
    }

    [Fact]
    public void GstinDoesNotApplyToAnUnregisteredBusiness()
    {
        var screen = OpenScreenWithValidDetails();

        Assert.False(screen.IsGstinApplicable);
        Assert.Null(screen.ErrorFor(nameof(screen.Gstin)));
        Assert.True(screen.Save());
        Assert.Equal("", OpenScreen().Gstin);
    }

    [Fact]
    public void DevanagariTextIsSavedAndShownAsTyped()
    {
        var screen = OpenScreenWithValidDetails();
        screen.LegalName = "शर्मा जनरल स्टोर्स";
        screen.TradeName = "शर्मा किराणा";
        screen.AddressLine1 = "१२, स्टेशन रोड";
        screen.AddressLine2 = "बस स्टँडजवळ";
        screen.City = "पुणे";

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal("शर्मा जनरल स्टोर्स", reopened.LegalName);
        Assert.Equal("शर्मा किराणा", reopened.TradeName);
        Assert.Equal("१२, स्टेशन रोड", reopened.AddressLine1);
        Assert.Equal("बस स्टँडजवळ", reopened.AddressLine2);
        Assert.Equal("पुणे", reopened.City);
    }

    [Fact]
    public void SavingAgainUpdatesTheOneBusiness()
    {
        OpenScreenWithValidDetails().Save();

        var screen = OpenScreen();
        screen.City = "Mumbai";
        screen.TradeName = "";
        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal("Mumbai", reopened.City);
        Assert.Equal("", reopened.TradeName);
        Assert.Equal("Sharma General Stores", reopened.LegalName);
    }

    [Fact]
    public void TheStateListIsEveryCurrentGstStateCode()
    {
        var states = OpenScreen().States;

        Assert.Equal(37, states.Count);
        Assert.Equal("01 – Jammu and Kashmir", states[0].ToString());
        Assert.Contains(states, s => s.ToString() == "27 – Maharashtra");
        Assert.Equal("97 – Other Territory", states[^1].ToString());
    }

    [Fact]
    public void EveryRegistrationTypeIsOfferedAndNoneIsChosenAtFirst()
    {
        var screen = OpenScreen();

        Assert.Equal(
            [RegistrationType.Regular, RegistrationType.Composition, RegistrationType.Unregistered],
            screen.RegistrationTypes);
        Assert.Null(screen.RegistrationType);
    }

    [Fact]
    public void SaveClosesTheScreenOnceSaved()
    {
        var screen = OpenScreenWithValidDetails();
        bool? closedAfterSaving = null;
        screen.CloseRequested += (_, saved) => closedAfterSaving = saved;

        screen.SaveCommand.Execute(null);

        Assert.True(closedAfterSaving);
        Assert.Equal("Sharma General Stores", OpenScreen().LegalName);
    }

    [Fact]
    public void SaveKeepsTheScreenOpenWhileThereAreErrors()
    {
        var screen = OpenScreen();
        var closed = false;
        screen.CloseRequested += (_, _) => closed = true;

        screen.SaveCommand.Execute(null);

        Assert.False(closed);
        Assert.Equal("Enter the Legal Name.", screen.ErrorFor(nameof(screen.LegalName)));
    }

    [Fact]
    public void CancelDiscardsUnsavedChanges()
    {
        OpenScreenWithValidDetails().Save();
        var screen = OpenScreen();
        bool? closedAfterSaving = null;
        screen.CloseRequested += (_, saved) => closedAfterSaving = saved;
        screen.LegalName = "Changed Name";
        screen.Pin = "12";

        screen.CancelCommand.Execute(null);

        Assert.False(closedAfterSaving);
        Assert.Equal("Sharma General Stores", screen.LegalName);
        Assert.Equal("411001", screen.Pin);
        Assert.False(screen.HasErrors);
        Assert.Equal("Sharma General Stores", OpenScreen().LegalName);
    }

    [Fact]
    public void NoBusinessHasBeenSavedOnAFreshDatabase()
    {
        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Fact]
    public void ABusinessHasBeenSavedOnceTheScreenSaves()
    {
        OpenScreenWithValidDetails().SaveCommand.Execute(null);

        Assert.True(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Fact]
    public void NoBusinessHasBeenSavedWhenTheScreenIsCancelled()
    {
        OpenScreenWithValidDetails().CancelCommand.Execute(null);

        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Fact]
    public void NoBusinessHasBeenSavedWhileTheScreenHasErrors()
    {
        var screen = OpenScreenWithValidDetails();
        screen.LegalName = "";
        screen.SaveCommand.Execute(null);

        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    private const string ChangeAppliesToNewBillsOnly =
        "Changing the Registration Type or GSTIN applies to new Bills only. Bills already issued "
        + "keep the details they were printed with.\n\nSave the change?";

    private static void SetText(BusinessDetailsViewModel screen, string field, string value)
    {
        switch (field)
        {
            case nameof(screen.LegalName):
                screen.LegalName = value;
                break;
            case nameof(screen.AddressLine1):
                screen.AddressLine1 = value;
                break;
            case nameof(screen.City):
                screen.City = value;
                break;
            case nameof(screen.Pin):
                screen.Pin = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(field), field, null);
        }
    }
}
