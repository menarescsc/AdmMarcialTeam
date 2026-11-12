using SQLite;

namespace AdmJudoTeam.Core.Entities;

/// <summary>
/// Inscripción de un judoka en un torneo, con el peso al momento de inscribir
/// y los objetivos tácticos. La diferencia de peso contra la categoría
/// (diff = actual - límite) se calcula en la capa de presentación.
/// </summary>
[Table("TournamentEntries")]
public class TournamentEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TournamentId { get; set; }

    [Indexed]
    public int MemberId { get; set; }

    /// <summary>Peso registrado al momento de la inscripción (kg).</summary>
    public double RegisteredWeightKg { get; set; }

    [MaxLength(300)]
    public string? TacticalGoals { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
