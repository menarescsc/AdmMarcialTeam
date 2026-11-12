using SQLite;

namespace AdmJudoTeam.Core.Entities;

/// <summary>
/// Campeonato / torneo. El pesaje oficial (<see cref="WeighInDate"/>) es la
/// fecha de referencia del motor de periodización.
/// </summary>
[Table("Tournaments")]
public class Tournament
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Club scope (logical FK): each club has its own tournaments.</summary>
    [Indexed]
    public int ClubId { get; set; }

    [NotNull, MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? Location { get; set; }

    public DateTime StartDate { get; set; }

    /// <summary>Fecha del pesaje oficial.</summary>
    public DateTime WeighInDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
