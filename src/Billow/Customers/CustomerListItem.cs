namespace Billow.Customers;

/// <summary>
/// One Customer as a row of the Customers list. Details the Customer doesn't have are empty. The
/// Trade Name isn't shown, but the search looks in it.
/// </summary>
public sealed record CustomerListItem(
    int Id, string Name, string TradeName, string Gstin, string City, string State, string Phone)
{
    /// <summary>
    /// Whether the name, Trade Name, phone or GSTIN contains the text, ignoring case. Spaces are
    /// ignored in the phone, so "9876543210" finds "98765 43210".
    /// </summary>
    public bool Matches(string text) =>
        new[] { Name, TradeName, Gstin }
            .Any(field => field.Contains(text, StringComparison.CurrentCultureIgnoreCase))
        || WithoutSpaces(Phone).Contains(WithoutSpaces(text), StringComparison.Ordinal);

    private static string WithoutSpaces(string text) => text.Replace(" ", "", StringComparison.Ordinal);
}
