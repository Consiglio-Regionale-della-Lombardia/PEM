/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts;
using PortaleRegione.DataBase;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.Persistance
{
    /// <summary>
    ///     Implementazione della relativa interfaccia
    /// </summary>
    public class NotificheRepository : Repository<NOTIFICHE>, INotificheRepository
    {
        public NotificheRepository(PortaleRegioneDbContext context) : base(context)
        {
        }

        public PortaleRegioneDbContext PRContext => Context as PortaleRegioneDbContext;

        public async Task<IEnumerable<NOTIFICHE_DESTINATARI>> GetDestinatariNotifica(long notificaId)
        {
            var query = PRContext
                .NOTIFICHE_DESTINATARI
                .Include(n => n.gruppi_politici)
                .Include(n => n.NOTIFICHE)
                .Where(nd => nd.UIDNotifica == notificaId);

            return await query
                .ToListAsync();
        }

        public bool CheckIfNotificabile(EmendamentiDto em, PersonaDto persona)
        {
            if (em.IDStato >= (int) StatiEnum.Depositato) return false;

            if (em.ATTI.Chiuso) return false;

            if (persona.IsSegreteriaAssemblea)
                return true;

            if (persona.Gruppo == null) return false;
            if (em.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (persona.IsConsigliereRegionale ||
                persona.IsAssessore ||
                persona.IsPresidente)
            {
                if (em.UIDPersonaProponente != persona.UID_persona) return false;

                var firma = PRContext.FIRME.Find(em.UIDEM, persona.UID_persona);
                if (firma == null) return false;

                if (!string.IsNullOrEmpty(firma.Data_ritirofirma)) return false;
            }

            return true;
        }

        public bool CheckIfNotificabile(AttoDASIDto atto, PersonaDto persona)
        {
            if (atto.UIDSeduta.HasValue)
            {
                return false;
            }

            if (persona.IsSegreteriaAssemblea)
                return true;

            if (persona.Gruppo == null) return false;
            if (atto.id_gruppo != persona.Gruppo.id_gruppo) return false;

            if (persona.IsConsigliereRegionale ||
                persona.IsAssessore ||
                persona.IsPresidente)
            {
                if (atto.UIDPersonaProponente.Value != persona.UID_persona) return false;

                var firma = PRContext.ATTI_FIRME.Find(atto.UIDAtto, persona.UID_persona);
                if (firma == null) return false;

                if (!string.IsNullOrEmpty(firma.Data_ritirofirma)) return false;
            }

            return true;
        }

        public async Task<int> CountInviate(PersonaDto currentUser, int idGruppo, bool Archivio,
            Filter<NOTIFICHE> filtro = null)
        {
            var query = PRContext
                .NOTIFICHE
                .Where(n => n.Mittente == currentUser.UID_persona);

            if (idGruppo > 0) query = query.Where(nd => nd.IdGruppo == idGruppo);

            query = query.Where(n => n.Chiuso == Archivio);

            if (currentUser.CurrentRole == RuoliIntEnum.Consigliere_Regionale
                || currentUser.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta
                || currentUser.CurrentRole == RuoliIntEnum.Presidente_Regione)
            {
                var listRuoli = new List<int>
                {
                    (int) RuoliIntEnum.Consigliere_Regionale,
                    (int) RuoliIntEnum.Assessore_Sottosegretario_Giunta,
                    (int) RuoliIntEnum.Presidente_Regione
                };
                query = query.Where(n => listRuoli.Contains(n.RuoloMittente.Value));
            }

            filtro?.BuildExpression(ref query);

            return await query.CountAsync();
        }

        public async Task<int> CountRicevute(PersonaDto currentUser, int idGruppo, bool Archivio, bool Solo_Non_Viste,
            Filter<NOTIFICHE> filtro = null)
        {
            var queryDestinatari = PRContext
                .NOTIFICHE_DESTINATARI
                .Where(n => true);

            if (currentUser.CurrentRole != RuoliIntEnum.Responsabile_Segreteria_Giunta &&
                currentUser.CurrentRole != RuoliIntEnum.Responsabile_Segreteria_Politica &&
                currentUser.CurrentRole != RuoliIntEnum.Segreteria_Politica &&
                currentUser.CurrentRole != RuoliIntEnum.Amministratore_PEM)
                queryDestinatari = queryDestinatari.Where(n => n.UIDPersona == currentUser.UID_persona);

            if (idGruppo > 0) queryDestinatari = queryDestinatari.Where(nd => nd.IdGruppo == idGruppo);

            if (Solo_Non_Viste == false)
            {
                var resultDestinatari = await queryDestinatari
                    .Select(n => n.UIDNotifica)
                    .ToListAsync();

                var query = PRContext
                    .NOTIFICHE
                    .Where(n => resultDestinatari.Contains(n.UIDNotifica));

                query = query.Where(n => n.Chiuso == Archivio);

                filtro?.BuildExpression(ref query);

                return await query.CountAsync();
            }

            if (currentUser.IsSegreteriaAssemblea)
                return 0;

            var resultNotificheNonViste = await queryDestinatari
                .Where(nd => nd.Visto == false)
                .Select(nd => nd.UIDNotifica)
                .ToListAsync();
            var query2 = PRContext
                .NOTIFICHE
                .Where(n => resultNotificheNonViste.Contains(n.UIDNotifica) && n.Chiuso == false);
            filtro?.BuildExpression(ref query2);

            var result = await query2.CountAsync();
            return result;
        }

        public async Task<IEnumerable<NOTIFICHE>> GetNotificheInviate(PersonaDto currentUser, int idGruppo,
            bool Archivio,
            int pageIndex,
            int pageSize,
            Filter<NOTIFICHE> filtro = null)
        {
            var query = PRContext
                .NOTIFICHE
                .Where(n => n.Mittente == currentUser.UID_persona);

            if (idGruppo > 0) query = query.Where(nd => nd.IdGruppo == idGruppo);

            query = query.Where(n => n.Chiuso == Archivio);

            if (currentUser.CurrentRole == RuoliIntEnum.Consigliere_Regionale
                || currentUser.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta
                || currentUser.CurrentRole == RuoliIntEnum.Presidente_Regione)
            {
                var listRuoli = new List<int>
                {
                    (int) RuoliIntEnum.Consigliere_Regionale,
                    (int) RuoliIntEnum.Assessore_Sottosegretario_Giunta,
                    (int) RuoliIntEnum.Presidente_Regione
                };
                query = query.Where(n => listRuoli.Contains(n.RuoloMittente.Value));
            }

            filtro?.BuildExpression(ref query);

            return await query.OrderByDescending(n => n.DataCreazione)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<NOTIFICHE>> GetNotificheRicevute(PersonaDto currentUser, int idGruppo,
            bool Archivio,
            bool Solo_Non_Viste,
            int pageIndex,
            int pageSize,
            Filter<NOTIFICHE> filtro = null)
        {
            var queryDestinatari = PRContext
                .NOTIFICHE_DESTINATARI
                .Where(n => true);

            if (currentUser.CurrentRole != RuoliIntEnum.Responsabile_Segreteria_Giunta &&
                currentUser.CurrentRole != RuoliIntEnum.Responsabile_Segreteria_Politica &&
                currentUser.CurrentRole != RuoliIntEnum.Segreteria_Politica &&
                !currentUser.IsAmministratorePEM)
            {
                queryDestinatari = queryDestinatari.Where(n => n.UIDPersona == currentUser.UID_persona);
            }

            if (idGruppo > 0) queryDestinatari = queryDestinatari.Where(nd => nd.IdGruppo == idGruppo);

            if (Solo_Non_Viste == false)
            {
                var resultDestinatari = await queryDestinatari
                    .Select(n => n.UIDNotifica)
                    .ToListAsync();

                var query = PRContext
                    .NOTIFICHE
                    .Where(n => resultDestinatari.Contains(n.UIDNotifica));

                query = query.Where(n => n.Chiuso == Archivio);

                filtro?.BuildExpression(ref query);

                return await query.OrderByDescending(n => n.DataCreazione)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            if (currentUser.IsSegreteriaAssemblea)
                return new List<NOTIFICHE>();
            queryDestinatari = queryDestinatari
                .Where(nd => nd.Visto == false);
            var resultNotificheNonViste = await queryDestinatari.Select(nd => nd.UIDNotifica).ToListAsync();
            var query2 = PRContext
                .NOTIFICHE
                .Where(n => resultNotificheNonViste.Contains(n.UIDNotifica) && n.Chiuso == false);
            filtro?.BuildExpression(ref query2);

            var result = await query2.ToListAsync();
            return result;
        }

        public async Task<NOTIFICHE> Get(long id)
        {
            return await PRContext.NOTIFICHE.FindAsync(id);
        }

        public async Task<bool> EsisteRitiroDasi(Guid attoUId, Guid personaUId)
        {
            return await PRContext
                .NOTIFICHE
                .AnyAsync(item => item.UIDAtto == attoUId
                               && item.Mittente == personaUId
                               && item.IDTipo == (int)TipoNotificaEnum.RITIRO);
        }
    }
}