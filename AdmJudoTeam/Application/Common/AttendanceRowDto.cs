namespace AdmJudoTeam.Application.Common;

/// <summary>
/// Fila de la vista de Modo Tatami: atleta + asistencia (1 toque) +
/// observación técnica individual.
/// </summary>
public sealed class AttendanceRowDto
{
    public int MemberId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public bool Present { get; set; }
    public string? Observation { get; set; }
}
