using Project.Application.DTOs.Job;
using Project.Application.DTOs.Job.DomainJob;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Application.Services
{
    public interface IJobManager
    {
        Task ConfigureDomainJobAsync(
            CreateDomainJobDTO input,
            CancellationToken cancellationToken);

        Task DisableAsync(
            string jobName,
            CancellationToken cancellationToken);

        Task<JobDTO?> GetAsync(
            string jobName,
            CancellationToken cancellationToken);
    }
}
