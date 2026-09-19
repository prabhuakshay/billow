namespace Billow.Customers;

/// <summary>One Customer as a row of the Customers list. Details the Customer doesn't have are empty.</summary>
public sealed record CustomerListItem(int Id, string Name, string Gstin, string City, string State, string Phone);
