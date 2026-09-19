using Billow.BusinessDetails;

namespace Billow.Tests.BusinessDetails;

/// <summary>Hands over <see cref="FilePath"/> as if the user had picked it (null: the user cancelled).</summary>
public sealed class FakeLogoFilePicker : ILogoFilePicker
{
    public string? FilePath { get; set; }

    public string? PickLogoFile() => FilePath;
}
