using System.Windows;
using Microsoft.Win32;

namespace Billow.BusinessDetails;

/// <summary>Picks the logo with the standard Windows Open dialog over <paramref name="owner"/>.</summary>
public sealed class OpenFileDialogLogoFilePicker(Window owner) : ILogoFilePicker
{
    public string? PickLogoFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose a logo",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|All files|*.*",
        };

        return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
    }
}
