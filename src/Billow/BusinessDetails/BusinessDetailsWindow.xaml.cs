using System.Windows;
using Billow.Data;

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

    /// <summary>The screen for Billow's own database, with real prompts and file pickers.</summary>
    public static BusinessDetailsWindow Create(Func<BillowDbContext> openDatabase) =>
        new(window => new BusinessDetailsViewModel(
            openDatabase,
            new MessageBoxConfirmationPrompt(window),
            new OpenFileDialogLogoFilePicker(window)));
}
