using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using OfficeOpenXml;
using PortaleRegione.Common;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.C102.ImportazioneDocumentiAlfresco;

internal class Program
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(Program));
    public static readonly string connectionString = ConfigurationManager.AppSettings["connection_string"];

    private static void Main(string[] args)
    {
        XmlConfigurator.Configure();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Specificare il path del file Excel.");
                return;
            }

            var excelPath = args[0]; // Path al file Excel passato come argomento
            if (!File.Exists(excelPath))
            {
                Console.WriteLine("Il file Excel specificato non esiste.");
                return;
            }

            var basePath = ConfigurationManager.AppSettings["root"];
            var destinationPath = ConfigurationManager.AppSettings["destination"];

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            var files = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
            var sb = new StringBuilder();

            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            {
                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    // Estrazione delle informazioni dal nome del file
                    var legislatura = ExtractLegislatura(fileNameWithoutExtension);
                    var tipoDocumento = ParseTipoDocumento(fileNameWithoutExtension);
                    var numeroAtto = ExtractNumeroAtto(fileNameWithoutExtension);
                    var tipoAtto = ParseTipoAtto(filePath);

                    // #1272
                    if ((TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.TESTO_ALLEGATO)
                    {
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            // Cerca l'atto nel database
                            var atto = GetAtto(connection, legislatura, numeroAtto, tipoAtto);
                            if (atto == null)
                            {
                                Debug($"Atto non trovato nel database [{fileName}].");
                                sb.AppendLine($"{fileName},atto_non_trovato");
                                continue;
                            }

                            var checkUfficio = CheckAttoUfficio(connection, atto.UIDAtto);
                            if (checkUfficio == false && !tipoAtto.Equals((int)TipoAttoEnum.RIS))
                            {
                                continue;
                            }
                        }
                    }
                    
                    var uidDocumento = ExtractUidFromFileName(fileNameWithoutExtension);
                    var pubblicato = true;
                    var idOrgano = string.Empty;
                    var newId = Guid.NewGuid();
                    
                    // Nome standard del documento basato sul tipo
                    var nomeDocumento = GetNomeDocumentoStandard(tipoDocumento);
                    // #1292
                    if ((TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.TESTO_PRIVACY)
                    {
                        pubblicato = false;
                    }else if ((TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.AGGIUNTIVO
                              || (TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.MONITORAGGIO
                              || (TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.RISPOSTA)
                    {
                        var worksheetName = tipoDocumento switch
                        {
                            (int)TipoDocumentoEnum.AGGIUNTIVO => "atto_documento_aggiuntivo",
                            (int)TipoDocumentoEnum.MONITORAGGIO => "atto_documento_monitoraggio",
                            (int)TipoDocumentoEnum.RISPOSTA => "risposta_giunta",
                            _ => null
                        };

                        if (worksheetName == null)
                        {
                            Debug($"Foglio non trovato per il tipo documento: {fileName}");
                            sb.AppendLine($"{fileName},foglio_non_trovato");
                            continue;
                        }

                        var worksheet = package.Workbook.Worksheets[worksheetName];
                        if (worksheet == null)
                        {
                            Debug($"Foglio {worksheetName} non trovato nel file Excel.");
                            sb.AppendLine($"{fileName},foglio_non_trovato");
                            continue;
                        }

                        for (var row = 2; row <= worksheet.Dimension.End.Row; row++) // Salta la riga di intestazione
                        {
                            var colonnaName = 9;
                            if ((TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.RISPOSTA)
                            {
                                colonnaName = 4;
                            }

                            var guidInSheet = worksheet.Cells[row, colonnaName].Text.Trim();
                            if (string.Equals(guidInSheet, uidDocumento, StringComparison.OrdinalIgnoreCase))
                            {
                                if ((TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.MONITORAGGIO)
                                {
                                    nomeDocumento = worksheet.Cells[row, 11].Text.Trim() + Path.GetExtension(filePath);
                                    pubblicato = worksheet.Cells[row, 10].Text.Trim() == "1";
                                    break;
                                }

                                if ((TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.AGGIUNTIVO)
                                {
                                    nomeDocumento = worksheet.Cells[row, 13].Text.Trim() + Path.GetExtension(filePath);
                                    pubblicato = worksheet.Cells[row, 12].Text.Trim() == "1";
                                    break;
                                }

                                if ((TipoDocumentoEnum)tipoDocumento == TipoDocumentoEnum.RISPOSTA)
                                {
                                    idOrgano = worksheet.Cells[row, 6].Text;
                                }
                            }
                        }
                    }

                    Debug($"Elaborazione documento: {fileName}");

                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Cerca l'atto nel database
                        var atto = GetAtto(connection, legislatura, numeroAtto, tipoAtto);
                        if (atto == null)
                        {
                            Debug($"Atto non trovato nel database [{fileName}].");
                            sb.AppendLine($"{fileName},atto_non_trovato");
                            continue;
                        }

                        var uuid = Guid.NewGuid().ToString("N"); // "N" format rimuove i trattini
                        var systemFilename = $"{uuid}{Path.GetExtension(fileName)}";
                        var newDirectoryPath = Path.Combine(destinationPath, atto.GetLegislatura(),
                            Utility.GetText_Tipo(tipoAtto), atto.Etichetta);
                        var newPath = Path.Combine(newDirectoryPath, systemFilename);

                        var sql = @"
INSERT INTO ATTI_DOCUMENTI (Uid, UIDAtto, Tipo, Data, Path, Titolo, Pubblica)
VALUES (@Uid, @UIDAtto, @TipoDocumento, GETDATE(), @PercorsoFile, @Titolo, @Pubblica);";
                        if (!string.IsNullOrEmpty(idOrgano))
                        {
                            if (int.TryParse(idOrgano, out var organo))
                            {
                                sql +=
                                    $"UPDATE ATTI_RISPOSTE SET UIDDocumento='{newId}' WHERE UIDAtto='{atto.UIDAtto}' AND IdOrgano={organo};";
                            }
                        }

                        var command = new SqlCommand(sql, connection);
                        command.Parameters.AddWithValue("@PercorsoFile",
                            Path.Combine(atto.GetLegislatura(), Utility.GetText_Tipo(tipoAtto), atto.Etichetta,
                                systemFilename));
                        command.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
                        command.Parameters.AddWithValue("@Titolo", nomeDocumento); // Nome standard
                        command.Parameters.AddWithValue("@Pubblica", pubblicato);
                        command.Parameters.AddWithValue("@UIDAtto", atto.UIDAtto);
                        command.Parameters.AddWithValue("@Uid", newId);

                        var stato = command.ExecuteNonQuery();
                        Debug($"Stato inserimento: {stato}");

                        connection.Close();

                        sb.AppendLine($"{fileName},{tipoDocumento},{numeroAtto},{atto.GetLegislatura()},OK");

                        if (!Directory.Exists(newDirectoryPath))
                            Directory.CreateDirectory(newDirectoryPath);

                        File.Copy(filePath, newPath, true);
                        Debug($"Percorso del nuovo file: {newPath}");
                    }
                }
            }

            var reportName = "docs_report.txt";
            var elaborationTicks = DateTime.Now.Ticks;
            var errorFolderPath = Path.Combine(Environment.CurrentDirectory, $"errore_{elaborationTicks}");
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

    private static string ExtractLegislatura(string fileName)
    {
        // Supponendo che "Legislatura" sia dopo "_Id_Leg_Rif_"
        var parts = fileName.Split('_');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                i + 2 < parts.Length && parts[i + 1].Equals("Leg", StringComparison.OrdinalIgnoreCase))
            {
                return parts[i + 3]; // Restituisce il valore dopo "Id_Leg_Rif_"
            }
        }

        return "UNKNOWN";
    }

    private static string ExtractNumeroAtto(string fileName)
    {
        // Supponendo che "Numero Atto" sia dopo "_Numero_Atto_"
        var parts = fileName.Split('_');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Equals("Numero", StringComparison.OrdinalIgnoreCase) &&
                i + 2 < parts.Length && parts[i + 1].Equals("Atto", StringComparison.OrdinalIgnoreCase))
            {
                return parts[i + 2]; // Restituisce il valore dopo "Numero_Atto_"
            }
        }

        return "UNKNOWN";
    }

    private static string ExtractUidFromFileName(string fileName)
    {
        // Cerca l'UID nel nome del file usando un GUID pattern
        var match = Regex.Match(fileName,
            @"([a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return "UNKNOWN";
    }

    public static string GetNomeDocumentoStandard(int tipoDocumento)
    {
        switch ((TipoDocumentoEnum)tipoDocumento)
        {
            case TipoDocumentoEnum.TESTO_ALLEGATO:
                return "Allegato parte integrante atto";
            case TipoDocumentoEnum.AGGIUNTIVO:
                return "Documento aggiuntivo";
            case TipoDocumentoEnum.MONITORAGGIO:
                return "Documento monitoraggio";
            case TipoDocumentoEnum.ABBINAMENTO:
                return "Documento abbinamento";
            case TipoDocumentoEnum.CHIUSURA_ITER:
                return "Testo approvato";
            case TipoDocumentoEnum.RISPOSTA:
            case TipoDocumentoEnum.TESTO_RISPOSTA:
                return "Testo risposta";
            case TipoDocumentoEnum.TESTO_PRIVACY:
                return "Documento originale"; // #1306
            case TipoDocumentoEnum.VERBALE_VOTAZIONE:
                return "Verbale votazione";
            default:
                throw new ArgumentOutOfRangeException($"Tipo documento non riconosciuto: {tipoDocumento}");
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
    private static bool CheckAttoUfficio(SqlConnection connection, Guid uidAtto)
    {
        var sql = @"
SELECT TOP 1 UIDAtto
FROM ATTI_FIRME
WHERE UIDAtto = @uidatto
AND ufficio = 1";

        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@uidatto", uidAtto);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return true;
                }

                return false;
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
        if (lowerTipoDocumento.Contains("interrogazione_question_time"))
        {
            return (int)TipoAttoEnum.IQT;
        }
        if (lowerTipoDocumento.Contains("interrogazione"))
        {
            return (int)TipoAttoEnum.ITR;
        }
        if (lowerTipoDocumento.Contains("mozione"))
        {
            return (int)TipoAttoEnum.MOZ;
        }
        if (lowerTipoDocumento.Contains("ordine_del_giorno"))
        {
            return (int)TipoAttoEnum.ODG;
        }
        if (lowerTipoDocumento.Contains("risoluzione"))
        {
            return (int)TipoAttoEnum.RIS;
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
        else if (lowerTipoDocumento.Contains("testo_risposta"))
        {
            return (int)TipoDocumentoEnum.TESTO_RISPOSTA;
        }
        else if (lowerTipoDocumento.Contains("risposta_associata"))
        {
            return (int)TipoDocumentoEnum.RISPOSTA;
        }
        else if (lowerTipoDocumento.Contains("privacy"))
        {
            return (int)TipoDocumentoEnum.TESTO_PRIVACY;
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

        public string GetLegislatura()
        {
            if (!string.IsNullOrEmpty(Etichetta))
            {
                var parti = Etichetta.Split('_');
                if (parti.Length > 0)
                    return parti[parti.Length - 1];
            }

            return string.Empty;
        }
    }
}