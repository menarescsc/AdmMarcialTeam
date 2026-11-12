using AdmJudoTeam.Core.Interfaces;
using Microsoft.Maui.Storage;

namespace AdmJudoTeam.Infrastructure.State;

/// <summary>
/// Holds the active club selection for the session and mirrors it into
/// Preferences so it survives app restarts. Loads the persisted value on
/// construction; 0 or missing means no club selected.
/// </summary>
public sealed class ClubContextService : IClubContextService
{
    private const string SelectedClubIdKey = "selected_club_id";

    public ClubContextService()
    {
        var stored = Preferences.Default.Get(SelectedClubIdKey, 0);
        SelectedClubId = stored > 0 ? stored : null;
    }

    public int? SelectedClubId { get; private set; }

    public void SelectClub(int clubId)
    {
        SelectedClubId = clubId;
        Preferences.Default.Set(SelectedClubIdKey, clubId);
    }

    public void Clear()
    {
        SelectedClubId = null;
        Preferences.Default.Remove(SelectedClubIdKey);
    }
}
