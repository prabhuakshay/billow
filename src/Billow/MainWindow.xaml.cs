using System.Windows;
using Billow.BusinessDetails;
using Billow.Data;

namespace Billow;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        VersionStatus.Content = $"Version {AppInfo.Version}";
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void BusinessDetails_Click(object sender, RoutedEventArgs e) =>
        new BusinessDetailsWindow(window => new BusinessDetailsViewModel(
            () => new BillowDbContext(),
            new MessageBoxConfirmationPrompt(window),
            new OpenFileDialogLogoFilePicker(window)))
        { Owner = this }.ShowDialog();

    private void About_Click(object sender, RoutedEventArgs e) =>
        new AboutWindow { Owner = this }.ShowDialog();
}
