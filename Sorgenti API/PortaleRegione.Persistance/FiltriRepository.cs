using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;

namespace PortaleRegione.Persistance
{
    public class FiltriRepository : Repository<FILTRI>, IFiltriRepository
    {
        public FiltriRepository(DbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<List<FILTRI>> GetByUser(Guid uidPersona)
        {
            var res = await PRContext.FILTRI
                .Where(f => f.UId_persona.Equals(uidPersona))
                .OrderBy(f => f.Preferito)
                .ThenBy(f => f.Nome)
                .ToListAsync();
            return res;
        }

        public async Task<FILTRI> Get(string nomeFiltro, Guid UidPersona)
        {
            var res = await PRContext
                .FILTRI
                .FirstOrDefaultAsync(f => f.UId_persona.Equals(UidPersona) && f.Nome.Equals(nomeFiltro));
            return res;
        }
    }
}