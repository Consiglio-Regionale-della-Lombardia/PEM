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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        internal const string USERS_IN_DATABASE = "USERS_IN_DATABASE";
        private readonly IUnitOfWork _unitOfWork;

        private readonly MemoryCache memoryCache = MemoryCache.Default;

        /// <summary>
        ///     Inizializza una nuova istanza di MainLogic con le dipendenze necessarie.
        /// </summary>
        /// <param name="unitOfWork">Fornisce l'accesso ai repository e ai metodi di commit.</param>
        public MainLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GetUsersInDb();
        }

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
                if (Utility.tipiNonVisibili.Contains((TipoAttoEnum)tipo)) continue;

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
        public async Task<List<LegislaturaDto>> GetLegislature()
        {
            var legislature = await _unitOfWork.Legislature.GetLegislature();

            // #1089
            var result = new List<LegislaturaDto>();
            foreach (var legislatura in legislature)
                result.Add(new LegislaturaDto
                {
                    id_legislatura = legislatura.id_legislatura,
                    num_legislatura = legislatura.num_legislatura,
                    attiva = legislatura.attiva,
                    durata_legislatura_da = legislatura.durata_legislatura_da,
                    durata_legislatura_a = legislatura.durata_legislatura_a
                });

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
                if (Utility.statiNonVisibili_Segreteria.Contains((int)stato)) continue;

                result.Add(new KeyValueDto
                {
                    id = (int)stato,
                    descr = Utility.GetText_StatoDASI((int)stato)
                });
            }

            return result;
        }

        /// <summary>
        ///     Recupera un elenco di stati di chiusura.
        /// </summary>
        /// <returns>Una lista di KeyValueDto che rappresenta gli stati degli atti.</returns>
        public List<KeyValueDto> GetStatiChiusura()
        {
            var result = new List<KeyValueDto>();
            var stati = Enum.GetValues(typeof(TipoChiusuraIterEnum));
            foreach (var stato in stati)
                result.Add(new KeyValueDto
                {
                    id = (int)stato,
                    descr = Utility.GetText_ChiusuraIterDASI((int)stato)
                });

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
        public async Task<AttoDasiPublicDto> GetAtto(Guid uidAtto, string hostUrl)
        {
            var currentMethod = new StackTrace().GetFrame(0).GetMethod().Name;
            try
            {
                var attoInDb = await _unitOfWork.DASI.Get(uidAtto);
                if (attoInDb == null)
                    throw new KeyNotFoundException($"Identificativo {uidAtto} non trovato.");

                var tipo_risposta_fornita =
                    Utility.GetText_TipoRispostaDASI(attoInDb.IDTipo_Risposta_Effettiva.GetValueOrDefault(0));
                KeyValueDto gruppo = null;
                PersonaPublicDto proponente = null;
                PersonaPublicDto personaRelatore1 = null;
                PersonaPublicDto personaRelatore2 = null;
                PersonaPublicDto personaRelatoreMinoranza = null;
                var proponenti = new List<KeyValueDto>();
                if (attoInDb.Tipo != (int)TipoAttoEnum.RIS)
                {
                    gruppo = await _unitOfWork.Persone.GetGruppo(attoInDb.id_gruppo);
                    proponente = await _unitOfWork.Persone.GetPersona(attoInDb.UIDPersonaProponente.Value);
                }
                else
                {
                    proponenti = await _unitOfWork.DASI.GetCommissioniProponenti(attoInDb.UIDAtto);

                    if (attoInDb.UIDPersonaRelatore1.HasValue)
                        personaRelatore1 = await _unitOfWork.Persone.GetPersona(attoInDb.UIDPersonaRelatore1.Value);
                    if (attoInDb.UIDPersonaRelatore2.HasValue)
                        personaRelatore2 = await _unitOfWork.Persone.GetPersona(attoInDb.UIDPersonaRelatore2.Value);
                    if (attoInDb.UIDPersonaRelatoreMinoranza.HasValue)
                        personaRelatoreMinoranza =
                            await _unitOfWork.Persone.GetPersona(attoInDb.UIDPersonaRelatoreMinoranza.Value);
                }

                if (gruppo != null && proponente != null)
                    proponente.DisplayName += $" ({gruppo.sigla.Trim()})";

                var commissioni = await _unitOfWork.DASI.GetCommissioniPerAtto(attoInDb.UIDAtto);
                var risposteInDb = await _unitOfWork.DASI.GetRisposte(attoInDb.UIDAtto);
                var risposte = risposteInDb.Select(r => new AttiRispostePublicDto
                {
                    data = r.Data,
                    data_trasmissione = r.DataTrasmissione,
                    data_trattazione = r.DataTrattazione,
                    organo = r.DescrizioneOrgano,
                    id_organo = r.IdOrgano,
                    tipo_risposta = tipo_risposta_fornita,
                    tipo_organo = Utility.GetText_TipoOrganoDASI(r.TipoOrgano)
                }).ToList();
                var documentiInDb = await _unitOfWork.DASI.GetDocumenti(attoInDb.UIDAtto);
                var documenti = documentiInDb.Select(d => new AttiDocumentiPublicDto
                {
                    Tipo = ((TipoDocumentoEnum)d.Tipo).ToString(),
                    Titolo = d.Titolo,
                    Link = $"{hostUrl}/{ApiRoutes.ScaricaDocumento}?path={d.Path.Replace('\\', '/')}", // #1429
                    TipoEnum = (TipoDocumentoEnum)d.Tipo
                }).ToList();

                var note = await _unitOfWork.DASI.GetNote(attoInDb.UIDAtto);
                note = note.Where(n => n.TipoEnum == TipoNotaEnum.GENERALE_PUBBLICA).ToList();

                var abbinamenti = await _unitOfWork.DASI.GetAbbinamenti(attoInDb.UIDAtto);

                // #1428
                if (attoInDb.UID_Atto_ODG.HasValue)
                    if (!abbinamenti.Any(a => a.UidAttoAbbinato.Equals(attoInDb.UID_Atto_ODG.Value)))
                    {
                        var attoAbbinatoODG = await _unitOfWork.Atti.Get(attoInDb.UID_Atto_ODG.Value);
                        abbinamenti.Add(new AttiAbbinamentoPublicDto
                        {
                            UidAbbinamento = Guid.NewGuid(),
                            Data = attoAbbinatoODG.DataCreazione.HasValue
                                ? attoAbbinatoODG.DataCreazione.Value.ToString("dd/MM/yyyy")
                                : "",
                            UidAttoAbbinato = attoInDb.UID_Atto_ODG.Value,
                            OggettoAttoAbbinato = attoAbbinatoODG.Oggetto,
                            TipoAttoAbbinato = Utility.GetText_Tipo(attoAbbinatoODG.IDTipoAtto),
                            NumeroAttoAbbinato = attoAbbinatoODG.IDTipoAtto == (int)TipoAttoEnum.ALTRO
                                ? "Dibattito"
                                : attoAbbinatoODG.NAtto
                        });
                    }

                // #1428
                if (attoInDb.UID_MOZ_Abbinata.HasValue)
                {
                    var attoAbbinatoMOZ = await _unitOfWork.DASI.Get(attoInDb.UID_MOZ_Abbinata.Value);
                    if (!abbinamenti.Any(a => a.UidAttoAbbinato.Equals(attoInDb.UID_MOZ_Abbinata.Value)))
                        abbinamenti.Add(new AttiAbbinamentoPublicDto
                        {
                            UidAbbinamento = Guid.NewGuid(),
                            UidAttoAbbinato = attoInDb.UID_MOZ_Abbinata.Value,
                            Data = attoAbbinatoMOZ.Timestamp.HasValue
                                ? attoAbbinatoMOZ.Timestamp.Value.ToString("dd/MM/yyyy")
                                : "",
                            OggettoAttoAbbinato = attoAbbinatoMOZ.OggettoView(),
                            TipoAttoAbbinato = Utility.GetText_Tipo(attoAbbinatoMOZ.Tipo),
                            NumeroAttoAbbinato = GetDisplayFromEtichetta(attoAbbinatoMOZ.Etichetta)
                        });
                }

                var firme = await GetFirme(attoInDb, FirmeTipoEnum.TUTTE);

                var linkTestoOriginale =
                    $"{AppSettingsConfigurationHelper.urlDASI_Originale.Replace("{{QRCODE}}", attoInDb.UID_QRCode.ToString())}";
                var linkTestoTrattazione =
                    $"{AppSettingsConfigurationHelper.urlDASI_Trattazione.Replace("{{QRCODE}}", attoInDb.UID_QRCode.ToString())}";

                var attoDto = new AttoDasiPublicDto
                {
                    uidAtto = attoInDb.UIDAtto,
                    oggetto = attoInDb.OggettoView(),
                    display = GetDisplayFromEtichetta(attoInDb.Etichetta),
                    id_stato = attoInDb.IDStato,
                    stato = Utility.GetText_StatoDASI(attoInDb.IDStato),
                    id_tipo = attoInDb.Tipo,
                    tipo = Utility.GetText_Tipo(attoInDb.Tipo),
                    tipo_esteso = Utility.GetText_TipoEstesoDASI(attoInDb.Tipo),
                    n_atto = attoInDb.NAtto_search.ToString(),
                    data_presentazione = CryptoHelper.DecryptString(attoInDb.DataPresentazione,
                        AppSettingsConfigurationHelper.masterKey),
                    tipo_risposta_richiesta = Utility.GetText_TipoRispostaDASI(attoInDb.IDTipo_Risposta),
                    tipo_risposta_fornita = tipo_risposta_fornita,
                    area_politica = Utility.GetText_AreaPolitica(attoInDb.AreaPolitica),
                    data_chiusura_iter = attoInDb.DataChiusuraIter?.ToString("dd/MM/yyyy"),
                    data_annunzio = attoInDb.DataAnnunzio?.ToString("dd/MM/yyyy"),
                    data_comunicazione_assemblea = attoInDb.DataComunicazioneAssemblea?.ToString("dd/MM/yyyy"), // #1088
                    stato_iter =
                        Utility.GetText_ChiusuraIterDASI(attoInDb.TipoChiusuraIter.HasValue
                            ? attoInDb.TipoChiusuraIter.Value
                            : 0),
                    gruppo = gruppo,
                    proponente = proponente,
                    commissioni = commissioni,
                    risposte = risposte,
                    documenti = documenti,
                    abbinamenti = abbinamenti,
                    dcrl = attoInDb.DCRL,
                    dcr = attoInDb.DCR,
                    dcrc = attoInDb.DCCR,
                    firme = firme,
                    burl = string.IsNullOrEmpty(attoInDb.BURL) ? string.Empty : attoInDb.BURL, // #1427
                    note = note,
                    link_testo_originale = linkTestoOriginale,
                    link_testo_trattazione = linkTestoTrattazione
                };

                if (attoInDb.Tipo == (int)TipoAttoEnum.RIS)
                {
                    attoDto.proponenti = proponenti;
                    attoDto.relatore1 = personaRelatore1;
                    attoDto.relatore2 = personaRelatore2;
                    attoDto.relatore_minoranza = personaRelatoreMinoranza;
                }
                else
                {
                    if (attoInDb.UIDPersonaProponente.HasValue)
                        attoDto.uid_proponente = attoInDb.UIDPersonaProponente.Value;

                    if (attoInDb.TipoMOZ > 0) attoDto.tipo_mozione = Utility.GetText_TipoMOZDASI(attoInDb.TipoMOZ);
                }

                return attoDto;
            }
            catch (Exception e)
            {
                Log.Error(currentMethod, e);
                throw e;
            }
        }

        private async Task<List<AttiFirmePublicDto>> GetFirme(ATTI_DASI atto, FirmeTipoEnum tipo)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .DASI
                    .GetFirme(atto, tipo);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<AttiFirmePublicDto>();

                var result = new List<AttiFirmePublicDto>();
                foreach (var firma in firme)
                {
                    var gruppo = await _unitOfWork.Persone.GetGruppo(firma.id_gruppo);
                    var dto = new AttiFirmePublicDto
                    {
                        UID_persona = firma.UID_persona,
                        id_persona = Users.First(u => u.UID_persona == firma.UID_persona).id_persona,
                        FirmaCert = CryptoHelper.DecryptString(firma.FirmaCert,
                            AppSettingsConfigurationHelper.masterKey),
                        PrimoFirmatario = firma.PrimoFirmatario,
                        Gruppo = gruppo,
                        Data_ritirofirma = string.IsNullOrEmpty(firma.Data_ritirofirma)
                            ? null
                            : CryptoHelper.DecryptString(firma.Data_ritirofirma,
                                AppSettingsConfigurationHelper.masterKey),
                        Data_firma = firma.Timestamp.ToString("dd/MM/yyyy")
                    };

                    if (firma.id_AreaPolitica.HasValue)
                        dto.AreaPolitica = Utility.GetText_AreaPolitica(firma.id_AreaPolitica.Value);

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
        public async Task<BaseResponse<AttoLightDto>> Cerca(CercaRequest request)
        {
            var filtroFromRequest = GetFiltroCercaFromRequest(request);

            var filtroBase = new Filter<ATTI_DASI>();
            filtroBase.ImportStatements(filtroFromRequest);
            var res = await _unitOfWork.DASI.GetAll(
                request.page,
                request.size,
                filtroBase,
                request);
            var tot = await _unitOfWork.DASI.Count(filtroBase,
                request);

            return new BaseResponse<AttoLightDto>(
                request.page,
                request.size,
                res.Select(a => new AttoLightDto
                {
                    uidAtto = a.UIDAtto,
                    oggetto = a.Oggetto,
                    natto = a.NAtto_search.ToString(),
                    display = GetDisplayFromEtichetta(a.Etichetta),
                    tipo = Utility.GetText_Tipo(a.Tipo),
                    tipo_esteso = Utility.GetText_TipoEstesoDASI(a.Tipo)
                }).ToList(),
                filtroFromRequest,
                tot);
        }

        private List<FilterStatement<AttoLightDto>> GetFiltroCercaFromRequest(CercaRequest request)
        {
            var res = new List<FilterStatement<AttoLightDto>>();

            if (request.id_legislatura.HasValue && request.id_legislatura > 0)
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Legislatura),
                    Value = request.id_legislatura.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });

            if (request.id_gruppo.HasValue && request.id_gruppo > 0)
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.id_gruppo),
                    Value = request.id_gruppo.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });

            if (!string.IsNullOrEmpty(request.n_atto))
            {
                if (request.n_atto.Contains("-"))
                {
                    var n_attoSplit = request.n_atto.Split('-');

                    res.Add(new FilterStatement<AttoLightDto>
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
                    res.Add(new FilterStatement<AttoLightDto>
                    {
                        PropertyId = nameof(ATTI_DASI.NAtto_search),
                        Value = int.Parse(request.n_atto),
                        Operation = Operation.EqualTo,
                        Connector = FilterStatementConnector.And
                    });
                }
            }

            if (request.data_presentazione_da.HasValue)
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Timestamp),
                    Value = request.data_presentazione_da.Value,
                    Operation = Operation.GreaterThanOrEqualTo,
                    Connector = FilterStatementConnector.And
                });

            if (request.data_presentazione_a.HasValue)
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Timestamp),
                    Value = request.data_presentazione_a.Value,
                    Operation = Operation.LessThanOrEqualTo,
                    Connector = FilterStatementConnector.And
                });

            if (!string.IsNullOrEmpty(request.oggetto))
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Oggetto),
                    Value = request.oggetto,
                    Operation = Operation.Contains,
                    Connector = FilterStatementConnector.And
                });

            if (!string.IsNullOrEmpty(request.burl))
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.BURL),
                    Value = request.burl,
                    Operation = Operation.Contains,
                    Connector = FilterStatementConnector.And
                });

            if (request.dcr.HasValue)
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.DCR),
                    Value = request.dcr.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });

            if (request.dccr.HasValue)
                res.Add(new FilterStatement<AttoLightDto>
                {
                    PropertyId = nameof(ATTI_DASI.DCCR),
                    Value = request.dccr.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });

            return res;
        }

        private string GetDisplayFromEtichetta(string etichetta)
        {
            var split = etichetta.Split('_');
            return $"{split[0]} {split[1]}";
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

        public HttpResponseMessage ScaricaDocumento(string path)
        {
            var complete_path = Path.Combine(
                AppSettingsConfigurationHelper.PercorsoCompatibilitaDocumenti,
                Path.GetFileName(path));

            if (!path.Contains("~"))
                complete_path = Path.Combine(
                    AppSettingsConfigurationHelper.PercorsoCompatibilitaDocumenti,
                    path);

            var result = Utility.ComposeFileResponse(complete_path);
            return result;
        }
    }
}