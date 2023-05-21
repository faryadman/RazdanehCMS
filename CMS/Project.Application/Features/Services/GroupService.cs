using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Group;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IMapper _mapper;

        public GroupService(IGroupRepository groupRepository, IMapper mapper)
        {
            _groupRepository = groupRepository;
            _mapper = mapper;
        }

        public async Task Create(CreateGroupDTO input)
        {
            var model = _mapper.Map<Group>(input);
            await _groupRepository.Add(model);
        }
        public async Task Edit(EditGroupDTO input)
        {
            var model = await _groupRepository.SingleOrDefaultAsync(x => x.Id == input.ItemId);
            model.Title = input.Title;
            await _groupRepository.Update(model);
        }
        public async Task Delete(int id)
        {
            await _groupRepository.Delete(id);
        }
        public async Task<List<GroupDTO>> GetAll()
        {
            var data = await _groupRepository.GetAll();

            return _mapper.Map<List<GroupDTO>>(data.OrderByDescending(x => x.UpdatedAt));
        }

        public async Task<List<GroupDTO>> GetFiltered(bool isAd)
        {
            var data = await _groupRepository.FindAsync(x => x.IsAd == isAd);

            return _mapper.Map<List<GroupDTO>>(data.OrderByDescending(x => x.UpdatedAt));
        }
    }
}
