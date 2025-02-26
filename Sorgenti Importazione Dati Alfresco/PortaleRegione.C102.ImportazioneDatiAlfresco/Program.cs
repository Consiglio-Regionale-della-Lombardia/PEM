using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using OfficeOpenXml;
using PortaleRegione.Crypto;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.C102.ImportazioneDatiAlfresco
{
    internal class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            if (args.Length == 0)
            {
                Console.WriteLine("Impostare i paramentri per l'esecuzione della console application");
                return;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var percorsoXLS = args[0];
            var foglioAtti = args[1].Split(',');
            var foglioFirme = args[2];
            var foglioRispostaAssociata = args[3];
            var foglioRispostaGiunta = args[4];
            var foglioMonitoraggioCommissioni = args[5];
            var foglioMonitoraggioGiunta = args[6];
            var foglioAssociazione_Sind_Ind = args[7];
            var foglioAssociazione_Gea = args[9];
            var foglioAssociazione_Altro = args[8];
            var foglioProponentiCommissioni = args[10];

            var legislatureFromDatabase = GetLegislatureFromDataBase();
            var proponenteId = GetDefaultIdFromDatabase();

            if (!File.Exists(percorsoXLS))
                throw new Exception("File non trovato");

            using (var package = new ExcelPackage(new FileInfo(percorsoXLS)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    throw new Exception("Nessun worksheet trovato nel file.");
                }

                var worksheetFirme = package.Workbook.Worksheets.First(w => w.Name.Equals(foglioFirme));
                var cellsFirme = worksheetFirme.Cells;
                var rowCountF = worksheetFirme.Dimension.Rows;

                var worksheetRisposteAssociate =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioRispostaAssociata));
                var cellsRisposteAssociate = worksheetRisposteAssociate.Cells;
                var rowCountRA = worksheetRisposteAssociate.Dimension.Rows;

                var worksheetRisposteGiunta =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioRispostaGiunta));
                var cellsRisposteGiunta = worksheetRisposteGiunta.Cells;
                var rowCountRG = worksheetRisposteGiunta.Dimension.Rows;

                var worksheetMonitoraggioCommissioni =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioMonitoraggioCommissioni));
                var cellsMonitoraggioCommissioni = worksheetMonitoraggioCommissioni.Cells;
                var rowCountMC = worksheetMonitoraggioCommissioni.Dimension.Rows;

                var worksheetMonitoraggioGiunta =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioMonitoraggioGiunta));
                var cellsMonitoraggioGiunta = worksheetMonitoraggioGiunta.Cells;
                var rowCountMG = worksheetMonitoraggioGiunta.Dimension.Rows;

                var worksheetProponentiCommissioni =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioProponentiCommissioni));
                var cellsProponentiCommissioni = worksheetProponentiCommissioni.Cells;
                var rowCountPC = worksheetProponentiCommissioni.Dimension.Rows;

                var worksheetAssociazione_Sind_Ind =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioAssociazione_Sind_Ind));
                var cellsAssociazione_Sind_Ind = worksheetAssociazione_Sind_Ind.Cells;
                var rowCountASind_Ind = worksheetAssociazione_Sind_Ind.Dimension.Rows;

                var abbinamentiDasi = new List<AbbinamentoDasi>();
                for (var rowASind_Ind = 2; rowASind_Ind <= rowCountASind_Ind; rowASind_Ind++)
                {
                    var abbinamentoSind_Ind = new AbbinamentoDasi
                    {
                        idNodoAlfresco = cellsAssociazione_Sind_Ind[rowASind_Ind, 2].Value.ToString(),
                        NumeroAtto = cellsAssociazione_Sind_Ind[rowASind_Ind, 4].Value.ToString(),
                        Legislatura = cellsAssociazione_Sind_Ind[rowASind_Ind, 6].Value.ToString(),
                        idAssociazione = cellsAssociazione_Sind_Ind[rowASind_Ind, 7].Value.ToString()
                    };

                    abbinamentiDasi.Add(abbinamentoSind_Ind);
                }

                var worksheetAssociazione_Gea =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioAssociazione_Gea));
                var cellsAssociazione_Gea = worksheetAssociazione_Gea.Cells;
                var rowCountASind_Gea = worksheetAssociazione_Gea.Dimension.Rows;

                var abbinamentiGea = new List<AbbinamentoGea>();
                for (var rowAGea = 2; rowAGea <= rowCountASind_Gea; rowAGea++)
                    try
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(cellsAssociazione_Gea[rowAGea, 3].Value)))
                            continue;

                        var abbinamentoGea = new AbbinamentoGea
                        {
                            idNodoAlfresco = cellsAssociazione_Gea[rowAGea, 2].Value.ToString(),
                            NumeroAtto = cellsAssociazione_Gea[rowAGea, 3].Value.ToString(),
                            Legislatura = cellsAssociazione_Gea[rowAGea, 5].Value.ToString(),
                            NumeroAtto_Gea = cellsAssociazione_Gea[rowAGea, 8].Value.ToString(),
                            TipoAtto_Gea = cellsAssociazione_Gea[rowAGea, 9].Value.ToString(),
                            Ufficiale = Convert.ToString(cellsAssociazione_Gea[rowAGea, 10].Value)
                        };

                        if (abbinamentoGea.IsUfficiale())
                            abbinamentiGea.Add(abbinamentoGea);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                var abbinamentiRaggruppatiPerLegislatura = abbinamentiGea
                    .GroupBy(a => a.Legislatura)
                    .Select(gruppo => new
                    {
                        Legislatura = gruppo.Key,
                        Abbinamenti = gruppo.ToList()
                    })
                    .ToList();

                var updateAttiChiusiRitirati =
                    @"UPDATE ATTI_DASI SET IDStato=14 WHERE IDStato=15";
                using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                {
                    connection.Open();
                    using (var commandUpdateAttiChiusiRitirati = new SqlCommand(updateAttiChiusiRitirati, connection))
                    {
                        var resQuery = commandUpdateAttiChiusiRitirati.ExecuteNonQuery();
                    }
                }

                var insertSeduta =
                    @"INSERT INTO SEDUTE (UIDSeduta, Data_seduta, Data_apertura, Data_effettiva_inizio, Data_effettiva_fine, id_legislatura, Note, DataCreazione) 
                     VALUES (@UIDSeduta, @Data_seduta, @Data_apertura, @Data_effettiva_inizio, @Data_effettiva_fine, @id_legislatura, @Note, GETDATE())";

                var checkSeduta =
                    @"SELECT COUNT(1) FROM SEDUTE WHERE CONVERT(DATE, Data_seduta) = CONVERT(DATE, @Data_seduta)";

                using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                {
                    connection.Open();
                    foreach (var item in abbinamentiRaggruppatiPerLegislatura)
                    {
                        var uidSeduta = Guid.NewGuid();
                        var legislaturaSeduta =
                            legislatureFromDatabase.First(l => l.id_legislatura.Equals(int.Parse(item.Legislatura)));

                        // Controlla se esiste già una seduta con la stessa data
                        bool sedutaExists;
                        using (var commandCheck = new SqlCommand(checkSeduta, connection))
                        {
                            commandCheck.Parameters.AddWithValue("@Data_seduta",
                                legislaturaSeduta.durata_legislatura_da.ToString("yyyy-MM-dd"));
                            sedutaExists = (int)commandCheck.ExecuteScalar() > 0;
                        }

                        // Inserisce la seduta solo se non esiste
                        if (!sedutaExists)
                        {
                            using (var command = new SqlCommand(insertSeduta, connection))
                            {
                                // Assegna i valori dei parametri
                                command.Parameters.AddWithValue("@UIDSeduta", uidSeduta);
                                command.Parameters.AddWithValue("@Data_seduta",
                                    legislaturaSeduta.durata_legislatura_da);
                                command.Parameters.AddWithValue("@Data_apertura",
                                    legislaturaSeduta.durata_legislatura_da);
                                command.Parameters.AddWithValue("@Data_effettiva_inizio",
                                    legislaturaSeduta.durata_legislatura_da);
                                command.Parameters.AddWithValue("@Data_effettiva_fine",
                                    legislaturaSeduta.durata_legislatura_da);
                                command.Parameters.AddWithValue("@id_legislatura", legislaturaSeduta.id_legislatura);
                                command.Parameters.AddWithValue("@Note", "Contenitore atti importati da Alfresco");

                                var resQuery = command.ExecuteNonQuery();

                                if (resQuery == 1)
                                {
                                    foreach (var abbinamentoGea in item.Abbinamenti)
                                    {
                                        abbinamentoGea.UIDAtto = Guid.NewGuid();

                                        var insertAttoGea = @"IF NOT EXISTS 
                            (SELECT 1 FROM ATTI WHERE NAtto = @NAtto AND IDTipoAtto = @IDTipoAtto AND UIDSeduta = @UIDSeduta)
                            BEGIN
                                INSERT INTO ATTI (UIDAtto, NAtto, IDTipoAtto, UIDSeduta)
                                    VALUES (@UIDAtto, @NAtto, @IDTipoAtto, @UIDSeduta)        
                            END";
                                        using (var commandAttoGea = new SqlCommand(insertAttoGea, connection))
                                        {
                                            commandAttoGea.Parameters.AddWithValue("@UIDAtto", abbinamentoGea.UIDAtto);
                                            commandAttoGea.Parameters.AddWithValue("@NAtto",
                                                abbinamentoGea.NumeroAtto_Gea);
                                            commandAttoGea.Parameters.AddWithValue("@UIDSeduta", uidSeduta);
                                            commandAttoGea.Parameters.AddWithValue("@IDTipoAtto",
                                                (int)ConvertToEnumTipoAtto(abbinamentoGea.TipoAtto_Gea));
                                            commandAttoGea.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Seduta già esistente per la data {legislaturaSeduta.durata_legislatura_da:yyyy-MM-dd}. Salto l'inserimento.");
                        }
                    }
                }


                var sb = new StringBuilder();
                var elaborationTicks = DateTime.Now.Ticks;
                // Costruisci il percorso della cartella "errore"
                var errorFolderPath =
                    Path.Combine(Environment.CurrentDirectory, $"errore_{elaborationTicks}");
                // Crea la cartella "errori" se non esiste già
                Directory.CreateDirectory(errorFolderPath);
                var fileName = "dati_report.txt";
                var filePath = Path.Combine(errorFolderPath, fileName);

                var attiImportati = new Dictionary<string, AttoImportato>();
                foreach (var foglio in foglioAtti)
                    using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                    {
                        connection.Open();
                        var worksheetAtti = package.Workbook.Worksheets.First(w => w.Name.Equals(foglio));
                        var cellsAtti = worksheetAtti.Cells;

                        var rowCount = worksheetAtti.Dimension.Rows;
                        sb.Clear();


                        for (var row = 2; row <= rowCount; row++)
                        {
                            var tipoAttoFromAlfresco = Convert.ToString(cellsAtti[row, 4].Value);
                            var legislaturaFromAlfresco = Convert.ToString(cellsAtti[row, 47].Value);
                            var numeroAtto = Convert.ToString(cellsAtti[row, 19].Value);

                            try
                            {
                                // #1295
                                var eliminatoInAlfresco = Convert.ToString(cellsAtti[row, 37].Value);
                                if (!string.IsNullOrEmpty(eliminatoInAlfresco))
                                {
                                    throw new Exception("Atto eliminato in Alfresco. Non importato.");
                                }

                                var attoImportato = new AttoImportato();
                                attoImportato.Legislatura = legislaturaFromAlfresco;
                                attoImportato.Tipo = tipoAttoFromAlfresco;
                                attoImportato.NumeroAtto = numeroAtto;

                                var chkf = 0;
                                var id_gruppo = 0;
                                // nodeid alfresco 
                                var nodeIdFromAlfresco = Convert.ToString(cellsAtti[row, 2].Value);
                                attoImportato.idNodoAlfresco = nodeIdFromAlfresco;

                                var statoId = (int)StatiAttoEnum.COMPLETATO;

                                //legislatura
                                if (string.IsNullOrEmpty(legislaturaFromAlfresco))
                                    throw new Exception("Legislatura non valida");

                                var legislatura =
                                    legislatureFromDatabase.FirstOrDefault(i =>
                                        i.id_legislatura.Equals(Convert.ToInt16(legislaturaFromAlfresco)));

                                if (legislatura == null)
                                    throw new Exception(
                                        $"Legislatura {legislaturaFromAlfresco} non trovata nel database.");

                                var tipoAttoEnum = ConvertToEnumTipoAtto(tipoAttoFromAlfresco);

                                //tipo mozione
                                var tipoMozioneAttoFromAlfresco = Convert.ToString(cellsAtti[row, 22].Value);
                                var tipoMozione = ParseDescr2Enum_TipoMozione(tipoMozioneAttoFromAlfresco);

                                //etichette
                                var etichettaAtto = $"{tipoAttoEnum}_{numeroAtto}_{legislatura.num_legislatura}";
                                var etichettaAtto_Cifrata =
                                    CryptoHelper.EncryptString(etichettaAtto, AppsettingsConfiguration.MASTER_KEY);
                                
                                var queryExists = @"SELECT UIDAtto FROM [ATTI_DASI] WHERE Etichetta = @Etichetta";

                                var update = false;
                                using (var commandExists = new SqlCommand(queryExists, connection))
                                {
                                    commandExists.Parameters.AddWithValue("@Etichetta", etichettaAtto);

                                    var result = commandExists.ExecuteScalar();

                                    if (result != null)
                                    {
                                        attoImportato.UidAtto = new Guid(result.ToString());
                                        update = true;
                                    }
                                }

                                if (!update)
                                    attoImportato.UidAtto = Guid.NewGuid();

                                #region NOTE

                                #region NOTE CHIUSURA ITER

                                var noteChiusuraIter = Convert.ToString(cellsAtti[row, 29].Value);

                                if (string.IsNullOrEmpty(noteChiusuraIter))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertNotaChiusuraIter =
                                        @"INSERT INTO ATTI_NOTE (Uid, UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertNotaChiusuraIter =
                                           new SqlCommand(queryInsertNotaChiusuraIter, connection))
                                    {
                                        commandInsertNotaChiusuraIter.Parameters.AddWithValue("@UIDAtto",
                                            attoImportato.UidAtto);
                                        commandInsertNotaChiusuraIter.Parameters.AddWithValue("@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertNotaChiusuraIter.Parameters.AddWithValue("@Tipo",
                                            (int)TipoNotaEnum.CHIUSURA_ITER);
                                        commandInsertNotaChiusuraIter.Parameters.AddWithValue("@Nota",
                                            noteChiusuraIter);
                                        commandInsertNotaChiusuraIter.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #region NOTE RISPOSTA

                                var noteRisposta = Convert.ToString(cellsAtti[row, 45].Value);

                                if (string.IsNullOrEmpty(noteRisposta))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertNotaRisposta =
                                        @"INSERT INTO ATTI_NOTE (Uid, UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertNotaRisposta =
                                           new SqlCommand(queryInsertNotaRisposta, connection))
                                    {
                                        commandInsertNotaRisposta.Parameters.AddWithValue("@UIDAtto",
                                            attoImportato.UidAtto);
                                        commandInsertNotaRisposta.Parameters.AddWithValue("@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertNotaRisposta.Parameters.AddWithValue("@Tipo",
                                            (int)TipoNotaEnum.RISPOSTA);
                                        commandInsertNotaRisposta.Parameters.AddWithValue("@Nota", noteRisposta);
                                        commandInsertNotaRisposta.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #region NOTE AGGIUNTIVE SIDAC

                                var noteAggiuntive = Convert.ToString(cellsAtti[row, 24].Value);

                                if (string.IsNullOrEmpty(noteAggiuntive))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertNoteAggiuntive =
                                        @"INSERT INTO ATTI_NOTE (Uid,UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertNoteAggiuntive =
                                           new SqlCommand(queryInsertNoteAggiuntive, connection))
                                    {
                                        commandInsertNoteAggiuntive.Parameters.AddWithValue("@UIDAtto",
                                            attoImportato.UidAtto);
                                        commandInsertNoteAggiuntive.Parameters.AddWithValue("@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertNoteAggiuntive.Parameters.AddWithValue("@Tipo",
                                            (int)TipoNotaEnum.GENERALE_PRIVATA);
                                        commandInsertNoteAggiuntive.Parameters.AddWithValue("@Nota", noteAggiuntive);
                                        commandInsertNoteAggiuntive.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #region NOTE AGGIUNTIVE DASI

                                var noteAggiuntive2 = Convert.ToString(cellsAtti[row, 25].Value);

                                if (string.IsNullOrEmpty(noteAggiuntive2))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertNoteAggiuntive2 =
                                        @"INSERT INTO ATTI_NOTE (Uid, UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertNoteAggiuntive2 =
                                           new SqlCommand(queryInsertNoteAggiuntive2, connection))
                                    {
                                        commandInsertNoteAggiuntive2.Parameters.AddWithValue("@UIDAtto",
                                            attoImportato.UidAtto);
                                        commandInsertNoteAggiuntive2.Parameters.AddWithValue("@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertNoteAggiuntive2.Parameters.AddWithValue("@Tipo",
                                            (int)TipoNotaEnum.GENERALE_PRIVATA);
                                        commandInsertNoteAggiuntive2.Parameters.AddWithValue("@Nota", noteAggiuntive2);
                                        commandInsertNoteAggiuntive2.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #region NOTE ANNOTAZIONI

                                // #1029
                                var noteAnnotazioni = Convert.ToString(cellsAtti[row, 26].Value);

                                if (string.IsNullOrEmpty(noteAnnotazioni))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertNoteAnnotazioni =
                                        @"INSERT INTO ATTI_NOTE (Uid, UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertNoteAnnotazioni =
                                           new SqlCommand(queryInsertNoteAnnotazioni, connection))
                                    {
                                        commandInsertNoteAnnotazioni.Parameters.AddWithValue("@UIDAtto",
                                            attoImportato.UidAtto);
                                        commandInsertNoteAnnotazioni.Parameters.AddWithValue("@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertNoteAnnotazioni.Parameters.AddWithValue("@Tipo",
                                            (int)TipoNotaEnum.GENERALE_PRIVATA);
                                        commandInsertNoteAnnotazioni.Parameters.AddWithValue("@Nota", noteAnnotazioni);
                                        commandInsertNoteAnnotazioni.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #region NOTE PRIVACY privacy_dati_personali_sensibili_note

                                var noteprivacy_dati_personali_sensibili_note =
                                    Convert.ToString(cellsAtti[row, 61].Value);

                                if (string.IsNullOrEmpty(noteprivacy_dati_personali_sensibili_note))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertPrivacy_dati_personali_sensibili_note =
                                        @"INSERT INTO ATTI_NOTE (Uid, UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertPrivacy_dati_personali_sensibili_note =
                                           new SqlCommand(queryInsertPrivacy_dati_personali_sensibili_note, connection))
                                    {
                                        commandInsertPrivacy_dati_personali_sensibili_note.Parameters.AddWithValue(
                                            "@UIDAtto", attoImportato.UidAtto);
                                        commandInsertPrivacy_dati_personali_sensibili_note.Parameters.AddWithValue(
                                            "@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertPrivacy_dati_personali_sensibili_note.Parameters.AddWithValue(
                                            "@Tipo",
                                            (int)TipoNotaEnum.GENERALE_PRIVATA);
                                        commandInsertPrivacy_dati_personali_sensibili_note.Parameters.AddWithValue(
                                            "@Nota",
                                            noteprivacy_dati_personali_sensibili_note);
                                        commandInsertPrivacy_dati_personali_sensibili_note.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #region NOTE PRIVACY privacy_dati_personali_giudiziari_note

                                var noteprivacy_dati_personali_giudiziari_note =
                                    Convert.ToString(cellsAtti[row, 62].Value);

                                if (string.IsNullOrEmpty(noteprivacy_dati_personali_giudiziari_note))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertPrivacy_dati_personali_giudiziari_note =
                                        @"INSERT INTO ATTI_NOTE (Uid, UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertPrivacy_dati_personali_giudiziari_note =
                                           new SqlCommand(queryInsertPrivacy_dati_personali_giudiziari_note,
                                               connection))
                                    {
                                        commandInsertPrivacy_dati_personali_giudiziari_note.Parameters.AddWithValue(
                                            "@UIDAtto", attoImportato.UidAtto);
                                        commandInsertPrivacy_dati_personali_giudiziari_note.Parameters.AddWithValue(
                                            "@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertPrivacy_dati_personali_giudiziari_note.Parameters.AddWithValue(
                                            "@Tipo",
                                            (int)TipoNotaEnum.GENERALE_PRIVATA);
                                        commandInsertPrivacy_dati_personali_giudiziari_note.Parameters.AddWithValue(
                                            "@Nota",
                                            noteprivacy_dati_personali_giudiziari_note);
                                        commandInsertPrivacy_dati_personali_giudiziari_note.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #region NOTE PRIVACY privacy_dati_personali_semplici_note

                                var noteprivacy_dati_personali_semplici_note =
                                    Convert.ToString(cellsAtti[row, 63].Value);

                                if (string.IsNullOrEmpty(noteprivacy_dati_personali_semplici_note))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    var queryInsertPrivacy_dati_personali_semplici_note =
                                        @"INSERT INTO ATTI_NOTE (Uid, UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (NEWID(), @UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    using (var commandInsertPrivacy_dati_personali_semplici_note =
                                           new SqlCommand(queryInsertPrivacy_dati_personali_semplici_note, connection))
                                    {
                                        commandInsertPrivacy_dati_personali_semplici_note.Parameters.AddWithValue(
                                            "@UIDAtto", attoImportato.UidAtto);
                                        commandInsertPrivacy_dati_personali_semplici_note.Parameters.AddWithValue(
                                            "@UIDPersona",
                                            Guid.Parse(
                                                "AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                        commandInsertPrivacy_dati_personali_semplici_note.Parameters.AddWithValue(
                                            "@Tipo",
                                            (int)TipoNotaEnum.GENERALE_PRIVATA);
                                        commandInsertPrivacy_dati_personali_semplici_note.Parameters.AddWithValue(
                                            "@Nota",
                                            noteprivacy_dati_personali_semplici_note);
                                        commandInsertPrivacy_dati_personali_semplici_note.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                #endregion

                                #region FIRME

                                if (!update)
                                {
                                    var primo_firmatario_assigned = false;

                                    // Itera attraverso le righe del foglio delle firme
                                    for (var rowF = 2; rowF <= rowCountF; rowF++)
                                    {
                                        // Ottieni il valore della cella nella seconda colonna della riga corrente
                                        var valoreCella = cellsFirme[rowF, 2].Value;

                                        // Verifica se il valore della cella corrisponde al numeroAtto
                                        if (valoreCella != null && valoreCella.ToString() == nodeIdFromAlfresco)
                                        {
                                            var data_firma = string.Empty;
                                            // Se il valore della cella corrisponde, aggiungi la riga alla lista
                                            if (cellsFirme[rowF, 8].Value != null &&
                                                !string.IsNullOrEmpty(cellsFirme[rowF, 8].Value.ToString()))
                                                data_firma = ParseDateTime(Convert.ToString(cellsFirme[rowF, 8].Value))
                                                    .ToString("dd/MM/yyyy HH:mm:ss");
                                            if (string.IsNullOrEmpty(data_firma))
                                                throw new Exception(
                                                    $"Data firma non valida [{cellsFirme[rowF, 8].Value}].");

                                            var data_ritiro_firma = Convert.ToString(cellsFirme[rowF, 9].Value);
                                            var id_persona = cellsFirme[rowF, 10].Value.ToString();
                                            var nome_firmatario = cellsFirme[rowF, 14].Value.ToString().Trim();
                                            var primo_firmatario_insert = false;
                                            if (!primo_firmatario_assigned)
                                            {
                                                primo_firmatario_insert = Convert.ToBoolean(cellsFirme[rowF, 16].Value);
                                                primo_firmatario_assigned = primo_firmatario_insert;
                                            }

                                            var find_uid_persona = GetUidPersona(connection, int.Parse(id_persona));

                                            if (!Guid.TryParse(Convert.ToString(find_uid_persona), out var guid))
                                            {
                                                find_uid_persona = Guid.NewGuid();
                                                var queryInsertPersona =
                                                    @"INSERT INTO join_persona_AD (UID_persona, id_persona)
                                       VALUES
                                     (@UID_persona, @id_persona)";
                                                // Esegui la query di inserimento dei dati nella tabella ATTI_FIRME
                                                using (var commandInsertPersona =
                                                       new SqlCommand(queryInsertPersona, connection))
                                                {
                                                    commandInsertPersona.Parameters.AddWithValue("@UID_persona",
                                                        find_uid_persona);
                                                    commandInsertPersona.Parameters.AddWithValue("@id_persona",
                                                        id_persona);
                                                    commandInsertPersona
                                                        .ExecuteNonQuery(); // Esegui l'inserimento dei dati
                                                }
                                            }

                                            var queryInsertFirmatario =
                                                @"IF NOT EXISTS (
                                                SELECT 1
                                                FROM ATTI_FIRME
                                                WHERE UIDAtto = @UIDAtto AND UID_persona = @UID_persona
                                            )
                                            BEGIN
                                                INSERT INTO ATTI_FIRME (UIDAtto, UID_persona, FirmaCert, Data_firma{FIELD_DATA_RITIRO_FIRMA}, PrimoFirmatario, id_gruppo, Timestamp, Valida, ufficio)
                                                    VALUES
                                                (@UIDAtto, @UID_persona, @FirmaCert, @Data_firma{PARAM_DATA_RITIRO_FIRMA}, @PrimoFirmatario, @id_gruppo, @Timestamp, 1, 1)
                                            END";

                                            if (string.IsNullOrEmpty(data_ritiro_firma))
                                                queryInsertFirmatario = queryInsertFirmatario
                                                    .Replace("{FIELD_DATA_RITIRO_FIRMA}", "")
                                                    .Replace("{PARAM_DATA_RITIRO_FIRMA}", "");
                                            else
                                                queryInsertFirmatario = queryInsertFirmatario
                                                    .Replace("{FIELD_DATA_RITIRO_FIRMA}", ", Data_ritirofirma")
                                                    .Replace("{PARAM_DATA_RITIRO_FIRMA}", ", @Data_ritirofirma");
                                            
                                            var id_gruppo_firmatario =
                                                GetIdGruppoFromFunction(int.Parse(id_persona), ParseDateTime(data_firma));

                                            // Esegui la query di inserimento dei dati nella tabella ATTI_FIRME
                                            using (var commandInsertFirmatario =
                                                   new SqlCommand(queryInsertFirmatario, connection))
                                            {
                                                commandInsertFirmatario.Parameters.AddWithValue("@UIDAtto",
                                                    attoImportato.UidAtto);
                                                commandInsertFirmatario.Parameters.AddWithValue("@UID_persona",
                                                    find_uid_persona);
                                                commandInsertFirmatario.Parameters.AddWithValue("@FirmaCert",
                                                    CryptoHelper.EncryptString(nome_firmatario,
                                                        AppsettingsConfiguration.MASTER_KEY));
                                                commandInsertFirmatario.Parameters.AddWithValue("@Data_firma",
                                                    CryptoHelper.EncryptString(data_firma,
                                                        AppsettingsConfiguration.MASTER_KEY));
                                                if (string.IsNullOrEmpty(data_ritiro_firma))
                                                {
                                                    // Ignored
                                                }
                                                else
                                                {
                                                    commandInsertFirmatario.Parameters.AddWithValue("@Data_ritirofirma",
                                                        CryptoHelper.EncryptString(
                                                            ParseDateTime(data_ritiro_firma)
                                                                .ToString("dd/MM/yyyy HH:mm:ss"),
                                                            AppsettingsConfiguration.MASTER_KEY));
                                                }

                                                commandInsertFirmatario.Parameters.AddWithValue("@PrimoFirmatario",
                                                    Convert.ToBoolean(primo_firmatario_insert));
                                                
                                                commandInsertFirmatario.Parameters.AddWithValue("@id_gruppo",
                                                    id_gruppo_firmatario);
                                                commandInsertFirmatario.Parameters.AddWithValue("@Timestamp",
                                                    ParseDateTime(data_firma));
                                                commandInsertFirmatario
                                                    .ExecuteNonQuery(); // Esegui l'inserimento dei dati
                                            }

                                            chkf++;

                                            if (Convert.ToBoolean(primo_firmatario_insert))
                                            {
                                                proponenteId = (Guid)find_uid_persona;
                                                id_gruppo = id_gruppo_firmatario;
                                            }
                                        }
                                    }

                                    if (tipoAttoEnum != TipoAttoEnum.RIS)
                                    {
                                        if (chkf == 0) throw new Exception("Nessuna firma trovata per l'atto.");

                                        if (chkf > 0 && id_gruppo == 0)
                                            throw new Exception("Nessun proponente trovato.");
                                    }
                                }

                                #endregion

                                #region RISPOSTE ASSOCIATE

                                // Itera attraverso le righe del foglio delle risposte associate
                                for (var rowRA = 2; rowRA <= rowCountRA; rowRA++)
                                {
                                    var valoreCella = cellsRisposteAssociate[rowRA, 2]?.Value?.ToString();
                                    if (valoreCella == null || valoreCella != nodeIdFromAlfresco) continue;

                                    var tipoOrgano = (int)TipoOrganoEnum.COMMISSIONE;
                                    var sub_query =
                                        @"(SELECT TOP(1) nome_organo FROM dbo.organi WHERE id_organo = @IdOrgano)";
                                    var tipoRispostaAssociata = cellsRisposteAssociate[rowRA, 10]?.Value?.ToString();
                                    var tipoRispostaAssociataInt = ConvertToIntTipoRisposta(tipoRispostaAssociata);
                                    if (tipoAttoEnum == TipoAttoEnum.IQT)
                                    {
                                        tipoRispostaAssociataInt = (int)TipoRispostaEnum.IMMEDIATA;
                                    }

                                    var dataRispostaAssociata = cellsRisposteAssociate[rowRA, 12]?.Value;
                                    var dataTrasmissioneRispostaAssociata = cellsRisposteAssociate[rowRA, 13]?.Value;
                                    var dataTrattazioneRispostaAssociata = cellsRisposteAssociate[rowRA, 14]?.Value;
                                    var idCommissioneRispostaAssociata = cellsRisposteAssociate[rowRA, 16]?.Value;

                                    var idOrgano = idCommissioneRispostaAssociata != null
                                        ? Convert.ToInt32(idCommissioneRispostaAssociata)
                                        : (int?)null;

                                    if ((TipoRispostaEnum)tipoRispostaAssociataInt == TipoRispostaEnum.SCRITTA
                                        || (TipoRispostaEnum)tipoRispostaAssociataInt == TipoRispostaEnum.ORALE
                                        || (TipoRispostaEnum)tipoRispostaAssociataInt == TipoRispostaEnum.IMMEDIATA)
                                    {
                                        var nodeIdRispostaMadre = cellsRisposteAssociate[rowRA, 7]?.Value?.ToString();
                                        for (var rowRG = 2; rowRG <= rowCountRG; rowRG++)
                                        {
                                            var valoreCellaRiferimento =
                                                cellsRisposteGiunta[rowRG, 2]?.Value?.ToString();
                                            if (valoreCellaRiferimento == null ||
                                                valoreCellaRiferimento != nodeIdRispostaMadre) continue;

                                            var idCommissioneGiunta = cellsRisposteGiunta[rowRG, 6]?.Value;
                                            tipoOrgano = (int)TipoOrganoEnum.GIUNTA;
                                            sub_query =
                                                @"(SELECT TOP (1) dbo.cariche.nome_carica
                  FROM dbo.cariche
                  WHERE dbo.cariche.id_carica = @IdOrgano)";

                                            var idOrganoGiunta = idCommissioneGiunta != null
                                                ? Convert.ToInt32(idCommissioneGiunta)
                                                : (int?)null;

                                            // Inserimento risposta associata
                                            InsertRispostaRecord(connection
                                                , attoImportato.UidAtto
                                                , tipoRispostaAssociata
                                                , tipoOrgano
                                                , dataRispostaAssociata
                                                , dataTrasmissioneRispostaAssociata
                                                , dataTrattazioneRispostaAssociata
                                                , idOrganoGiunta
                                                , sub_query);
                                        }
                                    }
                                    else
                                    {
                                        // Inserimento del record principale
                                        var uidRisposta = InsertRispostaRecord(connection, attoImportato.UidAtto,
                                            tipoRispostaAssociata,
                                            tipoOrgano, dataRispostaAssociata, dataTrasmissioneRispostaAssociata,
                                            dataTrattazioneRispostaAssociata, idOrgano, sub_query);

                                        var nodeIdRispostaMadre = cellsRisposteAssociate[rowRA, 7]?.Value?.ToString();
                                        for (var rowRG = 2; rowRG <= rowCountRG; rowRG++)
                                        {
                                            var valoreCellaRiferimento =
                                                cellsRisposteGiunta[rowRG, 2]?.Value?.ToString();
                                            if (valoreCellaRiferimento == null ||
                                                valoreCellaRiferimento != nodeIdRispostaMadre) continue;

                                            var idCommissioneGiunta = cellsRisposteGiunta[rowRG, 6]?.Value;
                                            tipoOrgano = (int)TipoOrganoEnum.GIUNTA;
                                            sub_query =
                                                @"(SELECT TOP (1) dbo.cariche.nome_carica
                  FROM dbo.join_persona_organo_carica AS jpoc 
                  INNER JOIN dbo.cariche ON jpoc.id_carica = dbo.cariche.id_carica 
                  WHERE dbo.cariche.id_carica = @IdOrgano)";

                                            var idOrganoGiunta = idCommissioneGiunta != null
                                                ? Convert.ToInt32(idCommissioneGiunta)
                                                : (int?)null;

                                            // Inserimento risposta associata
                                            InsertRispostaAssociataRecord(connection, attoImportato.UidAtto,
                                                tipoRispostaAssociata, tipoOrgano, idOrganoGiunta, sub_query,
                                                uidRisposta);
                                        }
                                    }
                                }

                                #endregion

                                #region MONITORAGGIO COMMISSIONI

                                for (var rowMC = 2; rowMC <= rowCountMC; rowMC++)
                                {
                                    var valoreCella = cellsMonitoraggioCommissioni[rowMC, 2].Value;

                                    if (valoreCella != null && valoreCella.ToString() == nodeIdFromAlfresco)
                                    {
                                        var tipoOrgano = (int)TipoOrganoEnum.COMMISSIONE;

                                        var nomeMonitorato = cellsMonitoraggioCommissioni[rowMC, 8].Value;
                                        var idMonitorato = cellsMonitoraggioCommissioni[rowMC, 10].Value;
                                        var queryInsertMonitoraggio =
                                            @"INSERT INTO ATTI_MONITORAGGIO (Uid, UIDAtto, TipoOrgano, DescrizioneOrgano, IdOrgano)
                                         VALUES"
                                            + "(@IdMonitoraggio, @UIDAtto, @Tipo, @Nome, @IdOrganoMonitorato)";

                                        using (var commandMonitoraggioCommissione =
                                               new SqlCommand(queryInsertMonitoraggio, connection))
                                        {
                                            commandMonitoraggioCommissione.Parameters.AddWithValue("@IdMonitoraggio",
                                                Guid.NewGuid());
                                            commandMonitoraggioCommissione.Parameters.AddWithValue("@UIDAtto",
                                                attoImportato.UidAtto);
                                            commandMonitoraggioCommissione.Parameters.AddWithValue("@Tipo", tipoOrgano);
                                            commandMonitoraggioCommissione.Parameters.AddWithValue("@Nome",
                                                nomeMonitorato);
                                            commandMonitoraggioCommissione.Parameters.AddWithValue(
                                                "@IdOrganoMonitorato",
                                                Convert.ToInt16(idMonitorato));

                                            commandMonitoraggioCommissione.ExecuteNonQuery();
                                        }
                                    }
                                }

                                #endregion

                                #region MONITORAGGIO GIUNTA

                                for (var rowMG = 2; rowMG <= rowCountMG; rowMG++)
                                {
                                    var valoreCella = cellsMonitoraggioGiunta[rowMG, 2].Value;

                                    if (valoreCella != null && valoreCella.ToString() == nodeIdFromAlfresco)
                                    {
                                        var tipoOrgano = (int)TipoOrganoEnum.GIUNTA;

                                        var nomeMonitorato = cellsMonitoraggioGiunta[rowMG, 8].Value;
                                        var idMonitorato = cellsMonitoraggioGiunta[rowMG, 10].Value;
                                        var queryInsertMonitoraggio =
                                            @"INSERT INTO ATTI_MONITORAGGIO (Uid, UIDAtto, TipoOrgano, DescrizioneOrgano, IdOrgano)
                                         VALUES"
                                            + "(@IdMonitoraggio, @UIDAtto, @Tipo, @Nome, @IdOrganoMonitorato)";

                                        using (var commandMonitoraggioGiunta =
                                               new SqlCommand(queryInsertMonitoraggio, connection))
                                        {
                                            commandMonitoraggioGiunta.Parameters.AddWithValue("@IdMonitoraggio",
                                                Guid.NewGuid());
                                            commandMonitoraggioGiunta.Parameters.AddWithValue("@UIDAtto",
                                                attoImportato.UidAtto);
                                            commandMonitoraggioGiunta.Parameters.AddWithValue("@Tipo", tipoOrgano);
                                            commandMonitoraggioGiunta.Parameters.AddWithValue("@Nome", nomeMonitorato);
                                            commandMonitoraggioGiunta.Parameters.AddWithValue("@IdOrganoMonitorato",
                                                Convert.ToInt16(idMonitorato));

                                            commandMonitoraggioGiunta.ExecuteNonQuery();
                                        }
                                    }
                                }

                                #endregion

                                #region PROPONENTI COMMISSIONI

                                for (var rowPC = 2; rowPC <= rowCountPC; rowPC++)
                                {
                                    var valoreCella = cellsProponentiCommissioni[rowPC, 2].Value;

                                    if (valoreCella != null && valoreCella.ToString() == nodeIdFromAlfresco)
                                    {
                                        var nomeCommissione = cellsProponentiCommissioni[rowPC, 9].Value;
                                        var idCommissione = cellsProponentiCommissioni[rowPC, 8].Value;
                                        var queryInsertProponentiCommissione =
                                            @"INSERT INTO ATTI_PROPONENTI (Uid, UIDAtto, DescrizioneOrgano, IdOrgano)
                                         VALUES"
                                            + "(@Uid, @UIDAtto, @Nome, @IdOrgano)";

                                        using (var commandProponentiCommissione =
                                               new SqlCommand(queryInsertProponentiCommissione, connection))
                                        {
                                            commandProponentiCommissione.Parameters.AddWithValue("@Uid",
                                                Guid.NewGuid());
                                            commandProponentiCommissione.Parameters.AddWithValue("@UIDAtto",
                                                attoImportato.UidAtto);
                                            commandProponentiCommissione.Parameters.AddWithValue("@Nome",
                                                nomeCommissione);
                                            commandProponentiCommissione.Parameters.AddWithValue("@IdOrgano",
                                                Convert.ToInt16(idCommissione));

                                            commandProponentiCommissione.ExecuteNonQuery();
                                        }
                                    }
                                }

                                #endregion

                                //oggetto
                                var oggetto = Convert.ToString(cellsAtti[row, 13].Value);

                                //oggetto presentato
                                var oggettoPresentato = Convert.ToString(cellsAtti[row, 21].Value);

                                if (!string.IsNullOrEmpty(oggettoPresentato)
                                    && string.IsNullOrEmpty(oggetto))
                                    oggetto = oggettoPresentato;

                                //premesse
                                var premesse =
                                    "Atto presentato in modalità cartacea. Il testo originale, scansionato, è inserito in allegato.";

                                //tipo risposta richiesta
                                var tipoRispostaRichiestaAttoFromAlfresco = Convert.ToString(cellsAtti[row, 20].Value);
                                var tipoRispostaRichiestaAttoInt =
                                    ConvertToIntTipoRisposta(tipoRispostaRichiestaAttoFromAlfresco);
                                if (tipoAttoEnum == TipoAttoEnum.IQT)
                                {
                                    tipoRispostaRichiestaAttoInt = (int)TipoRispostaEnum.IMMEDIATA;
                                }

                                //tipo risposta effettiva
                                var tipoRispostaEffettivaAttoFromAlfresco = Convert.ToString(cellsAtti[row, 11].Value);
                                var tipoRispostaEffettivaAttoInt =
                                    ConvertToIntTipoRisposta(tipoRispostaEffettivaAttoFromAlfresco);
                                if (tipoAttoEnum == TipoAttoEnum.IQT)
                                {
                                    tipoRispostaEffettivaAttoInt = (int)TipoRispostaEnum.IMMEDIATA;
                                }

                                //data presentazione
                                var dataPresentazioneFromAlfresco = Convert.ToString(cellsAtti[row, 16].Value);
                                if (string.IsNullOrEmpty(dataPresentazioneFromAlfresco))
                                    throw new Exception("Data presentazione vuota.");
                                var dataPresentazione = ParseDateTime(dataPresentazioneFromAlfresco);
                                var dataPresentazione_Cifrata = CryptoHelper.EncryptString(
                                    dataPresentazione.ToString("dd/MM/yyyy HH:mm:ss"),
                                    AppsettingsConfiguration.MASTER_KEY);

                                var area = Convert.ToString(cellsAtti[row, 23].Value);

                                var dataComunicazioneAssemblea = Convert.ToString(cellsAtti[row, 31].Value);
                                var dataAnnunzio = Convert.ToString(cellsAtti[row, 17].Value);
                                var codiceMateria = Convert.ToString(cellsAtti[row, 18].Value);
                                var protocollo = Convert.ToString(cellsAtti[row, 14].Value);

                                var pubblicato = cellsAtti[row, 8].Value.ToString().Equals("1");
                                var sollecito = cellsAtti[row, 9].Value.ToString().Equals("1");

                                var tipoChiusuraIter = Convert.ToString(cellsAtti[row, 28].Value);
                                var dataChiusuraIter = Convert.ToString(cellsAtti[row, 30].Value);
                                var uidSeduta = string.Empty;
                                if (tipoAttoEnum is TipoAttoEnum.MOZ
                                    || tipoAttoEnum is TipoAttoEnum.ODG
                                    || tipoAttoEnum is TipoAttoEnum.RIS)
                                {
                                    var queryGetSeduta =
                                        $"SELECT UIDSeduta FROM SEDUTE WHERE CONVERT(DATE, Data_seduta) = '{dataChiusuraIter}'";

                                    using (var commandGetSeduta = new SqlCommand(queryGetSeduta, connection))
                                    {
                                        var result = commandGetSeduta.ExecuteScalar();
                                        if (result != null)
                                        {
                                            uidSeduta = result.ToString();
                                        }
                                    }
                                }

                                var iterMultiplo = Convert.ToString(cellsAtti[row, 50].Value) == "1";

                                var emendatoFromAlfresco = Convert.ToString(cellsAtti[row, 32].Value);
                                var emendato = !string.IsNullOrEmpty(emendatoFromAlfresco)
                                    ? emendatoFromAlfresco.Equals("1")
                                    : false;

                                var tipoVotazione = Convert.ToString(cellsAtti[row, 33].Value);
                                var dcrl = Convert.ToString(cellsAtti[row, 34].Value);
                                var burl = Convert.ToString(cellsAtti[row, 35].Value);
                                var dcr = Convert.ToString(cellsAtti[row, 44].Value);
                                if (string.IsNullOrEmpty(dcr)) dcr = "0";

                                var dcrc = Convert.ToString(cellsAtti[row, 48].Value);
                                if (string.IsNullOrEmpty(dcrc)) dcrc = "0";

                                var areaTematica = Convert.ToString(cellsAtti[row, 72].Value);
                                var altriSoggetti = Convert.ToString(cellsAtti[row, 71].Value);

                                var competenzaMonitoraggio = Convert.ToString(cellsAtti[row, 68].Value);
                                var noteImpegni_e_scadenze = Convert.ToString(cellsAtti[row, 69].Value);
                                var statoAttuazione = Convert.ToString(cellsAtti[row, 70].Value);
                                var dataTrasmissioneMonitoraggio = Convert.ToString(cellsAtti[row, 73].Value);
                                var conclusoMonitoraggioFromAlfresco = Convert.ToString(cellsAtti[row, 74].Value);
                                var monitoraggioConcluso = !string.IsNullOrEmpty(conclusoMonitoraggioFromAlfresco) &&
                                                           conclusoMonitoraggioFromAlfresco.Equals("1");

                                var privacy_dati_personali_giudiziari_sn = cellsAtti[row, 53].Value.Equals("1");
                                var privacy_divieto_pubblicazione_salute_sn = cellsAtti[row, 54].Value.Equals("1");
                                var privacy_divieto_pubblicazione_vita_sessuale_sn = cellsAtti[row, 55].Value.Equals("1");
                                var privacy_divieto_pubblicazione_sn = cellsAtti[row, 56].Value.Equals("1");
                                var privacy_dati_personali_sensibili_sn = cellsAtti[row, 57].Value.Equals("1");
                                var privacy_divieto_pubblicazione_altri_sn = cellsAtti[row, 58].Value.Equals("1");
                                var privacy_dati_personali_semplici_sn = cellsAtti[row, 59].Value.Equals("1");
                                var privacy_sn = cellsAtti[row, 60].Value.Equals("1");

                                //Risposte
                                var dataTrasmissione = Convert.ToString(cellsAtti[row, 95].Value);
                                var dataSedutaRisposta = Convert.ToString(cellsAtti[row, 96].Value);
                                var dataProposta = Convert.ToString(cellsAtti[row, 97].Value);
                                var dataComunicazioneAssembleaRisposta = Convert.ToString(cellsAtti[row, 98].Value);

                                if (tipoAttoEnum == TipoAttoEnum.RIS)
                                {
                                    dataTrasmissione = Convert.ToString(cellsAtti[row, 99].Value);
                                    dataSedutaRisposta = Convert.ToString(cellsAtti[row, 100].Value);
                                    dataProposta = Convert.ToString(cellsAtti[row, 101].Value);
                                    dataComunicazioneAssembleaRisposta = Convert.ToString(cellsAtti[row, 102].Value);
                                }

                                //META DATI RELATORI RIS
                                var idRelatore = string.Empty;
                                var idRelatore2 = string.Empty;
                                var idRelatoreMinoranza = string.Empty;
                                var relatore1 = Guid.Empty;
                                var relatore2 = Guid.Empty;
                                var relatoreMinoranza = Guid.Empty;

                                if (tipoAttoEnum == TipoAttoEnum.RIS)
                                {
                                    idRelatore = Convert.ToString(cellsAtti[row, 109].Value);
                                    idRelatore2 = Convert.ToString(cellsAtti[row, 105].Value);
                                    idRelatoreMinoranza = Convert.ToString(cellsAtti[row, 104].Value);
                                    relatore1 = Guid.Empty;
                                    relatore2 = Guid.Empty;
                                    relatoreMinoranza = Guid.Empty;

                                    if (!string.IsNullOrEmpty(idRelatore))
                                    {
                                        var find_uid_persona_relatore = GetUidPersona(connection, int.Parse(idRelatore));

                                        if (Guid.TryParse(Convert.ToString(find_uid_persona_relatore),
                                                out var guid_relatore))
                                        {
                                            relatore1 = guid_relatore;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(idRelatore2))
                                    {
                                        var find_uid_persona_relatore2 = GetUidPersona(connection, int.Parse(idRelatore2));

                                        if (Guid.TryParse(Convert.ToString(find_uid_persona_relatore2),
                                                out var guid_relatore2))
                                        {
                                            relatore2 = guid_relatore2;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(idRelatoreMinoranza))
                                    {
                                        var find_uid_persona_relatore_minoranza =
                                            GetUidPersona(connection, int.Parse(idRelatoreMinoranza));

                                        if (Guid.TryParse(Convert.ToString(find_uid_persona_relatore_minoranza),
                                                out var guid_relatore_minoranza))
                                        {
                                            relatoreMinoranza = guid_relatore_minoranza;
                                        }
                                    }
                                }

                                var tipoChiusuraIterCommissione = Convert.ToString(cellsAtti[row, 111].Value);
                                var tipoVotazioneIterCommissione = Convert.ToString(cellsAtti[row, 113].Value);
                                var risultatoVotazioneIterCommissione = Convert.ToString(cellsAtti[row, 116].Value);
                                var dataChiusuraIterCommissione = Convert.ToString(cellsAtti[row, 118].Value);

                                if (!update)
                                {
                                    var query = @"
                                INSERT INTO [ATTI_DASI] 
                                (UIDAtto, Tipo, TipoMOZ, NAtto, Etichetta, NAtto_search, Oggetto, Premesse, IDTipo_Risposta, IDTipo_Risposta_Effettiva, DataPresentazione, IDStato, Legislatura, 
                                UIDPersonaCreazione, UIDPersonaPresentazione, idRuoloCreazione, UIDPersonaProponente, UIDPersonaPrimaFirma, 
                                UID_QRCode, id_gruppo, chkf, Timestamp, DataCreazione, OrdineVisualizzazione, AreaPolitica, Pubblicato, Sollecito, Protocollo, CodiceMateria{FIELD_DATA_ANNUNZIO}
{FIELD_TIPO_CHIUSURA_ITER}{FIELD_TIPO_CHIUSURA_ITER_COMMISSIONE}{FIELD_DATA_CHIUSURA_ITER}{FIELD_DATA_CHIUSURA_ITER_COMMISSIONE}{FIELD_TIPO_VOTAZIONE_ITER}{FIELD_TIPO_VOTAZIONE_ITER_COMMISSIONE}{FIELD_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}, Emendato, BURL, DCR, DCCR, DCRL, AreaTematica, AltriSoggetti, ImpegniScadenze, StatoAttuazione, CompetenzaMonitoraggio, Privacy_Dati_Personali_Giudiziari,
Privacy_Divieto_Pubblicazione_Salute, Privacy_Divieto_Pubblicazione_Vita_Sessuale, Privacy_Divieto_Pubblicazione, Privacy_Dati_Personali_Sensibili, Privacy_Divieto_Pubblicazione_Altri, Privacy_Dati_Personali_Semplici, 
Privacy{FIELD_DATA_DataComunicazioneAssemblea}, MonitoraggioConcluso{FIELD_DATA_TRASMISSIONE_MONITORAGGIO}{FIELD_DATA_TRASMISSIONE}{FIELD_DATA_SEDUTA_RISPOSTA}{FIELD_DATA_PROPOSTA}{FIELD_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}{FIELD_REL_1}{FIELD_REL_2}{FIELD_REL_MINORANZA}{FIELD_UIDSEDUTA}, IterMultiplo, Proietta, Firma_su_invito, Eliminato) 
                                VALUES 
                                (@UIDAtto, @Tipo, @TipoMOZ, @NAtto, @Etichetta, @NAtto_search, @Oggetto, @Premesse, @IDTipo_Risposta, @IDTipo_Risposta_Effettiva, @DataPresentazione, @IDStato, @Legislatura, 
                                @UIDPersonaCreazione, @UIDPersonaPresentazione, @idRuoloCreazione, @UIDPersonaProponente, @UIDPersonaPrimaFirma, 
                                @UID_QRCode, @id_gruppo, @chkf, @Timestamp, GETDATE(), @OrdineVisualizzazione, @AreaPolitica, @Pubblicato, @Sollecito, @Protocollo, @CodiceMateria{PARAM_DATA_ANNUNZIO}
{PARAM_TIPO_CHIUSURA_ITER}{PARAM_TIPO_CHIUSURA_ITER_COMMISSIONE}{PARAM_DATA_CHIUSURA_ITER}{PARAM_DATA_CHIUSURA_ITER_COMMISSIONE}{PARAM_TIPO_VOTAZIONE_ITER}{PARAM_TIPO_VOTAZIONE_ITER_COMMISSIONE}{PARAM_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}, @Emendato, @BURL, @DCR, @DCRC, @DCRL, @AreaTematica, @AltriSoggetti, @ImpegniScadenze, @StatoAttuazione, @CompetenzaMonitoraggio, @Privacy_Dati_Personali_Giudiziari,
@Privacy_Divieto_Pubblicazione_Salute, @Privacy_Divieto_Pubblicazione_Vita_Sessuale, @Privacy_Divieto_Pubblicazione, @Privacy_Dati_Personali_Sensibili, @Privacy_Divieto_Pubblicazione_Altri, @Privacy_Dati_Personali_Semplici, 
@Privacy{PARAM_DATA_DataComunicazioneAssemblea}, @MonitoraggioConcluso{PARAM_DATA_TRASMISSIONE_MONITORAGGIO}{PARAM_DATA_TRASMISSIONE}{PARAM_DATA_SEDUTA_RISPOSTA}{PARAM_DATA_PROPOSTA}{PARAM_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}{PARAM_REL_1}{PARAM_REL_2}{PARAM_REL_MINORANZA}{PARAM_UIDSEDUTA}, @IterMultiplo, 0, 0, 0)";

                                    if (string.IsNullOrEmpty(dataAnnunzio))
                                        query = query
                                            .Replace("{FIELD_DATA_ANNUNZIO}", "")
                                            .Replace("{PARAM_DATA_ANNUNZIO}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_ANNUNZIO}", ", DataAnnunzio")
                                            .Replace("{PARAM_DATA_ANNUNZIO}", ", @DataAnnunzio");

                                    if (string.IsNullOrEmpty(dataTrasmissioneMonitoraggio)
                                        || tipoAttoEnum == TipoAttoEnum.ITL
                                        || tipoAttoEnum == TipoAttoEnum.ITR
                                        || tipoAttoEnum == TipoAttoEnum.IQT)
                                        query = query
                                            .Replace("{FIELD_DATA_TRASMISSIONE_MONITORAGGIO}", "")
                                            .Replace("{PARAM_DATA_TRASMISSIONE_MONITORAGGIO}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_TRASMISSIONE_MONITORAGGIO}",
                                                ", DataTrasmissioneMonitoraggio")
                                            .Replace("{PARAM_DATA_TRASMISSIONE_MONITORAGGIO}",
                                                ", @DataTrasmissioneMonitoraggio");

                                    if (string.IsNullOrEmpty(dataComunicazioneAssemblea))
                                        query = query
                                            .Replace("{FIELD_DATA_DataComunicazioneAssemblea}", "")
                                            .Replace("{PARAM_DATA_DataComunicazioneAssemblea}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_DataComunicazioneAssemblea}",
                                                ", DataComunicazioneAssemblea")
                                            .Replace("{PARAM_DATA_DataComunicazioneAssemblea}",
                                                ", @DataComunicazioneAssemblea");

                                    if (string.IsNullOrEmpty(dataTrasmissione))
                                        query = query
                                            .Replace("{FIELD_DATA_TRASMISSIONE}", "")
                                            .Replace("{PARAM_DATA_TRASMISSIONE}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_TRASMISSIONE}",
                                                ", DataTrasmissione")
                                            .Replace("{PARAM_DATA_TRASMISSIONE}",
                                                ", @DataTrasmissione");

                                    if (string.IsNullOrEmpty(dataSedutaRisposta))
                                        query = query
                                            .Replace("{FIELD_DATA_SEDUTA_RISPOSTA}", "")
                                            .Replace("{PARAM_DATA_SEDUTA_RISPOSTA}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_SEDUTA_RISPOSTA}",
                                                ", DataSedutaRisposta")
                                            .Replace("{PARAM_DATA_SEDUTA_RISPOSTA}",
                                                ", @DataSedutaRisposta");

                                    if (string.IsNullOrEmpty(dataProposta))
                                        query = query
                                            .Replace("{FIELD_DATA_PROPOSTA}", "")
                                            .Replace("{PARAM_DATA_PROPOSTA}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_PROPOSTA}",
                                                ", DataProposta")
                                            .Replace("{PARAM_DATA_PROPOSTA}",
                                                ", @DataProposta");

                                    if (string.IsNullOrEmpty(dataComunicazioneAssembleaRisposta))
                                        query = query
                                            .Replace("{FIELD_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}", "")
                                            .Replace("{PARAM_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}",
                                                ", DataComunicazioneAssembleaRisposta")
                                            .Replace("{PARAM_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}",
                                                ", @DataComunicazioneAssembleaRisposta");

                                    if (string.IsNullOrEmpty(tipoChiusuraIter))
                                        query = query
                                            .Replace("{FIELD_TIPO_CHIUSURA_ITER}", "")
                                            .Replace("{PARAM_TIPO_CHIUSURA_ITER}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_TIPO_CHIUSURA_ITER}", ", TipoChiusuraIter")
                                            .Replace("{PARAM_TIPO_CHIUSURA_ITER}", ", @TipoChiusuraIter");

                                    if (string.IsNullOrEmpty(tipoChiusuraIterCommissione))
                                        query = query
                                            .Replace("{FIELD_TIPO_CHIUSURA_ITER_COMMISSIONE}", "")
                                            .Replace("{PARAM_TIPO_CHIUSURA_ITER_COMMISSIONE}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_TIPO_CHIUSURA_ITER_COMMISSIONE}",
                                                ", TipoChiusuraIterCommissione")
                                            .Replace("{PARAM_TIPO_CHIUSURA_ITER_COMMISSIONE}",
                                                ", @TipoChiusuraIterCommissione");

                                    if (string.IsNullOrEmpty(dataChiusuraIter))
                                        query = query
                                            .Replace("{FIELD_DATA_CHIUSURA_ITER}", "")
                                            .Replace("{PARAM_DATA_CHIUSURA_ITER}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_CHIUSURA_ITER}", ", DataChiusuraIter")
                                            .Replace("{PARAM_DATA_CHIUSURA_ITER}", ", @DataChiusuraIter");

                                    if (string.IsNullOrEmpty(dataChiusuraIterCommissione))
                                        query = query
                                            .Replace("{FIELD_DATA_CHIUSURA_ITER_COMMISSIONE}", "")
                                            .Replace("{PARAM_DATA_CHIUSURA_ITER_COMMISSIONE}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_DATA_CHIUSURA_ITER_COMMISSIONE}",
                                                ", DataChiusuraIterCommissione")
                                            .Replace("{PARAM_DATA_CHIUSURA_ITER_COMMISSIONE}",
                                                ", @DataChiusuraIterCommissione");

                                    if (string.IsNullOrEmpty(tipoVotazione))
                                        query = query
                                            .Replace("{FIELD_TIPO_VOTAZIONE_ITER}", "")
                                            .Replace("{PARAM_TIPO_VOTAZIONE_ITER}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_TIPO_VOTAZIONE_ITER}", ", TipoVotazioneIter")
                                            .Replace("{PARAM_TIPO_VOTAZIONE_ITER}", ", @TipoVotazioneIter");

                                    if (string.IsNullOrEmpty(tipoVotazioneIterCommissione))
                                        query = query
                                            .Replace("{FIELD_TIPO_VOTAZIONE_ITER_COMMISSIONE}", "")
                                            .Replace("{PARAM_TIPO_VOTAZIONE_ITER_COMMISSIONE}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_TIPO_VOTAZIONE_ITER_COMMISSIONE}",
                                                ", TipoVotazioneIterCommissione")
                                            .Replace("{PARAM_TIPO_VOTAZIONE_ITER_COMMISSIONE}",
                                                ", @TipoVotazioneIterCommissione");

                                    if (string.IsNullOrEmpty(risultatoVotazioneIterCommissione))
                                        query = query
                                            .Replace("{FIELD_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}", "")
                                            .Replace("{PARAM_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}",
                                                ", RisultatoVotazioneIterCommissione")
                                            .Replace("{PARAM_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}",
                                                ", @RisultatoVotazioneIterCommissione");

                                    if (string.IsNullOrEmpty(idRelatore))
                                        query = query
                                            .Replace("{FIELD_REL_1}", "")
                                            .Replace("{PARAM_REL_1}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_REL_1}", ", UIDPersonaRelatore1")
                                            .Replace("{PARAM_REL_1}", ", @UIDPersonaRelatore1");

                                    if (string.IsNullOrEmpty(idRelatore2))
                                        query = query
                                            .Replace("{FIELD_REL_2}", "")
                                            .Replace("{PARAM_REL_2}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_REL_2}", ", UIDPersonaRelatore2")
                                            .Replace("{PARAM_REL_2}", ", @UIDPersonaRelatore2");

                                    if (string.IsNullOrEmpty(idRelatoreMinoranza))
                                        query = query
                                            .Replace("{FIELD_REL_MINORANZA}", "")
                                            .Replace("{PARAM_REL_MINORANZA}", "");
                                    else
                                        query = query
                                            .Replace("{FIELD_REL_MINORANZA}", ", UIDPersonaRelatoreMinoranza")
                                            .Replace("{PARAM_REL_MINORANZA}", ", @UIDPersonaRelatoreMinoranza");

                                    if (tipoAttoEnum is TipoAttoEnum.MOZ
                                        || tipoAttoEnum is TipoAttoEnum.ODG
                                        || tipoAttoEnum is TipoAttoEnum.RIS)
                                    {
                                        if (string.IsNullOrEmpty(uidSeduta))
                                            query = query.Replace("{FIELD_UIDSEDUTA}", "")
                                                .Replace("{PARAM_UIDSEDUTA}", "");
                                        else
                                            query = query.Replace("{FIELD_UIDSEDUTA}", ", UIDSeduta")
                                                .Replace("{PARAM_UIDSEDUTA}", ", @UIDSeduta");
                                    }
                                    else
                                    {
                                        query = query.Replace("{FIELD_UIDSEDUTA}", "")
                                            .Replace("{PARAM_UIDSEDUTA}", "");
                                    }

                                    using (var command = new SqlCommand(query, connection))
                                    {
                                        // Assegna i valori dei parametri
                                        command.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                        command.Parameters.AddWithValue("@Tipo", (int)tipoAttoEnum);
                                        command.Parameters.AddWithValue("@TipoMOZ", tipoMozione);
                                        command.Parameters.AddWithValue("@NAtto", etichettaAtto_Cifrata);
                                        command.Parameters.AddWithValue("@Etichetta", etichettaAtto);
                                        command.Parameters.AddWithValue("@NAtto_search", numeroAtto);
                                        command.Parameters.AddWithValue("@Oggetto", CleanHtmlTags(oggetto));
                                        command.Parameters.AddWithValue("@Premesse", CleanHtmlTags(premesse));
                                        command.Parameters.AddWithValue("@IDTipo_Risposta",
                                            tipoRispostaRichiestaAttoInt);
                                        command.Parameters.AddWithValue("@IDTipo_Risposta_Effettiva",
                                            tipoRispostaEffettivaAttoInt);
                                        command.Parameters.AddWithValue("@DataPresentazione",
                                            dataPresentazione_Cifrata);
                                        command.Parameters.AddWithValue("@IDStato", statoId);
                                        command.Parameters.AddWithValue("@Legislatura", legislatura.id_legislatura);
                                        command.Parameters.AddWithValue("@Timestamp", dataPresentazione);
                                        command.Parameters.AddWithValue("@OrdineVisualizzazione", numeroAtto);
                                        command.Parameters.AddWithValue("@UID_QRCode", Guid.NewGuid());
                                        command.Parameters.AddWithValue("@idRuoloCreazione",
                                            (int)RuoliIntEnum.Consigliere_Regionale);
                                        command.Parameters.AddWithValue("@UIDPersonaCreazione", proponenteId);
                                        command.Parameters.AddWithValue("@UIDPersonaPresentazione", proponenteId);
                                        command.Parameters.AddWithValue("@UIDPersonaProponente", proponenteId);
                                        command.Parameters.AddWithValue("@UIDPersonaPrimaFirma", proponenteId);
                                        command.Parameters.AddWithValue("@id_gruppo", id_gruppo);
                                        command.Parameters.AddWithValue("@chkf", chkf);
                                        command.Parameters.AddWithValue("@AreaPolitica", ParseDescr2Enum_Area(area));
                                        command.Parameters.AddWithValue("@Pubblicato", pubblicato);
                                        command.Parameters.AddWithValue("@Sollecito", sollecito);
                                        command.Parameters.AddWithValue("@Protocollo", protocollo);
                                        command.Parameters.AddWithValue("@CodiceMateria", codiceMateria);
                                        command.Parameters.AddWithValue("@Emendato", emendato);
                                        command.Parameters.AddWithValue("@BURL", burl);
                                        command.Parameters.AddWithValue("@DCR", dcr);
                                        command.Parameters.AddWithValue("@DCRC", dcrc);
                                        command.Parameters.AddWithValue("@DCRL", dcrl);
                                        command.Parameters.AddWithValue("@AreaTematica", CleanHtmlTags(areaTematica));
                                        command.Parameters.AddWithValue("@AltriSoggetti", CleanHtmlTags(altriSoggetti));
                                        command.Parameters.AddWithValue("@ImpegniScadenze",
                                            CleanHtmlTags(noteImpegni_e_scadenze));
                                        command.Parameters.AddWithValue("@StatoAttuazione",
                                            CleanHtmlTags(statoAttuazione));
                                        command.Parameters.AddWithValue("@CompetenzaMonitoraggio",
                                            CleanHtmlTags(competenzaMonitoraggio));
                                        command.Parameters.AddWithValue("@MonitoraggioConcluso", monitoraggioConcluso);
                                        command.Parameters.AddWithValue("@IterMultiplo", iterMultiplo);

                                        command.Parameters.AddWithValue("@Privacy_Dati_Personali_Giudiziari",
                                            privacy_dati_personali_giudiziari_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione_Salute",
                                            privacy_divieto_pubblicazione_salute_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione_Vita_Sessuale",
                                            privacy_divieto_pubblicazione_vita_sessuale_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione",
                                            privacy_divieto_pubblicazione_sn);
                                        command.Parameters.AddWithValue("@Privacy_Dati_Personali_Sensibili",
                                            privacy_dati_personali_sensibili_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione_Altri",
                                            privacy_divieto_pubblicazione_altri_sn);
                                        command.Parameters.AddWithValue("@Privacy_Dati_Personali_Semplici",
                                            privacy_dati_personali_semplici_sn);
                                        command.Parameters.AddWithValue("@Privacy", privacy_sn);

                                        if (string.IsNullOrEmpty(dataAnnunzio))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataAnnunzio",
                                                ParseDateTime(dataAnnunzio));
                                        }

                                        if (string.IsNullOrEmpty(dataTrasmissioneMonitoraggio)
                                            || tipoAttoEnum == TipoAttoEnum.ITL
                                            || tipoAttoEnum == TipoAttoEnum.ITR
                                            || tipoAttoEnum == TipoAttoEnum.IQT)
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataTrasmissioneMonitoraggio",
                                                ParseDateTime(dataTrasmissioneMonitoraggio));
                                        }

                                        if (string.IsNullOrEmpty(dataComunicazioneAssemblea))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataComunicazioneAssemblea",
                                                ParseDateTime(dataComunicazioneAssemblea));
                                        }

                                        if (string.IsNullOrEmpty(dataTrasmissione))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataTrasmissione",
                                                ParseDateTime(dataTrasmissione));
                                        }

                                        if (string.IsNullOrEmpty(dataSedutaRisposta))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataSedutaRisposta",
                                                ParseDateTime(dataSedutaRisposta));
                                        }

                                        if (string.IsNullOrEmpty(dataComunicazioneAssembleaRisposta))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataComunicazioneAssembleaRisposta",
                                                ParseDateTime(dataComunicazioneAssembleaRisposta));
                                        }

                                        if (string.IsNullOrEmpty(dataProposta))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataProposta",
                                                ParseDateTime(dataProposta));
                                        }

                                        if (string.IsNullOrEmpty(tipoChiusuraIter))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoChiusuraIter",
                                                ParseDescr2Enum_ChiusuraIter(tipoChiusuraIter));
                                        }

                                        if (string.IsNullOrEmpty(tipoChiusuraIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoChiusuraIterCommissione",
                                                ParseDescr2Enum_ChiusuraIter(tipoChiusuraIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(dataChiusuraIter))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataChiusuraIter",
                                                ParseDateTime(dataChiusuraIter));
                                        }

                                        if (string.IsNullOrEmpty(dataChiusuraIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataChiusuraIterCommissione",
                                                ParseDateTime(dataChiusuraIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(tipoVotazione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoVotazioneIter",
                                                ParseDescr2Enum_TipoVotazioneIter(tipoVotazione));
                                        }

                                        if (string.IsNullOrEmpty(tipoVotazioneIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoVotazioneIterCommissione",
                                                ParseDescr2Enum_TipoVotazioneIter(tipoVotazioneIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(risultatoVotazioneIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@RisultatoVotazioneIterCommissione",
                                                ParseDescr2Enum_RisultatoVotazioneIter(
                                                    risultatoVotazioneIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(idRelatore))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@UIDPersonaRelatore1",
                                                relatore1);
                                        }

                                        if (string.IsNullOrEmpty(idRelatore2))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@UIDPersonaRelatore2",
                                                relatore2);
                                        }

                                        if (string.IsNullOrEmpty(idRelatoreMinoranza))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@UIDPersonaRelatoreMinoranza",
                                                relatoreMinoranza);
                                        }

                                        if (tipoAttoEnum is TipoAttoEnum.MOZ
                                            || tipoAttoEnum is TipoAttoEnum.ODG
                                            || tipoAttoEnum is TipoAttoEnum.RIS)
                                        {
                                            if (!string.IsNullOrEmpty(uidSeduta))
                                                command.Parameters.AddWithValue("@UIDSeduta",
                                                    uidSeduta);
                                        }

                                        // Esegui la query di inserimento
                                        var resQuery = command.ExecuteNonQuery();
                                        if (resQuery == 1)
                                        {
                                            Console.WriteLine($"[{row}/{rowCount}] {etichettaAtto}");
                                            attiImportati.Add(nodeIdFromAlfresco, attoImportato);
                                        }
                                    }
                                }
                                else
                                {
                                    var query = @"
                                    UPDATE [ATTI_DASI]
                                    SET  
                                        AreaPolitica = @AreaPolitica, 
                                        Pubblicato = @Pubblicato, 
                                        Sollecito = @Sollecito, 
                                        Protocollo = @Protocollo, 
                                        CodiceMateria = @CodiceMateria{FIELD_DATA_ANNUNZIO}{FIELD_TIPO_CHIUSURA_ITER}{FIELD_TIPO_CHIUSURA_ITER_COMMISSIONE}{FIELD_DATA_CHIUSURA_ITER}{FIELD_DATA_CHIUSURA_ITER_COMMISSIONE}{FIELD_TIPO_VOTAZIONE_ITER}{FIELD_TIPO_VOTAZIONE_ITER_COMMISSIONE}{FIELD_RISULTATO_VOTAZIONE_ITER_COMMISSIONE},
                                        Emendato = @Emendato, 
                                        BURL = @BURL, 
                                        DCR = @DCR, 
                                        DCCR = @DCRC, 
                                        DCRL = @DCRL, 
                                        IDTipo_Risposta_Effettiva = @IDTipo_Risposta_Effettiva,
                                        AreaTematica = @AreaTematica, 
                                        AltriSoggetti = @AltriSoggetti, 
                                        ImpegniScadenze = @ImpegniScadenze, 
                                        StatoAttuazione = @StatoAttuazione, 
                                        CompetenzaMonitoraggio = @CompetenzaMonitoraggio, 
                                        MonitoraggioConcluso = @MonitoraggioConcluso{FIELD_DATA_TRASMISSIONE_MONITORAGGIO}{FIELD_DATA_TRASMISSIONE}{FIELD_DATA_SEDUTA_RISPOSTA}{FIELD_DATA_PROPOSTA}{FIELD_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}, 
                                        IterMultiplo = @IterMultiplo, 
                                        Privacy_Dati_Personali_Giudiziari = @Privacy_Dati_Personali_Giudiziari,
                                        Privacy_Divieto_Pubblicazione_Salute = @Privacy_Divieto_Pubblicazione_Salute,
                                        Privacy_Divieto_Pubblicazione_Vita_Sessuale = @Privacy_Divieto_Pubblicazione_Vita_Sessuale,
                                        Privacy_Divieto_Pubblicazione = @Privacy_Divieto_Pubblicazione,
                                        Privacy_Dati_Personali_Sensibili = @Privacy_Dati_Personali_Sensibili,
                                        Privacy_Divieto_Pubblicazione_Altri = @Privacy_Divieto_Pubblicazione_Altri,
                                        Privacy_Dati_Personali_Semplici = @Privacy_Dati_Personali_Semplici,
                                        Privacy = @Privacy{FIELD_DATA_DataComunicazioneAssemblea}{FIELD_REL_1}{FIELD_REL_2}{FIELD_REL_MINORANZA}{FIELD_UIDSEDUTA},
                                        Firma_su_invito = 0
                                    WHERE UIDAtto = @UIDAtto";

                                    if (string.IsNullOrEmpty(dataAnnunzio))
                                        query = query
                                            .Replace("{FIELD_DATA_ANNUNZIO}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_ANNUNZIO}",
                                            ", DataAnnunzio = @DataAnnunzio");

                                    if (string.IsNullOrEmpty(dataTrasmissioneMonitoraggio)
                                        || tipoAttoEnum == TipoAttoEnum.ITL
                                        || tipoAttoEnum == TipoAttoEnum.ITR
                                        || tipoAttoEnum == TipoAttoEnum.IQT)
                                        query = query
                                            .Replace("{FIELD_DATA_TRASMISSIONE_MONITORAGGIO}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_TRASMISSIONE_MONITORAGGIO}",
                                            ", DataTrasmissioneMonitoraggio = @DataTrasmissioneMonitoraggio");

                                    if (string.IsNullOrEmpty(dataComunicazioneAssemblea))
                                        query = query
                                            .Replace("{FIELD_DATA_DataComunicazioneAssemblea}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_DataComunicazioneAssemblea}",
                                            ", DataComunicazioneAssemblea = @DataComunicazioneAssemblea");

                                    if (string.IsNullOrEmpty(dataTrasmissione))
                                        query = query
                                            .Replace("{FIELD_DATA_TRASMISSIONE}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_TRASMISSIONE}",
                                            ", DataTrasmissione = @DataTrasmissione");

                                    if (string.IsNullOrEmpty(dataSedutaRisposta))
                                        query = query
                                            .Replace("{FIELD_DATA_SEDUTA_RISPOSTA}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_SEDUTA_RISPOSTA}",
                                            ", DataSedutaRisposta = @DataSedutaRisposta");

                                    if (string.IsNullOrEmpty(dataProposta))
                                        query = query
                                            .Replace("{FIELD_DATA_PROPOSTA}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_PROPOSTA}",
                                            ", DataProposta = @DataProposta");

                                    if (string.IsNullOrEmpty(dataComunicazioneAssembleaRisposta))
                                        query = query
                                            .Replace("{FIELD_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_COMUNICAZIONE_ASSEMBLEA_RISPOSTA}",
                                            ", DataComunicazioneAssembleaRisposta = @DataComunicazioneAssembleaRisposta");

                                    if (string.IsNullOrEmpty(tipoChiusuraIter))
                                        query = query
                                            .Replace("{FIELD_TIPO_CHIUSURA_ITER}", "");
                                    else
                                        query = query.Replace("{FIELD_TIPO_CHIUSURA_ITER}",
                                            ", TipoChiusuraIter = @TipoChiusuraIter");

                                    if (string.IsNullOrEmpty(tipoChiusuraIterCommissione))
                                        query = query
                                            .Replace("{FIELD_TIPO_CHIUSURA_ITER_COMMISSIONE}", "");
                                    else
                                        query = query.Replace("{FIELD_TIPO_CHIUSURA_ITER_COMMISSIONE}",
                                            ", TipoChiusuraIterCommissione = @TipoChiusuraIterCommissione");

                                    if (string.IsNullOrEmpty(dataChiusuraIter))
                                        query = query
                                            .Replace("{FIELD_DATA_CHIUSURA_ITER}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_CHIUSURA_ITER}",
                                            ", DataChiusuraIter = @DataChiusuraIter");

                                    if (string.IsNullOrEmpty(dataChiusuraIterCommissione))
                                        query = query
                                            .Replace("{FIELD_DATA_CHIUSURA_ITER_COMMISSIONE}", "");
                                    else
                                        query = query.Replace("{FIELD_DATA_CHIUSURA_ITER_COMMISSIONE}",
                                            ", DataChiusuraIterCommissione = @DataChiusuraIterCommissione");

                                    if (string.IsNullOrEmpty(tipoVotazione))
                                        query = query
                                            .Replace("{FIELD_TIPO_VOTAZIONE_ITER}", "");
                                    else
                                        query = query.Replace("{FIELD_TIPO_VOTAZIONE_ITER}",
                                            ", TipoVotazioneIter = @TipoVotazioneIter");

                                    if (string.IsNullOrEmpty(tipoVotazioneIterCommissione))
                                        query = query
                                            .Replace("{FIELD_TIPO_VOTAZIONE_ITER_COMMISSIONE}", "");
                                    else
                                        query = query.Replace("{FIELD_TIPO_VOTAZIONE_ITER_COMMISSIONE}",
                                            ", TipoVotazioneIterCommissione = @TipoVotazioneIterCommissione");

                                    if (string.IsNullOrEmpty(risultatoVotazioneIterCommissione))
                                        query = query.Replace("{FIELD_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}", "");
                                    else
                                        query = query.Replace("{FIELD_RISULTATO_VOTAZIONE_ITER_COMMISSIONE}",
                                            ", RisultatoVotazioneIterCommissione = @RisultatoVotazioneIterCommissione");

                                    if (string.IsNullOrEmpty(idRelatore))
                                        query = query.Replace("{FIELD_REL_1}", "");
                                    else
                                        query = query.Replace("{FIELD_REL_1}",
                                            ", UIDPersonaRelatore1 = @UIDPersonaRelatore1");

                                    if (string.IsNullOrEmpty(idRelatore2))
                                        query = query.Replace("{FIELD_REL_2}", "");
                                    else
                                        query = query.Replace("{FIELD_REL_2}",
                                            ", UIDPersonaRelatore2 = @UIDPersonaRelatore2");

                                    if (string.IsNullOrEmpty(idRelatoreMinoranza))
                                        query = query.Replace("{FIELD_REL_MINORANZA}", "");
                                    else
                                        query = query.Replace("{FIELD_REL_MINORANZA}",
                                            ", UIDPersonaRelatoreMinoranza = @UIDPersonaRelatoreMinoranza");

                                    if (tipoAttoEnum is TipoAttoEnum.MOZ
                                        || tipoAttoEnum is TipoAttoEnum.ODG
                                        || tipoAttoEnum is TipoAttoEnum.RIS)
                                    {
                                        if (string.IsNullOrEmpty(uidSeduta))
                                            query = query.Replace("{FIELD_UIDSEDUTA}", "");
                                        else
                                            query = query.Replace("{FIELD_UIDSEDUTA}",
                                                ", UIDSeduta = @UIDSeduta");
                                    }
                                    else
                                    {
                                        query = query.Replace("{FIELD_UIDSEDUTA}", "");
                                    }

                                    using (var command = new SqlCommand(query, connection))
                                    {
                                        // Assegna i valori dei parametri
                                        command.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                        command.Parameters.AddWithValue("@AreaPolitica", ParseDescr2Enum_Area(area));
                                        command.Parameters.AddWithValue("@Pubblicato", pubblicato);
                                        command.Parameters.AddWithValue("@Sollecito", sollecito);
                                        command.Parameters.AddWithValue("@Protocollo", protocollo);
                                        command.Parameters.AddWithValue("@CodiceMateria", codiceMateria);
                                        command.Parameters.AddWithValue("@Emendato", emendato);
                                        command.Parameters.AddWithValue("@BURL", burl);
                                        command.Parameters.AddWithValue("@DCR", dcr);
                                        command.Parameters.AddWithValue("@DCRC", dcrc);
                                        command.Parameters.AddWithValue("@DCRL", dcrl);
                                        command.Parameters.AddWithValue("@AreaTematica", CleanHtmlTags(areaTematica));
                                        command.Parameters.AddWithValue("@IDTipo_Risposta_Effettiva",
                                            tipoRispostaEffettivaAttoInt);
                                        command.Parameters.AddWithValue("@AltriSoggetti", CleanHtmlTags(altriSoggetti));
                                        command.Parameters.AddWithValue("@ImpegniScadenze",
                                            CleanHtmlTags(noteImpegni_e_scadenze));
                                        command.Parameters.AddWithValue("@StatoAttuazione",
                                            CleanHtmlTags(statoAttuazione));
                                        command.Parameters.AddWithValue("@CompetenzaMonitoraggio",
                                            CleanHtmlTags(competenzaMonitoraggio));
                                        command.Parameters.AddWithValue("@MonitoraggioConcluso", monitoraggioConcluso);
                                        command.Parameters.AddWithValue("@IterMultiplo", iterMultiplo);

                                        command.Parameters.AddWithValue("@Privacy_Dati_Personali_Giudiziari",
                                            privacy_dati_personali_giudiziari_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione_Salute",
                                            privacy_divieto_pubblicazione_salute_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione_Vita_Sessuale",
                                            privacy_divieto_pubblicazione_vita_sessuale_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione",
                                            privacy_divieto_pubblicazione_sn);
                                        command.Parameters.AddWithValue("@Privacy_Dati_Personali_Sensibili",
                                            privacy_dati_personali_sensibili_sn);
                                        command.Parameters.AddWithValue("@Privacy_Divieto_Pubblicazione_Altri",
                                            privacy_divieto_pubblicazione_altri_sn);
                                        command.Parameters.AddWithValue("@Privacy_Dati_Personali_Semplici",
                                            privacy_dati_personali_semplici_sn);
                                        command.Parameters.AddWithValue("@Privacy", privacy_sn);

                                        if (string.IsNullOrEmpty(dataAnnunzio))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataAnnunzio",
                                                ParseDateTime(dataAnnunzio));
                                        }

                                        if (string.IsNullOrEmpty(dataTrasmissioneMonitoraggio)
                                            || tipoAttoEnum == TipoAttoEnum.ITL
                                            || tipoAttoEnum == TipoAttoEnum.ITR
                                            || tipoAttoEnum == TipoAttoEnum.IQT)
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataTrasmissioneMonitoraggio",
                                                ParseDateTime(dataTrasmissioneMonitoraggio));
                                        }

                                        if (string.IsNullOrEmpty(dataComunicazioneAssemblea))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataComunicazioneAssemblea",
                                                ParseDateTime(dataComunicazioneAssemblea));
                                        }

                                        if (string.IsNullOrEmpty(dataTrasmissione))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataTrasmissione",
                                                ParseDateTime(dataTrasmissione));
                                        }

                                        if (string.IsNullOrEmpty(dataSedutaRisposta))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataSedutaRisposta",
                                                ParseDateTime(dataSedutaRisposta));
                                        }

                                        if (string.IsNullOrEmpty(dataComunicazioneAssembleaRisposta))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataComunicazioneAssembleaRisposta",
                                                ParseDateTime(dataComunicazioneAssembleaRisposta));
                                        }

                                        if (string.IsNullOrEmpty(dataProposta))
                                        {
                                            // ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataProposta",
                                                ParseDateTime(dataProposta));
                                        }

                                        if (string.IsNullOrEmpty(tipoChiusuraIter))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoChiusuraIter",
                                                ParseDescr2Enum_ChiusuraIter(tipoChiusuraIter));
                                        }

                                        if (string.IsNullOrEmpty(tipoChiusuraIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoChiusuraIterCommissione",
                                                ParseDescr2Enum_ChiusuraIter(tipoChiusuraIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(dataChiusuraIter))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataChiusuraIter",
                                                ParseDateTime(dataChiusuraIter));
                                        }

                                        if (string.IsNullOrEmpty(dataChiusuraIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@DataChiusuraIterCommissione",
                                                ParseDateTime(dataChiusuraIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(tipoVotazione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoVotazioneIter",
                                                ParseDescr2Enum_TipoVotazioneIter(tipoVotazione));
                                        }

                                        if (string.IsNullOrEmpty(tipoVotazioneIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@TipoVotazioneIterCommissione",
                                                ParseDescr2Enum_TipoVotazioneIter(tipoVotazioneIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(risultatoVotazioneIterCommissione))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@RisultatoVotazioneIterCommissione",
                                                ParseDescr2Enum_RisultatoVotazioneIter(
                                                    risultatoVotazioneIterCommissione));
                                        }

                                        if (string.IsNullOrEmpty(idRelatore))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@UIDPersonaRelatore1",
                                                relatore1);
                                        }

                                        if (string.IsNullOrEmpty(idRelatore2))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@UIDPersonaRelatore2",
                                                relatore2);
                                        }

                                        if (string.IsNullOrEmpty(idRelatoreMinoranza))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@UIDPersonaRelatoreMinoranza",
                                                relatoreMinoranza);
                                        }

                                        if (string.IsNullOrEmpty(idRelatoreMinoranza))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue("@UIDPersonaRelatoreMinoranza",
                                                relatoreMinoranza);
                                        }

                                        if (tipoAttoEnum is TipoAttoEnum.MOZ
                                            || tipoAttoEnum is TipoAttoEnum.ODG
                                            || tipoAttoEnum is TipoAttoEnum.RIS)
                                        {
                                            if (!string.IsNullOrEmpty(uidSeduta))
                                                command.Parameters.AddWithValue("@UIDSeduta",
                                                    uidSeduta);
                                        }

                                        // Esegui la query di inserimento
                                        var resQuery = command.ExecuteNonQuery();
                                        if (resQuery == 1)
                                        {
                                            Console.WriteLine($"[{row}/{rowCount}] {etichettaAtto}");
                                            attiImportati.Add(nodeIdFromAlfresco, attoImportato);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _log.Error(
                                    $"Errore: {row}, {legislaturaFromAlfresco}, {tipoAttoFromAlfresco}, {numeroAtto}",
                                    e);
                                Console.WriteLine("Errore durante l'elaborazione della riga. Dettagli dell'errore: " +
                                                  e.Message);
                                sb.AppendLine(
                                    $"{foglio}, {row}, {legislaturaFromAlfresco}, {tipoAttoFromAlfresco}, {numeroAtto}, {e.Message}");
                            }
                        }

                        using (var sw = File.AppendText(filePath))
                        {
                            sw.WriteLine(sb.ToString());
                        }

                        Console.WriteLine("Complete!");
                    }

                using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                {
                    connection.Open();
                    if (attiImportati.Any())
                        foreach (var abbinamentoDasi in abbinamentiDasi)
                        {
                            if (!attiImportati.TryGetValue(abbinamentoDasi.idNodoAlfresco, out var attoPadre))
                            {
                                Console.WriteLine(
                                    $"Atto padre non trovato: {abbinamentoDasi.idNodoAlfresco}, {abbinamentoDasi.Legislatura}, {abbinamentoDasi.NumeroAtto}");
                                continue;
                            }

                            var abbinate = abbinamentiDasi.Where(a =>
                                a.idAssociazione.Equals(abbinamentoDasi.idAssociazione)
                                && !a.idNodoAlfresco
                                    .Equals(abbinamentoDasi.idNodoAlfresco));

                            foreach (var abbinataDasi in abbinate)
                            {
                                if (!attiImportati.TryGetValue(abbinataDasi.idNodoAlfresco, out var attoAbbinato))
                                {
                                    Console.WriteLine(
                                        $"Atto abbinato non trovato: {abbinataDasi.idNodoAlfresco}, {abbinataDasi.Legislatura}, {abbinataDasi.NumeroAtto}");
                                    continue;
                                }

                                var queryInsertAbbinamento =
                                    @"INSERT INTO ATTI_ABBINAMENTI (Uid, Data, UIDAtto, UIDAttoAbbinato)
                                                    VALUES
                                                (@IdAbbinamento, GETDATE(), @UIDAtto, @UIDAttoAbbinato)";
                                using (var commandInsertAbbinamento =
                                       new SqlCommand(queryInsertAbbinamento, connection))
                                {
                                    commandInsertAbbinamento.Parameters.AddWithValue("@IdAbbinamento", Guid.NewGuid());
                                    commandInsertAbbinamento.Parameters.AddWithValue("@UIDAtto", attoPadre.UidAtto);
                                    commandInsertAbbinamento.Parameters.AddWithValue("@UIDAttoAbbinato",
                                        attoAbbinato.UidAtto);
                                    commandInsertAbbinamento.ExecuteNonQuery();
                                }
                            }
                        }
                }

                using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                {
                    connection.Open();
                    if (attiImportati.Any())
                    {
                        var listaPiattaRaggruppata = abbinamentiRaggruppatiPerLegislatura
                            .SelectMany(g => g.Abbinamenti)
                            .ToList();
                        var count_nontrovati = 0;
                        var count_errore = 0;
                        var count_inseriti = 0;
                        foreach (var abbinamentoGea in listaPiattaRaggruppata)
                        {
                            if (!attiImportati.TryGetValue(abbinamentoGea.idNodoAlfresco, out var attoPadre))
                            {
                                count_nontrovati++;
                                Console.WriteLine(
                                    $"Atto padre non trovato: {abbinamentoGea.idNodoAlfresco}, {abbinamentoGea.Legislatura}, {abbinamentoGea.NumeroAtto}");
                                continue;
                            }

                            try
                            {
                                var queryInsertAbbinamentoGea =
                                    @"INSERT INTO ATTI_ABBINAMENTI (Uid, Data, UIDAtto, UIDAttoAbbinato)
                                                    VALUES
                                                (@IdAbbinamento, GETDATE(), @UIDAtto, @UIDAttoAbbinato)";
                                using (var commandInsertAbbinamentoGea =
                                       new SqlCommand(queryInsertAbbinamentoGea, connection))
                                {
                                    commandInsertAbbinamentoGea.Parameters.AddWithValue("@IdAbbinamento",
                                        Guid.NewGuid());
                                    commandInsertAbbinamentoGea.Parameters.AddWithValue("@UIDAtto", attoPadre.UidAtto);
                                    commandInsertAbbinamentoGea.Parameters.AddWithValue("@UIDAttoAbbinato",
                                        abbinamentoGea.UIDAtto);
                                    commandInsertAbbinamentoGea.ExecuteNonQuery();
                                }

                                count_inseriti++;
                            }
                            catch (Exception e)
                            {
                                count_errore++;
                            }
                        }

                        Console.WriteLine(
                            $"Risultato: {count_inseriti}, {count_nontrovati}, {count_errore}");
                    }
                }
            }
        }

        private static Guid GetDefaultIdFromDatabase()
        {
            var result = Guid.Empty;
            var connectionString = AppsettingsConfiguration.CONNECTIONSTRING;

            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            SELECT UID_persona 
            FROM View_UTENTI 
            WHERE userAD LIKE '%max.pagliaro'";

                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        var uid = command.ExecuteScalar();
                        if (uid != null && uid != DBNull.Value)
                        {
                            result = (Guid)uid;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Errore durante l'accesso al database: " + ex.Message);
                    }
                }
            }

            return result;
        }

        private static List<LegislaturaDto> GetLegislatureFromDataBase()
        {
            var result = new List<LegislaturaDto>();
            var connectionString = AppsettingsConfiguration.CONNECTIONSTRING;

            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            SELECT 
                id_legislatura, 
                num_legislatura, 
                durata_legislatura_da, 
                durata_legislatura_a, 
                attiva, 
                id_causa_fine 
            FROM 
                legislature";

                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var legislatura = new LegislaturaDto
                                {
                                    id_legislatura = reader.GetInt32(0),
                                    num_legislatura = reader.GetString(1),
                                    durata_legislatura_da = reader.GetDateTime(2),
                                    durata_legislatura_a = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                    attiva = reader.GetBoolean(4),
                                    id_causa_fine = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
                                };
                                result.Add(legislatura);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Errore durante l'accesso al database: " + ex.Message);
                    }
                }
            }

            return result;
        }

        private static int ParseDescr2Enum_TipoMozione(string tipoMozioneDescr)
        {
            switch (tipoMozioneDescr.ToLower())
            {
                case "mozione di censura":
                {
                    return (int)TipoMOZEnum.CENSURA;
                }
                case "mozione di sfiducia":
                {
                    return (int)TipoMOZEnum.SFIDUCIA;
                }
                default:
                {
                    return (int)TipoMOZEnum.ORDINARIA;
                }
            }
        }

        private static int GetIdGruppoFromFunction(int idPersona, DateTime dataFirma)
        {
            var query = "SELECT dbo.get_IDgruppoNelPeriodo_from_idpersona(@idPersona, @dataFirma, 0);";

            using (var conn = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
            {
                conn.Open();

                using (var command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@idPersona", idPersona);
                    command.Parameters.AddWithValue("@dataFirma", dataFirma);

                    var result = command.ExecuteScalar(); // ExecuteScalar is used for single value return

                    return (result != DBNull.Value && result != null) ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private static int ParseDescr2Enum_TipoVotazioneIter(string tipoVotazione)
        {
            switch (tipoVotazione.ToLower())
            {
                case "appello nominale":
                {
                    return (int)TipoVotazioneIterEnum.APPELLO_NOMINALE;
                }
                case "palese alzata di mano":
                {
                    return (int)TipoVotazioneIterEnum.PALESE_ALZATA_DI_MANO;
                }
                case "scrutinio segreto":
                {
                    return (int)TipoVotazioneIterEnum.SCRUTINIO_SEGRETO;
                }
                default:
                {
                    return (int)TipoVotazioneIterEnum.NESSUNO;
                }
            }
        }

        private static int ParseDescr2Enum_RisultatoVotazioneIter(string risultatoVotazione)
        {
            switch (risultatoVotazione.ToLower())
            {
                case "a maggiornaza":
                {
                    return (int)RisultatoVotazioneIterEnum.MAGGIORNAZA;
                }
                case "all'unanimità":
                {
                    return (int)RisultatoVotazioneIterEnum.UNANIMITÀ;
                }
                default:
                {
                    return (int)RisultatoVotazioneIterEnum.NESSUNO;
                }
            }
        }

        private static int ParseDescr2Enum_ChiusuraIter(string chiusuraIter)
        {
            switch (chiusuraIter.ToLower())
            {
                case "atto approvato":
                {
                    return (int)TipoChiusuraIterEnum.APPROVATO;
                }
                case "atto respinto":
                {
                    return (int)TipoChiusuraIterEnum.RESPINTO;
                }
                case "decadenza per fine legislatura":
                {
                    return (int)TipoChiusuraIterEnum.DECADENZA_PER_FINE_LEGISLATURA;
                }
                case "ritiro":
                case "ritirato":
                case "ritirata":
                case "ritira":
                {
                    return (int)TipoChiusuraIterEnum.RITIRATO;
                }
                case "inammissibile":
                {
                    return (int)TipoChiusuraIterEnum.INAMMISSIBILE;
                }
                case "chiusura per motivi diversi":
                {
                    return (int)TipoChiusuraIterEnum.CHIUSURA_PER_MOTIVI_DIVERSI;
                }
                case "decadenza per fine mandato consigliere":
                {
                    return (int)TipoChiusuraIterEnum.DECADENZA_PER_FINE_MANDATO_CONSIGLIERE;
                }
                case "decadenza":
                {
                    return (int)TipoChiusuraIterEnum.DECADUTO;
                }
                case "comunicazione all'assemblea": // #1086
                {
                    return (int)TipoChiusuraIterEnum.COMUNICAZIONE_ASSEMBLEA;
                }
                case "trattazione in assemblea": // #1086
                {
                    return (int)TipoChiusuraIterEnum.TRATTAZIONE_ASSEMBLEA;
                }
                default:
                {
                    return (int)TipoChiusuraIterEnum.CHIUSURA_PER_MOTIVI_DIVERSI;
                }
            }
        }

        private static int ParseDescr2Enum_Area(string area)
        {
            area = area.Trim();
            if (string.IsNullOrEmpty(area))
                return (int)AreaPoliticaIntEnum.Misto;
            switch (area.ToLower())
            {
                case "maggioranza":
                case "monoranza":
                    return (int)AreaPoliticaIntEnum.Maggioranza;
                case "minoranza":
                    return (int)AreaPoliticaIntEnum.Minoranza;
                case "misto maggioranza/minoranza":
                    return (int)AreaPoliticaIntEnum.Misto_Maggioranza_Minoranza;
                case "misto minoranza":
                    return (int)AreaPoliticaIntEnum.Misto_Minoranza;
                case "misto maggioranza":
                    return (int)AreaPoliticaIntEnum.Misto_Maggioranza;
                case "misto":
                    return (int)AreaPoliticaIntEnum.Misto;
                default:
                {
                    return (int)AreaPoliticaIntEnum.Misto;
                }
            }
        }

        private static int ConvertToIntTipoRisposta(string tipoRispostaFromAlfresco)
        {
            switch (tipoRispostaFromAlfresco.ToLower())
            {
                case "scritta":
                case "scritto":
                case "risposta_scritta":
                    return (int)TipoRispostaEnum.SCRITTA;
                case "orale":
                case "risposta_orale":
                    return (int)TipoRispostaEnum.ORALE;
                case "in commissione":
                case "risposta_in_commissione":
                    return (int)TipoRispostaEnum.COMMISSIONE;
                case "iter in assemblea + commissione":
                case "iter in assemblea e commissione":
                case "risposta_iter_in_assemblea_e_commissione":
                    return (int)TipoRispostaEnum.ITER_IN_ASSEMBLEA_COMMISSIONE;
                case "iter in assemblea":
                    return (int)TipoRispostaEnum.ITER_IN_ASSEMBLEA;
                default:
                    return 0;
            }
        }

        private static TipoAttoEnum ConvertToEnumTipoAtto(string tipoFromAlfresco)
        {
            switch (tipoFromAlfresco)
            {
                case "interrogazione":
                    return TipoAttoEnum.ITR;
                case "interpellanza":
                    return TipoAttoEnum.ITL;
                case "interrogazione_question_time":
                    return TipoAttoEnum.IQT;
                case "mozione":
                    return TipoAttoEnum.MOZ;
                case "ordine_del_giorno":
                    return TipoAttoEnum.ODG;
                case nameof(TipoAttoEnum.PDL):
                    return TipoAttoEnum.PDL;
                case nameof(TipoAttoEnum.REF):
                    return TipoAttoEnum.REF;
                case nameof(TipoAttoEnum.PDA):
                    return TipoAttoEnum.PDA;
                case nameof(TipoAttoEnum.DOC):
                    return TipoAttoEnum.DOC;
                case nameof(TipoAttoEnum.ORG):
                    return TipoAttoEnum.ORG;
                case nameof(TipoAttoEnum.PDN):
                    return TipoAttoEnum.PDN;
                case nameof(TipoAttoEnum.PLP):
                    return TipoAttoEnum.PLP;
                case nameof(TipoAttoEnum.PRE):
                    return TipoAttoEnum.PRE;
                case nameof(TipoAttoEnum.REL):
                    return TipoAttoEnum.REL;
                case "risoluzione":
                    return TipoAttoEnum.RIS;
                default:
                    return TipoAttoEnum.ALTRO;
            }
        }

        // Metodo per inserire il record principale
        private static Guid InsertRispostaRecord(SqlConnection connection, Guid uidAtto, string tipoRispostaAssociata,
            int tipoOrgano,
            object dataRisposta, object dataTrasmissione, object dataTrattazione, int? idOrgano, string subQuery)
        {
            var guidRisposta = Guid.NewGuid();
            var query =
                @"INSERT INTO ATTI_RISPOSTE (Uid, UIDAtto, Tipo, TipoOrgano, Data, DataTrasmissione, DataTrattazione, IdOrgano, DescrizioneOrgano)
          VALUES (@Uid, @UIDAtto, @Tipo, @TipoOrgano, @Data, @DataTrasmissione, @DataTrattazione, @IdOrgano, " +
                subQuery + ")";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Uid", guidRisposta);
                command.Parameters.AddWithValue("@UIDAtto", uidAtto);
                var tipoRispostaAssociataInt = ConvertToIntTipoRisposta(tipoRispostaAssociata);
                command.Parameters.AddWithValue("@Tipo", tipoRispostaAssociataInt);
                command.Parameters.AddWithValue("@TipoOrgano", tipoOrgano);
                command.Parameters.Add("@Data", SqlDbType.DateTime).Value = ConvertToSqlDateTime(dataRisposta);
                command.Parameters.Add("@DataTrasmissione", SqlDbType.DateTime).Value =
                    ConvertToSqlDateTime(dataTrasmissione);
                command.Parameters.Add("@DataTrattazione", SqlDbType.DateTime).Value =
                    ConvertToSqlDateTime(dataTrattazione);
                command.Parameters.AddWithValue("@IdOrgano", idOrgano ?? 0);

                command.ExecuteNonQuery();
                return guidRisposta;
            }
        }

        // Metodo per inserire le risposte associate
        private static void InsertRispostaAssociataRecord(SqlConnection connection, Guid uidAtto,
            string tipoRispostaAssociata, int tipoOrgano,
            int? idOrgano, string subQuery, Guid uidRispostaMadre)
        {
            var query =
                $@"INSERT INTO ATTI_RISPOSTE (UIDAtto, Tipo, TipoOrgano, IdOrgano, DescrizioneOrgano, UIDRispostaAssociata)
VALUES (@UIDAtto, @Tipo, @TipoOrgano, @IdOrgano, {subQuery}, @UIDRispostaAssociata)";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UIDAtto", uidAtto);
                command.Parameters.AddWithValue("@Tipo", ConvertToIntTipoRisposta(tipoRispostaAssociata));
                command.Parameters.AddWithValue("@TipoOrgano", tipoOrgano);
                command.Parameters.AddWithValue("@IdOrgano", idOrgano ?? 0);
                command.Parameters.AddWithValue("@UIDRispostaAssociata", uidRispostaMadre);
                command.ExecuteNonQuery();
            }
        }

        private static object GetUidPersona(SqlConnection connection, int idPersona)
        {
            var queryGetPersona =
                @"SELECT UID_persona FROM View_UTENTI WHERE id_persona = @id_persona";
            using (var commandGetPersona = new SqlCommand(queryGetPersona, connection))
            {
                commandGetPersona.Parameters.AddWithValue("@id_persona", idPersona);
                var find_uid_persona = commandGetPersona.ExecuteScalar();
                return find_uid_persona;
            }
        }

        public static string CleanHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Sostituisci i tag <b> e </b> con <strong> e </strong>
            input = input.Replace("<b>", "<strong>").Replace("</b>", "</strong>");
            input = input.Replace("<i>", "<em>").Replace("</i>", "</em>");

            return input;
        }

        public static DateTime ParseDateTime(string dateTime)
        {
            try
            {
                // Definisci una cultura specifica per il parsing (italiano)
                var italianCulture = new CultureInfo("it-IT");
                string[] supportedFormats =
                {
                    "yyyy-MM-dd HH:mm:ss", // Formato ISO senza "T"
                    "yyyy-MM-dd", // Formato ISO senza "T"
                    "yyyy-MM-ddTHH:mm:ss", // Formato ISO con "T"
                    "dd/MM/yyyy HH:mm:ss" // Formato italiano standard
                };

                if (dateTime.Contains("T"))
                {
                    // Parsing con il formato italiano
                    try
                    {
                        var dateT = DateTime.ParseExact(dateTime.Substring(0, 19).Replace("T", " "), supportedFormats,
                            italianCulture, DateTimeStyles.None);
                        return dateT;
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            $"Data in errore: {dateTime}, modificata in {dateTime.Substring(0, 19).Replace("T", " ")}",
                            e);
                    }
                }

                // Parsing della stringa con la cultura italiana
                var date = DateTime.ParseExact(dateTime, supportedFormats, italianCulture, DateTimeStyles.None);
                return date;
            }
            catch (Exception e)
            {
                throw new Exception($"Data in errore: {dateTime}", e);
            }
        }

        private static object ConvertToSqlDateTime(object dateValue)
        {
            if (dateValue == null || dateValue == DBNull.Value)
            {
                return DBNull.Value; // Ritorna DBNull se il valore è null
            }

            if (string.IsNullOrEmpty(dateValue.ToString()))
            {
                return DBNull.Value;
            }

            // Prova a fare il parsing se il valore è una stringa
            if (dateValue is string dateString)
            {
                try
                {
                    return ParseDateTime(dateString); // Parsing robusto della stringa
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"Formato data non valido: {dateString}");
                }
            }

            throw new ArgumentException("Valore data non riconosciuto.");
        }

        public class AttoImportato
        {
            public Guid UidAtto { get; set; }
            public string idNodoAlfresco { get; set; }
            public string Tipo { get; set; }
            public string Legislatura { get; set; }
            public string NumeroAtto { get; set; }
        }

        public class AbbinamentoDasi
        {
            public string idNodoAlfresco { get; set; }
            public string idAssociazione { get; set; }
            public string Legislatura { get; set; }
            public string NumeroAtto { get; set; }
        }

        public class AbbinamentoGea
        {
            public string idNodoAlfresco { get; set; }
            public string Legislatura { get; set; }
            public string NumeroAtto { get; set; }
            public string NumeroAtto_Gea { get; set; }
            public string TipoAtto_Gea { get; set; }
            public string Ufficiale { get; set; }
            public Guid UIDAtto { get; set; }

            public bool IsUfficiale()
            {
                if (string.IsNullOrEmpty(Ufficiale))
                    return false;

                if (Ufficiale.ToLower().Equals("ufficiale")) return true;

                return false;
            }
        }
    }
}