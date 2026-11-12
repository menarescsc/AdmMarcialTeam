using AdmJudoTeam.Core.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Media;

namespace AdmJudoTeam.Infrastructure.Device;

/// <summary>
/// Hardware nativo del cronómetro de clase (Modo Tatami): pantalla encendida,
/// vibración háptica y audio (Hajime / Matte). Todas las llamadas se
/// disparan en el hilo principal (requisito de las APIs de dispositivo de
/// Android) sin bloquear al llamador.
/// </summary>
public sealed class TimerHardwareService : ITimerHardwareService
{
    public void SetKeepScreenOn(bool enabled) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                DeviceDisplay.Current.KeepScreenOn = enabled;
            }
            catch (Exception)
            {
                // Nunca romper la clase por un flag de pantalla.
            }
        });

    public void Vibrate(int durationMs = 200) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (Vibration.Default.IsSupported)
                {
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(durationMs));
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Hardware sin vibración: el cronómetro sigue funcionando.
            }
        });

    public Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        // SpeakAsync(string, SpeechOptions?) no acepta CancellationToken en esta
        // versión de MAUI; el token se respeta como pre-chequeo cooperativo.
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var completion = new TaskCompletionSource();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await TextToSpeech.Default.SpeakAsync(text).ConfigureAwait(false);
                completion.SetResult();
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });
        return completion.Task;
    }
}
