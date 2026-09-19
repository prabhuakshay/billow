using Billow.BusinessDetails;
using Billow.Gst;

namespace Billow.Tests.BusinessDetails;

public sealed class BusinessDetailsViewModelTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeConfirmationPrompt _confirmationPrompt = new();
    private readonly FakeLogoFilePicker _logoFilePicker = new();
    private readonly TestImages _images = new();

    public void Dispose()
    {
        _database.Dispose();
        _images.Dispose();
    }

    private BusinessDetailsViewModel OpenScreen() =>
        new(_database.Open, _confirmationPrompt, _logoFilePicker);

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

    [Fact]
    public void AnAddedAdditionalRegistrationIsShownWhenTheScreenIsReopened()
    {
        var screen = OpenScreenWithValidDetails();

        screen.AddAdditionalRegistrationCommand.Execute(null);
        screen.AdditionalRegistrations[0].Label = "FSSAI Lic. No.";
        screen.AdditionalRegistrations[0].Number = "11521999000123";

        Assert.True(screen.Save());
        var row = Assert.Single(OpenScreen().AdditionalRegistrations);
        Assert.Equal("FSSAI Lic. No.", row.Label);
        Assert.Equal("11521999000123", row.Number);
    }

    [Fact]
    public void AdditionalRegistrationsCanBeReorderedAndKeepTheirOrderWhenSaved()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, "FSSAI Lic. No.", "11521999000123");
        AddAdditionalRegistration(screen, "D.L. No.", "MH-PZ3-123456");
        AddAdditionalRegistration(screen, "Udyam Reg. No.", "UDYAM-MH-26-0012345");

        screen.AdditionalRegistrations[2].MoveUpCommand.Execute(null);
        screen.AdditionalRegistrations[0].MoveDownCommand.Execute(null);

        Assert.Equal(["Udyam Reg. No.", "FSSAI Lic. No.", "D.L. No."], LabelsOf(screen));
        Assert.True(screen.Save());
        Assert.Equal(["Udyam Reg. No.", "FSSAI Lic. No.", "D.L. No."], LabelsOf(OpenScreen()));
    }

    [Fact]
    public void TheFirstAdditionalRegistrationCannotMoveUpNorTheLastDown()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, "FSSAI Lic. No.", "11521999000123");
        AddAdditionalRegistration(screen, "D.L. No.", "MH-PZ3-123456");

        screen.AdditionalRegistrations[0].MoveUpCommand.Execute(null);
        screen.AdditionalRegistrations[1].MoveDownCommand.Execute(null);

        Assert.Equal(["FSSAI Lic. No.", "D.L. No."], LabelsOf(screen));
    }

    [Fact]
    public void ARemovedAdditionalRegistrationIsGoneOnceSaved()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, "FSSAI Lic. No.", "11521999000123");
        AddAdditionalRegistration(screen, "D.L. No.", "MH-PZ3-123456");
        Assert.True(screen.Save());
        screen = OpenScreen();

        screen.AdditionalRegistrations[0].RemoveCommand.Execute(null);

        Assert.Equal(["D.L. No."], LabelsOf(screen));
        Assert.True(screen.Save());
        Assert.Equal(["D.L. No."], LabelsOf(OpenScreen()));
    }

    [Fact]
    public void ASavedAdditionalRegistrationCanBeEdited()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, "FSSAI Lic. No.", "11521999000123");
        Assert.True(screen.Save());
        screen = OpenScreen();

        screen.AdditionalRegistrations[0].Label = "FSSAI Reg. No.";
        screen.AdditionalRegistrations[0].Number = "21521999000456";

        Assert.True(screen.Save());
        var row = Assert.Single(OpenScreen().AdditionalRegistrations);
        Assert.Equal("FSSAI Reg. No.", row.Label);
        Assert.Equal("21521999000456", row.Number);
    }

    [Theory]
    [InlineData("FSSAI Lic. No.", "", nameof(AdditionalRegistrationViewModel.Number), "Enter the number.")]
    [InlineData("FSSAI Lic. No.", "   ", nameof(AdditionalRegistrationViewModel.Number), "Enter the number.")]
    [InlineData("", "11521999000123", nameof(AdditionalRegistrationViewModel.Label), "Enter the label.")]
    [InlineData("  ", "11521999000123", nameof(AdditionalRegistrationViewModel.Label), "Enter the label.")]
    public void AHalfFilledAdditionalRegistrationIsAnErrorAndBlocksSave(
        string label, string number, string field, string error)
    {
        var screen = OpenScreenWithValidDetails();
        var closed = false;
        screen.CloseRequested += (_, _) => closed = true;

        AddAdditionalRegistration(screen, label, number);
        screen.SaveCommand.Execute(null);

        var row = screen.AdditionalRegistrations[0];
        Assert.Equal(error, row.ErrorFor(field));
        Assert.True(row.HasErrors);
        Assert.True(screen.HasErrors);
        Assert.False(closed);
        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Fact]
    public void AHalfFilledAdditionalRegistrationsErrorClearsOnceItIsFilledIn()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, "D.L. No.", "");
        Assert.False(screen.Save());

        screen.AdditionalRegistrations[0].Number = "MH-PZ3-123456";

        Assert.Null(screen.AdditionalRegistrations[0].ErrorFor(nameof(AdditionalRegistrationViewModel.Number)));
        Assert.False(screen.HasErrors);
        Assert.True(screen.Save());
    }

    [Fact]
    public void AnEmptyAdditionalRegistrationIsNotAnErrorAndIsNotSaved()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, "FSSAI Lic. No.", "11521999000123");
        AddAdditionalRegistration(screen, " ", "");

        Assert.True(screen.Save());
        Assert.False(screen.AdditionalRegistrations[1].HasErrors);
        Assert.Equal(["FSSAI Lic. No."], LabelsOf(OpenScreen()));
    }

    [Fact]
    public void AnAdditionalRegistrationIsSavedWithoutSurroundingSpaces()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, " Drug Lic. No. 20B ", " MH-PZ3-123456 ");

        Assert.True(screen.Save());
        var row = Assert.Single(OpenScreen().AdditionalRegistrations);
        Assert.Equal("Drug Lic. No. 20B", row.Label);
        Assert.Equal("MH-PZ3-123456", row.Number);
    }

    [Fact]
    public void TheCommonAdditionalRegistrationLabelsAreSuggested()
    {
        Assert.Equal(
            ["FSSAI Lic. No.", "D.L. No.", "Udyam Reg. No.", "Shop Act Lic. No."],
            OpenScreen().SuggestedAdditionalRegistrationLabels);
    }

    [Fact]
    public void CancelPutsBackTheSavedAdditionalRegistrations()
    {
        var screen = OpenScreenWithValidDetails();
        AddAdditionalRegistration(screen, "FSSAI Lic. No.", "11521999000123");
        AddAdditionalRegistration(screen, "D.L. No.", "MH-PZ3-123456");
        Assert.True(screen.Save());
        screen = OpenScreen();
        screen.AdditionalRegistrations[1].MoveUpCommand.Execute(null);
        screen.AdditionalRegistrations[0].Number = "";
        screen.AdditionalRegistrations[1].RemoveCommand.Execute(null);
        AddAdditionalRegistration(screen, "Udyam Reg. No.", "UDYAM-MH-26-0012345");

        screen.CancelCommand.Execute(null);

        Assert.Equal(["FSSAI Lic. No.", "D.L. No."], LabelsOf(screen));
        Assert.Equal("MH-PZ3-123456", screen.AdditionalRegistrations[1].Number);
        Assert.False(screen.HasErrors);
    }

    [Fact]
    public void SavedContactAndPaymentDetailsAreShownWhenTheScreenIsReopened()
    {
        var screen = OpenScreenWithValidDetails();
        screen.Phone = "+91 98220 12345";
        screen.Email = "accounts@sharmastores.in";
        screen.UpiId = "sharmastores@okaxis";
        screen.BankAccountName = "Sharma General Stores";
        screen.BankAccountNumber = "50100123456789";
        screen.Ifsc = "HDFC0001234";
        screen.BankName = "HDFC Bank";
        screen.BankBranch = "Station Road, Pune";

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal("+91 98220 12345", reopened.Phone);
        Assert.Equal("accounts@sharmastores.in", reopened.Email);
        Assert.Equal("sharmastores@okaxis", reopened.UpiId);
        Assert.Equal("Sharma General Stores", reopened.BankAccountName);
        Assert.Equal("50100123456789", reopened.BankAccountNumber);
        Assert.Equal("HDFC0001234", reopened.Ifsc);
        Assert.Equal("HDFC Bank", reopened.BankName);
        Assert.Equal("Station Road, Pune", reopened.BankBranch);
    }

    [Fact]
    public void ContactAndPaymentDetailsAreOptional()
    {
        var screen = OpenScreenWithValidDetails();

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal("", reopened.Phone);
        Assert.Equal("", reopened.Email);
        Assert.Equal("", reopened.UpiId);
        Assert.Equal("", reopened.BankAccountName);
        Assert.Equal("", reopened.BankAccountNumber);
        Assert.Equal("", reopened.Ifsc);
        Assert.Equal("", reopened.BankName);
        Assert.Equal("", reopened.BankBranch);
    }

    [Theory]
    [InlineData("sharmastores")]
    [InlineData("sharmastores@")]
    [InlineData("@okaxis")]
    [InlineData("sharma stores@okaxis")]
    [InlineData("sharma@stores@okaxis")]
    [InlineData("sharmastores@ok axis")]
    public void ABadlyFormedUpiIdIsAnErrorAndBlocksSave(string upiId)
    {
        var screen = OpenScreenWithValidDetails();

        screen.UpiId = upiId;

        Assert.Equal(
            "A UPI ID is a name, then @, then the bank's handle, such as sharmastores@okaxis.",
            screen.ErrorFor(nameof(screen.UpiId)));
        Assert.False(screen.Save());
        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Theory]
    [InlineData("sharmastores@okaxis")]
    [InlineData("9822012345@ybl")]
    [InlineData("sharma.stores-1_pune@paytm")]
    public void AWellFormedUpiIdIsSaved(string upiId)
    {
        var screen = OpenScreenWithValidDetails();

        screen.UpiId = $" {upiId} ";

        Assert.Null(screen.ErrorFor(nameof(screen.UpiId)));
        Assert.True(screen.Save());
        Assert.Equal(upiId, OpenScreen().UpiId);
    }

    [Theory]
    [InlineData("HDFC000123")] // too short
    [InlineData("HDFC00012345")] // too long
    [InlineData("HDFC1001234")] // 5th character not 0
    [InlineData("HDF00001234")] // only 3 letters first
    [InlineData("HDFC0-01234")]
    [InlineData("HDFC 001234")]
    public void ABadlyFormedIfscIsAnErrorAndBlocksSave(string ifsc)
    {
        var screen = OpenScreenWithValidDetails();

        screen.Ifsc = ifsc;

        Assert.Equal(
            "An IFSC is 4 letters, then 0, then 6 letters or digits, such as HDFC0001234.",
            screen.ErrorFor(nameof(screen.Ifsc)));
        Assert.False(screen.Save());
        Assert.False(BusinessDetailsViewModel.HasSavedBusiness(_database.Open));
    }

    [Theory]
    [InlineData("HDFC0001234", "HDFC0001234")]
    [InlineData("SBIN0PUNE01", "SBIN0PUNE01")]
    [InlineData(" hdfc0001234 ", "HDFC0001234")]
    [InlineData("sbin0pune01", "SBIN0PUNE01")]
    public void AWellFormedIfscIsSavedUppercased(string ifsc, string saved)
    {
        var screen = OpenScreenWithValidDetails();

        screen.Ifsc = ifsc;

        Assert.Null(screen.ErrorFor(nameof(screen.Ifsc)));
        Assert.True(screen.Save());
        Assert.Equal(saved, OpenScreen().Ifsc);
    }

    [Fact]
    public void CancelPutsBackTheSavedContactAndPaymentDetails()
    {
        var screen = OpenScreenWithValidDetails();
        screen.UpiId = "sharmastores@okaxis";
        screen.Ifsc = "HDFC0001234";
        Assert.True(screen.Save());
        screen = OpenScreen();
        screen.Phone = "020 2612 3456";
        screen.UpiId = "not a upi id";
        screen.Ifsc = "HDFC";

        screen.CancelCommand.Execute(null);

        Assert.Equal("", screen.Phone);
        Assert.Equal("sharmastores@okaxis", screen.UpiId);
        Assert.Equal("HDFC0001234", screen.Ifsc);
        Assert.False(screen.HasErrors);
    }

    [Theory]
    [InlineData(1200, 600, 400, 200)]
    [InlineData(300, 900, 133, 400)]
    [InlineData(2000, 2000, 400, 400)]
    public void ALargeLogoIsShrunkSoItsLongestSideIs400Pixels(int width, int height, int storedWidth, int storedHeight)
    {
        var screen = OpenScreenWithValidDetails();

        ChooseLogo(screen, _images.Jpeg(width, height));

        Assert.Equal((storedWidth, storedHeight), TestImages.SizeOf(screen.Logo!));
    }

    [Theory]
    [InlineData(120, 80)]
    [InlineData(400, 250)]
    public void ASmallLogoIsNotEnlarged(int width, int height)
    {
        var screen = OpenScreenWithValidDetails();

        ChooseLogo(screen, _images.Jpeg(width, height));

        Assert.Equal((width, height), TestImages.SizeOf(screen.Logo!));
    }

    [Theory]
    [InlineData(120, 80)]
    [InlineData(1200, 800)]
    public void ALogoIsStoredAt96DotsPerInchSoItsPixelsAreItsSize(int width, int height)
    {
        var screen = OpenScreenWithValidDetails();

        ChooseLogo(screen, _images.Png(width, height, dpi: 300));

        Assert.Equal((96, 96), TestImages.DpiOf(screen.Logo!));
    }

    [Fact]
    public void AChosenLogoIsStoredAsPng()
    {
        var screen = OpenScreenWithValidDetails();

        ChooseLogo(screen, _images.Jpeg(64, 32));

        Assert.True(TestImages.IsPng(screen.Logo!));
        Assert.True(screen.HasLogo);
    }

    [Fact]
    public void ASavedLogoIsShownWhenTheScreenIsReopened()
    {
        var screen = OpenScreenWithValidDetails();
        ChooseLogo(screen, _images.Png(900, 300));
        var logo = screen.Logo;

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal(logo, reopened.Logo);
        Assert.Equal((400, 133), TestImages.SizeOf(reopened.Logo!));
        Assert.True(reopened.HasLogo);
    }

    [Fact]
    public void AnotherLogoReplacesTheCurrentOne()
    {
        var screen = OpenScreenWithValidDetails();
        ChooseLogo(screen, _images.Png(100, 50));
        Assert.True(screen.Save());
        screen = OpenScreen();

        ChooseLogo(screen, _images.Png(30, 60));
        Assert.True(screen.Save());

        Assert.Equal((30, 60), TestImages.SizeOf(OpenScreen().Logo!));
    }

    [Fact]
    public void RemovingTheLogoClearsItAndItStaysRemovedOnceSaved()
    {
        var screen = OpenScreenWithValidDetails();
        ChooseLogo(screen, _images.Png(100, 50));
        Assert.True(screen.Save());
        screen = OpenScreen();

        screen.RemoveLogoCommand.Execute(null);

        Assert.Null(screen.Logo);
        Assert.False(screen.HasLogo);
        Assert.True(screen.Save());
        Assert.Null(OpenScreen().Logo);
    }

    [Fact]
    public void ALogoIsOptional()
    {
        var screen = OpenScreenWithValidDetails();

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Null(reopened.Logo);
        Assert.False(reopened.HasLogo);
    }

    [Fact]
    public void CancellingThePickKeepsTheCurrentLogo()
    {
        var screen = OpenScreenWithValidDetails();
        ChooseLogo(screen, _images.Png(100, 50));
        var logo = screen.Logo;

        _logoFilePicker.FilePath = null;
        screen.ChooseLogoCommand.Execute(null);

        Assert.Same(logo, screen.Logo);
        Assert.Null(screen.LogoError);
    }

    [Fact]
    public void AnUnreadableFileShowsAnErrorAndKeepsTheCurrentLogo()
    {
        var screen = OpenScreenWithValidDetails();
        ChooseLogo(screen, _images.Png(100, 50));
        var logo = screen.Logo;

        ChooseLogo(screen, _images.NotAnImage());

        Assert.Equal(UnreadableLogo, screen.LogoError);
        Assert.Same(logo, screen.Logo);
    }

    [Fact]
    public void AFileThatIsGoneShowsAnError()
    {
        var screen = OpenScreenWithValidDetails();

        ChooseLogo(screen, Path.Combine(Path.GetTempPath(), $"billow-missing-{Guid.NewGuid():N}.png"));

        Assert.Equal(UnreadableLogo, screen.LogoError);
        Assert.Null(screen.Logo);
    }

    [Fact]
    public void AnUnreadableFileDoesNotBlockSave()
    {
        var screen = OpenScreenWithValidDetails();

        ChooseLogo(screen, _images.NotAnImage());

        Assert.False(screen.HasErrors);
        Assert.True(screen.Save());
    }

    [Fact]
    public void TheLogoErrorClearsOnceAReadableFileIsChosenOrTheLogoIsRemoved()
    {
        var screen = OpenScreenWithValidDetails();
        ChooseLogo(screen, _images.NotAnImage());

        ChooseLogo(screen, _images.Png(10, 10));
        Assert.Null(screen.LogoError);

        ChooseLogo(screen, _images.NotAnImage());
        screen.RemoveLogoCommand.Execute(null);
        Assert.Null(screen.LogoError);
    }

    [Fact]
    public void SavedAuthorisedSignatoryAndFooterTextAreShownWhenTheScreenIsReopened()
    {
        var screen = OpenScreenWithValidDetails();
        screen.AuthorisedSignatory = "  For Sharma General Stores  ";
        screen.FooterText = "Goods once sold will not be taken back.\r\nधन्यवाद! पुन्हा भेट द्या.\r\n";

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal("For Sharma General Stores", reopened.AuthorisedSignatory);
        Assert.Equal("Goods once sold will not be taken back.\r\nधन्यवाद! पुन्हा भेट द्या.", reopened.FooterText);
    }

    [Fact]
    public void AuthorisedSignatoryAndFooterTextAreOptional()
    {
        var screen = OpenScreenWithValidDetails();
        screen.AuthorisedSignatory = "   ";
        screen.FooterText = "\r\n  ";

        Assert.True(screen.Save());

        var reopened = OpenScreen();
        Assert.Equal("", reopened.AuthorisedSignatory);
        Assert.Equal("", reopened.FooterText);
    }

    [Fact]
    public void CancelPutsBackTheSavedPrintingDetails()
    {
        var screen = OpenScreenWithValidDetails();
        ChooseLogo(screen, _images.Png(100, 50));
        screen.AuthorisedSignatory = "For Sharma General Stores";
        screen.FooterText = "Thank you!";
        Assert.True(screen.Save());
        var logo = screen.Logo;
        screen = OpenScreen();
        screen.RemoveLogoCommand.Execute(null);
        ChooseLogo(screen, _images.NotAnImage());
        screen.AuthorisedSignatory = "Proprietor";
        screen.FooterText = "Visit again";

        screen.CancelCommand.Execute(null);

        Assert.Equal(logo, screen.Logo);
        Assert.Null(screen.LogoError);
        Assert.Equal("For Sharma General Stores", screen.AuthorisedSignatory);
        Assert.Equal("Thank you!", screen.FooterText);
    }

    private const string ChangeAppliesToNewBillsOnly =
        "Changing the Registration Type or GSTIN applies to new Bills only. Bills already issued "
        + "keep the details they were printed with.\n\nSave the change?";

    private const string UnreadableLogo =
        "Billow can't read that file as a picture. Choose a PNG, JPEG, BMP, GIF or TIFF image.";

    private static void AddAdditionalRegistration(BusinessDetailsViewModel screen, string label, string number)
    {
        screen.AddAdditionalRegistrationCommand.Execute(null);
        screen.AdditionalRegistrations[^1].Label = label;
        screen.AdditionalRegistrations[^1].Number = number;
    }

    /// <summary>Picks <paramref name="path"/> through the fake logo file picker.</summary>
    private void ChooseLogo(BusinessDetailsViewModel screen, string path)
    {
        _logoFilePicker.FilePath = path;
        screen.ChooseLogoCommand.Execute(null);
    }

    private static string[] LabelsOf(BusinessDetailsViewModel screen) =>
        [.. screen.AdditionalRegistrations.Select(row => row.Label)];

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
