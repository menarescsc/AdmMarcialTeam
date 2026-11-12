using AdmJudoTeam.Core.Enums;
using AdmJudoTeam.Core.Models;

namespace AdmJudoTeam.Core.Interfaces;

/// <summary>
/// Motor de periodización: resuelve la fase de preparación según las semanas
/// restantes hasta el pesaje oficial del torneo.
/// </summary>
public interface IJudoPeriodizationService
{
    /// <summary>Semanas (fraccionadas) entre la sesión y el pesaje oficial.</summary>
    double GetWeeksUntilWeighIn(DateTime sessionDate, DateTime weighInDate);

    /// <summary>
    /// Fase según semanas hasta el pesaje: &gt;= 6 General; 4-5 Específica;
    /// 2-3 Precompetitiva (Choque); 1 Tapering; semana del torneo Competición.
    /// </summary>
    TrainingPhase ResolvePhase(DateTime sessionDate, DateTime weighInDate);

    /// <summary>Perfil completo: fase + rondas/minutos recomendados de Randori y Uchikomi.</summary>
    PeriodizationProfile GetProfile(DateTime sessionDate, DateTime weighInDate);
}
