using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.ServerLog;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;

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
                //Operator = input.Operator == "Irancell" ? Domain.Enums.Operator.Irancell : Domain.Enums.Operator.HamraheAvval,
                ConnectionStatus = input.ConnectionStatus,
                Ip = input.Ip,
                ServerId = input.ServerId,
                UserId = input.UserId,
                City = input.City,
                Country = input.Country,
                Isp = input.Isp,
                Org = input.Org
            };

            var Operator = await _operatorIdentificationService.GetOperator(input.Isp, input.Operator);
            //var operatorIdentifications = await _operatorIdentificationService.GetAll();

            //if (!string.IsNullOrWhiteSpace(input.Operator))
            //{
            //    var operatorIdentification = operatorIdentifications.FirstOrDefault(x => !x.IsIsp && x.Text.Equals(input.Operator));
            //    if (operatorIdentification != null)
            //    {
            //        model.Operator = operatorIdentification.Operator;
            //    }
            //}

            //if (!string.IsNullOrWhiteSpace(input.Isp))
            //{
            //    var operatorIdentification = operatorIdentifications.FirstOrDefault(x => x.IsIsp && x.Text.Equals(input.Isp));
            //    if (operatorIdentification != null)
            //    {
            //        model.Operator = operatorIdentification.Operator;
            //    }
            //}
            model.Operator = Operator;
            await _serverLogRepository.Add(model);
        }

        public async Task<List<ServerLogDTO>> ListByServer(int serverId)
        {
            var data = await _serverLogRepository.FindAsync(x => x.ServerId == serverId);

            return _mapper.Map<List<ServerLogDTO>>(data.OrderByDescending(x => x.Id));
        }

        public async Task<List<ServerLogDTO>> List()
        {
            var data = await _serverLogRepository.GetAll();

            return _mapper.Map<List<ServerLogDTO>>(data.OrderByDescending(x => x.Id));
        }
        public async Task<ServerLogStatisticsDTO> GetAllLogsStatistics()
        {
            var logs = await _serverLogRepository.GetAll();

            var data = new ServerLogStatisticsDTO();
            data.AllLogsStatistics = logs.Count() != 0 ? new ServerLogStatistics
            {
                Count = logs.Count(),
                FailCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed).Count(),
                SuccessCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful).Count(),
            } : null;

            data.HamraheAvvalLogsStatistics = logs.Where(y => y.Operator == Domain.Enums.Operator.HamraheAvval).Count() != 0 ? new ServerLogStatistics
            {
                Count = logs.Where(y => y.Operator == Domain.Enums.Operator.HamraheAvval).Count(),
                FailCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed && y.Operator == Domain.Enums.Operator.HamraheAvval).Count(),
                SuccessCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful && y.Operator == Domain.Enums.Operator.HamraheAvval).Count(),
            } : null;

            data.IrancellLogsStatistics = logs.Where(y => y.Operator == Domain.Enums.Operator.Irancell).Count() != 0 ? new ServerLogStatistics
            {
                Count = logs.Where(y => y.Operator == Domain.Enums.Operator.Irancell).Count(),
                FailCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed && y.Operator == Domain.Enums.Operator.Irancell).Count(),
                SuccessCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful && y.Operator == Domain.Enums.Operator.Irancell).Count(),
            } : null;

            data.UnknownLogsStatistics = logs.Where(y => y.Operator == Domain.Enums.Operator.Unknown).Count() != 0 ? new ServerLogStatistics
            {
                Count = logs.Where(y => y.Operator == Domain.Enums.Operator.Unknown).Count(),
                FailCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed && y.Operator == Domain.Enums.Operator.Unknown).Count(),
                SuccessCount = logs.Where(y => y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful && y.Operator == Domain.Enums.Operator.Unknown).Count(),
            } : null;

            return data;
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
    }
}
