using AutoMapper;
using DNTPersianUtils.Core;
using Project.Application.DTOs.ApiLog;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.BlackList;
using Project.Application.DTOs.CronJobInfo;
using Project.Application.DTOs.Domain;
using Project.Application.DTOs.Group;
using Project.Application.DTOs.Job;
using Project.Application.DTOs.OperatorIdentification;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Application.Helpers;
using Project.Domain.Entities;

namespace Project.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            #region appSetting
            CreateMap<AppSetting, MinimalAppSettingDTO>().ReverseMap();
            CreateMap<AppSetting, CreateAppSettingDTO>().ReverseMap();
            CreateMap<AppSetting, EditAppSettingDTO>().ReverseMap();
            CreateMap<AppSetting, AppSettingDTO>()
                //.ForMember(dest => dest.AppId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServerApiRoute, opt => opt.MapFrom(src => "/api/GetServer/" + src.AppTitle))
                .ReverseMap();
            #endregion

            #region group
            CreateMap<Group, GroupDTO>().ReverseMap();
            CreateMap<Group, CreateGroupDTO>().ReverseMap();
            #endregion

            #region server
            CreateMap<Server, ServerDTO>().ReverseMap();
            CreateMap<Server, CreateServerDTO>().ReverseMap();
            CreateMap<Server, EditServerDTO>().ReverseMap();
            #endregion

            #region serverlog
            CreateMap<ServerLog, AddServerLogDTO>().ReverseMap();
            CreateMap<ServerLog, ServerLogDTO>()
                .ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.Operator.GetDisplayAttributeFrom()))
                .ReverseMap();
            #endregion  

            #region blackList
            CreateMap<BlackList, BlackListDTO>().ReverseMap();
            #endregion

            #region apilog
            CreateMap<ApiLog, ApiLogDTO>().ReverseMap();
            #endregion

            #region operator
            CreateMap<OperatorIdentification, OperatorIdentificationDTO>()
                .ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.Operator.GetDisplayAttributeFrom()))
                .ReverseMap();
            CreateMap<OperatorIdentification, CreateOperatorIdentificationDTO>().ReverseMap();
            #endregion

            #region cronjobinfo
            CreateMap<CronJobInfo, CronJobInfoDTO>()
                .ForMember(dest => dest.LastExecutionDateToString, opt => opt.MapFrom(src => src.LastExecutionDate.ToLongPersianDateTimeString(false))).ReverseMap();
            CreateMap<CronJobInfo, UpdateCronJobInfoDTO>().ReverseMap();
            #endregion

            #region domain

            CreateMap<Domain.Entities.Domain, CreateDomainDTO>().ReverseMap();
            CreateMap<Domain.Entities.Domain, DomainDTO>().ReverseMap();

            #endregion

            #region job

            CreateMap<Job, CreateJobDTO>().ReverseMap();
            CreateMap<Job, JobDTO>().ReverseMap();

            #endregion
        }
    }
}
