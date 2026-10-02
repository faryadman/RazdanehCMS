using System.Linq;
using DNTPersianUtils.Core;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.Application.DTOs.Job;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class JobController : Controller
    {

        private readonly IServerService _serverService;
        private readonly IDomainService _domainService;
        private readonly IIPConfigService _configService;
        private readonly IJobService _jobService;

        public JobController(IServerService serverService, IDomainService domainService, IJobService jobService, IIPConfigService configService)
        {
            _serverService = serverService;
            _domainService = domainService;
            _jobService = jobService;
            _configService = configService;
        }

        public async Task<IActionResult> Index()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var recurringJob = connection.GetRecurringJobs();
                IList<HangfireDTO> hangfireDtos = new List<HangfireDTO>();
                foreach (var dto in recurringJob)
                {
                    hangfireDtos.Add(new HangfireDTO()
                    {
                        CreatedAt = dto.CreatedAt.ToFriendlyPersianDateTextify() ?? "نامشخص",
                        Cron = dto.Cron,
                        Id = dto.Id,
                        LastExecTime = dto.LastExecution.ToFriendlyPersianDateTextify() ?? "نامشخص",
                        NextExecTime = dto.NextExecution.ToFriendlyPersianDateTextify() ?? "نامشخص",
                    });
                }
                ViewData["DetailModels"] = hangfireDtos;
            }
            var job = await _jobService.Detail("DomainJob");
            if (job == null)
            {
                return View();
            }
            ViewBag.Email = job.Email ?? "";
            ViewBag.ApiKey = job.ApiKey ?? "";
            ViewBag.JobPeriodTime = job.JobPeriodTime ?? 0;
            ViewBag.IsActiveJob = job.IsActive;
            return View();
        }
        public IActionResult TriggerJobNow(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new { status = "0", message = "داده‌ها ناقص است" });
                }
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var dto = connection.GetRecurringJobs(new[] { id }).FirstOrDefault();
                    if (dto == null || dto.Removed)
                    {
                        return Json(new { status = "0", message = "جاب موردنظر پیدا نشد" });
                    }
                    if (dto.LoadException != null)
                    {
                        return Json(new { status = "0", message = "تعریف جاب قابل بازیابی نیست" });
                    }
                }
                new RecurringJobManager(JobStorage.Current).TriggerJob(id);
                return Json(new { status = "1", message = "جاب در صف اجرا قرار گرفت" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "0", message = ex.Message });
            }
        }

        public IActionResult UpdateJobCron(string id, string cron)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(cron))
                {
                    return Json(new { status = "0", message = "داده‌ها ناقص است" });
                }
                cron = cron.Trim();
                if (cron.Length > 100)
                {
                    return Json(new { status = "0", message = "بازهٔ کرون معتبر نیست" });
                }
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var dto = connection.GetRecurringJobs(new[] { id }).FirstOrDefault();
                    if (dto == null || dto.Removed)
                    {
                        return Json(new { status = "0", message = "جاب موردنظر پیدا نشد" });
                    }
                    if (dto.LoadException != null)
                    {
                        return Json(new { status = "0", message = "تعریف جاب قابل بازیابی نیست" });
                    }
                    try
                    {
                        new RecurringJobManager(JobStorage.Current).AddOrUpdate(id, dto.Job, cron, new RecurringJobOptions());
                    }
                    catch (Exception)
                    {
                        return Json(new { status = "0", message = "بازهٔ کرون معتبر نیست" });
                    }
                }
                return Json(new { status = "1", message = "کرون جاب با موفقیت تغییر کرد" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "0", message = ex.Message });
            }
        }

        public async Task<IActionResult> GetDeleteDnsJobData()
        {
            var job = await _jobService.Detail("DeleteDnsJob");
            if (job == null)
            {
                return Json(new CreateJobDTO());
            }
            var jobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(job.JobConfig) ?? new CreateDomainJobDTO();
            ViewBag.Email = jobDto.Email ?? "";
            ViewBag.ApiKey = jobDto.ApiKey ?? "";
            ViewBag.JobPeriodTime = jobDto.JobPeriodTime ?? 0;
            ViewBag.JobExpireMinuteTime = jobDto.JobExpireMinuteTime ?? 0;
            ViewBag.IsActiveJob = jobDto.IsActiveJob;
            return Json(jobDto);
        }
        public async Task<IActionResult> GetJobSubdomainData()
        {
            var job = await _jobService.Detail("SubDomainJob");
            if (job == null)
            {
                return Json(new CreateJobDTO());
            }
            var jobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(job.JobConfig) ?? new CreateDomainJobDTO();
            ViewBag.Email = job.Email ?? "";
            ViewBag.ApiKey = job.ApiKey ?? "";
            ViewBag.JobPeriodTime = job.JobPeriodTime ?? 0;
            ViewBag.IsActiveJob = jobDto.IsActiveJob;
            return Json(jobDto);
        }
        public async Task<IActionResult> GetJobDomainData()
        {
            var job = await _jobService.Detail("DomainJob");
            if (job == null)
            {
                return Json(new CreateJobDTO());
            }
            var jobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(job.JobConfig) ?? new CreateDomainJobDTO();
            ViewBag.Email = job.Email ?? "";
            ViewBag.ApiKey = job.ApiKey ?? "";
            ViewBag.JobPeriodTime = job.JobPeriodTime ?? 0;
            ViewBag.JobExpireMinuteTime = jobDto.JobExpireMinuteTime ?? 0;
            ViewBag.FailConnectionPercent = jobDto.FailConnectionPercent ?? 0;
            ViewBag.FailConnectionCount = jobDto.FailConnectionCount ?? 0;
            ViewBag.IsActiveJob = job.IsActive;
            return Json(jobDto);
        }
        public async Task<IActionResult> GetJobIpConfigData()
        {
            var job = await _jobService.Detail("IpConfigJob");
            if (job == null)
            {
                return Json(new CreateJobDTO());
            }
            var jobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(job.JobConfig) ?? new CreateDomainJobDTO();
            ViewBag.Email = job.Email ?? "";
            ViewBag.ApiKey = job.ApiKey ?? "";
            ViewBag.JobPeriodTime = job.JobPeriodTime ?? 0;
            ViewBag.JobExpireMinuteTime = jobDto.JobExpireMinuteTime ?? 0;
            ViewBag.FailConnectionPercent = jobDto.FailConnectionPercent ?? 0;
            ViewBag.FailConnectionCount = jobDto.FailConnectionCount ?? 0;
            ViewBag.IsActiveJob = job.IsActive;
            return Json(jobDto);
        }
        private async Task InsertJob(CreateJobDTO input)
        {
            var job = _jobService.List().Result.Find(j => j.JobName == input.JobName)!;
            if (((bool)!input.IsActive)!)
            {
                RecurringJob.RemoveIfExists(input.JobName);
                return;
            }
            if (job != null)
            {
                await _jobService.Delete(job.Id);
            }
            await _jobService.CreateJob(input);
            switch (input.JobName)
            {
                case "DomainJob":
                    RecurringJob.AddOrUpdate("DomainJob", () => CheckDomainJob(), $"*/{input.JobPeriodTime} * * * *");
                    break;
                case "SubDomainJob":
                    RecurringJob.AddOrUpdate("SubDomainJob", () => CheckSubDomainJob(), $"*/{input.JobPeriodTime} * * * *");
                    break;
                case "DeleteDnsJob":
                    RecurringJob.AddOrUpdate("DeleteDnsJob", () => CheckDeleteDnsDomainJob(), $"*/{input.JobPeriodTime} * * * *");
                    break;
                case "IpConfigJob":
                    RecurringJob.AddOrUpdate("IpConfigJob", () => CheckIpConfigJob(), $"*/{input.JobPeriodTime} * * * *");
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(input.JobName), input.JobName);
            }


        }
        public async Task<IActionResult> CreateDomainJob(CreateDomainJobDTO input)
        {
            //Insert job to db
            await InsertJob(new CreateJobDTO
            {
                ApiKey = input.ApiKey,
                IsActive = input.IsActiveJob,
                JobName = "DomainJob",
                JobPeriodTime = input.JobPeriodTime,
                Email = input.Email,
                JobConfig = JsonConvert.SerializeObject(input),
                JobExpireMinuteTime = input.JobExpireMinuteTime
            });
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> CreateDeleteDnsJobJob(CreateDomainJobDTO input)
        {
            //Insert job to db
            await InsertJob(new CreateJobDTO
            {
                ApiKey = input.ApiKey,
                IsActive = input.IsActiveJob,
                JobName = "DeleteDnsJob",
                JobPeriodTime = input.JobPeriodTime,
                Email = input.Email,
                JobConfig = JsonConvert.SerializeObject(input),
                JobExpireMinuteTime = input.JobExpireMinuteTime
            });
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> CreateSubDomainJob(CreateDomainJobDTO input)
        {
            //Insert job to db
            await InsertJob(new CreateJobDTO
            {
                ApiKey = input.ApiKey,
                IsActive = input.IsActiveJob,
                JobName = "SubDomainJob",
                JobPeriodTime = input.JobPeriodTime,
                Email = input.Email,
                JobConfig = JsonConvert.SerializeObject(input),
                JobExpireMinuteTime = input.JobExpireMinuteTime
            });
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> CreateIpConfigJob(CreateDomainJobDTO input)
        {
            //Insert job to db
            await InsertJob(new CreateJobDTO
            {
                ApiKey = input.ApiKey,
                IsActive = input.IsActiveJob,
                JobName = "IpConfigJob",
                JobPeriodTime = input.JobPeriodTime,
                Email = input.Email,
                JobConfig = JsonConvert.SerializeObject(input),
                JobExpireMinuteTime = input.JobExpireMinuteTime
            });
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task CheckDomainJob()
        {
            //TODO: Just Get Active Domain
            var serverIds = await _serverService.GetActiveIds();
            var list = await _jobService.List();

            var jobDto = list.OrderByDescending(x => x.Id).LastOrDefault(job => job.JobName == "DomainJob");
            if (jobDto == null)
                return;

            var domainJobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(jobDto.JobConfig);

            foreach (var id in serverIds)
            {
                var server = await _serverService.GetServerStatistics(id);
                if (server?.AllLogsStatistics == null)
                    continue;
                //TODO: IF Success Result Convert to extention method!
                var totalSuccessConnection = server.AllLogsStatistics.Count;
                var successConnection = server.AllLogsStatistics.SuccessCount;
                var failConnection = server.AllLogsStatistics.FailCount;
                var percentSuccessConnection = (int)Math.Round((double)(100 * successConnection) / totalSuccessConnection);
                var percentFailConnection = (int)Math.Round((double)(100 * failConnection) / totalSuccessConnection);
                //TODO: IF Success Result Convert to extention method
                var start = server.DomainDateTime;
                var now = DateTime.UtcNow;
                var ts = now.Subtract(start);
                if (ts.TotalMinutes > domainJobDto!.JobExpireMinuteTime &&
                    percentFailConnection >= domainJobDto.FailConnectionPercent &&
                    failConnection >= domainJobDto.FailConnectionCount)
                {
                    await _domainService.ChangeDomain(id.ToString());
                }
            }
        }
        public async Task CheckSubDomainJob()
        {
            var list = await _jobService.List();
            var jobDto = list.OrderByDescending(x => x.Id).LastOrDefault(job => job.JobName == "SubDomainJob");
            if (jobDto == null)
                return;
            await _domainService.ChangeSubDomain();

        }
        public async Task CheckDeleteDnsDomainJob()
        {
            var list = await _jobService.List();
            var jobDto = list.OrderByDescending(x => x.Id).LastOrDefault(job => job.JobName == "DeleteDnsJob");
            var domainJobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(jobDto.JobConfig);
            if (jobDto == null)
                return;
            await _domainService.DeleteDnsAsync(expireMinuteOn: domainJobDto?.JobExpireMinuteTime.ToString());
        }
        public async Task CheckIpConfigJob()
        {
            var serverIds = await _serverService.GetActiveIds();
            var list = await _jobService.List();

            var jobDto = list.OrderByDescending(x => x.Id).LastOrDefault(job => job.JobName == "IpConfigJob");
            if (jobDto == null)
                return;

            var domainJobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(jobDto.JobConfig);

            foreach (var id in serverIds)
            {
                var server = await _serverService.GetServerStatistics(id);
                if (server?.AllLogsStatistics == null)
                    continue;
                var start = server.DomainDateTime;
                var now = DateTime.Now;
                var ts = now.Subtract(start);
                if (ts.TotalMinutes > domainJobDto!.JobExpireMinuteTime)
                {
                    await _configService.GenerateUpdateAsync(id.ToString(), jobDto.Email, jobDto.ApiKey);
                }
            }
        }
    }
}