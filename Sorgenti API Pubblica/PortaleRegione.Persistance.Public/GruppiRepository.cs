using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Persistance.Public
{
    public class GruppiRepository : IGruppiRepository
    {
        protected readonly DbContext Context;

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public GruppiRepository(DbContext context)
        {
            Context = context;
        }

        public async Task<List<KeyValueDto>> GetByLegislatura(int id_legislatura)
        {
            var query = PRContext
                .JOIN_GRUPPO_AD
                .Where(j => j.id_legislatura == id_legislatura)
                .Join(PRContext
                        .gruppi_politici,
                    p => p.id_gruppo,
                    g => g.id_gruppo,
                    (p, g) => g);
            var lstGruppi = await query
                .Select(g => new KeyValueDto
                {
                    id = g.id_gruppo,
                    descr = g.nome_gruppo,
                    sigla = g.codice_gruppo
                })
                .ToListAsync();
            return lstGruppi;
        }
    }
}