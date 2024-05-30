using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using PortaleRegione.Common;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.C102.ImportazioneDocumentiAlfresco;

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

            var files = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
            var sb = new StringBuilder();
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var match = Regex.Match(fileName, @"Atto_(\w+)_Id_Leg_Rif_(\d+)_Numero_Atto_(\d+)");

                if (match.Success)
                {
                    var tipo = match.Groups[1].Value;
                    var legislatura = match.Groups[2].Value;
                    var numeroAtto = match.Groups[3].Value;
                    var tipoAtto = ParseTipoAtto(Path.GetFullPath(filePath));
                    var tipoDocumento = ParseTipoDocumento(tipo);

                    Debug(
                        $"Tipo Documento: {match.Groups[1].Value}[{tipoDocumento}], Legislatura: {legislatura}, Numero Atto: {numeroAtto}");

                    var connectionString = ConfigurationManager.AppSettings["connection_string"];
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        var atto = GetAtto(connection, legislatura, numeroAtto, tipoAtto);
                        if (atto == null)
                        {
                            Debug($"Atto non trovato nel database. Legislatura: {legislatura}, Numero Atto: {numeroAtto}, Tipo Atto: {tipoAtto}");
                            sb.AppendLine($"{fileName},atto_non_trovato");
                            continue;
                        }
                        
                        var newDirectoryPath = Path.Combine(destinationPath, legislatura, Utility.GetText_Tipo(tipoAtto), atto.Etichetta);
                        var newPath = Path.Combine(newDirectoryPath, fileName);
                        
                        var sql = @"
        INSERT INTO ATTI_DOCUMENTI (UIDDocumento, UIDAtto, Tipo, Data, Path, Titolo, Pubblica)
        VALUES (NEWID(), @UIDAtto, @TipoDocumento, GETDATE(), @PercorsoFile, @Titolo, @Pubblica)";
                        var command = new SqlCommand(sql, connection);
                        command.Parameters.AddWithValue("@PercorsoFile",
                            Path.Combine(legislatura, Utility.GetText_Tipo(tipoAtto), atto.Etichetta, fileName));

                        command.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
                        command.Parameters.AddWithValue("@Titolo", fileName);
                        command.Parameters.AddWithValue("@Pubblica", true);
                        command.Parameters.AddWithValue("@UIDAtto", atto.UIDAtto);

                        var stato = command.ExecuteNonQuery();
                        Debug($"Stato: {stato}");
                        
                        connection.Close();

                        sb.AppendLine(
                            $"{fileName},{tipoDocumento},{numeroAtto},{legislatura},OK");

                        if (!Directory.Exists(newDirectoryPath))
                            Directory.CreateDirectory(newDirectoryPath);

                        File.Copy(filePath, newPath, true);
                        Debug($"Percorso del nuovo file: {newPath}");
                    }
                }
                else
                {
                    sb.AppendLine($"{fileName},anomalia");
                }
            }

            var reportName = "docs_report.txt";
            var elaborationTicks = DateTime.Now.Ticks;
            var errorFolderPath =
                Path.Combine(Environment.CurrentDirectory, $"errore_{elaborationTicks}");
            Directory.CreateDirectory(errorFolderPath);

            var reportPath = Path.Combine(errorFolderPath, reportName);

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

    private static Guid? GetUidAtto(SqlConnection connection, string legislatura, string numeroAtto, int tipoAtto)
    {
        var sql = @"
SELECT TOP 1 UIDAtto
FROM ATTI_DASI
WHERE Legislatura = @Legislatura
AND NAtto_search = @NumeroAtto
AND Tipo = @TipoAtto";

        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Legislatura", legislatura);
            command.Parameters.AddWithValue("@NumeroAtto", numeroAtto);
            command.Parameters.AddWithValue("@TipoAtto", tipoAtto);

            var result = command.ExecuteScalar();
            return result != DBNull.Value ? (Guid?)result : null;
        }
    }

    private static Atto GetAtto(SqlConnection connection, string legislatura, string numeroAtto, int tipoAtto)
    {
        var sql = @"
SELECT TOP 1 UIDAtto, Etichetta
FROM ATTI_DASI
WHERE Legislatura = @Legislatura
AND NAtto_search = @NumeroAtto
AND Tipo = @TipoAtto";

        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Legislatura", legislatura);
            command.Parameters.AddWithValue("@NumeroAtto", numeroAtto);
            command.Parameters.AddWithValue("@TipoAtto", tipoAtto);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Atto
                    {
                        UIDAtto = reader.GetGuid(reader.GetOrdinal("UIDAtto")),
                        Etichetta = reader.GetString(reader.GetOrdinal("Etichetta"))
                    };
                }
                else
                {
                    return null;
                }
            }
        }
    }

    private static int ParseTipoAtto(string tipoDocumento)
    {
        var lowerTipoDocumento = tipoDocumento.ToLower();

        if (lowerTipoDocumento.Contains("interpellanza"))
        {
            return (int)TipoAttoEnum.ITL;
        }
        else if (lowerTipoDocumento.Contains("interrogazione"))
        {
            return (int)TipoAttoEnum.ITR;
        }
        else if (lowerTipoDocumento.Contains("mozione"))
        {
            return (int)TipoAttoEnum.MOZ;
        }
        else if (lowerTipoDocumento.Contains("interrogazione_question_time"))
        {
            return (int)TipoAttoEnum.IQT;
        }
        else if (lowerTipoDocumento.Contains("ordine_del_giorno"))
        {
            return (int)TipoAttoEnum.ODG;
        }

        return 0;
    }

    private static int ParseTipoDocumento(string tipoDocumento)
    {
        var lowerTipoDocumento = tipoDocumento.ToLower();

        if (lowerTipoDocumento.Contains("aggiuntivo"))
        {
            return (int)TipoDocumentoEnum.AGGIUNTIVO;
        }
        else if (lowerTipoDocumento.Contains("monitoraggio"))
        {
            return (int)TipoDocumentoEnum.MONITORAGGIO;
        }
        else if (lowerTipoDocumento.Contains("abbinamento"))
        {
            return (int)TipoDocumentoEnum.ABBINAMENTO;
        }
        else if (lowerTipoDocumento.Contains("chiusura_iter"))
        {
            return (int)TipoDocumentoEnum.CHIUSURA_ITER;
        }
        else if (lowerTipoDocumento.Contains("risposta"))
        {
            return (int)TipoDocumentoEnum.RISPOSTA;
        }

        return (int)TipoDocumentoEnum.TESTO_ALLEGATO;
    }

    private static void Debug(string message)
    {
        _log.Debug(message);
        Console.WriteLine(message);
    }

    public class Atto
    {
        public Guid UIDAtto { get; set; }
        public string Etichetta { get; set; }
    }
}