BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    CREATE TABLE [JobExecutions] (
        [Id] uniqueidentifier NOT NULL,
        [JobName] nvarchar(64) NOT NULL,
        [ConfigurationId] int NOT NULL,
        [SettingsJson] nvarchar(max) NOT NULL,
        [WorkerId] nvarchar(64) NOT NULL,
        [Trigger] nvarchar(16) NOT NULL,
        [ScheduledForUtc] datetime2 NOT NULL,
        [StartedUtc] datetime2 NOT NULL,
        [CompletedUtc] datetime2 NULL,
        [Status] nvarchar(24) NOT NULL,
        [ErrorCode] nvarchar(48) NULL,
        [NeedsReview] bit NOT NULL,
        [ReviewedUtc] datetime2 NULL,
        [ReviewedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_JobExecutions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    CREATE TABLE [JobSchedules] (
        [JobName] nvarchar(64) NOT NULL,
        [ConfigurationId] int NOT NULL,
        [ConfigurationHash] nvarchar(64) NOT NULL,
        [Enabled] bit NOT NULL,
        [MinuteStep] int NOT NULL,
        [NextRunUtc] datetime2 NULL,
        [RunRequested] bit NOT NULL,
        [RequestedUtc] datetime2 NULL,
        [LastStartedUtc] datetime2 NULL,
        [LastCompletedUtc] datetime2 NULL,
        [LastStatus] nvarchar(24) NULL,
        [ValidationError] nvarchar(256) NULL,
        [SuccessfulExecutions] int NOT NULL,
        CONSTRAINT [PK_JobSchedules] PRIMARY KEY ([JobName])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    CREATE INDEX [IX_JobExecutions_JobName_StartedUtc] ON [JobExecutions] ([JobName], [StartedUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    CREATE INDEX [IX_JobExecutions_Status_NeedsReview] ON [JobExecutions] ([Status], [NeedsReview]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_JobExecutions_NeedsReview] ON [JobExecutions] ([NeedsReview]) WHERE [NeedsReview] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    CREATE INDEX [IX_JobExecutions_StartedUtc] ON [JobExecutions] ([StartedUtc] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    CREATE INDEX [IX_JobSchedules_Enabled_NextRunUtc] ON [JobSchedules] ([Enabled], [NextRunUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019211502_AddNativeJobScheduling'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019211502_AddNativeJobScheduling', N'10.0.12');
END;

COMMIT;
GO
