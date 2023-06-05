using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Features.Interfaces;
using Project.Persistence;

namespace Project.Web.AndroidAppsProject.Controllers
{
    public class LogDeleterController : Controller
    {
        private readonly IApiLogService _apiLogService;
        private readonly IServerLogService _serverLogService;
        private readonly ApplicationDbContext _context;

        public LogDeleterController(ApplicationDbContext context, IApiLogService apiLogService, IServerLogService serverLogService)
        {
            _context = context;
            _apiLogService = apiLogService;
            _serverLogService = serverLogService;
        }
        public async Task<IActionResult> Index()
        {
            RecurringJob.AddOrUpdate("deleteApiLogJob", () => Index(), "*/30 * * * *");
            await _apiLogService.DeleteApiLog();
            await _context.SaveChangesAsync();
            return Ok(true);
            //string query = "DELETE FROM ServerLogs";
            //using (SqlConnection? conn = new SqlConnection(_connectionString))
            //{
            //    conn.Open();
            //    await conn.QueryAsync<bool>(query);
            //    conn.Close();
            //}
            //RecurringJob.AddOrUpdate("compressjob", () => Compress(), "*/15 * * * *");
        }
        public async Task<IActionResult> Compress()
        {
            RecurringJob.AddOrUpdate("deleteServerLogsJob", () => Compress(), "*/15 * * * *");
            await _serverLogService.DeleteServerLogs();
            await _context.SaveChangesAsync();
            return Ok(true);
            //IEnumerable<int> rows;
            //string query = "select TOP(100000) id from apilogs order by id desc";
            //using (SqlConnection? conn = new SqlConnection(_connectionString))
            //{
            //    conn.Open();
            //    rows = await conn.QueryAsync<int>(query);
            //    var item = rows.LastOrDefault();
            //    var deleteQuery = "delete from ApiLogs where id < " + item;
            //    await conn.QueryAsync<bool>(deleteQuery);
            //    conn.Close();
            //}
            //return Ok(true);
        }
    }
}
