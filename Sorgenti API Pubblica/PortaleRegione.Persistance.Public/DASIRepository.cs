using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;

namespace PortaleRegione.Persistance.Public
{
    public class DASIRepository : IDASIRepository
    {
        protected readonly DbContext Context;

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public DASIRepository(DbContext context)
        {
            Context = context;
        }

        public Task<ATTI_DASI> Get(Guid attoUId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ATTI_DASI>> GetAll(int page, int size, Filter<ATTI_DASI> filtro = null)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato 
                               && item.Pubblicato);
            filtro?.BuildExpression(ref query);
            return await query
                .OrderBy(item => item.Tipo)
                .ThenByDescending(item => item.NAtto_search)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> Count(Filter<ATTI_DASI> filtro = null)
        {
            var query = PRContext
                .DASI
                .Where(item => !item.Eliminato 
                               && item.Pubblicato);

            filtro?.BuildExpression(ref query);
            return await query.CountAsync();
        }
    }
}