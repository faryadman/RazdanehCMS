using Project.Application.DTOs.OperatorIdentification;
using Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface IOperatorIdentificationService
    {
        Task<List<OperatorIdentificationDTO>> GetAll(bool isIsp);
        Task<List<OperatorIdentificationDTO>> GetAll();
        Task Create(CreateOperatorIdentificationDTO input);
        Task Delete(int id);

        Task<Operator> GetOperator(string isp,string Operator);
    }
}
