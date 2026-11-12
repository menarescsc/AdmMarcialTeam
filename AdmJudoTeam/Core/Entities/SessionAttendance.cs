using SQLite;

namespace AdmJudoTeam.Core.Entities;

/// <summary>
/// Asistencia de un judoka a una sesión (1 toque en Modo Tatami) con su
/// observación técnica individual. La pareja (sesión, miembro) es única:
/// el índice único se crea en la inicialización de la base.
/// </summary>
[Table("SessionAttendance")]
public class SessionAttendance
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TrainingSessionId { get; set; }

    [Indexed]
    public int MemberId { get; set; }

    public bool Present { get; set; }

    [MaxLength(300)]
    public string? Observation { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
