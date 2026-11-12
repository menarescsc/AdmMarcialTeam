using AdmJudoTeam.Core.Entities;
using AdmJudoTeam.Core.Interfaces;
using AdmJudoTeam.Infrastructure.Data;
using SQLite;

namespace AdmJudoTeam.Infrastructure.Repositories;

/// <summary>
/// CRUD asíncrono sobre SQLite. Nunca bloquea: todo async con ConfigureAwait(false).
/// Los métodos Save* insertan (Id == 0) o actualizan, y devuelven el Id.
/// </summary>
public sealed class JudoRepository : IJudoRepository
{
    private readonly JudoDatabase _database;

    public JudoRepository(JudoDatabase database)
    {
        _database = database;
    }

    public Task InitializeAsync() => _database.InitializeAsync();

    // ---- Club -------------------------------------------------------------

    public async Task<Club?> GetClubAsync(int clubId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<Club>().Where(c => c.Id == clubId).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<List<Club>> GetClubsAsync()
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<Club>().OrderBy(c => c.Name).ToListAsync().ConfigureAwait(false);
    }

    public async Task<int> SaveClubAsync(Club club)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return club.Id == 0 ? await InsertAsync(connection, club).ConfigureAwait(false)
                            : await UpdateAsync(connection, club).ConfigureAwait(false);
    }

    public async Task<int> DeleteClubAsync(Club club)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        var deletedRows = 0;

        // Cascade inside a single transaction so a partial failure never leaves
        // the club with orphaned dependents (or dependents without their club).
        await connection.RunInTransactionAsync(tran =>
        {
            tran.Execute("DELETE FROM SessionAttendance WHERE TrainingSessionId IN (SELECT Id FROM TrainingSessions WHERE ClubId = ?);", club.Id);
            tran.Execute("DELETE FROM TrainingBlocks WHERE TrainingSessionId IN (SELECT Id FROM TrainingSessions WHERE ClubId = ?);", club.Id);
            tran.Execute("DELETE FROM TrainingSessions WHERE ClubId = ?;", club.Id);
            tran.Execute("DELETE FROM TournamentEntries WHERE TournamentId IN (SELECT Id FROM Tournaments WHERE ClubId = ?);", club.Id);
            tran.Execute("DELETE FROM Tournaments WHERE ClubId = ?;", club.Id);
            tran.Execute("DELETE FROM Members WHERE ClubId = ?;", club.Id);
            deletedRows = tran.Execute("DELETE FROM Clubs WHERE Id = ?;", club.Id);
        }).ConfigureAwait(false);

        return deletedRows;
    }

    // ---- Members ----------------------------------------------------------

    public async Task<List<Member>> GetMembersAsync(int clubId, bool activeOnly = true)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        var query = connection.Table<Member>().Where(m => m.ClubId == clubId);
        if (activeOnly)
        {
            query = query.Where(m => m.IsActive);
        }

        return await query.OrderBy(m => m.LastName).ThenBy(m => m.FirstName).ToListAsync().ConfigureAwait(false);
    }

    public async Task<Member?> GetMemberAsync(int memberId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<Member>().Where(m => m.Id == memberId).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<int> SaveMemberAsync(Member member)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return member.Id == 0 ? await InsertAsync(connection, member).ConfigureAwait(false)
                              : await UpdateAsync(connection, member).ConfigureAwait(false);
    }

    public async Task<int> DeleteMemberAsync(Member member)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.DeleteAsync(member).ConfigureAwait(false);
    }

    // ---- Tournaments ------------------------------------------------------

    public async Task<List<Tournament>> GetTournamentsAsync(int clubId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<Tournament>().Where(t => t.ClubId == clubId)
            .OrderByDescending(t => t.StartDate).ToListAsync().ConfigureAwait(false);
    }

    public async Task<Tournament?> GetTournamentAsync(int tournamentId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<Tournament>().Where(t => t.Id == tournamentId).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<int> SaveTournamentAsync(Tournament tournament)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return tournament.Id == 0 ? await InsertAsync(connection, tournament).ConfigureAwait(false)
                                  : await UpdateAsync(connection, tournament).ConfigureAwait(false);
    }

    public async Task<int> DeleteTournamentAsync(Tournament tournament)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.DeleteAsync(tournament).ConfigureAwait(false);
    }

    // ---- Tournament entries -----------------------------------------------

    public async Task<List<TournamentEntry>> GetTournamentEntriesAsync(int tournamentId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<TournamentEntry>()
            .Where(e => e.TournamentId == tournamentId)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<int> SaveTournamentEntryAsync(TournamentEntry entry)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return entry.Id == 0 ? await InsertAsync(connection, entry).ConfigureAwait(false)
                             : await UpdateAsync(connection, entry).ConfigureAwait(false);
    }

    public async Task<int> DeleteTournamentEntryAsync(TournamentEntry entry)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.DeleteAsync(entry).ConfigureAwait(false);
    }

    // ---- Training sessions ------------------------------------------------

    public async Task<List<TrainingSession>> GetTrainingSessionsAsync(int clubId, DateTime? from = null, DateTime? to = null)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        var query = connection.Table<TrainingSession>().Where(s => s.ClubId == clubId);
        if (from.HasValue)
        {
            query = query.Where(s => s.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.Date <= to.Value);
        }

        return await query.OrderByDescending(s => s.Date).ToListAsync().ConfigureAwait(false);
    }

    public async Task<TrainingSession?> GetTrainingSessionAsync(int sessionId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<TrainingSession>().Where(s => s.Id == sessionId).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<int> SaveTrainingSessionAsync(TrainingSession session, IReadOnlyList<TrainingBlock>? blocks = null)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);

        if (blocks is null)
        {
            return session.Id == 0 ? await InsertAsync(connection, session).ConfigureAwait(false)
                                   : await UpdateAsync(connection, session).ConfigureAwait(false);
        }

        await connection.RunInTransactionAsync(tran =>
        {
            if (session.Id == 0)
            {
                tran.Insert(session);
            }
            else
            {
                tran.Update(session);
            }

            tran.Execute("DELETE FROM TrainingBlocks WHERE TrainingSessionId = ?;", session.Id);
            foreach (var block in blocks)
            {
                block.TrainingSessionId = session.Id;
                tran.Insert(block);
            }
        }).ConfigureAwait(false);

        return session.Id;
    }

    public async Task<int> DeleteTrainingSessionAsync(TrainingSession session)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);

        await connection.RunInTransactionAsync(tran =>
        {
            tran.Execute("DELETE FROM TrainingBlocks WHERE TrainingSessionId = ?;", session.Id);
            tran.Execute("DELETE FROM SessionAttendance WHERE TrainingSessionId = ?;", session.Id);
            tran.Delete(session);
        }).ConfigureAwait(false);

        return session.Id;
    }

    public async Task<List<TrainingBlock>> GetTrainingBlocksAsync(int sessionId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<TrainingBlock>()
            .Where(b => b.TrainingSessionId == sessionId)
            .OrderBy(b => b.Sequence)
            .ToListAsync().ConfigureAwait(false);
    }

    // ---- Attendance -------------------------------------------------------

    public async Task<List<SessionAttendance>> GetSessionAttendanceAsync(int sessionId)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.Table<SessionAttendance>()
            .Where(a => a.TrainingSessionId == sessionId)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<int> SaveSessionAttendanceAsync(SessionAttendance attendance)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);

        // Upsert por la pareja única (sesión, miembro): el Modo Tatami alterna
        // Present sobre un único registro por atleta y sesión.
        var existing = await connection.Table<SessionAttendance>()
            .Where(a => a.TrainingSessionId == attendance.TrainingSessionId && a.MemberId == attendance.MemberId)
            .FirstOrDefaultAsync().ConfigureAwait(false);

        if (existing is null)
        {
            attendance.Id = 0;
            return await InsertAsync(connection, attendance).ConfigureAwait(false);
        }

        existing.Present = attendance.Present;
        existing.Observation = attendance.Observation;
        existing.RecordedAt = attendance.RecordedAt;
        await connection.UpdateAsync(existing).ConfigureAwait(false);
        attendance.Id = existing.Id;
        return existing.Id;
    }

    public async Task<int> DeleteSessionAttendanceAsync(SessionAttendance attendance)
    {
        var connection = await _database.GetConnectionAsync().ConfigureAwait(false);
        return await connection.DeleteAsync(attendance).ConfigureAwait(false);
    }

    // ---- Helpers ----------------------------------------------------------

    private static async Task<int> InsertAsync<T>(SQLiteAsyncConnection connection, T entity) where T : new()
    {
        await connection.InsertAsync(entity).ConfigureAwait(false);
        return GetId(entity);
    }

    private static async Task<int> UpdateAsync<T>(SQLiteAsyncConnection connection, T entity) where T : new()
    {
        await connection.UpdateAsync(entity).ConfigureAwait(false);
        return GetId(entity);
    }

    private static int GetId<T>(T entity) => entity switch
    {
        Club club => club.Id,
        Member member => member.Id,
        Tournament tournament => tournament.Id,
        TournamentEntry entry => entry.Id,
        TrainingSession session => session.Id,
        TrainingBlock block => block.Id,
        SessionAttendance attendance => attendance.Id,
        _ => throw new NotSupportedException($"Entidad sin Id conocida: {typeof(T).Name}")
    };
}
