using System.Collections.Generic;
using System.Threading.Tasks;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Contracts.Public
{
    public interface IPersoneRepository
    {
        Task<List<KeyValueDto>> GetCariche(int idLegislatura);
        Task<List<KeyValueDto>> GetCommissioni(int idLegislatura);
        Task<List<KeyValueDto>> GetGruppiByLegislatura(int idLegislatura);
    }
}