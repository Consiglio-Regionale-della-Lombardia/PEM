using System.Collections.Generic;
using System.Threading.Tasks;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Contracts.Public
{
    public interface IGruppiRepository
    {
        Task<List<KeyValueDto>> GetByLegislatura(int id_legislatura);
    }
}