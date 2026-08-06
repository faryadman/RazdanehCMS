using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.BlackList;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Services
{
    public class BlackListService : IBlackListService
    {
        private readonly IBlackListRepository _blackListRepository;
        private readonly IMapper _mapper;

        public BlackListService(IBlackListRepository blackListRepository, IMapper mapper)
        {
            _blackListRepository = blackListRepository;
            _mapper = mapper;
        }

        public async Task Create(int serverId, string ip)
        {
            var exists = await _blackListRepository.FindQueryable(x => x.ServerId == serverId && x.Ip == ip).AnyAsync();

            if (exists)
                throw new BadRequestException("ip is already blacklisted");

            var model = new BlackList
            {
                ServerId = serverId,
                Ip = ip,
            };
            await _blackListRepository.Add(model);
        }

        public async Task Delete(int id)
        {
            await _blackListRepository.Delete(id);
        }

        public async Task<List<BlackListDTO>> List(int? serverId)
        {
            var query = _blackListRepository.GetAllQueryable();

            query = query.Where(x => x.IsActive==true);

            if (serverId != null)
                query = query.Where(x => x.ServerId == serverId.Value);

            var data = await query.Include(x=>x.Server).OrderByDescending(x=>x.Id).ToListAsync();

            return _mapper.Map<List<BlackListDTO>>(data);
        }
    }
}
