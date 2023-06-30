using Project.Application.DTOs.Job;

namespace Project.Application.Features.Interfaces
{
    public interface IJobService
    {
        Task<List<JobDTO>> List();
        Task<JobDTO> Detail(int id);
        Task<JobDTO> Detail(string jobName);
        Task<JobDTO> LastDetail();
        Task Delete(int id);
        Task CreateJob(CreateJobDTO input);
    }
}
