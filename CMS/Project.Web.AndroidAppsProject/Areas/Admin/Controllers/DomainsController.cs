using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Project.Application.DTOs.Domain;
using Project.Application.Features.Interfaces;
using System.Text;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DomainsController : Controller
    {
        private readonly IDomainService _domainService;
        private readonly ICronJobInfoService _cronJobInfoService;

        public DomainsController(IDomainService domainService, ICronJobInfoService cronJobInfoService)
        {
            _domainService = domainService;
            _cronJobInfoService = cronJobInfoService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List()
        {
            var data = await _domainService.GetAll();
            return Json(data);
        }
        public async Task<IActionResult> CreateDomain(CreateDomainDTO input)
        {
            await _domainService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> DeleteDomain(int id)
        {
            await _domainService.Delete(id);
            return Json(new { status = "1", message = "done successfully" });
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to upload.");
                return BadRequest(ModelState);
            }

            try
            {
                using (StreamReader reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] data = line.Split(',');

                        string domain = data[0];
                        InsertDataIntoDatabase(domain);
                    }
                }

                return Ok("File uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        private const string ConnectionString = "Data Source=168.119.140.221,1433;Initial Catalog=test;Persist Security Info=True;User ID=sa;Password=Admin@123;TrustServerCertificate=True";

        private static void InsertDataIntoDatabase(string domain)
        {
            string query = "INSERT INTO Domains (Domain) VALUES (@Domain)";

            using SqlConnection connection = new SqlConnection(ConnectionString);
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@domain", domain);

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
