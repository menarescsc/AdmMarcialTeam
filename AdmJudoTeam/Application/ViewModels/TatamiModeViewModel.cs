using System.Collections.ObjectModel;
using AdmJudoTeam.Application.Common;
using AdmJudoTeam.Core.Entities;
using AdmJudoTeam.Core.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// VM del Modo Tatami (ejecución de clase): asistencia en 1 toque con
/// persistencia inmediata, observaciones técnicas individuales y cronómetro
/// de rondas Hajime / Matte con audio, vibración y pantalla encendida.
/// La vista solo consume esta clase (MVVM estricto).
/// </summary>
public partial class TatamiModeViewModel : BaseViewModel, IDisposable
{
    private readonly IJudoRepository _repository;
    private readonly ITimerHardwareService _hardware;
    private readonly IClubContextService _clubContext;
    private readonly System.Timers.Timer _timer;
    private TrainingSession? _session;

    private TimeSpan _roundDuration = TimeSpan.FromMinutes(5);
    private TimeSpan _timeRemaining = TimeSpan.FromMinutes(5);
    private bool _isTimerRunning;
    private int _round = 1;

    public TatamiModeViewModel(IJudoRepository repository, ITimerHardwareService hardware, IClubContextService clubContext)
    {
        _repository = repository;
        _hardware = hardware;
        _clubContext = clubContext;
        _timer = new System.Timers.Timer(250) { AutoReset = true };
        _timer.Elapsed += OnTimerTick;
    }

    /// <summary>False when no club is selected; the view redirects to the selector.</summary>
    public bool HasSelectedClub => _clubContext.SelectedClubId.HasValue;

    public ObservableCollection<AttendanceRowDto> Rows { get; } = new();

    public string SessionLabel =>
        _session is null ? "Sesión de hoy" : $"Clase del {_session.Date:dd/MM/yyyy}";

    public int PresentCount => Rows.Count(row => row.Present);
    public int AbsentCount => Rows.Count - PresentCount;

    public string TimeLabel =>
        $"{(int)_timeRemaining.TotalMinutes:00}:{_timeRemaining.Seconds:00}";

    public string DurationLabel => $"{(int)_roundDuration.TotalMinutes} min";

    public int Round
    {
        get => _round;
        private set => SetProperty(ref _round, value);
    }

    public bool IsTimerRunning
    {
        get => _isTimerRunning;
        private set => SetProperty(ref _isTimerRunning, value);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            SetStatus(null);

            if (_clubContext.SelectedClubId is not int clubId)
            {
                Rows.Clear();
                SetStatus("Seleccione un club primero.", isError: true);
                return;
            }

            var today = DateTime.Now.Date;
            var sessions = await _repository
                .GetTrainingSessionsAsync(clubId, today, today.AddDays(1).AddTicks(-1))
                .ConfigureAwait(false);

            _session = sessions.FirstOrDefault();
            if (_session is null)
            {
                // The class session always belongs to the active club context.
                _session = new TrainingSession
                {
                    ClubId = clubId,
                    Date = DateTime.Now,
                    Focus = "Clase del día",
                    DurationMinutes = 60
                };
                _session.Id = await _repository.SaveTrainingSessionAsync(_session).ConfigureAwait(false);
            }

            var members = await _repository.GetMembersAsync(clubId, activeOnly: true).ConfigureAwait(false);
            var attendance = await _repository.GetSessionAttendanceAsync(_session.Id).ConfigureAwait(false);

            Rows.Clear();
            foreach (var member in members)
            {
                var saved = attendance.FirstOrDefault(a => a.MemberId == member.Id);
                Rows.Add(new AttendanceRowDto
                {
                    MemberId = member.Id,
                    FullName = $"{member.LastName}, {member.FirstName}",
                    Present = saved?.Present ?? false,
                    Observation = saved?.Observation
                });
            }

            OnPropertyChanged(nameof(SessionLabel));
            NotifyAttendanceCounts();
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo cargar la sesión: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>1 toque = alternar asistencia y guardar de inmediato.</summary>
    [RelayCommand]
    private async Task ToggleAttendanceAsync(AttendanceRowDto row)
    {
        if (_session is null)
        {
            return;
        }

        row.Present = !row.Present;
        await SaveAttendanceAsync(row).ConfigureAwait(false);
        NotifyAttendanceCounts();
    }

    [RelayCommand]
    private Task SaveObservationAsync(AttendanceRowDto row) => SaveAttendanceAsync(row);

    [RelayCommand]
    private Task HajimeAsync()
    {
        if (IsTimerRunning)
        {
            return Task.CompletedTask;
        }

        if (_timeRemaining <= TimeSpan.Zero)
        {
            _timeRemaining = _roundDuration;
            OnPropertyChanged(nameof(TimeLabel));
        }

        _timer.Start();
        IsTimerRunning = true;
        _hardware.SetKeepScreenOn(true);
        return _hardware.SpeakAsync("Hajime");
    }

    [RelayCommand]
    private Task MatteAsync()
    {
        if (!IsTimerRunning)
        {
            return Task.CompletedTask;
        }

        _timer.Stop();
        IsTimerRunning = false;
        _hardware.SetKeepScreenOn(false);
        return _hardware.SpeakAsync("Matte");
    }

    [RelayCommand]
    private void ResetTimer()
    {
        _timer.Stop();
        IsTimerRunning = false;
        Round = 1;
        _timeRemaining = _roundDuration;
        OnPropertyChanged(nameof(TimeLabel));
        _hardware.SetKeepScreenOn(false);
    }

    [RelayCommand]
    private void SetRoundDuration(int minutes)
    {
        if (minutes <= 0)
        {
            return;
        }

        _roundDuration = TimeSpan.FromMinutes(minutes);
        if (!IsTimerRunning)
        {
            _timeRemaining = _roundDuration;
            OnPropertyChanged(nameof(TimeLabel));
        }

        OnPropertyChanged(nameof(DurationLabel));
    }

    public void Dispose()
    {
        _timer.Elapsed -= OnTimerTick;
        _timer.Stop();
        _timer.Dispose();
        _hardware.SetKeepScreenOn(false);
    }

    private void OnTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromMilliseconds(250));
        if (_timeRemaining > TimeSpan.Zero)
        {
            OnPropertyChanged(nameof(TimeLabel));
            return;
        }

        // Fin de ronda: Matte con vibración y audio; la siguiente ronda queda
        // preparada en pausa (Hajime relanza y recarga la duración).
        _timeRemaining = TimeSpan.Zero;
        _timer.Stop();
        IsTimerRunning = false;
        Round += 1;
        OnPropertyChanged(nameof(TimeLabel));
        _hardware.SetKeepScreenOn(false);
        _hardware.Vibrate(500);
        _ = _hardware.SpeakAsync("Matte");
    }

    private async Task SaveAttendanceAsync(AttendanceRowDto row)
    {
        if (_session is null)
        {
            return;
        }

        try
        {
            await _repository.SaveSessionAttendanceAsync(new SessionAttendance
            {
                TrainingSessionId = _session.Id,
                MemberId = row.MemberId,
                Present = row.Present,
                Observation = row.Observation,
                RecordedAt = DateTime.Now
            }).ConfigureAwait(false);
            SetStatus("Asistencia guardada.");
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo guardar la asistencia: {ex.Message}", isError: true);
        }
    }

    private void NotifyAttendanceCounts()
    {
        OnPropertyChanged(nameof(PresentCount));
        OnPropertyChanged(nameof(AbsentCount));
    }
}
