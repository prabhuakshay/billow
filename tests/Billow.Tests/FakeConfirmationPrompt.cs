namespace Billow.Tests;

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
