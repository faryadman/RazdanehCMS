using Project.Application.DTOs.IP;

namespace Project.Application.Features.Interfaces
{
    public interface IIpService
    {
        Task<List<IpDTO>> List();
        Task<IpDTO> Detail(string ipName);
        Task Delete(int id);
        Task Delete();
        Task Create(CreateIpDTO input);
        Task Update(IpDTO input);
    }
}
