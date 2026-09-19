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

    private void BusinessDetails_Click(object sender, RoutedEventArgs e)
    {
        var businessDetails = BusinessDetailsWindow.Create(() => new BillowDbContext());
        businessDetails.Owner = this;
        businessDetails.ShowDialog();
    }

    private void About_Click(object sender, RoutedEventArgs e) =>
        new AboutWindow { Owner = this }.ShowDialog();
}
