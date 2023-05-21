using Project.Application.DTOs.CronJobInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface ICronJobInfoService
    {
        Task Update(UpdateCronJobInfoDTO input);
        Task<CronJobInfoDTO> Get();
        Task MassDelete();
    }
}
