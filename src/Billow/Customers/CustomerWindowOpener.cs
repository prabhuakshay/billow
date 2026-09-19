using System.Windows;

namespace Billow.Customers;

/// <summary>Opens the Customer form as a <see cref="CustomerWindow"/> dialog over <paramref name="owner"/>.</summary>
public sealed class CustomerWindowOpener(Window owner) : ICustomerFormOpener
{
    public bool Open(CustomerViewModel form) =>
        new CustomerWindow(form) { Owner = owner }.ShowDialog() == true;
}
