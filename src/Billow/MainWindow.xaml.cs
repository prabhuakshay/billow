using System.Windows;
using System.Windows.Controls;

namespace Billow;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        VersionStatus.Content = $"Version {AppInfo.Version}";

        // Open on the first section.
        var sales = (TreeViewItem)NavTree.Items[0];
        ((TreeViewItem)sales.Items[0]).IsSelected = true;
    }

    private void NavTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem item)
        {
            PageTitle.Text = item.Header.ToString();
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void About_Click(object sender, RoutedEventArgs e) =>
        new AboutWindow { Owner = this }.ShowDialog();
}
