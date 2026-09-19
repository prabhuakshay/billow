using System.Windows;
using System.Windows.Input;
using Billow.Data;

namespace Billow.Customers;

/// <summary>A thin view of <see cref="CustomersViewModel"/>.</summary>
public partial class CustomersWindow : Window
{
    public CustomersWindow(Func<Window, CustomersViewModel> createViewModel)
    {
        InitializeComponent();

        DataContext = createViewModel(this);
    }

    /// <summary>The list for Billow's own database, opening the Customer form over this window.</summary>
    public static CustomersWindow Create() =>
        new(window => new CustomersViewModel(() => new BillowDbContext(), new CustomerWindowOpener(window)));

    private void Close_Executed(object sender, ExecutedRoutedEventArgs e) => Close();
}
