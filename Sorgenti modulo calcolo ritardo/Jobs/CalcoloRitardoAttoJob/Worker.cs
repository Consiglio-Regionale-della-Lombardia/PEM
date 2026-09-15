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
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CalcoloRitardoAttoJob
{
    public class Worker
    {
        private readonly ThreadWorkerModel _model;

        public Worker(ThreadWorkerModel model)
        {
            _model = model;
        }

        public async Task<bool> ExecuteAsync()
        {
            var rowsToUpdate = new List<(Guid UIDAtto, int NewRitardo)>();

            try
            {
                using (var connection = new SqlConnection(_model.connectionString))
                {
                    await connection.OpenAsync();

                    // Una riga per atto + la prima risposta non eliminata (se c'è).
                    // Quando non c'è risposta DataRisposta torna NULL e viene gestito sotto.
                    var selectQuery = $@"
                        SELECT
                            d.UIDAtto,
                            d.DataAnnunzio,
                            d.Ritardo,
                            r.Data AS DataRisposta
                        FROM ATTI_DASI d
                        OUTER APPLY (
                            SELECT TOP 1 r.Data
                            FROM ATTI_RISPOSTE r
                            WHERE r.UIDAtto = d.UIDAtto
                              AND r.Eliminato = 0
                            ORDER BY r.Data ASC
                        ) r
                        WHERE d.Eliminato = 0
                          AND (d.IDStato = 3 OR d.IDStato = 4 OR d.IDStato = 14)";

                    using (var selectCmd = new SqlCommand(selectQuery, connection))
                    using (var reader = await selectCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Guid uidAtto = reader.GetGuid(reader.GetOrdinal("UIDAtto"));

                            DateTime? dataAnnunzio = reader.IsDBNull(reader.GetOrdinal("DataAnnunzio"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("DataAnnunzio"));

                            int currentRitardo = reader.GetInt32(reader.GetOrdinal("Ritardo"));

                            // Atto non ancora risposto: per far avanzare il conteggio uso la data odierna.
                            DateTime dataRisposta;
                            if (reader.IsDBNull(reader.GetOrdinal("DataRisposta")))
                            {
                                dataRisposta = DateTime.Now;
                            }
                            else
                            {
                                dataRisposta = reader.GetDateTime(reader.GetOrdinal("DataRisposta"));
                            }

                            // Senza data di annunzio non c'è nulla da cui partire, salto l'atto.
                            if (!dataAnnunzio.HasValue)
                            {
                                continue;
                            }

                            // Gap di 20 gg dall'annunzio: dal 21esimo giorno si conta
                            // un giorno di ritardo per ogni giorno trascorso. Il clamp qui sotto
                            // azzera i casi ancora dentro il gap.
                            int calculatedRitardo = (int)((dataRisposta.Date - dataAnnunzio.Value.Date).TotalDays) - 20;

                            if (calculatedRitardo < 0)
                            {
                                calculatedRitardo = 0;
                            }

                            // Aggiorno solo se il valore cambia davvero, così evito UPDATE inutili
                            // e righe di audit superflue sul trigger di ATTI_DASI.
                            if (calculatedRitardo != currentRitardo)
                            {
                                rowsToUpdate.Add((uidAtto, calculatedRitardo));
                            }
                        }
                    }

                    foreach (var row in rowsToUpdate)
                    {
                        var updateQuery = "UPDATE ATTI_DASI SET Ritardo = @NewRitardo WHERE UIDAtto = @UIDAtto";
                        using (var updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@NewRitardo", row.NewRitardo);
                            updateCmd.Parameters.AddWithValue("@UIDAtto", row.UIDAtto);
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: convogliare l'errore su un logger strutturato invece che su Console.Error.
                Console.Error.WriteLine($"Error executing Worker: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
