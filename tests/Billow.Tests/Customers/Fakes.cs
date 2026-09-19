using Billow.Customers;

namespace Billow.Tests.Customers;

/// <summary>
/// Stands in for the Customer window: runs <see cref="UseForm"/> as if the user were filling in
/// the form, then reports whether the form closed saved, as the window would.
/// </summary>
public sealed class FakeCustomerFormOpener : ICustomerFormOpener
{
    /// <summary>What the user does on the form. By default, presses Save.</summary>
    public Action<CustomerViewModel> UseForm { get; set; } = form => form.SaveCommand.Execute(null);

    /// <summary>Every form opened, in order.</summary>
    public List<CustomerViewModel> Opened { get; } = [];

    public bool Open(CustomerViewModel form)
    {
        var saved = false;
        form.CloseRequested += (_, wasSaved) => saved = wasSaved;
        Opened.Add(form);
        UseForm(form);
        return saved;
    }
}
