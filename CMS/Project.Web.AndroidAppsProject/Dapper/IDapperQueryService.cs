using Project.Application.DTOs.Server;
using Project.Domain.Entities;
using Project.Domain.Enums;

namespace Project.Web.AndroidAppsProject.Dapper
{
    public interface IDapperQueryService
    {
        Task<List<Server>> GetServerByApp(string groups, bool isAd, Operator operatorType);
    }
}
