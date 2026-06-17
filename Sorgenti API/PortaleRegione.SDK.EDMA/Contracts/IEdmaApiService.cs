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

using System.Threading.Tasks;
using PortaleRegione.SDK.EDMA.Models;
using PortaleRegione.SDK.EDMA.Models.Response;

namespace PortaleRegione.SDK.EDMA.Contracts
{
    /// <summary>
    ///     Servizi EDMA utilizzati per il flusso di protocollazione degli atti
    ///     DASI. L'ordine delle chiamate previsto e':
    ///     <list type="number">
    ///         <item><see cref="CreaInserisciPraticaAsync"/></item>
    ///         <item><see cref="CreaDocumentoAsync"/> (pdf principale)</item>
    ///         <item><see cref="CreaInserisciDocumentoFiglioAsync"/> per ogni allegato</item>
    ///         <item><see cref="AssociaDocumentiAsync"/> (lega il pdf alla pratica)</item>
    ///         <item><see cref="ProtocollazioneApplicativaAsync"/> (assegna la segnatura)</item>
    ///     </list>
    ///     I metodi sono indipendenti per permettere riprese idempotenti in
    ///     caso di errore parziale del flusso.
    /// </summary>
    public interface IEdmaApiService
    {
        Task<EdmaResponse<int>> CaricaMetaDocumentoIdAsync(string codiceMetadocumento);

        Task<EdmaResponse<FascicoloPraticaOutput>> CreaInserisciPraticaAsync(
            string idSottoFascicoloPadre,
            int metamoduloPadre,
            FascicoloPratica pratica);

        Task<EdmaResponse<DocumentoFileOutput>> CreaDocumentoAsync(
            DocumentoBase documento,
            string nomeFile,
            byte[] fileBytes,
            string estensioneFile);

        Task<EdmaResponse<DocumentoFileOutput>> CreaInserisciDocumentoFiglioAsync(
            string idDocumentoPadre,
            string codiceMetadocPadre,
            DocumentoBase figlio,
            string nomeFile,
            byte[] fileBytes,
            string estensioneFile);

        Task<EdmaResponse<bool>> AssociaDocumentiAsync(
            string idPratica,
            string idDocumento,
            int metamoduloDocumento);

        Task<EdmaResponse<ProtocollazioneOutput>> ProtocollazioneApplicativaAsync(
            string idDocumento,
            ParametriProtocollazioneApplicativa parametri);
    }
}
