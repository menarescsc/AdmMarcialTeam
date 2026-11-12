namespace AdmJudoTeam.Core.Interfaces;

/// <summary>
/// Comparte un archivo generado (por ejemplo la nómina PDF) con otras apps.
/// </summary>
public interface IFileShareService
{
    Task ShareFileAsync(string filePath, string title);
}
