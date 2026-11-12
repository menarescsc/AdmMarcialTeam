using AdmJudoTeam.Core.Enums;
using AdmJudoTeam.Core.Interfaces;
using AdmJudoTeam.Core.Models;

namespace AdmJudoTeam.Core.Services;

/// <summary>
/// Motor de periodización táctica. Calcula la fase de preparación según las
/// semanas restantes hasta el pesaje oficial (la fecha de referencia que usa
/// el módulo de torneos; si se prefiere el primer día del torneo, se pasa esa
/// fecha al mismo método):
///
///   - >= 6 semanas ......... Preparación General / Capacidad
///   - 4 a 5 semanas ........ Volumen Específico / Tokui-waza
///   - 2 a 3 semanas ........ Choque Precompetitivo / Simulación Shiai y Golden Score
///   - 1 semana ............. Tapering / Descarga y velocidad
///   - Semana del torneo .... Activación y control de pesaje
///
/// Los volúmenes recomendados son valores base editables por entrenador.
/// </summary>
public sealed class JudoPeriodizationService : IJudoPeriodizationService
{
    public double GetWeeksUntilWeighIn(DateTime sessionDate, DateTime weighInDate) =>
        (weighInDate - sessionDate).TotalDays / 7d;

    public TrainingPhase ResolvePhase(DateTime sessionDate, DateTime weighInDate) =>
        ResolvePhase(GetWeeksUntilWeighIn(sessionDate, weighInDate));

    public PeriodizationProfile GetProfile(DateTime sessionDate, DateTime weighInDate)
    {
        var weeks = GetWeeksUntilWeighIn(sessionDate, weighInDate);
        var phase = ResolvePhase(weeks);
        return BuildProfile(phase, weeks);
    }

    private static TrainingPhase ResolvePhase(double weeks) => weeks switch
    {
        >= 6 => TrainingPhase.General,
        >= 4 => TrainingPhase.Specific,
        >= 2 => TrainingPhase.PreCompetitive,
        >= 1 => TrainingPhase.Tapering,
        _ => TrainingPhase.Competition
    };

    private static PeriodizationProfile BuildProfile(TrainingPhase phase, double weeks) => phase switch
    {
        TrainingPhase.General => new PeriodizationProfile
        {
            Phase = phase,
            WeeksUntilWeighIn = weeks,
            RecommendedUchikomiRounds = 10,
            RecommendedRandoriRounds = 3,
            RecommendedRandoriMinutes = 15,
            Focus = "Preparación General / Capacidad"
        },
        TrainingPhase.Specific => new PeriodizationProfile
        {
            Phase = phase,
            WeeksUntilWeighIn = weeks,
            RecommendedUchikomiRounds = 8,
            RecommendedRandoriRounds = 4,
            RecommendedRandoriMinutes = 20,
            Focus = "Volumen Específico / Tokui-waza"
        },
        TrainingPhase.PreCompetitive => new PeriodizationProfile
        {
            Phase = phase,
            WeeksUntilWeighIn = weeks,
            RecommendedUchikomiRounds = 6,
            RecommendedRandoriRounds = 5,
            RecommendedRandoriMinutes = 25,
            Focus = "Choque Precompetitivo / Simulación Shiai y Golden Score"
        },
        TrainingPhase.Tapering => new PeriodizationProfile
        {
            Phase = phase,
            WeeksUntilWeighIn = weeks,
            RecommendedUchikomiRounds = 4,
            RecommendedRandoriRounds = 2,
            RecommendedRandoriMinutes = 10,
            Focus = "Tapering / Descarga y velocidad"
        },
        _ => new PeriodizationProfile
        {
            Phase = TrainingPhase.Competition,
            WeeksUntilWeighIn = weeks,
            RecommendedUchikomiRounds = 2,
            RecommendedRandoriRounds = 1,
            RecommendedRandoriMinutes = 5,
            Focus = "Activación y control de pesaje"
        }
    };
}
