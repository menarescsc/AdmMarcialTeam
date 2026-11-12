using AdmJudoTeam.Core.Entities;

namespace AdmJudoTeam.Core.Interfaces;

/// <summary>
/// CRUD asíncrono sobre la base SQLite local. Los métodos Save* insertan
/// (Id == 0) o actualizan, y devuelven el Id de la entidad.
/// </summary>
public interface IJudoRepository
{
    Task InitializeAsync();

    // Club
    Task<Club?> GetClubAsync(int clubId);
    Task<List<Club>> GetClubsAsync();
    Task<int> SaveClubAsync(Club club);

    /// <summary>
    /// Deletes the club and, inside a single transaction, all of its dependent
    /// data: session attendance, training blocks, training sessions, tournament
    /// entries, tournaments and members. Returns the number of club rows
    /// deleted (0 or 1).
    /// </summary>
    Task<int> DeleteClubAsync(Club club);

    // Members
    Task<List<Member>> GetMembersAsync(int clubId, bool activeOnly = true);
    Task<Member?> GetMemberAsync(int memberId);
    Task<int> SaveMemberAsync(Member member);
    Task<int> DeleteMemberAsync(Member member);

    // Tournaments
    Task<List<Tournament>> GetTournamentsAsync(int clubId);
    Task<Tournament?> GetTournamentAsync(int tournamentId);
    Task<int> SaveTournamentAsync(Tournament tournament);
    Task<int> DeleteTournamentAsync(Tournament tournament);

    // Tournament entries
    Task<List<TournamentEntry>> GetTournamentEntriesAsync(int tournamentId);
    Task<int> SaveTournamentEntryAsync(TournamentEntry entry);
    Task<int> DeleteTournamentEntryAsync(TournamentEntry entry);

    // Training sessions
    Task<List<TrainingSession>> GetTrainingSessionsAsync(int clubId, DateTime? from = null, DateTime? to = null);
    Task<TrainingSession?> GetTrainingSessionAsync(int sessionId);
    Task<int> SaveTrainingSessionAsync(TrainingSession session, IReadOnlyList<TrainingBlock>? blocks = null);
    Task<int> DeleteTrainingSessionAsync(TrainingSession session);
    Task<List<TrainingBlock>> GetTrainingBlocksAsync(int sessionId);

    // Attendance
    Task<List<SessionAttendance>> GetSessionAttendanceAsync(int sessionId);
    Task<int> SaveSessionAttendanceAsync(SessionAttendance attendance);
    Task<int> DeleteSessionAttendanceAsync(SessionAttendance attendance);
}
