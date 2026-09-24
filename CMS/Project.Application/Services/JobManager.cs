using Newtonsoft.Json;
using Project.Application.DTOs.Job;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Features.Interfaces;
using Project.Application.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Application.Services
{
    //public sealed class JobManager : IJobManager
    //{
    //    private readonly IJobService _jobService;
    //    private readonly IRecurringJobManager _recurringJobManager;

    //    public JobManager(
    //        IJobService jobService,
    //        IRecurringJobManager recurringJobManager)
    //    {
    //        _jobService = jobService;
    //        _recurringJobManager = recurringJobManager;
    //    }

    //    public async Task ConfigureDomainJobAsync(
    //        CreateDomainJobDTO input,
    //        CancellationToken cancellationToken)
    //    {
    //        Validate(input);

    //        var job = await _jobService.GetByNameAsync(
    //            JobNames.Domain,
    //            cancellationToken);

    //        var jobDto = new CreateJobDTO
    //        {
    //            JobName = JobNames.Domain,
    //            Email = input.Email,
    //            ApiKey = input.ApiKey,
    //            IsActive = input.IsActiveJob,
    //            JobPeriodTime = input.JobPeriodTime,
    //            JobExpireMinuteTime = input.JobExpireMinuteTime,
    //            JobConfig = JsonConvert.SerializeObject(input)
    //        };

    //        if (job is null)
    //        {
    //            await _jobService.CreateJob(
    //                jobDto,
    //                cancellationToken);
    //        }
    //        else
    //        {
    //            await _jobService.UpdateJob(
    //                job.Id,
    //                jobDto,
    //                cancellationToken);
    //        }

    //        if (!input.IsActiveJob)
    //        {
    //            _recurringJobManager.RemoveIfExists(
    //                JobNames.Domain);

    //            return;
    //        }

    //        var cron = $"*/{input.JobPeriodTime} * * * *";

    //        _recurringJobManager.AddOrUpdate<DomainJob>(
    //            JobNames.Domain,
    //            job => job.ExecuteAsync(),
    //            cron);
    //    }

    //    public async Task DisableAsync(
    //        string jobName,
    //        CancellationToken cancellationToken)
    //    {
    //        var job = await _jobService.GetByNameAsync(
    //            jobName,
    //            cancellationToken);

    //        if (job is not null)
    //        {
    //            await _jobService.DisableAsync(
    //                job.Id,
    //                cancellationToken);
    //        }

    //        _recurringJobManager.RemoveIfExists(jobName);
    //    }

    //    public async Task<JobDTO?> GetAsync(
    //        string jobName,
    //        CancellationToken cancellationToken)
    //    {
    //        return await _jobService.GetByNameAsync(
    //            jobName,
    //            cancellationToken);
    //    }

    //    private static void Validate(CreateDomainJobDTO input)
    //    {
    //        if (input.JobPeriodTime <= 0)
    //            throw new ArgumentException(
    //                "Job period must be greater than zero.");

    //        if (input.JobExpireMinuteTime <= 0)
    //            throw new ArgumentException(
    //                "Job expiration time must be greater than zero.");

    //        if (input.FailConnectionPercent < 0 ||
    //            input.FailConnectionPercent > 100)
    //        {
    //            throw new ArgumentException(
    //                "Fail connection percent must be between 0 and 100.");
    //        }
    //    }
    //}
}
