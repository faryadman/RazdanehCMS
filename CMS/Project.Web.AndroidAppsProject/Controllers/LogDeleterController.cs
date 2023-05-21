using Microsoft.AspNetCore.Mvc;
using Dapper;
using Microsoft.Data.SqlClient;
using Project.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Project.Web.AndroidAppsProject.Controllers
{
    public class LogDeleterController : Controller
    {
        private readonly string _connectionString;
        public LogDeleterController()
        {
            _connectionString = "Data Source=168.119.140.221,1433;Initial Catalog=database;Persist Security Info=True;User ID=db;Password=6y2w~Kx10;TrustServerCertificate=True";
        }
        public async Task<IActionResult> Index()
        {
            string query = "DELETE FROM ServerLogs";
            using (SqlConnection? conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                await conn.QueryAsync<bool>(query);
                conn.Close();
            }
            return Ok(true);
        }  
        public async Task<IActionResult> Compress()
        {
            
            IEnumerable<int> rows;
            string query = "select TOP(100000) id from apilogs order by id desc";
            using (SqlConnection? conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                rows = await conn.QueryAsync<int>(query);
                var item = rows.LastOrDefault();
                var deleteQuery = "delete from ApiLogs where id < " + item;
                await conn.QueryAsync<bool>(deleteQuery);
                conn.Close();
            }
            return Ok(true);
        }
    }
}
