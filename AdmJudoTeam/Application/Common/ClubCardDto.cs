namespace AdmJudoTeam.Application.Common;

/// <summary>
/// Club summary for the club selector list. Carries an in-memory logo preview
/// (data URL) so views never touch the file system; it is never persisted.
/// </summary>
public sealed class ClubCardDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Address { get; init; }

    /// <summary>In-memory data URL preview of the club logo; null when absent.</summary>
    public string? LogoPreview { get; init; }
}
