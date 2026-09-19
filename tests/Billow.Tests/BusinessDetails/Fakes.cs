using Billow.BusinessDetails;

namespace Billow.Tests.BusinessDetails;

/// <summary>Answers every confirmation with <see cref="Answer"/>, and records the questions asked.</summary>
public sealed class FakeConfirmationPrompt : IConfirmationPrompt
{
    public bool Answer { get; set; } = true;

    public List<string> Questions { get; } = [];

    public bool Confirm(string question)
    {
        Questions.Add(question);
        return Answer;
    }
}

/// <summary>Hands over <see cref="FilePath"/> as if the user had picked it (null: the user cancelled).</summary>
public sealed class FakeLogoFilePicker : ILogoFilePicker
{
    public string? FilePath { get; set; }

    public string? PickLogoFile() => FilePath;
}
