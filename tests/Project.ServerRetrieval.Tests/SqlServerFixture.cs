using System.Security.Cryptography;
using Testcontainers.MsSql;
using Xunit;

namespace Project.ServerRetrieval.Tests;

[CollectionDefinition("Server retrieval SQL Server")]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer container = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server@sha256:cf8f0ab2ebe61feec2eed2629061a07d247f791c2a8ddda74a4ac9f06a291718")
        .WithPassword("tC!" + RandomNumberGenerator.GetHexString(32) + "a1")
        .Build();

    public string ConnectionString => container.GetConnectionString();
    public Task InitializeAsync() => container.StartAsync();
    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
