using System.Windows;

namespace Billow.BusinessDetails;

/// <summary>A thin view of <see cref="BusinessDetailsViewModel"/>. DialogResult is true if the details were saved.</summary>
public partial class BusinessDetailsWindow : Window
{
    public BusinessDetailsWindow(Func<Window, BusinessDetailsViewModel> createViewModel)
    {
        InitializeComponent();

        var viewModel = createViewModel(this);
        viewModel.CloseRequested += (_, saved) => DialogResult = saved;
        DataContext = viewModel;
    }
}
