namespace Billow.BusinessDetails;

/// <summary>Lets the user pick an image file for the logo, so the view model never opens a dialog itself.</summary>
public interface ILogoFilePicker
{
    /// <summary>The path of the picked file, or null if the user cancelled.</summary>
    string? PickLogoFile();
}
