using Hangfire;
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
        private readonly IJobService _jobService;


        public JobController(IServerService serverService, IDomainService domainService, IJobService jobService)
        {
            _serverService = serverService;
            _domainService = domainService;
            _jobService = jobService;
        }

        public async Task<IActionResult> Index()
        {
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
        public async Task<IActionResult> GetJobHostDomainData()
        {
            var job = await _jobService.Detail("HostDomainJob");
            if (job == null)
            {
                return Json(new CreateJobDTO());
            }
            ViewBag.Email = job.Email ?? "";
            ViewBag.ApiKey = job.ApiKey ?? "";
            ViewBag.JobPeriodTime = job.JobPeriodTime ?? 0;
            ViewBag.IsActiveJob = job.IsActive;
            return Json(job);
        }
        public async Task<IActionResult> GetJobSubdomainData()
        {
            var job = await _jobService.Detail("SubDomainJob");
            if (job == null)
            {
                return Json(new CreateJobDTO());
            }
            ViewBag.Email = job.Email ?? "";
            ViewBag.ApiKey = job.ApiKey ?? "";
            ViewBag.JobPeriodTime = job.JobPeriodTime ?? 0;
            ViewBag.IsActiveJob = job.IsActive;
            return Json(job);
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
                case "HostDomainJob":
                    RecurringJob.AddOrUpdate("HostDomainJob", () => CheckHostDomainJob(), $"*/{input.JobPeriodTime} * * * *");
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
        public async Task<IActionResult> CreateHostDomainJob(CreateDomainJobDTO input)
        {
            //Insert job to db
            await InsertJob(new CreateJobDTO
            {
                ApiKey = input.ApiKey,
                IsActive = input.IsActiveJob,
                JobName = "HostDomainJob",
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
                    await _domainService.ChangeDomain(id.ToString(), jobDto.Email, jobDto.ApiKey);
                }
            }
        }
        public async Task CheckSubDomainJob()
        {
            var serverIds = await _serverService.GetActiveIds();
            var list = await _jobService.List();
            var jobDto = list.OrderByDescending(x => x.Id).LastOrDefault(job => job.JobName == "SubDomainJob");
            if (jobDto == null)
                return;
            foreach (var id in serverIds)
            {
                await _domainService.ChangeSubDomain(id.ToString(), jobDto.Email, jobDto.ApiKey);
            }
        }
        public async Task CheckHostDomainJob()
        {
            var serverIds = await _serverService.GetActiveIds();
            var list = await _jobService.List();
            var jobDto = list.OrderByDescending(x => x.Id).FirstOrDefault(job => job.JobName == "HostDomainJob");
            var domainJobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(jobDto.JobConfig);
            if (jobDto == null)
                return;

            foreach (var id in serverIds)
            {
                await _domainService.GenerateDnsAsync(id.ToString(), domainJobDto?.JobExpireMinuteTime.ToString(), jobDto.Email, jobDto.ApiKey);
            }
        }


        //public async Task<IActionResult> ChangeSubDomain(string id)
        //{
        //    var server = await _serverService.Detail(id);
        //    var config = server.Config;
        //    var jsonObject = JObject.Parse(config);
        //    var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString().Split(".");
        //    var hostString = jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"]?.ToString().Split(".");
        //    var subName = GenerateWordExtention.GenerateWords(4)[0];
        //    var serverName = $"{subName}.{serverNameString?[1]}.{serverNameString?[2]}";
        //    var host = $"{subName}.{hostString?[1]}.{hostString?[2]}";
        //    jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"] = serverName;
        //    jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"] = host;
        //    var updatedJsonString = jsonObject.ToString();
        //    server.Config = updatedJsonString;
        //    await _serverService.Edit(new EditServerDTO()
        //    {
        //        Config = server.Config,
        //        ServerName = server.ServerName,
        //        ConfigValue = server.ConfigValue,
        //        ConfigKey = server.ConfigKey,
        //        Ip = server.Ip,
        //        IsForHamraheAvval = server.IsForHamraheAvval,
        //        IsForIrancell = server.IsForIrancell,
        //        ItemId = server.Id,
        //        Location = server.Location,
        //        CurrentDomainValue = server.CurrentDomainValue,
        //        IsNewDomain = true,
        //        DomainDateTime = DateTime.UtcNow
        //    });
        //    var currentDomainValue = serverNameString;
        //    // Your Cloudflare API credentials
        //    var cfEmail = "hamednadarkhani1993@gmail.com";
        //    var cfApiKey = "c31b2d5ee16f7a7a3d092fc5a5755768cff1a";

        //    // Your Cloudflare zone ID and domain name
        //    var cfZoneId = string.Empty;
        //    var cfDomain = $"{currentDomainValue?[1]}.{currentDomainValue?[2]}";
        //    var auth = new CloudFlareAuth(cfEmail, cfApiKey);
        //    var cfClient = new CloudFlareClient(auth);
        //    var zones = await cfClient.GetAllZonesAsync();
        //    foreach (var zone in zones)
        //    {
        //        if (zone.Name == cfDomain)
        //            cfZoneId = new IdentifierTag(zone.Id);
        //    }
        //    // Get the list of DNS records in the zone
        //    var dnsRecords = await cfClient.GetDnsRecordsAsync(cfZoneId);


        //    var cloudflare = new CloudflareApiClient();
        //    // Create the CNAME record with the new value
        //    await cloudflare.CreateDnsRecordAsync(cfZoneId, serverName, server.CurrentDomainValue, cfApiKey, cfEmail); ;


        //    return Json(new { status = "1", message = "done successfully" });
        //}
        //public async Task<IActionResult> ChangeHostDomain(int serverId, string email, string apiKey)
        //{
        //    var server = await _serverService.Detail(serverId);
        //    var config = server.Config;
        //    var jsonObject = JObject.Parse(config);
        //    var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString().Split(".");
        //    var hostString = jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"]?.ToString().Split(".");
        //    var subServerName = GenerateWordExtention.GenerateWords(6)[0];
        //    var subHostName = GenerateWordExtention.GenerateWords(4)[0];
        //    var serverName = $"{subServerName}.{serverNameString?[1]}.{serverNameString?[2]}";
        //    var host = $"{subHostName}.{hostString?[1]}.{hostString?[2]}";
        //    jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"] = serverName;
        //    jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"] = host;
        //    var updatedJsonString = jsonObject.ToString();
        //    server.Config = updatedJsonString;
        //    await _serverService.Edit(new EditServerDTO()
        //    {
        //        Config = server.Config,
        //        ServerName = server.ServerName,
        //        ConfigValue = server.ConfigValue,
        //        ConfigKey = server.ConfigKey,
        //        Ip = server.Ip,
        //        IsForHamraheAvval = server.IsForHamraheAvval,
        //        IsForIrancell = server.IsForIrancell,
        //        ItemId = server.Id,
        //        Location = server.Location,
        //        CurrentDomainValue = server.CurrentDomainValue,
        //        IsNewDomain = true,
        //        DomainDateTime = DateTime.UtcNow
        //    });
        //    var currentDomainValue = hostString;
        //    // Your Cloudflare API credentials
        //    var cfEmail = email;
        //    var cfApiKey = apiKey;

        //    // Your Cloudflare zone ID and domain name
        //    var cfZoneId = string.Empty;
        //    var cfDomain = $"{currentDomainValue?[1]}.{currentDomainValue?[2]}";
        //    var auth = new CloudFlareAuth(cfEmail, cfApiKey);
        //    var cfClient = new CloudFlareClient(auth);
        //    var zones = await cfClient.GetAllZonesAsync();
        //    foreach (var zone in zones)
        //    {
        //        if (zone.Name == cfDomain)
        //            cfZoneId = new IdentifierTag(zone.Id);
        //    }
        //    // Get the list of DNS records in the zone
        //    var dnsRecords = await cfClient.GetDnsRecordsAsync(cfZoneId);

        //    // Find the CNAME record based on its name

        //    var cloudflare = new CloudflareApiClient();
        //    // Create the CNAME record with the new value
        //    await cloudflare.CreateDnsRecordAsync(cfZoneId, host, server.CurrentDomainValue, cfApiKey, cfEmail);

        //    return Json(new { status = "1", message = "done successfully" });
        //}

    }
}