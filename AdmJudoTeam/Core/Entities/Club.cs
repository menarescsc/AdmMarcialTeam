using SQLite;

namespace AdmJudoTeam.Core.Entities;

/// <summary>
/// Club / dojo.
/// </summary>
[Table("Clubs")]
public class Club
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(40)]
    public string? Phone { get; set; }

    [MaxLength(120)]
    public string? Email { get; set; }

    /// <summary>Absolute file path of the club logo image; stores the path only.</summary>
    [MaxLength(260)]
    public string? LogoPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
