namespace Billow.Customers;

/// <summary>
/// One Customer as a row of the Customers list. Details the Customer doesn't have are empty. The
/// Trade Name isn't shown, but the search looks in it.
/// </summary>
public sealed record CustomerListItem(
    int Id, string Name, string TradeName, string Gstin, string City, string State, string Phone)
{
    /// <summary>Whether the name, Trade Name, phone or GSTIN contains the text, ignoring case.</summary>
    public bool Matches(string text) =>
        new[] { Name, TradeName, Phone, Gstin }
            .Any(field => field.Contains(text, StringComparison.CurrentCultureIgnoreCase));
}
