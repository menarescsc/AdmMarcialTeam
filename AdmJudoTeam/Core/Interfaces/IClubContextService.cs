namespace AdmJudoTeam.Core.Interfaces;

/// <summary>
/// Tracks which club is currently active. This is the single source of truth
/// for the club selection; ViewModels read it from DI to scope queries and new
/// records. The selection is persisted across app restarts.
/// </summary>
public interface IClubContextService
{
    /// <summary>Id of the selected club, or null when none is selected.</summary>
    int? SelectedClubId { get; }

    /// <summary>Selects the active club and persists the selection.</summary>
    void SelectClub(int clubId);

    /// <summary>Clears the selection (a club must then be picked again).</summary>
    void Clear();
}
