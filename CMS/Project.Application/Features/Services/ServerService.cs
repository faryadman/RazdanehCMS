using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.Group;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using Project.Domain.Enums;

namespace Project.Application.Features.Services
{
    public class ServerService : IServerService
    {
        private readonly IServerRepository _serverRepository;
        private readonly IAppSettingService _appSettingService;
        private readonly IMapper _mapper;
        private readonly IServerLogService _serverLogService;
        private readonly IOperatorIdentificationService _operatorIdentificationService;
        private readonly IMemoryCache _memoryCache;
        private MemoryCacheEntryOptions _cacheEntryOptions;
        public ServerService(IServerRepository serverRepository, IMapper mapper, IAppSettingService appSettingService, IOperatorIdentificationService operatorIdentificationService, IMemoryCache memoryCache, IServerLogService serverLogService)
        {
            _serverRepository = serverRepository;
            _mapper = mapper;
            _appSettingService = appSettingService;
            _operatorIdentificationService = operatorIdentificationService;
            _memoryCache = memoryCache;
            _serverLogService = serverLogService;
            _cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
        }

        public async Task<List<ServerDTO>> GetWithFilter(int? groupId, int? appId, bool isAd, int filter = 1)
        {
            var query = _serverRepository.GetAllQueryable();
            query = query.Where(x => x.IsActive && x.IsAd == isAd && x.IsActive == true);

            if (filter != 0)
            {
                query = filter == 1 ? query.Where(x => x.IsAvailable) : query.Where(x => !x.IsAvailable);
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
                AllLogsStatistics = x.Logs.Any(y => y.IsActive) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful),
                } : null,

