using System.Windows;

namespace Billow;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        VersionStatus.Content = $"Version {AppInfo.Version}";
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void About_Click(object sender, RoutedEventArgs e) =>
        new AboutWindow { Owner = this }.ShowDialog();
}
