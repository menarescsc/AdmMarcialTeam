namespace AdmJudoTeam.Core.Interfaces;

/// <summary>
/// Generación de reportes en PDF (Fase 5 del spec): nómina oficial de pesaje.
/// </summary>
public interface ITournamentReportService
{
    /// <summary>
    /// Genera la nómina oficial del torneo en PDF y devuelve la ruta del archivo.
    /// </summary>
    Task<string> GenerateRosterReportAsync(int tournamentId, CancellationToken cancellationToken = default);
}
