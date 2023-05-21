using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.CronJobInfo;
using Project.Application.Features.Interfaces;
using System.Data;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CronJobInfoController : Controller
    {
        private readonly ICronJobInfoService _cronJobInfoService;
        private readonly ICronJobInfoRepository _cronJobInfoRepository;
        private readonly string _connectionString;
        public CronJobInfoController(ICronJobInfoService cronJobInfoService, ICronJobInfoRepository cronJobInfoRepository)
        {
            _cronJobInfoService = cronJobInfoService;
            _cronJobInfoRepository = cronJobInfoRepository;
            _connectionString = "Data Source=168.119.140.221,1433;Initial Catalog=database;Persist Security Info=True;User ID=db;Password=6y2w~Kx10;TrustServerCertificate=True";
        }
        public async Task<IActionResult> Get()
        {
            var data = await _cronJobInfoService.Get();
            return Json(data);
        }
        public async Task<IActionResult> Update(UpdateCronJobInfoDTO input)
        {
            await _cronJobInfoService.Update(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        private async Task<IActionResult> Create()
        {
            await _cronJobInfoRepository.Add(new Domain.Entities.CronJobInfo
            {
                ExecutedCount = 0,
                LastExecutionDate = DateTime.Now,
                Timer = 100
            });
            var data = await _cronJobInfoRepository.GetAll();
            return Ok(data);
        }
        public async Task<IActionResult> MassDelete()
        {
            string query = "DELETE FROM ServerLogs";
            using (SqlConnection? conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                await conn.QueryAsync<bool>(query);
                conn.Close();
            }
            return Json(new { status = "1", message = "done successfully" });
        }
    }
}
