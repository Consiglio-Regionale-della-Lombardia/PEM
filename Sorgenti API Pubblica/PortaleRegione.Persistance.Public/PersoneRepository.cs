using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DataBase;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Model;
using Z.EntityFramework.Plus;

namespace PortaleRegione.Persistance.Public
{
    public class PersoneRepository : IPersoneRepository
    {
        protected readonly DbContext Context;

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public PersoneRepository(DbContext context)
        {
            Context = context;
        }

        public async Task<List<KeyValueDto>> GetCariche(int idLegislatura)
        {
            var result = await PRContext
                .View_cariche_assessori_per_legislatura
                .Where(c => c.id_legislatura == idLegislatura)
                .OrderBy(item => item.ordine)
                .ThenBy(item => item.nome_carica)
                .ToListAsync();
            return result.Select(c => new KeyValueDto
            {
                id = c.id_carica,
                descr = c.nome_carica
            }).ToList();
        }

        public async Task<List<KeyValueDto>> GetCommissioni(int idLegislatura)
        {
            var result = await PRContext
                .View_Commissioni_per_legislatura
                .Where(c => c.id_legislatura == idLegislatura)
                .OrderBy(item => item.ordinamento)
                .ThenBy(item => item.nome_organo)
                .ToListAsync();

            return result.Select(c => new KeyValueDto
            {
                id = c.id_organo,
                descr = c.nome_organo
            }).ToList();
        }

        public async Task<List<KeyValueDto>> GetGruppiByLegislatura(int idLegislatura)
        {
            var query = PRContext
                .JOIN_GRUPPO_AD
                .Where(j => j.id_legislatura == idLegislatura)
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

        public async Task<List<PersonaPublicDto>> GetFirmatariByLegislatura(int idLegislatura)
        {
            PRContext.View_consiglieri_in_carica.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();
            PRContext.View_UTENTI.FromCache(DateTimeOffset.Now.AddHours(8)).ToList();

            var consiglieri = await PRContext
                .View_consiglieri_per_legislatura
                .Where(p => p.id_legislatura == idLegislatura && p.id_persona > 0)
                .Select(p => new PersonaPublicDto
                {
                    id = p.id_persona,
                    DisplayName = p.DisplayName
                })
                .ToListAsync();
            return consiglieri;
        }
    }
}