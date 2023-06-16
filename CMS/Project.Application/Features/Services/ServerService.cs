using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.ApiLog;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.Group;
using Project.Application.DTOs.Server;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Features.Services
{
    public class ServerService : IServerService
    {
        private readonly IServerRepository _serverRepository;
        private readonly IAppSettingService _appSettingService;
        private readonly IApiLogService _apiLogService;
        private readonly IMapper _mapper;
        private readonly IOperatorIdentificationService _operatorIdentificationService;

        public ServerService(IServerRepository serverRepository, IMapper mapper, IAppSettingService appSettingService, IApiLogService apiLogService, IOperatorIdentificationService operatorIdentificationService)
        {
            _serverRepository = serverRepository;
            _mapper = mapper;
            _appSettingService = appSettingService;
            _apiLogService = apiLogService;
            _operatorIdentificationService = operatorIdentificationService;
        }

        public async Task<List<ServerDTO>> GetWithFilter(int? groupId, int? appId, bool isAd, int filter = 1)
        {
            var query = _serverRepository.GetAllQueryable();
            query = query.Where(x => x.IsActive && x.IsAd == isAd);

            if (filter != 0)
            {
                if (filter == 1)
                {
                    query = query.Where(x => x.IsAvailable);
                }
                else
                {
                    query = query.Where(x => !x.IsAvailable);
                }
            }

            if (groupId != null)
                query = query.Where(x => x.GroupId == groupId);

            if (appId != null)
            {
                var app = await _appSettingService.Detail(appId.Value);

                var groups = string.IsNullOrWhiteSpace(app.GroupsThatAppIsJoinedIn) ? new string[] { } : app.GroupsThatAppIsJoinedIn.Split("_");
                query = query.Where(x => groups.Contains(x.GroupId.ToString()));
            }

            query = query.Include(x => x.Logs);

            var data = await query.Include(x => x.Group).OrderBy(x => x.Id).Select(x => new ServerDTO
            {
                Config = x.Config,
                Group = _mapper.Map<GroupDTO>(x.Group),
                GroupId = x.GroupId,
                Id = x.Id,
                Ip = x.Ip,
                IsAd = x.IsAd,
                IsNewDomain = x.IsNewDomain,
                Location = x.Location,
                ServerName = x.ServerName,
                UpdatedAt = x.UpdatedAt,
                IsForIrancell = x.IsForIrancell,
                IsForHamraheAvval = x.IsForHamraheAvval,
                IsAvailable = x.IsAvailable,
                CurrentDomainValue = x.CurrentDomainValue,
                AllLogsStatistics = x.Logs.Where(y => y.IsActive).Count() != 0 ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Where(y => y.IsActive).Count(),
                    FailCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed).Count(),
                    SuccessCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful).Count(),
                } : null,

                HamraheAvvalLogsStatistics = x.Logs.Where(y => y.IsActive && y.Operator == Domain.Enums.Operator.HamraheAvval).Count() != 0 ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Where(y => y.IsActive && y.Operator == Domain.Enums.Operator.HamraheAvval).Count(),
                    FailCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed && y.Operator == Domain.Enums.Operator.HamraheAvval).Count(),
                    SuccessCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful && y.Operator == Domain.Enums.Operator.HamraheAvval).Count(),
                } : null,

                IrancellLogsStatistics = x.Logs.Where(y => y.IsActive && y.Operator == Domain.Enums.Operator.Irancell).Count() != 0 ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Where(y => y.IsActive && y.Operator == Domain.Enums.Operator.Irancell).Count(),
                    FailCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed && y.Operator == Domain.Enums.Operator.Irancell).Count(),
                    SuccessCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful && y.Operator == Domain.Enums.Operator.Irancell).Count(),
                } : null,

                UnknownLogsStatistics = x.Logs.Where(y => y.IsActive && y.Operator == Domain.Enums.Operator.Unknown).Count() != 0 ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Where(y => y.IsActive && y.Operator == Domain.Enums.Operator.Unknown).Count(),
                    FailCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Failed && y.Operator == Domain.Enums.Operator.Unknown).Count(),
                    SuccessCount = x.Logs.Where(y => y.IsActive && y.ConnectionStatus == Domain.Enums.ConnectionStatus.Successful && y.Operator == Domain.Enums.Operator.Unknown).Count(),
                } : null,

            }).ToListAsync();

            return _mapper.Map<List<ServerDTO>>(data);
        }

        public async Task Create(CreateServerDTO input)
        {
            var model = _mapper.Map<Server>(input);
            await _serverRepository.Add(model);
        }
        public async Task Edit(EditServerDTO input)
        {
            var model = await _serverRepository.SingleOrDefaultAsync(x => x.Id == input.ItemId);
            model.Ip = input.Ip;
            model.ServerName = input.ServerName;
            model.Config = input.Config;
            model.ConfigKey = input.ConfigKey;
            model.ConfigValue = input.ConfigValue;
            model.Location = input.Location;
            model.IsForHamraheAvval = input.IsForHamraheAvval;
            model.IsForIrancell = input.IsForIrancell;
            model.CurrentDomainValue = input.CurrentDomainValue;
            await _serverRepository.Update(model);
        }
        public async Task Delete(int id)
        {
            await _serverRepository.Delete(id);
        }

        public async Task Duplicate(int id)
        {
            var server = await _serverRepository.SingleOrDefaultAsync(x => x.Id == id);

            var dto = _mapper.Map<CreateServerDTO>(server);

            dto.ServerName = dto.ServerName + $" (Sample Of Id ={id})";

            var model = _mapper.Map<Server>(dto);

            await _serverRepository.Add(model);

        }

        public async Task<ServerDTO> GetByApp(string apiRoute, bool isAd, string isp, string Operator)
        {
            var operatorType = await _operatorIdentificationService.GetOperator(isp, Operator);

            AppSettingDTO app = await _appSettingService.DetailByApiRoute(apiRoute);

            if (string.IsNullOrWhiteSpace(app.GroupsThatAppIsJoinedIn))
                throw new BadRequestException("this app has no server");

            string[] groups = app.GroupsThatAppIsJoinedIn.Split("_");

            IEnumerable<Server> query = await _serverRepository.FindAsync(x =>
                groups.Contains(x.GroupId.ToString())
                && x.IsAd == isAd
                && x.IsAvailable);

            query = query.OrderByDescending(x => x.Id);

            if (operatorType != Domain.Enums.Operator.Unknown)
            {
                if (operatorType == Domain.Enums.Operator.Irancell)
                {
                    query = query.Where(x => x.IsForIrancell).AsQueryable();
                }
                if (operatorType == Domain.Enums.Operator.HamraheAvval)
                {
                    query = query.Where(x => x.IsForHamraheAvval).AsQueryable();
                }
            }

            int dataCount = query.Count();

            if (query == null || dataCount == 0)
                throw new BadRequestException("this app has no server");

            if (dataCount == 1)
            {
                return _mapper.Map<ServerDTO>(query.FirstOrDefault());
            }

            ApiLogDTO lastLog = await _apiLogService.GetLastLog(app.Id);


            Server server = new Server();

            if (app.SendRandomServer)
            {
                int serverNotToReturnId = lastLog == null ? 0 : lastLog.ServerId;
                //int serverNotToReturnId = 0;

                Random random = new Random();

                IEnumerable<Server> allowedServers = query.Where(x => x.Id != serverNotToReturnId);

                int index = random.Next(allowedServers.Count());

                server = allowedServers.ElementAt(index);
            }
            else
            {
                if (lastLog == null)
                {
                    Random random = new Random();
                    int index = random.Next(query.Count());
                    server = query.ElementAt(index);
                }
                else
                {
                    int lastServerIndex = query.Select(x => x.Id).ToList().IndexOf(lastLog.ServerId);

                    if (lastServerIndex == dataCount - 1)
                    {
                        server = query.FirstOrDefault();
                    }
                    else
                    {
                        int index = lastServerIndex == -1 ? 0 : lastServerIndex;
                        server = query.ElementAt(index + 1);
                    }
                }
            }
            //await _apiLogService.Create(new ApiLogDTO
            //{
            //    AppSettingId = app.Id,
            //    ServerId = server.Id
            //});

            ServerDTO dto = _mapper.Map<ServerDTO>(server);
            dto.Config = dto.Config.Replace("@" + dto.ConfigKey, DateTime.Now.Ticks.ToString() + "." + dto.ConfigValue);

            return dto;
        }

        public async Task<ServerDTO> Detail(int id)
        {
            var model = await _serverRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (model == null || !model.IsActive)
                throw new NotFoundException("سرور یافت نشد");

            return _mapper.Map<ServerDTO>(model);
        }

        public async Task DeleteByGroupId(int groupId)
        {
            var data = await _serverRepository.FindAsync(x => x.GroupId == groupId);
            foreach (var item in data)
            {
                item.IsActive = false;
                await _serverRepository.Update(item);
            }
        }

        public async Task<List<int>> GetAllIds()
        {
            var data = await _serverRepository.GetAll();

            return data.Select(x => x.Id).ToList();
        }

        public async Task ToggleIsAvailableInput(int id)
        {
            var model = await _serverRepository.SingleOrDefaultAsync(x => x.Id == id);
            model.IsAvailable = !model.IsAvailable;
            await _serverRepository.Update(model);
        }
    }
}
