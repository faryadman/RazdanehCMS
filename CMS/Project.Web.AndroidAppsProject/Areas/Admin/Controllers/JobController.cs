using CloudFlare.NET;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.DTOs.Job;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.DTOs.Server;
using Project.Application.Extensions;
using Project.Application.Features;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class JobController : Controller
    {

        private readonly IServerService _serverService;
        private readonly IServerLogService _serverLogService;
        private readonly IBlackListService _blackListService;
        private readonly ICronJobInfoService _cronJobInfoService;
        private readonly IDomainService _domainService;
        private readonly IJobService _jobService;


        public JobController(IServerService serverService, IBlackListService blackListService, IServerLogService serverLogService, ICronJobInfoService cronJobInfoService, IDomainService domainService, IJobService jobService)
        {
            _serverService = serverService;
            _blackListService = blackListService;
            _serverLogService = serverLogService;
            _cronJobInfoService = cronJobInfoService;
            _domainService = domainService;
            _jobService = jobService;
        }
        public async Task<IActionResult> Index()
        {
            var list = await _jobService.List();
            if (!list.Any())
            {
                ViewBag.Email = "";
                ViewBag.ApiKey = "";
                ViewBag.JobPeriodTime = "";
                ViewBag.JobExpireMinuteTime = "";
                ViewBag.FailConnectionPercent = "";
                ViewBag.FailConnectionCount = "";
                ViewBag.IsActiveJob = false;
            }
            foreach (var job in list.Where(job => job.JobName == "DomainJob"))
            {
                var jobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO?>(job.JobConfig);
                ViewBag.Email = jobDto.Email;
                ViewBag.ApiKey = jobDto.ApiKey;
                ViewBag.JobPeriodTime = jobDto.JobPeriodTime;
                ViewBag.JobExpireMinuteTime = jobDto.JobExpireMinuteTime;
                ViewBag.FailConnectionPercent = jobDto.FailConnectionPercent;
                ViewBag.FailConnectionCount = jobDto.FailConnectionCount;
                ViewBag.IsActiveJob = jobDto.IsActiveJob;
            }
            return View();
        }

        private async Task InsertJob(CreateJobDTO input)
        {
            var job = _jobService.List().Result.Find(j => j.JobName == input.JobName)!;
            if (job != null)
            {

                await _jobService.Delete(job.Id);
            }

            if ((bool)!input.IsActive)
            {
                RecurringJob.RemoveIfExists(input.JobName);
                return;
            }

            await _jobService.CreateJob(input);
        }

        public async Task CreateDomainJob(CreateDomainJobDTO input)
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
            //create job into hangfire
            RecurringJob.AddOrUpdate("DomainJob", () => CheckDomainJob(), $"*/{input.JobPeriodTime} * * * *");
        }
        public async Task CheckDomainJob()
        {
            //TODO: Just Get Active Domain
            var serverIds = await _serverService.GetActiveIds();
            var list = await _jobService.List();

            var jobDto = list.FirstOrDefault(job => job.JobName == "DomainJob")?.JobConfig;
            if (jobDto == null)
                return;

            var domainJobDto = JsonConvert.DeserializeObject<CreateDomainJobDTO>(jobDto);

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

                if (percentFailConnection >= percentSuccessConnection || percentFailConnection >= domainJobDto.FailConnectionPercent || failConnection >= domainJobDto.FailConnectionCount)
                {
                    //TODO: IF Success Result Convert to extention method
                    DateTime start = server.DomainDateTime;
                    DateTime now = DateTime.UtcNow;
                    TimeSpan ts = now.Subtract(start);
                    if (ts.TotalMinutes > domainJobDto.JobExpireMinuteTime)
                    {
                        await ChangeDomain(id);
                    }
                }
            }
        }
        public async Task<IActionResult> ChangeDomain(int serverId, bool deleteDomain = true)
        {

            ViewBag.ServerId = serverId;
            var domains = await _domainService.GetAll();
            if (domains is not { Count: > 0 }) return Json(new { status = "2", message = "domain don't exist!" });
            var server = await _serverService.Detail(serverId);
            var config = server.Config;
            var currentDomainValue = domains?[0].DomainName;
            var jsonObject = JObject.Parse(config);

            // Your Cloudflare API credentials
            var cfEmail = "hamednadarkhani1993@gmail.com";
            var cfApiKey = "c31b2d5ee16f7a7a3d092fc5a5755768cff1a";

            // Your Cloudflare zone ID and domain name
            var cfZoneId = string.Empty;
            var cfDomain = currentDomainValue;

            // The new CNAME value
            var newCnameValue = !deleteDomain ?
                jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString() : GenerateWordExtention.GenerateWords(5)[0]; // create new word VALUE
            var cnameContent = server.CurrentDomainValue;
            // Set up Cloudflare API client
            var auth = new CloudFlareAuth(cfEmail, cfApiKey);
            var cfClient = new CloudFlareClient(auth);
            var zones = await cfClient.GetAllZonesAsync();
            foreach (var zone in zones)
            {
                if (zone.Name == cfDomain)
                    cfZoneId = new IdentifierTag(zone.Id);

            }
            // Get the list of DNS records in the zone
            var dnsRecords = await cfClient.GetDnsRecordsAsync(cfZoneId);

            // Find the CNAME record based on its name
            var cnameRecord = dnsRecords.Result.FirstOrDefault(record => record.Type == DnsRecordType.CNAME);
            var cloudflare = new CloudflareApiClient();
            if (cnameRecord != null)
            {
                // Update the CNAME record with the new value
                await cloudflare.UpdateDnsRecordAsync(cfZoneId, cnameRecord.Id, newCnameValue, cnameContent, cfApiKey, cfEmail);
            }
            else
            {
                // Create the CNAME record with the new value
                await cloudflare.CreateDnsRecordAsync(cfZoneId, newCnameValue, cnameContent, cfApiKey, cfEmail); ;
            }

            var cnameValue = $"{newCnameValue}.{cfDomain}";
            // Change value serverName
            jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"] = cnameValue;

            // Change value Host
            jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"] = cnameValue;

            var updatedJsonString = jsonObject.ToString();
            server.Config = updatedJsonString;

            await _serverService.Edit(new EditServerDTO()
            {
                Config = server.Config,
                ServerName = server.ServerName,
                ConfigValue = server.ConfigValue,
                ConfigKey = server.ConfigKey,
                Ip = server.Ip,
                IsForHamraheAvval = server.IsForHamraheAvval,
                IsForIrancell = server.IsForIrancell,
                ItemId = server.Id,
                Location = server.Location,
                CurrentDomainValue = server.CurrentDomainValue,
                IsNewDomain = true,
                DomainDateTime = DateTime.UtcNow
            });
            if (deleteDomain)
            {
                if (domains != null) await _domainService.Delete(domains[0].Id);
            }
            return RedirectToAction("Index");
        }
        public void Test()
        {

        }
    }
}
