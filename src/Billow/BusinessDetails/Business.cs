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

    /// <summary>The Additional Registrations printed on Bills. Not sorted: order them by <see cref="AdditionalRegistration.Position"/>.</summary>
    public List<AdditionalRegistration> AdditionalRegistrations { get; } = [];
}
