namespace Billow.BusinessDetails;

/// <summary>Asks the user a yes/no question, so the view model never opens a dialog itself.</summary>
public interface IConfirmationPrompt
{
    /// <summary>True if the user answered yes.</summary>
    bool Confirm(string question);
}
