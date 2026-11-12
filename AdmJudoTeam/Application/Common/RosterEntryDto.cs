namespace AdmJudoTeam.Application.Common;

/// <summary>
/// Fila de la vista de roster de torneo: atleta + inscripción + alerta de
/// peso contra la categoría IJF (diff = actual - límite).
/// </summary>
public sealed class RosterEntryDto
{
    public int MemberId { get; init; }
    public int? EntryId { get; set; }
    public string FullName { get; init; } = string.Empty;
    public string CategoryLabel { get; init; } = string.Empty;
    public bool IsOpenCategory { get; init; }
    public double? ActualWeightKg { get; init; }
    public double LimitKg { get; init; }

    public bool IsEnrolled { get; set; }
    public string? TacticalGoals { get; set; }

    /// <summary>diff = actual - límite. Null sin peso registrado o en categoría abierta.</summary>
    public double? WeightDiff =>
        ActualWeightKg is null || IsOpenCategory ? null : ActualWeightKg.Value - LimitKg;

    public bool IsOverweight => WeightDiff > 0;

    public string WeightLabel =>
        ActualWeightKg is null ? "sin peso" : ActualWeightKg.Value.ToString("0.0") + " kg";

    public string LimitLabel => IsOpenCategory ? "abierta" : LimitKg.ToString("0") + " kg";

    public string DiffLabel =>
        WeightDiff is null
            ? "—"
            : (WeightDiff > 0 ? "+" : "") + WeightDiff.Value.ToString("0.0") + " kg";
}