                HamraheAvvalLogsStatistics = x.Logs.Any(y => y.IsActive && y.Operator == Operator.HamraheAvval) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive && y.Operator == Operator.HamraheAvval),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.HamraheAvval),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.HamraheAvval),
                } : null,

                IrancellLogsStatistics = x.Logs.Any(y => y.IsActive && y.Operator == Operator.Irancell) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive && y.Operator == Operator.Irancell),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Irancell),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Irancell),
                } : null,

                UnknownLogsStatistics = x.Logs.Any(y => y.IsActive && y.Operator == Operator.Unknown) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive && y.Operator == Operator.Unknown),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Unknown),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Unknown),
                } : null,

            }).ToListAsync();

            return _mapper.Map<List<ServerDTO>>(data);
        }

        public Task<ServerDTO> GetServerStatistics(int serverId)
        {
            var query = _serverRepository.GetAllQueryable();
            query = query.Where(x => x.IsActive && x.Id == serverId && x.IsActive == true);
            query = query.Include(x => x.Logs);
            var data = query.Select(x => new ServerDTO
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
                DomainDateTime = x.DomainDateTime,
                AllLogsStatistics = x.Logs.Any(y => y.IsActive) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful),
                } : null,

                HamraheAvvalLogsStatistics = x.Logs.Any(y => y.IsActive && y.Operator == Operator.HamraheAvval) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive && y.Operator == Operator.HamraheAvval),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.HamraheAvval),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.HamraheAvval),
                } : null,

                IrancellLogsStatistics = x.Logs.Any(y => y.IsActive && y.Operator == Operator.Irancell) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive && y.Operator == Operator.Irancell),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Irancell),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Irancell),
                } : null,

                UnknownLogsStatistics = x.Logs.Any(y => y.IsActive && y.Operator == Operator.Unknown) ? new DTOs.ServerLog.ServerLogStatistics
                {
                    Count = x.Logs.Count(y => y.IsActive && y.Operator == Operator.Unknown),
                    FailCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Failed && y.Operator == Operator.Unknown),
                    SuccessCount = x.Logs.Count(y => y.IsActive && y.ConnectionStatus == ConnectionStatus.Successful && y.Operator == Operator.Unknown),
                } : null,

            }).SingleOrDefault();

            return Task.FromResult(_mapper.Map<ServerDTO>(data));
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
            model.IsNewDomain = input.IsNewDomain;
            model.DomainDateTime = input.DomainDateTime;
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

            dto.ServerName += $" (Sample Of Id ={id})";

            var model = _mapper.Map<Server>(dto);

            await _serverRepository.Add(model);

        }
        public async Task<ServerDTO> GetByApp(string apiRoute, bool isAd, string isp, string Operator)
        {
            var operatorType = await _operatorIdentificationService.GetOperator(isp, Operator);
            var app = await GetCachedAppSetting(apiRoute); // Use a method to fetch app settings with caching

            if (string.IsNullOrWhiteSpace(app.GroupsThatAppIsJoinedIn))
                throw new BadRequestException("this app has no server");

            var groups = app.GroupsThatAppIsJoinedIn.Split("_");

            var query = await GetServersByGroups(groups, isAd);

            query = query.OrderByDescending(x => x.Id);

            query = ApplyOperatorFilter(query, operatorType);  //TODO : this is not null!

            var server = await SelectServer(query, app.SendRandomServer, app.Id);

            ServerDTO dto = _mapper.Map<ServerDTO>(server);
            dto.Config = UpdateConfig(dto.Config, dto.ConfigKey, dto.ConfigValue);

            return dto;
        }

        private async Task<AppSettingDTO> GetCachedAppSetting(string apiRoute)
        {
            if (_memoryCache.TryGetValue($"AppSetting_{apiRoute}", out AppSettingDTO cachedAppSetting))
            {
                return cachedAppSetting;
            }

            var appSetting = await _appSettingService.DetailByApiRoute(apiRoute);
            // تنظیم انقضای داده‌ها به یک دقیقه
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
            _memoryCache.Set($"AppSetting_{apiRoute}", appSetting, cacheEntryOptions);

            return appSetting;
        }

        private async Task<IEnumerable<Server>> GetServersByGroups(string[] groups, bool isAd)
        {
            return await _serverRepository.FindAsync(x =>
                groups.Contains(x.GroupId.ToString())
                && x.IsAd == isAd
                && x.IsAvailable
                && x.IsActive == true
                );
        }

        private static IEnumerable<Server> ApplyOperatorFilter(IEnumerable<Server> query, Operator operatorType)
        {
            return operatorType switch
            {
                Operator.Irancell => query.Where(x => x.IsForIrancell),
                Operator.HamraheAvval => query.Where(x => x.IsForHamraheAvval),
                _ => query
            };
        }

        private async Task<Server> SelectServer(IEnumerable<Server> servers, bool sendRandomServer, int appSettingId)
        {
            Server server;
            var lastLog = await _serverLogService.GetLastLog();
            if (sendRandomServer)
            {
                var serverNotToReturnId = lastLog?.ServerId ?? 0;
                var random = new Random();
                var allowedServers = servers.Where(x => x.Id != serverNotToReturnId);
                var enumerable = allowedServers.ToList();
                var index = random.Next(enumerable.Count());

                server = enumerable.ElementAt(index);
            }
            else
            {
                IEnumerable<Server> enumerable = new List<Server>();
                if (lastLog == null)
                {
                    var random = new Random();
                    var index = random.Next(enumerable.Count());
                    server = enumerable.ElementAt(index);
                }
                else
                {
                    var lastServerIndex = servers.Select(x => x.Id).ToList().IndexOf(lastLog.ServerId);

                    server = lastServerIndex <= 1 ? enumerable.FirstOrDefault() : enumerable.ElementAt(lastServerIndex + 1);
                }
            }
            return server;
            // Implement your logic here for selecting a server based on sendRandomServer and appSettingId
            // Return the selected server
        }

        private static string UpdateConfig(string config, string configKey, string configValue)
        {
            return config.Replace("@" + configKey, DateTime.Now.Ticks.ToString() + "." + configValue);
        }

        public async Task<ServerDTO> Detail(int id)
        {
            var model = await _serverRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (model is not { IsActive: true })
                throw new NotFoundException("سرور یافت نشد");

            return _mapper.Map<ServerDTO>(model);
        }

        public async Task<ServerDTO> Detail(string id)
        {
            var model = await _serverRepository.SingleOrDefaultAsync(x => x.Id.ToString() == id);
            if (model is not { IsActive: true })
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

            return data.Where(x => x.IsActive).Select(x => x.Id).ToList();
        }

        public async Task<List<int>> GetActiveIds()
        {
            var data = await _serverRepository.GetAll();
            return data.Where(x => x.IsActive && x.IsAvailable).Select(x => x.Id).ToList();
        }
        public async Task ToggleIsAvailableInput(int id)
        {
            var model = await _serverRepository.SingleOrDefaultAsync(x => x.Id == id);
            model.IsAvailable = !model.IsAvailable;
            await _serverRepository.Update(model);
        }

        public async Task SuccessServerLog(AddServerLogDTO input)
        {
            if (_memoryCache.TryGetValue($"SuccessServerLog_{input.ServerId}_{input.UserId}", out AddServerLogDTO? _))
            {
                return;
            }

            var server = await Detail(input.ServerId);
            // ذخیره اطلاعات در کش با تنظیمات انقضای داده‌ها
            _memoryCache.Set($"SuccessServerLog_{input.ServerId}_{input.UserId}", server, _cacheEntryOptions);

            input.Ip = server.Ip;
            input.ConnectionStatus = Domain.Enums.ConnectionStatus.Successful;
            await _serverLogService.Create(input);
        }
        public async Task FailedServerLog(AddServerLogDTO input)
        {
            if (_memoryCache.TryGetValue($"FailedServerLog_{input.ServerId}_{input.UserId}", out AddServerLogDTO? _))
            {
                return;
            }

            var server = await Detail(input.ServerId);
            // تنظیم انقضای داده‌ها به یک دقیقه
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
            // ذخیره اطلاعات در کش با تنظیمات انقضای داده‌ها
            _memoryCache.Set($"FailedServerLog_{input.ServerId}_{input.UserId}", server, cacheEntryOptions);

            input.Ip = server.Ip;
            input.ConnectionStatus = Domain.Enums.ConnectionStatus.Failed;
            await _serverLogService.Create(input);
        }
    }
}
