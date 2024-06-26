﻿/*
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
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.Api.Public.Helpers;
using PortaleRegione.Common;
using PortaleRegione.Contracts.Public;
using PortaleRegione.Crypto;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request.Public;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Api.Public.Business_Layer
{
    /// <summary>
    ///     Gestisce la logica di business per le richieste API, incapsulando la manipolazione dei dati e l'accesso al
    ///     database.
    /// </summary>
    public class MainLogic
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        ///     Inizializza una nuova istanza di MainLogic con le dipendenze necessarie.
        /// </summary>
        /// <param name="unitOfWork">Fornisce l'accesso ai repository e ai metodi di commit.</param>
        public MainLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GetUsersInDb();
        }

        /// <summary>
        ///     Ottiene un elenco dei tipi di atto disponibili, escludendo quelli non visibili.
        /// </summary>
        /// <returns>Una lista di KeyValueDto che rappresenta i tipi di atto.</returns>
        public List<KeyValueDto> GetTipi()
        {
            var result = new List<KeyValueDto>();
            var tipi = Enum.GetValues(typeof(TipoAttoEnum));
            foreach (var tipo in tipi)
            {
                if (Utility.tipiNonVisibili.Contains((TipoAttoEnum)tipo))
                {
                    continue;
                }

                result.Add(new KeyValueDto
                {
                    id = (int)tipo,
                    descr = Utility.GetText_Tipo((int)tipo)
                });
            }

            return result;
        }

        /// <summary>
        ///     Recupera un elenco di legislature disponibili dal database.
        /// </summary>
        /// <returns>Una task che, al suo completamento, restituisce una lista di KeyValueDto con le legislature.</returns>
        public async Task<List<KeyValueDto>> GetLegislature()
        {
            var legislature = await _unitOfWork.Legislature.GetLegislature();

            var result = new List<KeyValueDto>();
            foreach (var legislatura in legislature)
            {
                result.Add(new KeyValueDto
                {
                    id = legislatura.id_legislatura,
                    descr = legislatura.num_legislatura
                });
            }

            return result;
        }

        /// <summary>
        ///     Ottiene un elenco di tipi di risposta disponibili.
        /// </summary>
        /// <returns>Una lista di KeyValueDto con i tipi di risposta.</returns>
        public List<KeyValueDto> GetTipiRisposta()
        {
            var result = new List<KeyValueDto>
            {
                new KeyValueDto
                {
                    id = (int)TipoRispostaEnum.SCRITTA,
                    descr = "Scritta"
                },
                new KeyValueDto
                {
                    id = (int)TipoRispostaEnum.ORALE,
                    descr = "Orale"
                },
                new KeyValueDto
                {
                    id = (int)TipoRispostaEnum.COMMISSIONE,
                    descr = "In commissione"
                }
            };

            return result;
        }

        /// <summary>
        ///     Recupera un elenco di stati possibili per gli atti, escludendo quelli non visibili.
        /// </summary>
        /// <returns>Una lista di KeyValueDto che rappresenta gli stati degli atti.</returns>
        public List<KeyValueDto> GetStati()
        {
            var result = new List<KeyValueDto>();
            var stati = Enum.GetValues(typeof(StatiAttoEnum));
            foreach (var stato in stati)
            {
                if (Utility.statiNonVisibili_Segreteria.Contains((int)stato))
                {
                    continue;
                }

                result.Add(new KeyValueDto
                {
                    id = (int)stato,
                    descr = Utility.GetText_StatoDASI((int)stato)
                });
            }

            return result;
        }

        /// <summary>
        ///     Ottiene un elenco di gruppi per una data legislatura.
        /// </summary>
        /// <param name="idLegislatura">L'identificativo della legislatura di interesse.</param>
        /// <returns>Una task che, al suo completamento, restituisce una lista di KeyValueDto con i gruppi.</returns>
        public async Task<List<KeyValueDto>> GetGruppiByLegislatura(int idLegislatura)
        {
            var gruppi = await _unitOfWork.Persone.GetGruppiByLegislatura(idLegislatura);
            return gruppi;
        }

        /// <summary>
        ///     Recupera un elenco di cariche per una specifica legislatura.
        /// </summary>
        /// <param name="idLegislatura">L'identificativo della legislatura di interesse.</param>
        /// <returns>Una task che, al suo completamento, restituisce una lista di KeyValueDto con le cariche.</returns>
        public async Task<List<KeyValueDto>> GetCaricheByLegislatura(int idLegislatura)
        {
            var cariche = await _unitOfWork.Persone.GetCariche(idLegislatura);
            return cariche;
        }

        /// <summary>
        ///     Ottiene un elenco di commissioni per una determinata legislatura.
        /// </summary>
        /// <param name="idLegislatura">L'identificativo della legislatura di interesse.</param>
        /// <returns>Una task che, al suo completamento, restituisce una lista di KeyValueDto con le commissioni.</returns>
        public async Task<List<KeyValueDto>> GetCommissioniByLegislatura(int idLegislatura)
        {
            var commissioni = await _unitOfWork.Persone.GetCommissioni(idLegislatura);
            return commissioni;
        }

        /// <summary>
        ///     Ottiene un elenco di firmatari per una specifica legislatura.
        /// </summary>
        /// <param name="idLegislatura">L'identificativo della legislatura di interesse.</param>
        /// <returns>Una task che, al suo completamento, restituisce una lista di PersonaPublicDto con i firmatari.</returns>
        public async Task<List<PersonaPublicDto>> GetFirmatariByLegislatura(int idLegislatura)
        {
            var firmatari = await _unitOfWork.Persone.GetFirmatariByLegislatura(idLegislatura);
            return firmatari;
        }

        /// <summary>
        ///     Recupera i dettagli di un atto specifico utilizzando il suo identificativo unico.
        /// </summary>
        /// <param name="uidAtto">L'identificativo unico dell'atto.</param>
        /// <returns>Una task che, al suo completamento, restituisce un AttoDasiPublicDto con i dettagli dell'atto.</returns>
        public async Task<AttoDasiPublicDto> GetAtto(Guid uidAtto)
        {
            var attoInDb = await _unitOfWork.DASI.Get(uidAtto);
            if (attoInDb == null)
                throw new KeyNotFoundException($"Identificativo {uidAtto} non trovato.");
            var attoDto = new AttoDasiPublicDto
            {
                uidAtto = attoInDb.UIDAtto,
                oggetto = attoInDb.Oggetto,
                display = GetDisplayFromEtichetta(attoInDb.Etichetta),
                stato = Utility.GetText_StatoDASI(attoInDb.IDStato),
                tipo = Utility.GetText_Tipo(attoInDb.Tipo),
                data_presentazione = CryptoHelper.DecryptString(attoInDb.DataPresentazione,
                    AppSettingsConfigurationHelper.masterKey),
                premesse = attoInDb.Premesse,
                richiesta = attoInDb.Richiesta,
                tipo_risposta = Utility.GetText_TipoRispostaDASI(attoInDb.IDTipo_Risposta),
                area_politica = "",
                data_iscrizione = attoInDb.DataIscrizioneSeduta?.ToString("dd/MM/yyyy")
            };

            var firmeAnte = await GetFirme(attoInDb, FirmeTipoEnum.PRIMA_DEPOSITO);
            var firmePost = await GetFirme(attoInDb, FirmeTipoEnum.DOPO_DEPOSITO);
            attoDto.firme = firmeAnte;
            attoDto.firme_dopo_deposito = firmePost;

            return attoDto;
        }

        private async Task<List<AttiFirmeDto>> GetFirme(ATTI_DASI atto, FirmeTipoEnum tipo)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .DASI
                    .GetFirme(atto, tipo);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<AttiFirmeDto>();

                var result = new List<AttiFirmeDto>();
                foreach (var firma in firme)
                {
                    var dto = new AttiFirmeDto
                    {
                        UIDAtto = firma.UIDAtto,
                        UID_persona = firma.UID_persona,
                        id_persona = Users.First(u => u.UID_persona == firma.UID_persona).id_persona,
                        FirmaCert = CryptoHelper.DecryptString(firma.FirmaCert,
                            AppSettingsConfigurationHelper.masterKey),
                        PrimoFirmatario = firma.PrimoFirmatario,
                        id_gruppo = firma.id_gruppo,
                        ufficio = firma.ufficio,
                        Data_ritirofirma = string.IsNullOrEmpty(firma.Data_ritirofirma)
                            ? null
                            : CryptoHelper.DecryptString(firma.Data_ritirofirma,
                                AppSettingsConfigurationHelper.masterKey),
                        Timestamp = firma.Timestamp,
                        Capogruppo = firma.Capogruppo,
                        id_AreaPolitica = firma.id_AreaPolitica,
                        Data_firma = firma.Timestamp.ToString("dd/MM/yyyy"),
                        Prioritario = firma.Prioritario
                    };

                    result.Add(dto);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetFirme - DASI", e);
                throw e;
            }
        }

        /// <summary>
        ///     Effettua una ricerca di atti basata su vari criteri di filtro.
        /// </summary>
        /// <param name="request">L'oggetto richiesta contenente i criteri di ricerca.</param>
        /// <returns>Una task che, al suo completamento, restituisce un oggetto BaseResponse con i risultati della ricerca.</returns>
        public async Task<BaseResponse<AttoDASILightDto>> Cerca(CercaRequest request)
        {
            var filtroFromRequest = GetFiltroCercaFromRequest(request);
            var filtroBase = new Filter<ATTI_DASI>();
            filtroBase.ImportStatements(filtroFromRequest);
            var res = await _unitOfWork.DASI.GetAll(
                request.page,
                request.size,
                filtroBase);
            var tot = await _unitOfWork.DASI.Count(filtroBase);
            return new BaseResponse<AttoDASILightDto>(
                request.page,
                request.size,
                res.Select(a => new AttoDASILightDto
                {
                    uidAtto = a.UIDAtto,
                    oggetto = a.Oggetto,
                    display = GetDisplayFromEtichetta(a.Etichetta)
                }).ToList(),
                filtroFromRequest,
                tot);
        }

        private List<FilterStatement<AttoDASILightDto>> GetFiltroCercaFromRequest(CercaRequest request)
        {
            var res = new List<FilterStatement<AttoDASILightDto>>();
            if (request.id_tipo.HasValue && request.id_tipo > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Tipo),
                    Value = request.id_tipo.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (request.id_legislatura.HasValue && request.id_legislatura > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Legislatura),
                    Value = request.id_legislatura.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (request.id_stato.HasValue && request.id_stato > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.IDStato),
                    Value = request.id_stato.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (request.id_tipo_risposta.HasValue && request.id_tipo_risposta > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.IDTipo_Risposta),
                    Value = request.id_tipo_risposta.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (request.id_gruppo.HasValue && request.id_gruppo > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.id_gruppo),
                    Value = request.id_gruppo.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (!string.IsNullOrEmpty(request.n_atto))
            {
                if (request.n_atto.Contains("-"))
                {
                    var n_attoSplit = request.n_atto.Split('-');

                    res.Add(new FilterStatement<AttoDASILightDto>
                    {
                        PropertyId = nameof(ATTI_DASI.NAtto_search),
                        Value = int.Parse(n_attoSplit[0]),
                        Value2 = int.Parse(n_attoSplit[1]),
                        Operation = Operation.Between,
                        Connector = FilterStatementConnector.And
                    });
                }
                else
                {
                    res.Add(new FilterStatement<AttoDASILightDto>
                    {
                        PropertyId = nameof(ATTI_DASI.NAtto_search),
                        Value = int.Parse(request.n_atto),
                        Operation = Operation.EqualTo,
                        Connector = FilterStatementConnector.And
                    });
                }
            }

            if (request.data_presentazione_da.HasValue)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Timestamp),
                    Value = request.data_presentazione_da.Value,
                    Operation = Operation.GreaterThanOrEqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (request.data_presentazione_a.HasValue)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Timestamp),
                    Value = request.data_presentazione_a.Value,
                    Operation = Operation.LessThanOrEqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            return res;
        }

        private string GetDisplayFromEtichetta(string etichetta)
        {
            var split = etichetta.Split('_');
            return $"{split[0]} {split[1]}";
        }

        private readonly MemoryCache memoryCache = MemoryCache.Default;
        internal const string USERS_IN_DATABASE = "USERS_IN_DATABASE";

        internal List<PersonaLightDto> Users
        {
            get
            {
                if (memoryCache.Contains(USERS_IN_DATABASE))
                    return memoryCache.Get(USERS_IN_DATABASE) as List<PersonaLightDto>;

                return new List<PersonaLightDto>();
            }
            set => memoryCache.Add(USERS_IN_DATABASE, value, DateTimeOffset.UtcNow.AddHours(8));
        }

        internal void GetUsersInDb()
        {
            if (Users.Any())
                return;
            var task_op = Task.Run(async () => await _unitOfWork.Persone.GetAll());
            var personeInDb = task_op.Result;
            var personeInDbLight = personeInDb.Select(p => new PersonaLightDto
            {
                id_persona = p.id_persona,
                UID_persona = p.UID_persona.Value,
                cognome = p.cognome,
                nome = p.nome,
                foto = p.foto
            }).ToList();

            Users = personeInDbLight;
        }
    }
}