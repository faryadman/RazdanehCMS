using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Project.Application.DTOs.IP;
using Project.Application.DTOs.ServerLog;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class LegacyApiContractTests
{
    public static IEnumerable<object[]> Scenarios() => ContractScenarios.All.Select(scenario => new object[] { scenario.Name });

    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task Response_matches_the_pre_refactor_contract(string name)
    {
        var scenario = ContractScenarios.All.Single(candidate => candidate.Name == name);
        using var host = new LegacyApiHost();
        using var request = new HttpRequestMessage(new HttpMethod(scenario.Method), scenario.Url);
        if (scenario.Body != null)
            request.Content = new StringContent(scenario.Body, Encoding.UTF8, "application/json");

        using var response = await host.Client.SendAsync(request);
        Assert.Equal(scenario.StatusCode, (int)response.StatusCode);
        var actual = new JObject
        {
            ["statusCode"] = (int)response.StatusCode,
            ["contentType"] = response.Content.Headers.ContentType?.ToString(),
            ["body"] = JToken.Parse(await response.Content.ReadAsStringAsync())
        };
        AssertSnapshot(scenario.Name, actual);
    }

    [Fact]
    public void All_MVC_routes_methods_and_bindings_are_preserved()
    {
        using var host = new LegacyApiHost();
        AssertSnapshot("mvc-actions", ContractMetadata.Actions(host.Services));
    }

    [Fact]
    public void All_legacy_DTO_property_names_types_and_nullability_are_preserved()
    {
        using var host = new LegacyApiHost();
        AssertSnapshot("dto-schemas", ContractMetadata.Schemas(host.Services));
    }

    [Fact]
    public void MVC_serialization_filters_and_view_options_are_preserved()
    {
        using var host = new LegacyApiHost();
        AssertSnapshot("mvc-options", ContractMetadata.Options(host.Services));
    }

    [Fact]
    public async Task General_app_passes_the_legacy_route_arguments_and_ad_flags_to_services()
    {
        using var host = new LegacyApiHost();
        using var response = await host.Client.GetAsync("/Api/GetGeneralApp/sample/sample-isp/sample-operator");
        response.EnsureSuccessStatusCode();
        await host.AppSettings.Received(1).DetailByApiRoute("sample");
        await host.Servers.Received(1).GetByApp("sample", false, "sample-isp", "sample-operator");
        await host.Servers.Received(1).GetByApp("sample", true, "sample-isp", "sample-operator");
    }

    [Fact]
    public async Task Server_list_preserves_the_interface_default_filter()
    {
        using var host = new LegacyApiHost();
        using var response = await host.Client.GetAsync("/api/Servers/List?groupId=3&appId=7&isAd=true");
        response.EnsureSuccessStatusCode();
        await host.Servers.Received(1).GetWithFilter(3, 7, true, 0);
    }

    [Fact]
    public async Task Add_ip_preserves_the_query_remote_address_and_user_agent()
    {
        using var host = new LegacyApiHost();
        using var response = await host.Client.GetAsync("/Api/AddIp?tcp=443");
        response.EnsureSuccessStatusCode();
        await host.Ips.Received(1).Insert(Arg.Is<CreateIpDTO>(input =>
            input.Tcp == "443" && input.Ip == "192.0.2.20" && input.UserAgent == "LegacyContractTests/1.0"));
    }

    [Fact]
    public async Task Invalid_log_body_never_reaches_the_write_service()
    {
        using var host = new LegacyApiHost();
        using var body = new StringContent("{}", Encoding.UTF8, "application/json");
        using var response = await host.Client.PostAsync("/Api/SuccessServerLog", body);
        Assert.Equal(400, (int)response.StatusCode);
        await host.Servers.DidNotReceive().SuccessServerLog(Arg.Any<AddServerLogDTO>());
    }

    private static void AssertSnapshot(string name, JToken actual)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Snapshots", name + ".json");
        Assert.True(File.Exists(path), $"فایل قرارداد پیدا نشد: {name}.json");
        var expected = JToken.Parse(File.ReadAllText(path));
        // JSON whitespace is irrelevant; property order, array order, casing, types and nulls remain checked.
        Assert.Equal(expected.ToString(Formatting.None), actual.ToString(Formatting.None));
    }
}
