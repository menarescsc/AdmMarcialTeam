using AdmJudoTeam.Core.Entities;
using SQLite;

namespace AdmJudoTeam.Infrastructure.Data;

/// <summary>
/// Propietaria de la conexión SQLite: crea tablas e índices una única vez,
/// con inicialización asíncrona segura (idempotente y thread-safe).
/// La base vive en FileSystem.AppDataDirectory (almacenamiento local de la app).
/// </summary>
public sealed class JudoDatabase
{
    private const string DatabaseFileName = "admjudoteam.db3";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;
    private bool _initialized;

    private SQLiteAsyncConnection Connection =>
        _connection ??= new SQLiteAsyncConnection(
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName));

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        await InitializeAsync().ConfigureAwait(false);
        return Connection;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            // Nota: con SQLitePCLRaw 3.x + SourceGear.sqlite3 (traídos por
            // sqlite-net-pcl 1.11.x) el provider se auto-inicializa; no hay
            // llamada explícita a batteries (Batteries_V2 es interno en 3.x).

            var connection = Connection;
            await connection.CreateTableAsync<Club>().ConfigureAwait(false);
            await connection.CreateTableAsync<Member>().ConfigureAwait(false);
            await connection.CreateTableAsync<Tournament>().ConfigureAwait(false);
            await connection.CreateTableAsync<TournamentEntry>().ConfigureAwait(false);
            await connection.CreateTableAsync<TrainingSession>().ConfigureAwait(false);
            await connection.CreateTableAsync<TrainingBlock>().ConfigureAwait(false);
            await connection.CreateTableAsync<SessionAttendance>().ConfigureAwait(false);

            // Un registro de asistencia por pareja (sesión, miembro): el modo
            // tatami alterna Present sobre un único registro.
            await connection.ExecuteAsync(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_SessionAttendance_Session_Member " +
                "ON SessionAttendance (TrainingSessionId, MemberId);").ConfigureAwait(false);

            // One-time additive backfill: rows created before multi-club support
            // have no club scope (ClubId = 0). Assign them to the oldest club so
            // existing data stays visible. Idempotent and a safe no-op while the
            // Clubs table is empty; new rows always carry a real ClubId.
            await connection.ExecuteAsync(
                "UPDATE Members SET ClubId = (SELECT MIN(Id) FROM Clubs) " +
                "WHERE ClubId = 0 AND EXISTS (SELECT 1 FROM Clubs);").ConfigureAwait(false);
            await connection.ExecuteAsync(
                "UPDATE Tournaments SET ClubId = (SELECT MIN(Id) FROM Clubs) " +
                "WHERE ClubId = 0 AND EXISTS (SELECT 1 FROM Clubs);").ConfigureAwait(false);
            await connection.ExecuteAsync(
                "UPDATE TrainingSessions SET ClubId = (SELECT MIN(Id) FROM Clubs) " +
                "WHERE ClubId = 0 AND EXISTS (SELECT 1 FROM Clubs);").ConfigureAwait(false);

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
