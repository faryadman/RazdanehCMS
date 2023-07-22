using Dapper;
using Microsoft.Data.SqlClient;
using Project.Domain.Entities;

namespace Project.Web.AndroidAppsProject.CronJob
{
    public class CronJobService : ICronJobService
    {
        private readonly string _connectionString;
        public CronJobService()
        {
            _connectionString = "Data Source=163.172.117.78,1433;Initial Catalog=test;Persist Security Info=True;User ID=sa;Password=Admin@123;TrustServerCertificate=True;encrypt=false";
        }
        //private readonly ApplicationDbContext _context;
        //private readonly IServerLogRepository _serverLogRepository; 

        //public CronJobService(IServerLogRepository serverLogRepository)
        //{
        //    _serverLogRepository = serverLogRepository;
        //}

        //public CronJobService(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        public async Task Reset()
        {
            CronJobInfo cronJobInfo = new CronJobInfo();
            string get = "Select * from CronJobInfos where Id = 1";
            string query = "DELETE FROM ServerLogs";
            string updateCronJobInfo = @"Update CronJobInfos set LastExecutionDate=@Date,ExecutedCount=@ExecutedCount";
            using (SqlConnection? conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                cronJobInfo = await conn.QuerySingleAsync<CronJobInfo>(get);

                int dateDifference = DateTime.UtcNow.Subtract(cronJobInfo.LastExecutionDate).Minutes;
                if (dateDifference % cronJobInfo.Timer == 0)
                {
                    await conn.ExecuteAsync(updateCronJobInfo, new { Date = DateTime.UtcNow, ExecutedCount = cronJobInfo.ExecutedCount + 1 });
                    await conn.QueryAsync<bool>(query);
                }
                conn.Close();
            }
        }
    }
}
