using System.Windows;
using Billow.Data;

namespace Billow;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        VersionText.Text = $"Version {AppInfo.Version}";
        DataFolderText.Text = AppPaths.DataDirectory;
    }
}
