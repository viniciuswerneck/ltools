using System.Data;
using System.Text;
using LTools.SqlDebugger.Models;
using MySqlConnector;

namespace LTools.SqlDebugger.Services;

public class MySqlQueryWatcher : IDisposable
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private MySqlConnection? _connection;
    private CancellationTokenSource? _cts;
    private DateTime _lastPoll;
    private bool _disposed;
    private bool _wasGeneralLogOn;

    public event Action<SqlQuery>? QueryReceived;
    public event Action<string>? StatusChanged;
    public event Action<string>? ErrorOccurred;

    public bool IsRunning { get; private set; }
    public int TotalQueries { get; private set; }

    public MySqlQueryWatcher(string host, int port, string database, string user, string password)
    {
        _databaseName = database;
        var builder = new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = (uint)port,
            Database = database,
            UserID = user,
            Password = password,
            AllowUserVariables = true,
            DefaultCommandTimeout = 5
        };
        _connectionString = builder.ConnectionString;
    }

    public async Task StartAsync()
    {
        if (IsRunning) return;

        try
        {
            _connection = new MySqlConnection(_connectionString);
            await _connection.OpenAsync();

            _wasGeneralLogOn = await IsGeneralLogOnAsync();
            await ExecuteNonQueryAsync("SET GLOBAL log_output = 'TABLE'");
            await ExecuteNonQueryAsync("SET GLOBAL general_log = ON");

            using var initCmd = _connection.CreateCommand();
            initCmd.CommandText = "SELECT COALESCE(MAX(event_time), '2000-01-01') FROM mysql.general_log";
            var raw = await initCmd.ExecuteScalarAsync();
            _lastPoll = raw switch
            {
                DateTime dt => dt,
                string s when DateTime.TryParse(s, out var dt2) => dt2,
                _ => DateTime.Parse("2000-01-01")
            };

            StatusChanged?.Invoke($"Monitorando. Aguardando queries do projeto {_databaseName}...");
            IsRunning = true;
            _cts = new CancellationTokenSource();
            _ = PollLoopAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"Erro ao conectar: {ex.Message}");
            await RestoreGeneralLogAsync();
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }

    private async Task<bool> IsGeneralLogOnAsync()
    {
        try
        {
            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = "SELECT @@global.general_log";
            var result = await cmd.ExecuteScalarAsync();
            return result is not null && (result.ToString() == "1" || result.ToString() == "ON");
        }
        catch
        {
            return false;
        }
    }

    private async Task RestoreGeneralLogAsync()
    {
        try
        {
            if (_wasGeneralLogOn)
                await ExecuteNonQueryAsync("SET GLOBAL general_log = ON");
            else
                await ExecuteNonQueryAsync("SET GLOBAL general_log = OFF");
        }
        catch { }
    }

    public async Task StopAsync()
    {
        IsRunning = false;
        _cts?.Cancel();

        if (_connection != null)
        {
            await RestoreGeneralLogAsync();
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }

        StatusChanged?.Invoke("Monitoramento parado.");
    }

    private async Task PollLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _connection != null)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    break;

                var queries = await PollNewQueriesAsync();
                foreach (var q in queries)
                {
                    TotalQueries++;
                    QueryReceived?.Invoke(q);
                }

                await Task.Delay(400, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Erro no monitoramento: {ex.Message}");
                break;
            }
        }

        IsRunning = false;
    }

    private async Task<List<SqlQuery>> PollNewQueriesAsync()
    {
        var queries = new List<SqlQuery>();

        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = @"
            SELECT event_time, argument, thread_id
            FROM mysql.general_log
            WHERE command_type IN ('Query', 'Prepare', 'Execute')
              AND event_time > @lastPoll
            ORDER BY event_time";
        cmd.Parameters.AddWithValue("@lastPoll", _lastPoll);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tsRaw = reader.GetValue(0);
            var ts = tsRaw switch
            {
                DateTime dt => dt,
                string s when DateTime.TryParse(s, out var dt2) => dt2,
                _ => DateTime.MinValue
            };
            if (ts == DateTime.MinValue) continue;

            var sqlRaw = reader.GetValue(1);
            var sql = sqlRaw switch
            {
                string s => s,
                byte[] b => Encoding.UTF8.GetString(b),
                _ => sqlRaw?.ToString() ?? ""
            };
            if (IsInternalQuery(sql, _databaseName)) continue;

            if (ts > _lastPoll) _lastPoll = ts;

            queries.Add(new SqlQuery
            {
                Timestamp = ts.Kind == DateTimeKind.Utc ? ts.ToLocalTime() : ts,
                Sql = sql,
                Duration = 0,
                Connection = "MySQL"
            });
        }

        return queries;
    }

    private static bool IsInternalQuery(string sql, string dbName)
    {
        var s = sql.TrimStart();
        if (s.Length == 0) return true;

        var upper = s.ToUpperInvariant();

        if (upper.StartsWith("SET ")) return true;
        if (upper.StartsWith("SELECT @@")) return true;
        if (upper == "SELECT 1") return true;
        if (upper.StartsWith("SHOW ")) return true;
        if (upper == "PING") return true;
        if (upper.StartsWith("FLUSH ")) return true;
        if (upper.StartsWith("SELECT DATABASE()")) return true;
        if (upper.StartsWith("SELECT CURRENT_USER()")) return true;
        if (s.Contains("mysql.general_log", StringComparison.OrdinalIgnoreCase)) return true;

        if (upper.StartsWith("USE "))
        {
            var db = s[3..].Trim().Trim('`', '\'', '"');
            return !db.Equals(dbName, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private async Task ExecuteNonQueryAsync(string sql)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = 5;
        await cmd.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        if (_connection != null)
        {
            try
            {
                using var cmd = _connection.CreateCommand();
                var val = _wasGeneralLogOn ? "ON" : "OFF";
                cmd.CommandText = $"SET GLOBAL general_log = {val}";
                cmd.CommandTimeout = 5;
                cmd.ExecuteNonQuery();
            }
            catch { }
            _connection.Dispose();
            _connection = null;
        }
    }
}
