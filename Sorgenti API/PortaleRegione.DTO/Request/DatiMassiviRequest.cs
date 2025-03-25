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

namespace PortaleRegione.DTO.Request;

public class DatiMassiviRequest
{
    // Stato dell'atto e relativo controllo checkbox
    public bool StatoCheck { get; set; }
    public string Stato { get; set; }

    // Data annunzio e relativo controllo checkbox
    public bool DataAnnunzioCheck { get; set; }
    public string DataAnnunzio { get; set; }

    // Tipo chiusura iter e relativo controllo checkbox
    public bool TipoChiusuraIterCheck { get; set; }
    public string TipoChiusuraIter { get; set; }

    // Data chiusura iter e relativo controllo checkbox
    public bool DataChiusuraIterCheck { get; set; }
    public string DataChiusuraIter { get; set; }

    // Tipo votazione e relativo controllo checkbox
    public bool TipoVotazioneCheck { get; set; }
    public string TipoVotazione { get; set; }

    // Data comunicazione assemblea e relativo controllo checkbox
    public bool DataComunicazioneAssembleaCheck { get; set; }
    public string DataComunicazioneAssemblea { get; set; }

    // Emendato e relativo controllo checkbox
    public bool EmendatoCheck { get; set; }
    public bool Emendato { get; set; }

    // Pubblicato e relativo controllo checkbox
    public bool PubblicatoCheck { get; set; }
    public bool Pubblicato { get; set; }
}