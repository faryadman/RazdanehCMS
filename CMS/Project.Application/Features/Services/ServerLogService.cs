using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.ApiLog;
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
                Operator = Domain.Enums.Operator.Unknown,
                ConnectionStatus = input.ConnectionStatus,
                Ip = input.Ip,
                ServerId = input.ServerId,
                UserId = input.UserId,
                City = input.City,
                Country = input.Country,
                Isp = input.Isp,
                Org = input.Org
            };
            var @operator = await _operatorIdentificationService.GetOperator(input.Isp, input.Operator);
            model.Operator = @operator;
            await _serverLogRepository.Add(model);
        }

        public async Task<List<ServerLogDTO>> ListByServer(int serverId)
        {
            var data = await _serverLogRepository.FindAsync(x => x.ServerId == serverId && x.IsActive == true);

            return _mapper.Map<List<ServerLogDTO>>(data.OrderByDescending(x => x.Id));
        }


        public async Task<List<ServerLog>> List()
        {
            var data = await _serverLogRepository.GetAll();

            return data.ToList();
        }
        public Task<ServerLogStatisticsDTO> GetAllLogsStatistics()
        {
            var logs = _serverLogRepository.GetAllQueryable();

            var data = new ServerLogStatisticsDTO
            {
                AllLogsStatistics = new ServerLogStatistics
                {
                    Count = logs.Count(),
                    FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed),
                    SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful),
                },
                HamraheAvvalLogsStatistics = new ServerLogStatistics
                {
                    Count = logs.Count(y => y.Operator == Operator.HamraheAvval),
                    FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.HamraheAvval),
                    SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.HamraheAvval),
                },
                IrancellLogsStatistics = new ServerLogStatistics
                {
                    Count = logs.Count(y => y.Operator == Operator.Irancell),
                    FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Irancell),
                    SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Irancell),
                },
                UnknownLogsStatistics = new ServerLogStatistics
                {
                    Count = logs.Count(y => y.Operator == Operator.Unknown),
                    FailCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Unknown),
                    SuccessCount = logs.Count(y => y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Unknown),
                }
            };

            return Task.FromResult(data);
        }
        public async Task Delete(ServerLog log)
        {
            await _serverLogRepository.RemoveWithoutSaveChange(log);
            await _serverLogRepository.SaveChangesTask();
        }
        public async Task Delete(int id)
        {
            await _serverLogRepository.Remove(id);
        }
        public async Task DeleteServerLogs(int count = 100000)
        {
            var list = await _serverLogRepository.GetAll();
            if (list.Count <= 0) return;
            foreach (var log in list)
            {
                await _serverLogRepository.RemoveWithoutSaveChange(log);
            }
            await _serverLogRepository.SaveChangesTask();
        }
        public void RestServerLogs()
        {
            DeleteServerLogs().GetAwaiter().GetResult();
        }
        public Task<ApiLogDTO> GetLastLog()
        {
            var query = _serverLogRepository.FindQueryable(x => x.IsActive == true).OrderByDescending(x => x.Id);
            var model = query.LastOrDefault();
            return Task.FromResult(_mapper.Map<ApiLogDTO>(model));
        }
    }
}
