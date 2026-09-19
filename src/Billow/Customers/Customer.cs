namespace Billow.Customers;

/// <summary>
/// A party the Business sells to, as stored in the database. With a GSTIN it is a B2B Customer;
/// without one, a B2C Customer.
/// </summary>
public class Customer
{
    public int Id { get; set; }

    /// <summary>The Legal Name of a B2B Customer, or the plain name of a B2C Customer.</summary>
    public string Name { get; set; } = "";

    public string? TradeName { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    /// <summary>
    /// The two-digit GST state code, e.g. "27". Null only for a B2C Customer entered before any
    /// Business was saved, who has no State to default to.
    /// </summary>
    public string? StateCode { get; set; }

    public string? Pin { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    /// <summary>The normalised GSTIN of a B2B Customer, or null for a B2C Customer. Unique when present.</summary>
    public string? Gstin { get; set; }

    /// <summary>An inactive Customer is kept on record but left out of the Customers list.</summary>
    public bool IsActive { get; set; } = true;
}
