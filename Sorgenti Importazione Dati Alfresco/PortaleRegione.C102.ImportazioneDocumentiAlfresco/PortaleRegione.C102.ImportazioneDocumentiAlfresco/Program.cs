using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.C102.ImportazioneDocumentiAlfresco
{
    internal class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            try
            {
                var basePath = ConfigurationManager.AppSettings["root"];
                var destinationPath = ConfigurationManager.AppSettings["destination"];

                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                // Trova tutti i file che corrispondono al pattern *_Testo_atto.*
                var files = Directory.GetFiles(basePath, "*_Testo_atto.*", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    // Estrarre informazioni dal nome del file utilizzando espressioni regolari
                    var match = Regex.Match(filePath, @"Atto_(\w+)_Id_Leg_Rif_(\d+)_Num_(\d+)_");

                    if (match.Success)
                    {
                        // Leggi il tipo di documento, legislatura e numero di atto dall'espressione regolare
                        var tipoDocumento = match.Groups[1].Value;
                        var legislatura = match.Groups[2].Value;
                        var numeroAtto = match.Groups[3].Value;

                        // Stampa le informazioni estratte
                        Debug(
                            $"Tipo Documento: {tipoDocumento}, Legislatura: {legislatura}, Numero Atto: {numeroAtto}");

                        var fileName = Path.GetFileName(filePath);
                        var newPath = Path.Combine(destinationPath, fileName);

                        // Copia il file nella nuova posizione
                        File.Copy(filePath, newPath, true);

                        var connectionString = ConfigurationManager.AppSettings["connection_string"];
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Esegui l'inserimento
                            var sql =
                                "UPDATE ATTI_DASI SET PATH_AllegatoGenerico = @PercorsoFile WHERE NAtto_Search = @NumeroAtto AND Legislatura = @Legislatura AND Tipo = @TipoDocumento";
                            var command = new SqlCommand(sql, connection);
                            command.Parameters.AddWithValue("@PercorsoFile",
                                $"~/DocumentiPEM/{Path.GetFileName(newPath)}"); // Utilizza la tilde nel percorso
                            command.Parameters.AddWithValue("@NumeroAtto", numeroAtto);
                            command.Parameters.AddWithValue("@Legislatura", legislatura);
                            command.Parameters.AddWithValue("@TipoDocumento", ParseTipoDocumento(tipoDocumento));

                            var rowsAffected = command.ExecuteNonQuery();

                            Debug($"Righe modificate: {rowsAffected}");

                            connection.Close();
                        }

                        // Stampa il percorso del nuovo file
                        Debug($"Percorso del nuovo file: {newPath}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug(e.Message);
                throw;
            }

            Debug("Operazione completata.");
            Console.ReadLine();
        }

        private static int ParseTipoDocumento(string tipoDocumento)
        {
            switch (tipoDocumento.ToLower())
            {
                case "interpellanza":
                {
                    return (int)TipoAttoEnum.ITL;
                }
                case "interrogazione":
                {
                    return (int)TipoAttoEnum.ITR;
                }
                case "mozione":
                {
                    return (int)TipoAttoEnum.MOZ;
                }
                case "interrogazione_question_time":
                {
                    return (int)TipoAttoEnum.IQT;
                }
                case "ordine_del_giorno":
                {
                    return (int)TipoAttoEnum.ODG;
                }
            }

            return 0;
        }

        private static void Debug(string message)
        {
            _log.Debug(message);
            Console.WriteLine(message);
        }
    }
}