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
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace CleanLogRetention
{
    public class Worker
    {
        // Le tabelle di audit sono scritte dai trigger, che valorizzano DatAudit con la data dell'evento.
        // Chi ha una colonna diversa la dichiara nella configurazione con la sintassi TABELLA:COLONNA.
        private const string ColonnaDataPredefinita = "DatAudit";

        private readonly ThreadWorkerModel _model;

        public Worker(ThreadWorkerModel model)
        {
            _model = model;
        }

        public async Task<bool> ExecuteAsync()
        {
            try
            {
                // Otteniamo le tabelle dalla configurazione
                var tables = _model.tables.Split(',');

                using (var connection = new SqlConnection(_model.connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var table in tables)
                    {
                        var voce = table.Trim();
                        if (string.IsNullOrEmpty(voce))
                            continue;

                        string tableName;
                        string columnName;
                        LeggiVoce(voce, out tableName, out columnName);

                        var startTime = DateTime.Now;
                        var retentionDate = DateTime.Now.AddDays(-_model.retention);

                        // Ogni tabella va per conto suo: un nome sbagliato in configurazione
                        // non deve impedire la pulizia di quelle che seguono
                        try
                        {
                            var rowsDeleted =
                                await DeleteOldLogsAsync(connection, tableName, columnName, retentionDate);

                            LogResults(tableName, startTime, endTime: DateTime.Now, retentionDate: retentionDate,
                                rowsDeleted: rowsDeleted);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Errore sulla tabella {tableName}: {ex.Message}");
                            LogErrore(tableName, startTime, DateTime.Now, retentionDate, ex);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'esecuzione: {ex.Message}");
                return false;
            }
        }

        private static void LeggiVoce(string voce, out string tableName, out string columnName)
        {
            var separatore = voce.IndexOf(':');
            if (separatore < 0)
            {
                tableName = voce;
                columnName = ColonnaDataPredefinita;
                return;
            }

            tableName = voce.Substring(0, separatore).Trim();
            var colonna = voce.Substring(separatore + 1).Trim();
            columnName = string.IsNullOrEmpty(colonna) ? ColonnaDataPredefinita : colonna;
        }

        private async Task<int> DeleteOldLogsAsync(SqlConnection connection, string tableName, string columnName, DateTime retentionDate)
        {
            var query = $"DELETE FROM [{Escape(tableName)}] WHERE [{Escape(columnName)}] < @RetentionDate";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RetentionDate", retentionDate);

                return await command.ExecuteNonQueryAsync();
            }
        }

        // Nome tabella e colonna arrivano dalla configurazione e finiscono concatenati nella query
        private static string Escape(string identificatore)
        {
            return identificatore.Replace("]", "]]");
        }

        private void LogResults(string tableName, DateTime startTime, DateTime endTime, DateTime retentionDate, int rowsDeleted)
        {
            var logEntry = $"DataInizio: {startTime:yyyy-MM-dd HH:mm:ss}, DataFine: {endTime:yyyy-MM-dd HH:mm:ss}, Tabella: {tableName}, DataInizioRetention: {retentionDate:yyyy-MM-dd HH:mm:ss}, RigheEliminate: {rowsDeleted}";

            Scrivi(logEntry);
        }

        private void LogErrore(string tableName, DateTime startTime, DateTime endTime, DateTime retentionDate, Exception ex)
        {
            var logEntry = $"DataInizio: {startTime:yyyy-MM-dd HH:mm:ss}, DataFine: {endTime:yyyy-MM-dd HH:mm:ss}, Tabella: {tableName}, DataInizioRetention: {retentionDate:yyyy-MM-dd HH:mm:ss}, Errore: {ex.Message}";

            Scrivi(logEntry);
        }

        private void Scrivi(string logEntry)
        {
            var logFileName = $"log_clean_retention_{DateTime.Now:yyyyMMdd}.txt";
            var logFilePath = Path.Combine(_model.pathReport, logFileName);

            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la scrittura del log: {ex.Message}");
            }
        }
    }
}
