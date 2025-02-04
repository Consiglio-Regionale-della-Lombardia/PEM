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
            // List to hold the rows that need updating:
            var rowsToUpdate = new List<(Guid UIDAtto, int NewRitardo)>();

            try
            {
                using (var connection = new SqlConnection(_model.connectionString))
                {
                    await connection.OpenAsync();

                    // Retrieve rows from ATTI_DASI along with DataRisposta (if available) from ATTI_RISPOSTE.
                    // Using OUTER APPLY to get the TOP 1 Data (ordered ascending) for each ATTI_DASI.
                    var selectQuery = @"
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
                            ORDER BY r.Data ASC
                        ) r
                        WHERE d.Eliminato = 0
                          AND (d.IDStato = 3 OR d.IDStato = 4 OR d.IDStato = 14)";

                    using (var selectCmd = new SqlCommand(selectQuery, connection))
                    using (var reader = await selectCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Get UIDAtto
                            Guid uidAtto = reader.GetGuid(reader.GetOrdinal("UIDAtto"));

                            // Retrieve DataAnnunzio if present. (It is a datetime column and can be null.)
                            DateTime? dataAnnunzio = reader.IsDBNull(reader.GetOrdinal("DataAnnunzio"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("DataAnnunzio"));

                            // Get the current Ritardo value.
                            int currentRitardo = reader.GetInt32(reader.GetOrdinal("Ritardo"));

                            // Get DataRisposta if present, otherwise substitute DateTime.Now.
                            DateTime dataRisposta;
                            if (reader.IsDBNull(reader.GetOrdinal("DataRisposta")))
                            {
                                dataRisposta = DateTime.Now;
                            }
                            else
                            {
                                dataRisposta = reader.GetDateTime(reader.GetOrdinal("DataRisposta"));
                            }

                            // If DataAnnunzio is null, we cannot compute a valid difference.
                            if (!dataAnnunzio.HasValue)
                            {
                                continue;
                            }

                            // Calculate the difference in days between DataRisposta and DataAnnunzio, then add 20 days.
                            int calculatedRitardo = (int)((dataRisposta.Date - dataAnnunzio.Value.Date).TotalDays) + 20;

                            // If the calculation is negative, set to zero.
                            if (calculatedRitardo < 0)
                            {
                                calculatedRitardo = 0;
                            }

                            // Only update if the new value differs from the current value.
                            if (calculatedRitardo != currentRitardo)
                            {
                                rowsToUpdate.Add((uidAtto, calculatedRitardo));
                            }
                        }
                    }

                    // Update each row that requires a change.
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
                // Consider logging the exception details.
                Console.Error.WriteLine($"Error executing Worker: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
