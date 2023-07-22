using Dapper;
using Microsoft.Data.SqlClient;
using Project.Domain.Entities;
using Project.Domain.Enums;

namespace Project.Web.AndroidAppsProject.Dapper
{
    public class DapperQueryService : IDapperQueryService
    {
        private readonly string _connectionString;
        public DapperQueryService()
        {
            _connectionString = "Data Source=163.172.117.78,1433;Initial Catalog=test;Persist Security Info=True;User ID=sa;Password=Admin@123;TrustServerCertificate=True;encrypt=false";
        }

        public async Task<List<Server>> GetServerByApp(string groups, bool isAd, Operator operatorType)
        {
            string query = "select * from servers where isavailable = 1 and isAd=@isAd";

            string addedQuery = " and";
            foreach (var item in groups.Split("_"))
            {
                if (!string.IsNullOrWhiteSpace(item))
                    addedQuery += " groupId =" + item + " or ";
            }

            addedQuery = addedQuery.Substring(0, addedQuery.Length - 4);

            query = query + addedQuery;

            if (operatorType != Domain.Enums.Operator.Unknown)
            {
                if (operatorType == Domain.Enums.Operator.Irancell)
                {
                    query += " and IsForIrancell = 1";
                }
                if (operatorType == Domain.Enums.Operator.HamraheAvval)
                {
                    query += " and IsForHamraheAvval = 1";
                }
            }

            query += " order by id desc";
            IEnumerable<Server> list;
            using (SqlConnection? conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                list = await conn.QueryAsync<Server>(query, new { isAd = isAd });
                conn.Close();
            }

            return list.ToList();

        }
    }
}
