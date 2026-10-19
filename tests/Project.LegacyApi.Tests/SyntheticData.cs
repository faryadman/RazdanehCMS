using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.BlackList;
using Project.Application.DTOs.Group;
using Project.Application.DTOs.IP;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Domain.Enums;

namespace Project.LegacyApi.Tests;

public static class SyntheticData
{
    public static readonly DateTime Timestamp = new(2026, 10, 18, 8, 0, 0, DateTimeKind.Utc);

    public static AppSettingDTO AppSetting() => new()
    {
        Id = 7, ApiRoute = "sample", AppTitle = "برنامهٔ نمونه",
        InterAC = "ac", InterAD = "ad", InterSP = "sp", InterPreOne = "one", InterPreTwo = null!,
        SplashTimeOut = 1500, IsAdmobActive = true, IsSplashActive = false,
        StoreVersion = "2.3.4", IsUpdateForce = true, ResponseTimeout = 9000, RetryNumber = 3,
        ShareUi = "نمونه", CheckIp = true, Url = "https://example.invalid/app",
        AppId = "sample-app", BannerId = "sample-banner", GroupsThatAppIsJoinedIn = "3_9",
        ServerApiRoute = "/api/GetServer/sample", SendRandomServer = true, IsLogAllowed = false,
        IsAdServerAllowed = true, TimerCounter = 5000000000L, IsGdprAllowed = true,
        IsAcAllowed = false, IsSpAllowed = true, IsTimeToStopAllowed = true, TimeToStop = 60
    };

    public static ServerDTO Server(bool isAd = false) => new()
    {
        Id = isAd ? 43 : 42, UpdatedAt = Timestamp, ServerName = isAd ? "سرور تبلیغی" : "سرور نمونه",
        Location = "نمونه", Ip = "192.0.2.10",
        Config = "{\"outbounds\":[{\"settings\":{\"vnext\":[{\"address\":\"sample.example.invalid\",\"port\":443}]}}]}",
        ConfigKey = "host", ConfigValue = "example.invalid", CurrentDomainValue = "origin.example.invalid",
        ZoneId = null, ZoneIdLastSynced = null, IsAd = isAd, IsNewDomain = true, IsAvailable = true,
        Group = new GroupDTO { Id = 3, Title = "گروه نمونه", IsAd = isAd, UpdatedAt = Timestamp },
        GroupId = 3, IsForIrancell = true, IsForHamraheAvval = false, DomainDateTime = Timestamp,
        AllLogsStatistics = new ServerLogStatistics { Count = 5, SuccessCount = 3, FailCount = 2 },
        IrancellLogsStatistics = null!, HamraheAvvalLogsStatistics = null!, UnknownLogsStatistics = null!
    };

    public static List<ServerLogDTO> Logs() =>
    [
        new() { Id = 12, UpdatedAt = Timestamp, ServerId = 42, UserId = "synthetic-user",
            Ip = "192.0.2.10", Isp = "sample-isp", City = null!, Org = "sample-org", Country = "XX",
            Operator = Operator.Unknown, OperatorName = "Unknown", ConnectionStatus = ConnectionStatus.Failed }
    ];

    public static List<IpDTO> Ips() =>
    [
        new() { Id = 2, UpdatedAt = Timestamp, Ip = "192.0.2.2", Tcp = "443" },
        new() { Id = 1, UpdatedAt = Timestamp, Ip = "2001:db8::1", Tcp = "80" }
    ];

    public static List<BlackListDTO> BlackList() =>
    [new() { Id = 5, UpdatedAt = Timestamp, ServerId = 42, Ip = "192.0.2.99", Server = Server() }];
}
