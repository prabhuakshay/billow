namespace Billow.Customers;

/// <summary>Shows the Customer form so the user can fill it in, and waits until it closes.</summary>
public interface ICustomerFormOpener
{
    /// <summary>True if the form closed with the Customer saved.</summary>
    bool Open(CustomerViewModel form);
}
