using AdmJudoTeam.Core.Enums;

namespace AdmJudoTeam.Core.Models;

/// <summary>
/// Perfil de periodización resultante para una sesión, según las semanas
/// restantes hasta el pesaje oficial del torneo.
/// </summary>
public sealed class PeriodizationProfile
{
    public TrainingPhase Phase { get; init; }

    /// <summary>Semanas (fraccionadas) entre la sesión y el pesaje oficial.</summary>
    public double WeeksUntilWeighIn { get; init; }

    public int RecommendedRandoriRounds { get; init; }
    public int RecommendedRandoriMinutes { get; init; }
    public int RecommendedUchikomiRounds { get; init; }

    /// <summary>Resumen corto de la fase, para mostrar en UI.</summary>
    public string Focus { get; init; } = string.Empty;
}
