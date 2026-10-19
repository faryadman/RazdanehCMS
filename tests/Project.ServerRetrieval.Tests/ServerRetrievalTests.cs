using System.Data;
using System.Data.Common;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Server;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Application.Middlewares;
using Project.Domain.Entities;
using Project.Persistence;
using Project.Web.AndroidAppsProject.Configuration;
using Xunit;

namespace Project.ServerRetrieval.Tests;

[Collection("Server retrieval SQL Server")]
public sealed class ServerRetrievalTests(SqlServerFixture fixture)
{
    private async Task<RetrievalDatabase> CreateDatabaseAsync()
    {
        var database = new RetrievalDatabase(fixture.ConnectionString);
        try
        {
            await database.SeedAsync();
            return database;
        }
        catch
        {
            await database.DisposeAsync();
            throw;
        }
    }

    [Fact]
    public async Task Repository_uses_the_request_context_and_scope_disposal_closes_its_sql_connection()
    {
        await using var database = await CreateDatabaseAsync();
        var services = new ServiceCollection();
        services.ConfigurePersistenceServices(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = database.Context.Database.GetConnectionString()
            }).Build());
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        ApplicationDbContext context;
        DbConnection connection;
        await using (var scope = provider.CreateAsyncScope())
        {
            context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            connection = context.Database.GetDbConnection();
            Assert.Same(context, scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
            await context.Database.OpenConnectionAsync();
            var repository = scope.ServiceProvider.GetRequiredService<IServerRepository>();
            var server = await repository.GetActiveWithGroupAsync(database.Available.Id);
            Assert.Equal(database.NormalGroup.Title, server!.Group.Title);
            Assert.Equal(ConnectionState.Open, connection.State);
            Assert.Empty(context.ChangeTracker.Entries());
        }
        Assert.Equal(ConnectionState.Closed, connection.State);
        Assert.Throws<ObjectDisposedException>(() => context.Model);
    }

    [Fact]
    public async Task List_returns_complete_fields_group_and_active_log_statistics_in_one_query()
    {
        await using var db = await CreateDatabaseAsync();
        var servers = await db.Servers.GetWithFilter(null, null, false, 0);
        Assert.Equal(new[] { db.Available.Id, db.Unavailable.Id, db.OtherGroupServer.Id }, servers.Select(server => server.Id));
        var server = servers[0];
        AssertComplete(server, db.Available);
        Assert.Equal(db.NormalGroup.Title, server.Group.Title);
        Assert.Equal(3, server.AllLogsStatistics.Count);
        Assert.Equal(1, server.AllLogsStatistics.SuccessCount);
        Assert.Equal(2, server.AllLogsStatistics.FailCount);
        Assert.Equal(1, server.IrancellLogsStatistics.SuccessCount);
        Assert.Equal(1, server.HamraheAvvalLogsStatistics.FailCount);
        Assert.Equal(1, server.UnknownLogsStatistics.FailCount);
        Assert.Null(servers[1].AllLogsStatistics);
        Assert.Equal(1, db.Queries.Reads);
        Assert.Empty(db.Context.ChangeTracker.Entries<Server>());
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    public async Task Group_and_availability_filters_keep_their_existing_meaning(int filter, int count)
    {
        await using var db = await CreateDatabaseAsync();
        var servers = await db.Servers.GetWithFilter(db.NormalGroup.Id, null, false, filter);
        Assert.Equal(count, servers.Count);
        Assert.All(servers, server => Assert.Equal(db.NormalGroup.Id, server.GroupId));
        if (filter == 1) Assert.All(servers, server => Assert.True(server.IsAvailable));
        if (filter == 2) Assert.All(servers, server => Assert.False(server.IsAvailable));
    }

    [Fact]
    public async Task App_group_filter_and_advertisement_filter_are_applied_in_SQL()
    {
        await using var db = await CreateDatabaseAsync();
        var normal = await db.Servers.GetWithFilter(null, db.App.Id, false, 0);
        Assert.Equal(new[] { db.Available.Id, db.Unavailable.Id, db.OtherGroupServer.Id }, normal.Select(server => server.Id));
        var ads = await db.Servers.GetWithFilter(null, null, true, 0);
        Assert.Equal(db.Advertisement.Id, Assert.Single(ads).Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Detail_loads_the_group_from_a_fresh_untracked_query(bool stringId)
    {
        await using var db = await CreateDatabaseAsync();
        var server = stringId ? await db.Servers.Detail("\u200d" + db.Available.Id) : await db.Servers.Detail(db.Available.Id);
        AssertComplete(server, db.Available);
        Assert.NotNull(server.Group);
        Assert.Equal(db.NormalGroup.Title, server.Group.Title);
        Assert.Equal(1, db.Queries.Reads);
        Assert.Empty(db.Context.ChangeTracker.Entries<Server>());
    }

    [Fact]
    public async Task Inactive_and_missing_servers_remain_not_found()
    {
        await using var db = await CreateDatabaseAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => db.Servers.Detail(db.Inactive.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => db.Servers.Detail(int.MaxValue));
        await Assert.ThrowsAsync<NotFoundException>(() => db.Servers.Detail("not-an-id"));
    }

    [Fact]
    public async Task Statistics_returns_complete_fields_and_preserves_null_for_missing_servers()
    {
        await using var db = await CreateDatabaseAsync();
        var server = await db.Servers.GetServerStatistics(db.Available.Id);
        AssertComplete(server, db.Available);
        Assert.Equal(3, server.AllLogsStatistics.Count);
        Assert.Null(await db.Servers.GetServerStatistics(db.Inactive.Id));
        Assert.Null(await db.Servers.GetServerStatistics(int.MaxValue));
    }

    [Fact]
    public async Task Missing_app_filter_returns_a_controlled_bad_request_error()
    {
        await using var db = await CreateDatabaseAsync();
        await Assert.ThrowsAsync<BadRequestException>(() => db.Servers.GetWithFilter(null, int.MaxValue, false, 0));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task App_without_groups_returns_an_empty_list(string? groups)
    {
        await using var db = await CreateDatabaseAsync();
        var app = await db.Context.AppSettings.FindAsync(db.App.Id);
        app!.GroupsThatAppIsJoinedIn = groups!;
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        Assert.Empty(await db.Servers.GetWithFilter(null, db.App.Id, false, 0));
    }

    [Fact]
    public async Task A_group_with_no_matching_servers_returns_an_empty_list()
    {
        await using var db = await CreateDatabaseAsync();
        Assert.Empty(await db.Servers.GetWithFilter(int.MaxValue, null, false, 0));
    }

    [Fact]
    public async Task Admin_list_and_detail_return_json_using_the_real_repository_and_SQL_Server()
    {
        await using var db = await CreateDatabaseAsync();
        using var host = new HostBuilder().ConfigureWebHost(web => web.UseTestServer()
            .ConfigureServices(services =>
            {
                services.AddLegacyAndroidMvc().AddApplicationPart(typeof(Project.Web.AndroidAppsProject.Controllers.ApiController).Assembly);
                services.AddSingleton<IServerService>(db.Servers);
                services.AddSingleton(Substitute.For<IBlackListService>());
                services.AddSingleton(Substitute.For<IServerLogService>());
                services.AddSingleton(Substitute.For<IDomainService>());
                services.AddMemoryCache();
            })
            .Configure(app =>
            {
                app.UseMiddleware<ExceptionMiddleware>();
                app.UseMvc(routes => routes.MapRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}"));
            })).Start();
        using var client = host.GetTestClient();
        using var listResponse = await client.GetAsync($"/admin/servers/list?groupId={db.NormalGroup.Id}&isAd=false&filter=0");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = JArray.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Equal(2, list.Count);
        Assert.Equal(db.Available.Config, (string?)list[0]["config"]);
        Assert.Equal(db.NormalGroup.Title, (string?)list[0]["group"]?["title"]);
        using var detailResponse = await client.GetAsync($"/admin/servers/detail?id={db.Available.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = JObject.Parse(await detailResponse.Content.ReadAsStringAsync());
        Assert.Equal(db.Available.ConfigKey, (string?)detail["configKey"]);
        Assert.Equal(db.NormalGroup.Title, (string?)detail["group"]?["title"]);
        using var missingAppResponse = await client.GetAsync($"/admin/servers/list?appId={int.MaxValue}&isAd=false&filter=0");
        Assert.Equal(HttpStatusCode.BadRequest, missingAppResponse.StatusCode);
        var error = JObject.Parse(await missingAppResponse.Content.ReadAsStringAsync());
        Assert.Equal(3, (int)error["status"]!);
        Assert.Equal(JTokenType.Array, error["errors"]!.Type);
    }

    private static void AssertComplete(ServerDTO actual, Server expected)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.ServerName, actual.ServerName);
        Assert.Equal(expected.Config, actual.Config);
        Assert.Equal(expected.ConfigKey, actual.ConfigKey);
        Assert.Equal(expected.ConfigValue, actual.ConfigValue);
        Assert.Equal(expected.ZoneId, actual.ZoneId);
        Assert.Equal(expected.ZoneIdLastSynced, actual.ZoneIdLastSynced);
        Assert.Equal(expected.DomainDateTime, actual.DomainDateTime);
        Assert.Equal(expected.CurrentDomainValue, actual.CurrentDomainValue);
    }
}
