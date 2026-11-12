namespace AdmJudoTeam.Core.Interfaces;

/// <summary>
/// Hardware nativo usado por el cronómetro de clase (Modo Tatami):
/// pantalla encendida, vibración háptica y audio (Hajime / Matte).
/// </summary>
public interface ITimerHardwareService
{
    /// <summary>Mantiene la pantalla encendida durante la sesión y la libera al finalizar/pausar.</summary>
    void SetKeepScreenOn(bool enabled);

    void Vibrate(int durationMs = 200);

    Task SpeakAsync(string text, CancellationToken cancellationToken = default);
}
