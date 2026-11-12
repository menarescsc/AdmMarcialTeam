using AdmJudoTeam.Core.Enums;
using SQLite;

namespace AdmJudoTeam.Core.Entities;

/// <summary>
/// Sesión de entrenamiento. Si está vinculada a un torneo, la fase y los
/// volúmenes provienen del motor de periodización.
/// </summary>
[Table("TrainingSessions")]
public class TrainingSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Club scope (logical FK): each club has its own training sessions.</summary>
    [Indexed]
    public int ClubId { get; set; }

    /// <summary>Torneo de referencia (FK lógica, opcional).</summary>
    [Indexed]
    public int? TournamentId { get; set; }

    [Indexed]
    public DateTime Date { get; set; }

    [MaxLength(120)]
    public string? Focus { get; set; }

    public TrainingPhase Phase { get; set; }

    public SessionIntensity Intensity { get; set; }

    public int DurationMinutes { get; set; }

    /// <summary>Volumen total de Uchikomi (golpes/series).</summary>
    public int UchikomiRounds { get; set; }

    /// <summary>Minutos totales de Randori.</summary>
    public int RandoriMinutes { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
