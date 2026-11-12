using SQLite;

namespace AdmJudoTeam.Core.Entities;

/// <summary>
/// Bloque ordenado dentro de una sesión (calentamiento, Uchikomi, Nage-komi,
/// Randori, etc.). <see cref="BlockType"/> es texto libre para no cerrar la
/// taxonomía de bloques en un enum.
/// </summary>
[Table("TrainingBlocks")]
public class TrainingBlock
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TrainingSessionId { get; set; }

    /// <summary>Orden del bloque dentro de la sesión (1..n).</summary>
    public int Sequence { get; set; }

    [NotNull, MaxLength(60)]
    public string BlockType { get; set; } = string.Empty;

    public int Rounds { get; set; }

    public int DurationMinutes { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }
}
