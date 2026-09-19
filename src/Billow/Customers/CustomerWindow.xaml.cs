using System.Windows;

namespace Billow.Customers;

/// <summary>A thin view of <see cref="CustomerViewModel"/>. DialogResult is true if the Customer was saved.</summary>
public partial class CustomerWindow : Window
{
    public CustomerWindow(CustomerViewModel viewModel)
    {
        InitializeComponent();

        viewModel.CloseRequested += (_, saved) => DialogResult = saved;
        DataContext = viewModel;
    }
}
