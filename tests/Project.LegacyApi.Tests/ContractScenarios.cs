namespace Project.LegacyApi.Tests;

public sealed record ContractScenario(string Name, string Method, string Url, string? Body, int StatusCode = 200);

public static class ContractScenarios
{
    public const string LogBody = """
        {"serverId":42,"userId":"synthetic-user","ip":"192.0.2.99","isp":"sample-isp","city":null,"org":"sample-org","country":"XX","operator":"sample-operator","apiRoute":"sample","connectionStatus":1}
        """;
    public const string CreateBody = """
        {"serverName":"sample","location":"sample","ip":"192.0.2.10","config":"{}","currentDomainValue":"origin.example.invalid","groupId":3}
        """;
    public const string EditBody = """
        {"itemId":42,"serverName":"sample","location":"sample","ip":"192.0.2.10","config":"{}","currentDomainValue":"origin.example.invalid"}
        """;

    public static IReadOnlyList<ContractScenario> All { get; } =
    [
        new("general-app", "GET", "/Api/GetGeneralApp/sample/sample-isp/sample-operator", null),
        new("app-setting", "GET", "/Api/AppSetting/sample", null),
        new("server", "GET", "/Api/GetServer/sample/sample-isp/sample-operator", null),
        new("ad-server", "GET", "/Api/GetAdServer/sample/sample-isp/sample-operator", null),
        new("success-log", "POST", "/Api/SuccessServerLog", LogBody),
        new("failed-log", "POST", "/Api/FailedServerLog", LogBody),
        new("failed-server", "POST", "/Api/FailedServer", LogBody),
        new("server-logs", "GET", "/Api/ServerLogs?serverId=42", null),
        new("add-ip", "GET", "/Api/AddIp?tcp=443", null),
        new("add-ip-default", "GET", "/Api/AddIp", null),
        new("list-ip", "GET", "/Api/ListIp", null),
        new("server-list", "GET", "/api/Servers/List?groupId=3&appId=7&isAd=false", null),
        new("server-detail", "GET", "/api/Servers/Detail?id=42", null),
        new("server-create", "POST", "/api/Servers/Create", CreateBody),
        new("server-edit", "POST", "/api/Servers/Edit", EditBody),
        new("server-delete", "POST", "/api/Servers/Delete?id=42", null),
        new("server-delete-get", "GET", "/api/Servers/Delete?id=42", null),
        new("blacklist-create", "POST", "/api/BlackList/Create", """{"serverId":42,"ip":"192.0.2.99"}"""),
        new("blacklist-data", "GET", "/api/BlackList/GetData?serverId=42", null),
        new("missing-app", "GET", "/Api/AppSetting/missing", null, 404),
        new("null-app", "GET", "/Api/AppSetting/empty", null),
        new("unexpected-error", "GET", "/Api/AppSetting/broken", null, 400),
        new("invalid-tcp", "GET", "/Api/AddIp?tcp=invalid", null, 400),
        new("missing-user", "POST", "/Api/SuccessServerLog", """{"serverId":42}""", 400),
        new("invalid-json", "POST", "/Api/FailedServerLog", "{", 400)
    ];
}
