using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.OperatorIdentification;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Services
{
    public class OperatorIdentificationService : IOperatorIdentificationService
    {
        private readonly IOperatorIdentificationRepository _operatorIdentificationRepository;
        private readonly IMapper _mapper;

        public OperatorIdentificationService(IOperatorIdentificationRepository operatorIdentificationRepository, IMapper mapper)
        {
            _operatorIdentificationRepository = operatorIdentificationRepository;
            _mapper = mapper;
        }

        public async Task Create(CreateOperatorIdentificationDTO input)
        {
            var model = _mapper.Map<OperatorIdentification>(input);
            await _operatorIdentificationRepository.Add(model);
        }

        public async Task Delete(int id)
        {
            await _operatorIdentificationRepository.Delete(id);
        }

        public async Task<List<OperatorIdentificationDTO>> GetAll(bool isIsp)
        {
            var data = await _operatorIdentificationRepository.FindAsync(x => x.IsIsp == isIsp);

            return _mapper.Map<List<OperatorIdentificationDTO>>(data.OrderByDescending(x=>x.Id));
        }

        public async Task<List<OperatorIdentificationDTO>> GetAll()
        {
            var data = await _operatorIdentificationRepository.GetAll();
            return _mapper.Map<List<OperatorIdentificationDTO>>(data.OrderByDescending(x => x.Id));
        }

        public async Task<Operator> GetOperator(string isp, string Operator)
        {
            Operator result = Domain.Enums.Operator.Unknown;
            var operatorIdentifications = await GetAll();

            if (!string.IsNullOrWhiteSpace(Operator))
            {
                var operatorIdentification = operatorIdentifications.FirstOrDefault(x => !x.IsIsp && x.Text.Equals(Operator));
                if (operatorIdentification != null)
                {
                    result = operatorIdentification.Operator;
                }
            }

            if (!string.IsNullOrWhiteSpace(isp))
            {
                var operatorIdentification = operatorIdentifications.FirstOrDefault(x => x.IsIsp && x.Text.Equals(isp));
                if (operatorIdentification != null)
                {
                    result = operatorIdentification.Operator;
                }
            }

            return result;
        }
    }
}
