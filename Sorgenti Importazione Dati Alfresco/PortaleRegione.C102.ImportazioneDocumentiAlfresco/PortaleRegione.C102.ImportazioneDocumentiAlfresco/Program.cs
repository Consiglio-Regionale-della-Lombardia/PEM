using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
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
                var sb = new StringBuilder();
                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileName(filePath);
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

                        var newPath = Path.Combine(destinationPath, fileName);

                        // Copia il file nella nuova posizione
                        File.Copy(filePath, newPath, true);

                        var connectionString = ConfigurationManager.AppSettings["connection_string"];
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            var sql = @"
    DECLARE @Stato NVARCHAR(100);

    SET @Stato = 
        CASE
            WHEN EXISTS (SELECT 1 FROM [ATTI_DASI] 
                         WHERE PATH_AllegatoGenerico IS NOT NULL
                         AND NAtto_Search = @NumeroAtto 
                         AND Legislatura = @Legislatura 
                         AND Tipo = @TipoDocumento)
                THEN 'Non importato perché l''atto ha già un documento.'
            WHEN NOT EXISTS (SELECT 1 FROM [ATTI_DASI] 
                             WHERE NAtto_Search = @NumeroAtto 
                             AND Legislatura = @Legislatura 
                             AND Tipo = @TipoDocumento)
                THEN 'Non importato perché l''atto non è presente nel database.'
            ELSE 'OK'
        END;

    IF @Stato = 'OK'
    BEGIN
        UPDATE ATTI_DASI 
        SET PATH_AllegatoGenerico = @PercorsoFile 
        WHERE NAtto_Search = @NumeroAtto 
        AND Legislatura = @Legislatura 
        AND Tipo = @TipoDocumento
    END

    SELECT @Stato AS Stato";
                            var command = new SqlCommand(sql, connection);
                            command.Parameters.AddWithValue("@PercorsoFile",
                                $"~/DocumentiPEM/{Path.GetFileName(newPath)}"); // Utilizza la tilde nel percorso
                            command.Parameters.AddWithValue("@NumeroAtto", numeroAtto);
                            command.Parameters.AddWithValue("@Legislatura", legislatura);
                            command.Parameters.AddWithValue("@TipoDocumento", ParseTipoDocumento(tipoDocumento));
                            
                            var stato = (string)command.ExecuteScalar();
                            Debug($"Stato: {stato}");

                            connection.Close();

                            sb.AppendLine(
                                $"{fileName},{tipoDocumento},{numeroAtto},{legislatura},{(stato != "OK" ? stato : "OK")}");
                        }

                        // Stampa il percorso del nuovo file
                        Debug($"Percorso del nuovo file: {newPath}");
                    }
                    else
                    {
                        sb.AppendLine($"{fileName},anomalia");
                    }
                }

                // Costruisci il nome del file usando il timestamp e il numero di riga
                var reportName = "docs_report.txt";
                var elaborationTicks = DateTime.Now.Ticks;
                var errorFolderPath =
                    Path.Combine(Environment.CurrentDirectory, $"errore_{elaborationTicks}");
                Directory.CreateDirectory(errorFolderPath);
                // Costruisci il percorso completo del file all'interno della cartella "errori"
                var reportPath = Path.Combine(errorFolderPath, reportName);

                // Scrivi il messaggio dell'eccezione nel file
                File.WriteAllText(reportPath, sb.ToString());
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