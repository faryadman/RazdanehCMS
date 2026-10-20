using System.Data.Common;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Project.Application.DTOs.Server;
using Project.Application.Exceptions;
using Project.Application.Features;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;
using Project.Application.Jobs;
using Project.Application.Profiles;
using Project.Persistence;
using Project.Persistence.Repositories;
using Xunit;
using DomainEntity = Project.Domain.Entities.Domain;
using CnameRecord = Project.Application.Features.CloudflareApiClient.CnameRecord;

namespace Project.ServerRetrieval.Tests;

[Collection("Server retrieval SQL Server")]
public sealed class DomainRetrievalTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task Inactive_list_contains_only_inactive_non_deleted_domains()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("active.invalid", true), new("used.invalid", false), new("deleted.invalid", false, true));
        Assert.Equal("used.invalid", Assert.Single(await db.Service.ListInactiveDomain()).DomainName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(99)]
    public async Task Legacy_list_filters_keep_active_non_deleted_results_and_dto_fields(int filter)
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("active.invalid", true), new("used.invalid", false), new("deleted.invalid", true, true));
        var domain = Assert.Single(await db.Service.GetByFilter(filter));
        Assert.Equal("active.invalid", domain.DomainName);
        Assert.Equal("synthetic-zone", domain.ZoneId);
        Assert.Equal("synthetic.txt", domain.FileName);
        Assert.True(domain.IsActive);
        Assert.False(domain.IsDeleted);
        var sql = Assert.Single(db.Sql.Commands, sql => sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && sql.Contains("FROM [Domains]", StringComparison.Ordinal));
        Assert.Contains("WHERE", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Empty(db.Context.ChangeTracker.Entries());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rotation_skips_blank_domain_names_instead_of_failing_on_the_first_row(string? name)
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput(name, true), new("ready.invalid", true));
        Assert.Equal("انجام شد.", await db.Service.ChangeDomain("7"));
        await db.Cloudflare.Received(1).CreateDnsRecordAsync("synthetic-zone",
            Arg.Is<string>(label => label.Length == 5 && label.All(letter => letter >= 'a' && letter <= 'z')),
            "origin.invalid", Arg.Any<CancellationToken>());
        Assert.False(await db.Context.Domains.Where(domain => domain.DomainName == "ready.invalid").Select(domain => domain.IsActive).SingleAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rotation_selects_one_ready_domain_and_copies_its_zone_to_the_server(bool tcp)
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("pending.invalid", true, Zone: null), new("ready.invalid", true, Zone: "ready-zone"), new("later.invalid", true));
        if (tcp) await db.Service.ChangeTcp("7");
        else await db.Service.ChangeDomain("7");
        await db.Servers.Received(1).UpdateServer(Arg.Is<ServerDTO>(server => server.ZoneId == "ready-zone" && server.Config.Contains("ready.invalid")));
        await db.Cloudflare.Received(1).CreateDnsRecordAsync("ready-zone", Arg.Any<string>(), "origin.invalid", Arg.Any<CancellationToken>());
        Assert.DoesNotContain(db.Cloudflare.ReceivedCalls(), call => call.GetMethodInfo().Name == "GetAllZonesAsync");
        var read = Assert.Single(db.Sql.Commands, sql => sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && sql.Contains("FROM [Domains]", StringComparison.Ordinal));
        Assert.Contains("TOP(1)", read);
        Assert.Contains("ORDER BY", read);
        Assert.Contains("WHERE", read);
    }

    [Fact]
    public async Task Empty_pool_has_a_specific_error_and_never_calls_dns_or_updates_the_server()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("used.invalid", false), new("deleted.invalid", true, true));
        var error = await Assert.ThrowsAsync<NotFoundException>(() => db.Service.ChangeDomain("7"));
        Assert.Contains("دامنهٔ فعال و قابل‌استفاده", error.Message);
        Assert.Empty(db.Cloudflare.ReceivedCalls());
        await db.Servers.DidNotReceive().UpdateServer(Arg.Any<ServerDTO>());
    }

    [Fact]
    public async Task Server_lookup_errors_are_not_replaced_with_domain_status_errors()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("ready.invalid", true));
        var failure = new NotFoundException("خطای ساختگی واکشی سرور");
        db.Servers.Detail("7").Returns(Task.FromException<ServerDTO>(failure));
        Assert.Same(failure, await Assert.ThrowsAsync<NotFoundException>(() => db.Service.ChangeDomain("7")));
        Assert.Empty(db.Cloudflare.ReceivedCalls());
        Assert.True((await db.Context.Domains.AsNoTracking().SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Provider_lookup_errors_are_not_replaced_with_domain_status_errors()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("ready.invalid", true));
        var failure = new NotFoundException("خطای ساختگی واکشی DNS");
        db.Cloudflare.GetCnameRecord(Arg.Any<string>(), Arg.Any<CloudFlare.Client.Enumerators.DnsRecordType>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CnameRecord?>(failure));
        Assert.Same(failure, await Assert.ThrowsAsync<NotFoundException>(() => db.Service.ChangeDomain("7")));
        await db.Servers.DidNotReceive().UpdateServer(Arg.Any<ServerDTO>());
        Assert.True((await db.Context.Domains.AsNoTracking().SingleAsync()).IsActive);
    }

    [Fact]
    public async Task A_missing_zone_is_resolved_before_dns_and_saved_without_losing_domain_fields()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("pending.invalid", true, Zone: null));
        db.Cloudflare.GetAllZonesAsync("pending.invalid", Arg.Any<CancellationToken>())
            .Returns(new List<CnameRecord> { new() { id = "resolved-zone", name = "pending.invalid" } });
        await db.Service.ChangeDomain("7");
        await db.Cloudflare.Received(1).CreateDnsRecordAsync("resolved-zone", Arg.Any<string>(), "origin.invalid", Arg.Any<CancellationToken>());
        var saved = await db.Context.Domains.AsNoTracking().SingleAsync();
        Assert.Equal("resolved-zone", saved.ZoneId);
        Assert.Equal("synthetic.txt", saved.FileName);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task Zone_sync_preserves_a_zone_entered_concurrently_by_an_admin()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("pending.invalid", true, Zone: null));
        db.Cloudflare.GetAllZonesAsync("pending.invalid", Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            await db.Context.Domains.ExecuteUpdateAsync(update => update.SetProperty(domain => domain.ZoneId, "admin-zone"));
            return (IList<CnameRecord>)new List<CnameRecord> { new() { id = "provider-zone", name = "pending.invalid" } };
        });
        await db.Service.InitZoneId();
        Assert.Equal("admin-zone", (await db.Context.Domains.AsNoTracking().SingleAsync()).ZoneId);
    }

    [Fact]
    public async Task Cleanup_uses_one_delete_and_preserves_active_rows()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(Enumerable.Range(0, 20).Select(index => new DomainInput($"used{index}.invalid", false))
            .Append(new("active.invalid", true)).ToArray());
        await db.Service.Remove();
        Assert.Equal("active.invalid", (await db.Context.Domains.AsNoTracking().SingleAsync()).DomainName);
        Assert.Single(db.Sql.Commands, sql => sql.TrimStart().StartsWith("DELETE", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Subdomain_rotation_can_reuse_the_base_domain_consumed_by_domain_rotation()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("ready.invalid", true));
        await db.Service.ChangeDomain("7");
        await db.Service.ChangeSubDomain("7");
        await db.Service.ChangeSubDomain("7");
        await db.Cloudflare.Received(3).CreateDnsRecordAsync("synthetic-zone", Arg.Any<string>(), "origin.invalid", Arg.Any<CancellationToken>());
        Assert.False((await db.Context.Domains.AsNoTracking().SingleAsync()).IsActive);
    }

    [Theory]
    [InlineData("ready.invalid", "ready.invalid")]
    [InlineData("ready.example.invalid", "old.ready.example.invalid")]
    public async Task Subdomain_lookup_matches_the_registered_base_instead_of_assuming_three_labels(string domain, string hostname)
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput(domain, false));
        var server = await db.Servers.Detail("7");
        var config = JObject.Parse(server.Config);
        config.SelectToken("outbounds[0].streamSettings.tlsSettings.serverName")!.Replace(hostname);
        config.SelectToken("outbounds[0].streamSettings.wsSettings.headers.Host")!.Replace(hostname);
        server.Config = config.ToString();
        await db.Service.ChangeSubDomain("7");
        await db.Servers.Received(1).UpdateServer(Arg.Is<ServerDTO>(saved => saved.ZoneId == "synthetic-zone" && saved.Config.Contains(domain)));
    }

    [Fact]
    public async Task A_soft_deleted_base_domain_cannot_be_used_to_rotate_a_subdomain()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("ready.invalid", true, true));
        var server = await db.Servers.Detail("7");
        server.Config = server.Config.Replace("old.invalid", "prefix.ready.invalid");
        await Assert.ThrowsAsync<NotFoundException>(() => db.Service.ChangeSubDomain("7"));
        Assert.Empty(db.Cloudflare.ReceivedCalls());
    }

    [Fact]
    public async Task Subdomain_lookup_chooses_the_longest_registered_suffix_and_rejects_an_unrelated_host()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("invalid", false), new("ready.invalid", false, Zone: "ready-zone"));
        var server = await db.Servers.Detail("7");
        server.Config = server.Config.Replace("old.invalid", "prefix.ready.invalid");
        await db.Service.ChangeSubDomain("7");
        Assert.Equal("ready-zone", server.ZoneId);
        var config = JObject.Parse(server.Config);
        config.SelectToken("outbounds[0].streamSettings.wsSettings.headers.Host")!.Replace("unrelated.invalid");
        server.Config = config.ToString();
        var previousCalls = db.Cloudflare.ReceivedCalls().Count();
        await Assert.ThrowsAsync<BadRequestException>(() => db.Service.ChangeSubDomain("7"));
        Assert.Equal(previousCalls, db.Cloudflare.ReceivedCalls().Count());
    }

    [Fact]
    public async Task Missing_provider_zone_is_reported_before_any_dns_write_or_pool_consumption()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("pending.invalid", true, Zone: null));
        db.Cloudflare.GetAllZonesAsync("pending.invalid", Arg.Any<CancellationToken>()).Returns(Array.Empty<CnameRecord>());
        var error = await Assert.ThrowsAsync<NotFoundException>(() => db.Service.ChangeDomain("7"));
        Assert.Contains("زون", error.Message);
        await db.Cloudflare.DidNotReceive().CreateDnsRecordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await db.Servers.DidNotReceive().UpdateServer(Arg.Any<ServerDTO>());
        Assert.True((await db.Context.Domains.AsNoTracking().SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Zone_sync_does_not_restore_or_modify_a_domain_disabled_during_the_provider_read()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("pending.invalid", true, Zone: null));
        db.Cloudflare.GetAllZonesAsync("pending.invalid", Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            await db.Context.Domains.ExecuteUpdateAsync(update => update.SetProperty(domain => domain.IsActive, false));
            return (IList<CnameRecord>)new List<CnameRecord> { new() { id = "provider-zone", name = "pending.invalid" } };
        });
        await db.Service.InitZoneId();
        var saved = await db.Context.Domains.AsNoTracking().SingleAsync();
        Assert.False(saved.IsActive);
        Assert.Null(saved.ZoneId);
    }

    [Fact]
    public async Task Zone_sync_never_attaches_a_zone_to_a_domain_renamed_during_the_provider_read()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("pending.invalid", true, Zone: null));
        db.Cloudflare.GetAllZonesAsync("pending.invalid", Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            await db.Context.Domains.ExecuteUpdateAsync(update => update.SetProperty(domain => domain.DomainName, "renamed.invalid"));
            return (IList<CnameRecord>)new List<CnameRecord> { new() { id = "old-name-zone", name = "pending.invalid" } };
        });
        await Assert.ThrowsAsync<NotFoundException>(() => db.Service.InitZoneId());
        var saved = await db.Context.Domains.AsNoTracking().SingleAsync();
        Assert.Equal("renamed.invalid", saved.DomainName);
        Assert.Null(saved.ZoneId);
    }

    [Fact]
    public async Task Provider_cancellation_is_preserved_and_never_consumes_a_pool_item()
    {
        await using var db = await DomainDatabase.CreateAsync(fixture.ConnectionString);
        await db.SeedAsync(new DomainInput("ready.invalid", true));
        using var stopping = new CancellationTokenSource();
        db.Execution.CancellationToken = stopping.Token;
        db.Cloudflare.GetCnameRecord(Arg.Any<string>(), Arg.Any<CloudFlare.Client.Enumerators.DnsRecordType>(), stopping.Token)
            .Returns(_ =>
            {
                stopping.Cancel();
                return Task.FromCanceled<CnameRecord?>(stopping.Token);
            });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => db.Service.ChangeDomain("7"));
        await db.Servers.DidNotReceive().UpdateServer(Arg.Any<ServerDTO>());
        Assert.True((await db.Context.Domains.AsNoTracking().SingleAsync()).IsActive);
    }

    private sealed record DomainInput(string? Name, bool Active, bool Deleted = false, string? Zone = "synthetic-zone");

    private sealed class DomainDatabase : IAsyncDisposable
    {
        public ApplicationDbContext Context { get; }
        public DomainService Service { get; }
        public ICloudflareApiClient Cloudflare { get; } = Substitute.For<ICloudflareApiClient>();
        public IServerService Servers { get; } = Substitute.For<IServerService>();
        public DomainCommands Sql { get; } = new();
        public JobExecutionScope Execution { get; } = new();

        private DomainDatabase(string connection)
        {
            var target = new SqlConnectionStringBuilder(connection) { InitialCatalog = "domains_" + Guid.NewGuid().ToString("N") };
            Context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(target.ConnectionString).AddInterceptors(Sql).Options);
            var mapper = new MapperConfiguration(options => options.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
            Service = new DomainService(mapper, new DomainRepository(Context), Servers, Cloudflare, Execution);
            Servers.Detail("7").Returns(new ServerDTO
            {
                Id = 7, ZoneId = "old-zone", CurrentDomainValue = "origin.invalid",
                Config = """{"outbounds":[{"settings":{"vnext":[{"address":"old.invalid"}]},"streamSettings":{"tlsSettings":{"serverName":"old.invalid"},"wsSettings":{"headers":{"Host":"old.invalid"}}}}]}"""
            });
        }

        public static async Task<DomainDatabase> CreateAsync(string connection)
        {
            var database = new DomainDatabase(connection);
            try { await database.Context.Database.EnsureCreatedAsync(); return database; }
            catch { await database.DisposeAsync(); throw; }
        }

        public async Task SeedAsync(params DomainInput[] domains)
        {
            foreach (var domain in domains)
            {
                Context.Domains.Add(new DomainEntity { DomainName = domain.Name!, FileName = "synthetic.txt", ZoneId = domain.Zone!, IsActive = domain.Active, IsDeleted = domain.Deleted });
                await Context.SaveChangesAsync();
            }
            Context.ChangeTracker.Clear();
            Sql.Commands.Clear();
        }

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    private sealed class DomainCommands : DbCommandInterceptor
    {
        public List<string> Commands { get; } = [];
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        { Commands.Add(command.CommandText); return ValueTask.FromResult(result); }
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        { Commands.Add(command.CommandText); return result; }
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        { Commands.Add(command.CommandText); return ValueTask.FromResult(result); }
    }
}
