namespace Billow.BusinessDetails;

/// <summary>
/// A licence or registration number, other than the GSTIN, that the Business prints on its
/// Bills, as stored in the database.
/// </summary>
public class AdditionalRegistration
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    /// <summary>Where it comes in the Business's list, counting from 0.</summary>
    public int Position { get; set; }

    /// <summary>What the number is, e.g. "FSSAI Lic. No.".</summary>
    public string Label { get; set; } = "";

    public string Number { get; set; } = "";
}
