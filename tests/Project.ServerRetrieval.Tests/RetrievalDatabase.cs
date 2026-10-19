using System.Data.Common;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;
using Project.Application.Profiles;
using Project.Domain.Entities;
using Project.Domain.Enums;
using Project.Persistence;
using Project.Persistence.Repositories;

namespace Project.ServerRetrieval.Tests;

public sealed class RetrievalDatabase : IAsyncDisposable
{
    private readonly MemoryCache cache = new(new MemoryCacheOptions());
    public ApplicationDbContext Context { get; }
    public ServerService Servers { get; }
    public QueryCounter Queries { get; } = new();
    public Group NormalGroup { get; private set; } = null!;
    public Server Available { get; private set; } = null!;
    public Server Unavailable { get; private set; } = null!;
    public Server Advertisement { get; private set; } = null!;
    public Server Inactive { get; private set; } = null!;
    public Server OtherGroupServer { get; private set; } = null!;
    public AppSetting App { get; private set; } = null!;

    public RetrievalDatabase(string containerConnectionString)
    {
        // Only the newly created test container can supply this connection; application configuration is not read.
        var connection = new SqlConnectionStringBuilder(containerConnectionString)
        {
            InitialCatalog = "retrieval_" + Guid.NewGuid().ToString("N")
        };
        Context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection.ConnectionString).AddInterceptors(Queries).Options);
        var mapper = new MapperConfiguration(options => options.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
        var groups = new GroupService(new GroupRepository(Context), mapper);
        var apps = new AppSettingService(new AppSettingRepository(Context), mapper, groups, cache);
        Servers = new ServerService(new ServerRepository(Context), mapper, apps,
            Substitute.For<IOperatorIdentificationService>(), cache, Substitute.For<IServerLogService>());
    }

    public async Task SeedAsync()
    {
        await Context.Database.EnsureCreatedAsync();
        NormalGroup = new Group { Title = "گروه نمونه" };
        var adGroup = new Group { Title = "گروه تبلیغ", IsAd = true };
        var otherGroup = new Group { Title = "گروه دوم" };
        Context.Groups.AddRange(NormalGroup, adGroup, otherGroup);
        await Context.SaveChangesAsync();

        Available = CreateServer("سرور موجود", NormalGroup);
        Unavailable = CreateServer("سرور ناموجود", NormalGroup);
        Unavailable.IsAvailable = false;
        Advertisement = CreateServer("سرور تبلیغی", adGroup);
        Advertisement.IsAd = true;
        Inactive = CreateServer("سرور حذف‌شده", NormalGroup);
        Inactive.IsActive = false;
        OtherGroupServer = CreateServer("سرور گروه دوم", otherGroup);
        // Separate saves make the expected ID ordering independent of EF's insert batching.
        foreach (var server in new[] { Available, Unavailable, Advertisement, Inactive, OtherGroupServer })
        {
            Context.Servers.Add(server);
            await Context.SaveChangesAsync();
        }
        App = new AppSetting { ApiRoute = "retrieval-sample", AppTitle = "برنامه نمونه",
            GroupsThatAppIsJoinedIn = $"{NormalGroup.Id}_{otherGroup.Id}" };
        Context.AppSettings.Add(App);
        Context.ServerLogs.AddRange(
            Log(Operator.Irancell, ConnectionStatus.Successful),
            Log(Operator.HamraheAvval, ConnectionStatus.Failed),
            Log(Operator.Unknown, ConnectionStatus.Failed),
            Log(Operator.Unknown, ConnectionStatus.Successful, false));
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        Queries.Reads = 0;
    }

    private static Server CreateServer(string name, Group group) => new()
    {
        ServerName = name, GroupId = group.Id, Ip = "192.0.2.10", Location = "نمونه", IsAvailable = true,
        Config = "{\"outbounds\":[{\"streamSettings\":{\"tlsSettings\":{\"serverName\":\"example.invalid\"}}}]}",
        ConfigKey = "sample-key", ConfigValue = "example.invalid", CurrentDomainValue = "origin.example.invalid",
        ZoneId = "synthetic-zone", ZoneIdLastSynced = new DateTime(2026, 10, 18, 10, 0, 0, DateTimeKind.Utc),
        DomainDateTime = new DateTime(2026, 10, 17, 8, 0, 0, DateTimeKind.Utc), IsForIrancell = true
    };

    private ServerLog Log(Operator @operator, ConnectionStatus status, bool active = true) => new()
    {
        ServerId = Available.Id, UserId = "synthetic-user", Operator = @operator,
        ConnectionStatus = status, IsActive = active
    };

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        cache.Dispose();
    }
}

public sealed class QueryCounter : DbCommandInterceptor
{
    public int Reads;
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Interlocked.Increment(ref Reads);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Reads);
        return ValueTask.FromResult(result);
    }
}
