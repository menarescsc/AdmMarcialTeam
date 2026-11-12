using CommunityToolkit.Mvvm.ComponentModel;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// Base común de ViewModels: estado de carga y mensaje de estado observables.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string? _statusMessage;
    private bool _statusIsError;

    public bool IsBusy
    {
        get => _isBusy;
        protected set => SetProperty(ref _isBusy, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        protected set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// True when the current status message reports an error. Views use it to
    /// pick the alert banner style (success vs danger) so severity is never
    /// conveyed by color alone.
    /// </summary>
    public bool StatusIsError
    {
        get => _statusIsError;
        private set => SetProperty(ref _statusIsError, value);
    }

    /// <summary>
    /// Publishes a status message and whether it represents an error.
    /// </summary>
    protected void SetStatus(string? message, bool isError = false)
    {
        StatusMessage = message;
        StatusIsError = isError;
    }
}
