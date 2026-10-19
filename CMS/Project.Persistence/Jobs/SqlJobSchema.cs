#nullable enable
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Project.Application.Jobs;
using Project.Persistence.Migrations;

namespace Project.Persistence.Jobs;

internal static class SqlJobSchema
{
    // The caller owns the configuration transaction and application lock. DDL, migration history,
    // recovery marker and the store operation commit together; concurrent instances use that same lock.
    public static async Task EnsureCreatedAsync(ApplicationDbContext database, CancellationToken cancellationToken)
    {
        try
        {
            var tables = await database.Database.SqlQueryRaw<int>("""
                SELECT (CASE WHEN OBJECT_ID(N'JobSchedules', N'U') IS NOT NULL THEN 1 ELSE 0 END)
                     + (CASE WHEN OBJECT_ID(N'JobExecutions', N'U') IS NOT NULL THEN 2 ELSE 0 END) AS Value
                """).SingleAsync(cancellationToken);
            var hasSchedules = (tables & 1) != 0;
            var hasExecutions = (tables & 2) != 0;
            if (hasSchedules && hasExecutions) return;
            // This bootstrap extends an existing application database; it must never initialize unrelated tables.
            if (await database.Database.SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID(N'Jobs', N'U') IS NOT NULL THEN 1 ELSE 0 END AS Value")
                    .SingleAsync(cancellationToken) == 0)
                throw new JobSchemaNotReadyException();

            var history = database.GetService<IHistoryRepository>();
            var hasHistory = await history.ExistsAsync(cancellationToken);
            var migrationApplied = hasHistory && (await history.GetAppliedMigrationsAsync(cancellationToken))
                .Any(row => row.MigrationId == JobSchemaNotReadyException.MigrationId);
            var lostState = migrationApplied
                || (hasSchedules && await database.JobSchedules.AnyAsync(cancellationToken))
                || (hasExecutions && await database.JobExecutions.AnyAsync(cancellationToken));

            // Use the original, additive migration as the only definition of columns and indexes.
            // Never run the application's historical migrations against a legacy database at runtime.
            var migration = new AddNativeJobScheduling();
            var model = database.GetService<IModelRuntimeInitializer>().Initialize(migration.TargetModel, designTime: true);
            var generator = database.GetService<IMigrationsSqlGenerator>();
            foreach (var table in migration.UpOperations.OfType<CreateTableOperation>())
            {
                if ((table.Name == "JobSchedules" && hasSchedules)
                    || (table.Name == "JobExecutions" && hasExecutions)) continue;
                var operations = new MigrationOperation[] { table }.Concat(migration.UpOperations
                    .OfType<CreateIndexOperation>().Where(index => index.Table == table.Name)).ToArray();
                foreach (var command in generator.Generate(operations, model))
                    await database.Database.ExecuteSqlRawAsync(command.CommandText, cancellationToken);
            }

            // Preserve unknown legacy history. If EF history already exists, record only this migration,
            // so a subsequent EF update does not attempt to recreate these tables.
            if (hasHistory && !migrationApplied)
                await database.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(
                    JobSchemaNotReadyException.MigrationId, model.GetProductVersion()!)), cancellationToken);

            // Missing state after previous use is different from first installation. Recreate the table,
            // but require review before dispatch because the deleted history/queue cannot be recovered.
            if (lostState)
            {
                const string recoverySettings = "{}";
                await database.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO JobExecutions
                        (Id, JobName, ConfigurationId, SettingsJson, WorkerId, [Trigger], ScheduledForUtc,
                         StartedUtc, CompletedUtc, Status, ErrorCode, NeedsReview)
                    VALUES ({Guid.NewGuid()}, {JobCatalog.Domain}, 0, {recoverySettings}, N'schema-bootstrap', N'Recovery',
                            SYSUTCDATETIME(), SYSUTCDATETIME(), SYSUTCDATETIME(), N'Interrupted', N'schema_recreated', 1)
                    """, cancellationToken);
            }
        }
        catch (SqlException exception) when (exception.Number is 229 or 262 or 2760)
        {
            // Do not expose connection details or provider messages through the admin error response.
            throw new JobSchemaNotReadyException();
        }
    }
}
