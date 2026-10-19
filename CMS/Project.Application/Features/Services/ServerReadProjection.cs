using System.Linq.Expressions;
using Project.Application.DTOs.Group;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Domain.Entities;
using Project.Domain.Enums;

namespace Project.Application.Features.Services;

internal static class ServerReadProjection
{
    public static readonly Expression<Func<Server, ServerDTO>> WithStatistics = server => new ServerDTO
    {
        Id = server.Id,
        ServerName = server.ServerName,
        Location = server.Location,
        Ip = server.Ip,
        Config = server.Config,
        ConfigKey = server.ConfigKey,
        ConfigValue = server.ConfigValue,
        CurrentDomainValue = server.CurrentDomainValue,
        ZoneId = server.ZoneId,
        ZoneIdLastSynced = server.ZoneIdLastSynced,
        DomainDateTime = server.DomainDateTime,
        UpdatedAt = server.UpdatedAt,
        IsAd = server.IsAd,
        IsNewDomain = server.IsNewDomain,
        IsAvailable = server.IsAvailable,
        IsForIrancell = server.IsForIrancell,
        IsForHamraheAvval = server.IsForHamraheAvval,
        GroupId = server.GroupId,
        Group = server.Group == null ? null : new GroupDTO
        {
            Id = server.Group.Id,
            Title = server.Group.Title,
            IsAd = server.Group.IsAd,
            UpdatedAt = server.Group.UpdatedAt
        },
        AllLogsStatistics = server.Logs.Any(log => log.IsActive) ? new ServerLogStatistics
        {
            Count = server.Logs.Count(log => log.IsActive),
            SuccessCount = server.Logs.Count(log => log.IsActive && log.ConnectionStatus == ConnectionStatus.Successful),
            FailCount = server.Logs.Count(log => log.IsActive && log.ConnectionStatus == ConnectionStatus.Failed)
        } : null,
        IrancellLogsStatistics = server.Logs.Any(log => log.IsActive && log.Operator == Operator.Irancell) ? new ServerLogStatistics
        {
            Count = server.Logs.Count(log => log.IsActive && log.Operator == Operator.Irancell),
            SuccessCount = server.Logs.Count(log => log.IsActive && log.Operator == Operator.Irancell && log.ConnectionStatus == ConnectionStatus.Successful),
            FailCount = server.Logs.Count(log => log.IsActive && log.Operator == Operator.Irancell && log.ConnectionStatus == ConnectionStatus.Failed)
        } : null,
        HamraheAvvalLogsStatistics = server.Logs.Any(log => log.IsActive && log.Operator == Operator.HamraheAvval) ? new ServerLogStatistics
        {
            Count = server.Logs.Count(log => log.IsActive && log.Operator == Operator.HamraheAvval),
            SuccessCount = server.Logs.Count(log => log.IsActive && log.Operator == Operator.HamraheAvval && log.ConnectionStatus == ConnectionStatus.Successful),
            FailCount = server.Logs.Count(log => log.IsActive && log.Operator == Operator.HamraheAvval && log.ConnectionStatus == ConnectionStatus.Failed)
        } : null,
        UnknownLogsStatistics = server.Logs.Any(log => log.IsActive && log.Operator == Operator.Unknown) ? new ServerLogStatistics
        {
            Count = server.Logs.Count(log => log.IsActive && log.Operator == Operator.Unknown),
            SuccessCount = server.Logs.Count(log => log.IsActive && log.Operator == Operator.Unknown && log.ConnectionStatus == ConnectionStatus.Successful),
            FailCount = server.Logs.Count(log => log.IsActive && log.Operator == Operator.Unknown && log.ConnectionStatus == ConnectionStatus.Failed)
        } : null
    };
}
