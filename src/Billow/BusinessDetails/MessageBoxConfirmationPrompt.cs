using System.Windows;

namespace Billow.BusinessDetails;

/// <summary>Asks with a Yes/No message box over <paramref name="owner"/>.</summary>
public sealed class MessageBoxConfirmationPrompt(Window owner) : IConfirmationPrompt
{
    public bool Confirm(string question) =>
        MessageBox.Show(owner, question, "Billow", MessageBoxButton.YesNo, MessageBoxImage.Question)
            == MessageBoxResult.Yes;
}
