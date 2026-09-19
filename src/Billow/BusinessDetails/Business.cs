namespace Billow.BusinessDetails;

/// <summary>The one Business this Billow install bills for, as stored in the database.</summary>
public class Business
{
    public int Id { get; set; }

    public string LegalName { get; set; } = "";

    public string? TradeName { get; set; }

    public string AddressLine1 { get; set; } = "";

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = "";

    /// <summary>The two-digit GST state code, e.g. "27".</summary>
    public string StateCode { get; set; } = "";

    public string Pin { get; set; } = "";

    public RegistrationType RegistrationType { get; set; }

    /// <summary>The normalised GSTIN, or null for an Unregistered Business.</summary>
    public string? Gstin { get; set; }

    public string? Pan { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? UpiId { get; set; }

    public string? BankAccountName { get; set; }

    public string? BankAccountNumber { get; set; }

    /// <summary>The bank branch's IFSC, uppercased.</summary>
    public string? Ifsc { get; set; }

    public string? BankName { get; set; }

    public string? BankBranch { get; set; }

    /// <summary>The Additional Registrations printed on Bills. Not sorted: order them by <see cref="AdditionalRegistration.Position"/>.</summary>
    public List<AdditionalRegistration> AdditionalRegistrations { get; } = [];
}
