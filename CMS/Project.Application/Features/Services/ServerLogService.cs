using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.ServerLog;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using Project.Domain.Enums;

namespace Project.Application.Features.Services
{
    public class ServerLogService : IServerLogService
    {
        private readonly IServerLogRepository _serverLogRepository;
        private readonly IOperatorIdentificationService _operatorIdentificationService;
        private readonly IMapper _mapper;

        public ServerLogService(IServerLogRepository serverLogRepository, IMapper mapper, IOperatorIdentificationService operatorIdentificationService)
        {
            _serverLogRepository = serverLogRepository;
            _mapper = mapper;
            _operatorIdentificationService = operatorIdentificationService;
        }

        public async Task Create(AddServerLogDTO input)
        {
            var model = new ServerLog
            {
                Operator = (Domain.Enums.Operator)input.ConnectionStatus,
                ConnectionStatus = input.ConnectionStatus,
                Ip = input.Ip,
                ServerId = input.ServerId,
                UserId = input.UserId,
                City = input.City,
                Country = input.Country,
                Isp = input.Isp,
                Org = input.Org
            };

            //var Operator = await _operatorIdentificationService.GetOperator(input.Isp, input.Operator);
            //model.Operator = Operator;
            await _serverLogRepository.Add(model);
        }

        public async Task<List<ServerLogDTO>> ListByServer(int serverId)
        {
            var data = await _serverLogRepository.FindAsync(x => x.ServerId == serverId && x.IsActive == true);

            return _mapper.Map<List<ServerLogDTO>>(data.OrderByDescending(x => x.Id));
        }

        public async Task<List<ServerLogDTO>> List()
        {
            var data = await _serverLogRepository.GetAll();

            return _mapper.Map<List<ServerLogDTO>>(data.OrderByDescending(x => x.Id));
        }
        public Task<ServerLogStatisticsDTO> GetAllLogsStatistics()
        {
            var logs = _serverLogRepository.GetAllQueryable();

            var data = new ServerLogStatisticsDTO
            {
                AllLogsStatistics = !logs.Any() ? new ServerLogStatistics
                {
                    Count = logs.Count(),
                    FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed),
                    SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful),
                } : null,
                HamraheAvvalLogsStatistics = logs.All(y => y.Operator != Operator.HamraheAvval)
                    ? new ServerLogStatistics
                    {
                        Count = logs.Count(y => y.Operator == Operator.HamraheAvval),
                        FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.HamraheAvval),
                        SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.HamraheAvval),
                    } : null,
                IrancellLogsStatistics = logs.All(y => y.Operator == Operator.Irancell) ? new ServerLogStatistics
                {
                    Count = logs.Count(y => y.Operator == Operator.Irancell),
                    FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Irancell),
                    SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Irancell),
                } : null,
                UnknownLogsStatistics = logs.All(y => y.Operator == Operator.Unknown) ? new ServerLogStatistics
                {
                    Count = logs.Count(y => y.Operator == Operator.Unknown),
                    FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Unknown),
                    SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Unknown),
                } : null
            };

            return Task.FromResult(data);
        }

        public async Task DeleteServerLogs(int count = 100000)
        {
            var list = await _serverLogRepository.GetAll();
            if (list.Count <= 0) return;
            foreach (var log in list)
            {
                await _serverLogRepository.Remove(log.Id);
            }
        }
        public void RestServerLogs()
        {
            DeleteServerLogs().GetAwaiter().GetResult();
        }
    }
}
