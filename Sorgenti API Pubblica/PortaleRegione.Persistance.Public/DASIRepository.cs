using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

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

        public async Task<ATTI_DASI> Get(Guid attoUId)
        {
            var atto = await PRContext
                .DASI
                .FirstOrDefaultAsync(item => !item.Eliminato 
                               && item.Pubblicato
                               && item.UIDAtto == attoUId);
            return atto;
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

        public async Task<List<ATTI_FIRME>> GetFirme(ATTI_DASI atto, FirmeTipoEnum tipo)
        {
            if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO && tipo == FirmeTipoEnum.DOPO_DEPOSITO)
                return new List<ATTI_FIRME>();

            var firmaProponente = await PRContext
                .ATTI_FIRME
                .SingleOrDefaultAsync(f =>
                    f.UIDAtto == atto.UIDAtto
                    && f.UID_persona == atto.UIDPersonaProponente
                    && f.Valida);

            var query = PRContext
                .ATTI_FIRME
                .Where(f => f.UIDAtto == atto.UIDAtto
                            && f.UID_persona != atto.UIDPersonaProponente
                            && f.Valida);
            switch (tipo)
            {
                case FirmeTipoEnum.TUTTE:
                    break;
                case FirmeTipoEnum.PRIMA_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp <= atto.Timestamp);
                    break;
                case FirmeTipoEnum.DOPO_DEPOSITO:
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                        query = query.Where(f => f.Timestamp > atto.Timestamp);
                    break;
                case FirmeTipoEnum.ATTIVI:
                    query = query.Where(f => string.IsNullOrEmpty(f.Data_ritirofirma));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
            }

            query = query.OrderBy(f => f.Timestamp);

            var lst = await query
                .ToListAsync();

            if (firmaProponente != null && tipo != FirmeTipoEnum.DOPO_DEPOSITO) lst.Insert(0, firmaProponente);

            return lst;
        }
    }
}