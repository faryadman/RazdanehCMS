#nullable enable
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Project.Application.Jobs;
using Project.Domain.Entities;

namespace Project.Persistence.Jobs;

public sealed class SqlJobExecutionLock(IConfiguration configuration) : IJobExecutionLock
{
    private const string Resource = "RazdanehCMS:jobs:execution:v1";

    public async Task<IJobLease?> TryAcquireAsync(CancellationToken cancellationToken)
    {
        // A dedicated, non-pooled session owns the lock until the handler AND its scope have finished.
        var connectionString = new SqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"))
            { Pooling = false, ConnectRetryCount = 0 };
        var connection = new SqlConnection(connectionString.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = """
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0;
                SELECT @result;
                """;
            command.Parameters.AddWithValue("@resource", Resource);
            if (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) < 0)
            {
                await connection.DisposeAsync();
                return null;
            }
            return new Lease(connection);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private sealed class Lease : IJobLease
    {
        private readonly SqlConnection connection;
        private readonly CancellationTokenSource stopping = new();
        private readonly CancellationTokenSource executionCancelled = new();
        private readonly Task monitor;
        private JobClaim? claim;
        public bool ConnectionLost { get; private set; }
        public bool ConfigurationChanged { get; private set; }
        public CancellationToken CancellationToken => executionCancelled.Token;

        public Lease(SqlConnection connection)
        {
            this.connection = connection;
            monitor = MonitorAsync();
        }

        public void WatchConfiguration(JobClaim execution) => Volatile.Write(ref claim, execution);

        private async Task MonitorAsync()
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
                while (await timer.WaitForNextTickAsync(stopping.Token))
                {
                    await using var command = connection.CreateCommand();
                    command.CommandTimeout = 5;
                    command.CommandText = "SELECT APPLOCK_MODE('public', @resource, 'Session')";
                    command.Parameters.AddWithValue("@resource", Resource);
                    if (!string.Equals(await command.ExecuteScalarAsync(stopping.Token) as string, "Exclusive", StringComparison.Ordinal))
                        throw new InvalidOperationException();
                    var current = Volatile.Read(ref claim);
                    if (current == null) continue;
                    command.Parameters.Clear();
                    command.CommandText = "SELECT TOP(1) Id, IsActive, JobPeriodTime, JobConfig FROM Jobs WHERE JobName = @name ORDER BY Id DESC";
                    command.Parameters.AddWithValue("@name", current.JobName);
                    await using var reader = await command.ExecuteReaderAsync(stopping.Token);
                    if (!await reader.ReadAsync(stopping.Token))
                    {
                        ConfigurationChanged = true;
                        executionCancelled.Cancel();
                        continue;
                    }
                    var latest = new Job
                    {
                        Id = reader.GetInt32(0), JobName = current.JobName, IsActive = reader.GetBoolean(1),
                        JobPeriodTime = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                        JobConfig = reader.IsDBNull(3) ? null : reader.GetString(3)
                    };
                    // An enabled edit applies to the next execution; the current run keeps its captured settings.
                    // Disabling requests cooperative cancellation without starting a replacement in parallel.
                    if (!latest.IsActive && JobSettings.Fingerprint(latest) != current.ConfigurationHash)
                    {
                        ConfigurationChanged = true;
                        executionCancelled.Cancel();
                    }
                }
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
            catch
            {
                ConnectionLost = true;
                executionCancelled.Cancel();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await stopping.CancelAsync();
            try { await monitor; }
            finally
            {
                // Closing this non-pooled physical session releases its application lock, even on a broken connection.
                try { await connection.DisposeAsync(); }
                finally
                {
                    stopping.Dispose();
                    executionCancelled.Dispose();
                }
            }
        }
    }
}
