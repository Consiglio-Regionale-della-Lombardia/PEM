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

namespace PortaleRegione.DTO.Request;

public class QueryExtendedRequest
{
    public List<int> Soggetti { get; set; } = new List<int>();
    public List<int> Stati { get; set; } = new List<int>();
    public List<int> Tipi { get; set; } = new List<int>();
    public List<int> TipiRispostaRichiesta { get; set; } = new List<int>();
    public List<int> TipiChiusura { get; set; } = new List<int>();
    public List<int> TipiVotazione { get; set; } = new List<int>();
    public List<int> TipiDocumento { get; set; } = new List<int>();
    public bool DocumentiMancanti { get; set; } = false;
    public List<Guid> Proponenti { get; set; } = new List<Guid>();
    public List<Guid> Provvedimenti { get; set; } = new List<Guid>();
    public List<Guid> AttiDaFirmare { get; set; } = new List<Guid>();
    public List<int> Risposte { get; set; } = new List<int>();
    public List<int> Organi { get; set; } = new List<int>();
    public List<int> Organi_Commissione { get; set; } = new List<int>();
    public List<int> Organi_Giunta { get; set; } = new List<int>();
    public List<DateTime> DataTrasmissione { get; set; } = new List<DateTime>();
    public bool DataTrasmissioneIsNull { get; set; } = false;
    public bool RispostaMancante { get; set; } = false;
    public bool OrganiIsNull { get; set; } = false;
    public List<DateTime> DataSeduta { get; set; } = new List<DateTime>();
    public List<DateTime> DataRisposta { get; set; } = new List<DateTime>();
    public bool DataRispostaIsNull { get; set; } = false;
    public bool DataAnnunzioIsNull { get; set; } = false;
    public bool DataComunicazioneAssembleaIsNull { get; set; } = false;
    public bool DataChiusuraIterIsNull { get; set; } = false;
    public List<DateTime> DataComunicazioneAssemblea { get; set; } = new List<DateTime>();
    public List<DateTime> DataAnnunzio { get; set; } = new List<DateTime>();
    public List<DateTime> DataChiusuraIter { get; set; } = new List<DateTime>();
    public List<DateTime> DataTrattazione { get; set; } = new List<DateTime>();
    public bool DataTrattazioneIsNull { get; set; } = false;
    public List<DateTime> DataPresentazione { get; set; } = new List<DateTime>();
    public bool DataPresentazioneIsNull { get; set; } = false;
    public List<int> GruppiProponenti { get; set; } = new List<int>();
    public List<Guid> Firmatari { get; set; } = new List<Guid>();
    public List<int> GruppiFirmatari { get; set; } = new List<int>();
    public List<int> AreaPolitica { get; set; } = new List<int>();
    public bool? Ritardo { get; set; }
    public List<bool> RitardoList { get; set; } = new List<bool>();
    public bool TipoVotazioneMancante { get; set; } = false;

    public QueryExtendedRequest Clone()
    {
        return new QueryExtendedRequest
        {
            Soggetti = new List<int>(Soggetti),
            Stati = new List<int>(Stati),
            Tipi = new List<int>(Tipi),
            TipiRispostaRichiesta = new List<int>(TipiRispostaRichiesta),
            TipiChiusura = new List<int>(TipiChiusura),
            TipiVotazione = new List<int>(TipiVotazione),
            TipiDocumento = new List<int>(TipiDocumento),
            DocumentiMancanti = DocumentiMancanti,
            Proponenti = new List<Guid>(Proponenti),
            Provvedimenti = new List<Guid>(Provvedimenti),
            AttiDaFirmare = new List<Guid>(AttiDaFirmare),
            Risposte = new List<int>(Risposte),
            Organi = new List<int>(Organi),
            DataTrasmissione = new List<DateTime>(DataTrasmissione),
            DataTrasmissioneIsNull = DataTrasmissioneIsNull,
            RispostaMancante = RispostaMancante,
            OrganiIsNull = OrganiIsNull,
            DataSeduta = new List<DateTime>(DataSeduta),
            DataRisposta = new List<DateTime>(DataRisposta),
            DataRispostaIsNull = DataRispostaIsNull,
            DataAnnunzioIsNull = DataAnnunzioIsNull,
            DataComunicazioneAssembleaIsNull = DataComunicazioneAssembleaIsNull,
            DataChiusuraIterIsNull = DataChiusuraIterIsNull,
            DataComunicazioneAssemblea = new List<DateTime>(DataComunicazioneAssemblea),
            DataAnnunzio = new List<DateTime>(DataAnnunzio),
            DataChiusuraIter = new List<DateTime>(DataChiusuraIter),
            DataTrattazione = new List<DateTime>(DataTrattazione),
            DataTrattazioneIsNull = DataTrattazioneIsNull,
            DataPresentazione = new List<DateTime>(DataPresentazione),
            DataPresentazioneIsNull = DataPresentazioneIsNull,
            GruppiProponenti = new List<int>(GruppiProponenti),
            Firmatari = new List<Guid>(Firmatari),
            GruppiFirmatari = new List<int>(GruppiFirmatari),
            AreaPolitica = new List<int>(AreaPolitica)
        };
    }
}