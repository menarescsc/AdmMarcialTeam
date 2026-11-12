using AdmJudoTeam.Application.Common;
using AdmJudoTeam.Core.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AdmJudoTeam.Application.ViewModels;

/// <summary>
/// VM of the club panel: identity summary plus counters (active athletes,
/// competitions) for the selected club, and quick access to its sections.
/// All data is scoped to the active club context.
/// </summary>
public partial class ClubPanelViewModel : BaseViewModel
{
    private readonly IJudoRepository _repository;
    private readonly IClubContextService _clubContext;

    public ClubPanelViewModel(IJudoRepository repository, IClubContextService clubContext)
    {
        _repository = repository;
        _clubContext = clubContext;
    }

    /// <summary>False when no club is selected; the view redirects to the selector.</summary>
    public bool HasSelectedClub => _clubContext.SelectedClubId.HasValue;

    /// <summary>Club name for the identity card.</summary>
    public string? ClubName { get; private set; }

    /// <summary>Club address line, when present.</summary>
    public string? ClubAddress { get; private set; }

    /// <summary>
    /// In-memory data URL preview of the club logo; never persisted.
    /// </summary>
    public string? ClubLogoPreview { get; private set; }

    /// <summary>Number of active athletes in the selected club.</summary>
    public int ActiveAthleteCount { get; private set; }

    /// <summary>Number of competitions registered for the selected club.</summary>
    public int TournamentCount { get; private set; }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (_clubContext.SelectedClubId is not int clubId)
        {
            SetStatus("Seleccione un club primero.", isError: true);
            return;
        }

        try
        {
            IsBusy = true;
            SetStatus(null);

            var club = await _repository.GetClubAsync(clubId).ConfigureAwait(false);
            ClubName = club?.Name;
            ClubAddress = club?.Address;
            ClubLogoPreview = ImagePreview.ToDataUrl(club?.LogoPath);
            ActiveAthleteCount = (await _repository.GetMembersAsync(clubId, activeOnly: true).ConfigureAwait(false)).Count;
            TournamentCount = (await _repository.GetTournamentsAsync(clubId).ConfigureAwait(false)).Count;

            OnPropertyChanged(nameof(ClubName));
            OnPropertyChanged(nameof(ClubAddress));
            OnPropertyChanged(nameof(ClubLogoPreview));
            OnPropertyChanged(nameof(ActiveAthleteCount));
            OnPropertyChanged(nameof(TournamentCount));
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo cargar el panel del club: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
