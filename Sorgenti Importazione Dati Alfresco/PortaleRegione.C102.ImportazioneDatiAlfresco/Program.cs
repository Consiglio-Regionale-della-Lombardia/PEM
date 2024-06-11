using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OfficeOpenXml;
using PortaleRegione.Crypto;
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using static PortaleRegione.C102.ImportazioneDatiAlfresco.Program;

namespace PortaleRegione.C102.ImportazioneDatiAlfresco
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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

            var auth = AutenticazioneAPI();

            var legislatureFromApi = GetLegislatureFromAPI(auth);

            using (var package = new ExcelPackage(new FileInfo(percorsoXLS)))
            {
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

                    if (abbinamentoSind_Ind.Legislatura == "33")
                    {
                        continue;
                    }

                    abbinamentiDasi.Add(abbinamentoSind_Ind);
                }

                var worksheetAssociazione_Gea =
                    package.Workbook.Worksheets.First(w => w.Name.Equals(foglioAssociazione_Gea));
                var cellsAssociazione_Gea = worksheetAssociazione_Gea.Cells;
                var rowCountASind_Gea = worksheetAssociazione_Gea.Dimension.Rows;

                var abbinamentiGea = new List<AbbinamentoGea>();
                for (var rowAGea = 2; rowAGea <= rowCountASind_Gea; rowAGea++)
                {
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

                        if (abbinamentoGea.Legislatura == "33")
                        {
                            continue;
                        }

                        if (abbinamentoGea.IsUfficiale())
                            abbinamentiGea.Add(abbinamentoGea);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                var abbinamentiRaggruppatiPerLegislatura = abbinamentiGea
                    .GroupBy(a => a.Legislatura)
                    .Select(gruppo => new
                    {
                        Legislatura = gruppo.Key,
                        Abbinamenti = gruppo.ToList()
                    })
                    .ToList();

                var insertSeduta = @"INSERT INTO SEDUTE (UIDSeduta, Data_seduta, id_legislatura, Note, DataCreazione) 
                                     VALUES (@UIDSeduta, @Data_seduta, @id_legislatura, @Note, GETDATE())";
                using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                {
                    connection.Open();
                    foreach (var item in abbinamentiRaggruppatiPerLegislatura)
                    {
                        if (item.Legislatura == "33")
                        {
                            continue;
                        }

                        var command = new SqlCommand(insertSeduta, connection);
                        var uidSeduta = Guid.NewGuid();
                        var legislaturaSeduta = legislatureFromApi.First(l => l.id_legislatura.Equals(int.Parse(item.Legislatura)));
                        // Assegna i valori dei parametri
                        command.Parameters.AddWithValue("@UIDSeduta", uidSeduta);
                        command.Parameters.AddWithValue("@Data_seduta", legislaturaSeduta.durata_legislatura_da);
                        command.Parameters.AddWithValue("@id_legislatura", legislaturaSeduta.id_legislatura);
                        command.Parameters.AddWithValue("@Note", "Contenitore atti importati da Alfresco");

                        var resQuery = command.ExecuteNonQuery();
                        if (resQuery == 1)
                        {
                            foreach (var abbinamentoGea in item.Abbinamenti)
                            {
                                var insertAttoGea = @"IF NOT EXISTS 
                                    (SELECT 1 FROM ATTI WHERE NAtto = @NAtto AND IDTipoAtto = @IDTipoAtto AND UIDSeduta = @UIDSeduta)
                                    BEGIN
                                        INSERT INTO ATTI (UIDAtto, NAtto, IDTipoAtto, UIDSeduta)
                                            VALUES (@UIDAtto, @NAtto, @IDTipoAtto, @UIDSeduta)        
                                    END";
                                var commandAttoGea = new SqlCommand(insertAttoGea, connection);
                                abbinamentoGea.UIDAtto = Guid.NewGuid();
                                commandAttoGea.Parameters.AddWithValue("@UIDAtto", abbinamentoGea.UIDAtto);
                                commandAttoGea.Parameters.AddWithValue("@NAtto", abbinamentoGea.NumeroAtto_Gea);
                                commandAttoGea.Parameters.AddWithValue("@UIDSeduta", uidSeduta);
                                commandAttoGea.Parameters.AddWithValue("@IDTipoAtto", (int)ConvertToEnumTipoAtto(abbinamentoGea.TipoAtto_Gea));
                                var resQueryGea = commandAttoGea.ExecuteNonQuery();
                            }
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

                var attiImportati = new Dictionary<string, AttoImportato>();
                foreach (var foglio in foglioAtti)
                {
                    using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                    {
                        connection.Open();
                        var worksheetAtti = package.Workbook.Worksheets.First(w => w.Name.Equals(foglio));
                        var cellsAtti = worksheetAtti.Cells;

                        var rowCount = worksheetAtti.Dimension.Rows;
                        sb.Clear();
                        

                        for (var row = 2; row <= rowCount; row++)
                        {
                            var attoImportato = new AttoImportato();
                            var tipoAttoFromAlfresco = Convert.ToString(cellsAtti[row, 4].Value);
                            var legislaturaFromAlfresco = Convert.ToString(cellsAtti[row, 47].Value);

                            if (legislaturaFromAlfresco == "33")
                                continue;

                            var numeroAtto = Convert.ToString(cellsAtti[row, 19].Value);

                            attoImportato.Legislatura = legislaturaFromAlfresco;
                            attoImportato.Tipo = tipoAttoFromAlfresco;
                            attoImportato.NumeroAtto = numeroAtto;

                            try
                            {
                                var proponenteId = auth.id;
                                var chkf = 0;
                                var id_gruppo = 0;
                                // nodeid alfresco 
                                var nodeIdFromAlfresco = Convert.ToString(cellsAtti[row, 2].Value);
                                attoImportato.idNodoAlfresco = nodeIdFromAlfresco;

                                var stato = Convert.ToString(cellsAtti[row, 10].Value);
                                var statoId = ParseDescr2Enum_Stato(stato);

                                if (statoId == (int)StatiAttoEnum.PRESENTATO)
                                {
                                    throw new Exception("Atto scartato perchè in stato presentato.");
                                }

                                //legislatura
                                if (string.IsNullOrEmpty(legislaturaFromAlfresco))
                                    throw new Exception("Legislatura non valida");

                                var legislatura =
                                    legislatureFromApi.FirstOrDefault(i =>
                                        i.id_legislatura.Equals(Convert.ToInt16(legislaturaFromAlfresco)));

                                if (legislatura == null)
                                {
                                    throw new Exception(
                                        $"Legislatura {legislaturaFromAlfresco} non trovata nel database.");
                                }

                                var tipoAttoEnum = ConvertToEnumTipoAtto(tipoAttoFromAlfresco);

                                //tipo mozione
                                var tipoMozioneAttoFromAlfresco = Convert.ToString(cellsAtti[row, 22].Value);
                                var tipoMozione = ParseDescr2Enum_TipoMozione(tipoMozioneAttoFromAlfresco);

                                //etichette
                                var etichettaAtto = $"{tipoAttoEnum}_{numeroAtto}_{legislatura.num_legislatura}";
                                var etichettaAtto_Cifrata =
                                    CryptoHelper.EncryptString(etichettaAtto, AppsettingsConfiguration.MASTER_KEY);

                                var queryExists = @"SELECT UIDAtto FROM [ATTI_DASI] WHERE Etichetta = @Etichetta";

                                using (var commandExists = new SqlCommand(queryExists, connection))
                                {
                                    commandExists.Parameters.AddWithValue("@Etichetta", etichettaAtto);

                                    var result = commandExists.ExecuteScalar();

                                    if (result != null)
                                    {
                                        attoImportato.UidAtto = new Guid(result.ToString());
                                        Console.WriteLine($"[{row}/{rowCount}] {etichettaAtto}");
                                        attiImportati.Add(nodeIdFromAlfresco, attoImportato);

                                        sb.AppendLine(
                                            $"{foglio}, {row}, {legislaturaFromAlfresco}, {tipoAttoFromAlfresco}, {numeroAtto}, Il record esiste già.");
                                        continue;
                                    }
                                }

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
                                        @"INSERT INTO ATTI_NOTE (UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (@UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    var commandInsertNotaChiusuraIter =
                                        new SqlCommand(queryInsertNotaChiusuraIter, connection);
                                    commandInsertNotaChiusuraIter.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                    commandInsertNotaChiusuraIter.Parameters.AddWithValue("@UIDPersona",
                                        Guid.Parse("AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                    commandInsertNotaChiusuraIter.Parameters.AddWithValue("@Tipo",
                                        (int)TipoNotaEnum.CHIUSURA_ITER);
                                    commandInsertNotaChiusuraIter.Parameters.AddWithValue("@Nota", noteChiusuraIter);
                                    commandInsertNotaChiusuraIter.ExecuteNonQuery();
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
                                        @"INSERT INTO ATTI_NOTE (UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (@UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    var commandInsertNotaRisposta = new SqlCommand(queryInsertNotaRisposta, connection);
                                    commandInsertNotaRisposta.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                    commandInsertNotaRisposta.Parameters.AddWithValue("@UIDPersona",
                                        Guid.Parse("AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                    commandInsertNotaRisposta.Parameters.AddWithValue("@Tipo",
                                        (int)TipoNotaEnum.RISPOSTA);
                                    commandInsertNotaRisposta.Parameters.AddWithValue("@Nota", noteRisposta);
                                    commandInsertNotaRisposta.ExecuteNonQuery();
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
                                        @"INSERT INTO ATTI_NOTE (UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (@UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    var commandInsertNoteAggiuntive =
                                        new SqlCommand(queryInsertNoteAggiuntive, connection);
                                    commandInsertNoteAggiuntive.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                    commandInsertNoteAggiuntive.Parameters.AddWithValue("@UIDPersona",
                                        Guid.Parse("AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                    commandInsertNoteAggiuntive.Parameters.AddWithValue("@Tipo",
                                        (int)TipoNotaEnum.GENERALE_PRIVATA);
                                    commandInsertNoteAggiuntive.Parameters.AddWithValue("@Nota", noteAggiuntive);
                                    commandInsertNoteAggiuntive.ExecuteNonQuery();
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
                                        @"INSERT INTO ATTI_NOTE (UIDAtto, UIDPersona, Tipo, Data, Nota)
                                                    VALUES
                                                (@UIDAtto, @UIDPersona, @Tipo, GETDATE(), @Nota)";
                                    var commandInsertNoteAggiuntive2 =
                                        new SqlCommand(queryInsertNoteAggiuntive2, connection);
                                    commandInsertNoteAggiuntive2.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                    commandInsertNoteAggiuntive2.Parameters.AddWithValue("@UIDPersona",
                                        Guid.Parse("AC98DA99-862D-4CFF-90E7-D5B324AAA7AE")); // corrisponde a matteo.c
                                    commandInsertNoteAggiuntive2.Parameters.AddWithValue("@Tipo",
                                        (int)TipoNotaEnum.GENERALE_PRIVATA);
                                    commandInsertNoteAggiuntive2.Parameters.AddWithValue("@Nota", noteAggiuntive2);
                                    commandInsertNoteAggiuntive2.ExecuteNonQuery();
                                }

                                #endregion

                                #endregion

                                #region FIRME

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
                                        if (cellsFirme[rowF, 8].Value != null)
                                            data_firma = Convert.ToString(cellsFirme[rowF, 8].Value);

                                        var data_ritiro_firma = Convert.ToString(cellsFirme[rowF, 9].Value);
                                        var id_persona = cellsFirme[rowF, 10].Value.ToString();
                                        var nome_firmatario = cellsFirme[rowF, 14].Value.ToString().Trim();
                                        var primo_firmatario = Convert.ToBoolean(cellsFirme[rowF, 16].Value);

                                        var queryGetPersona =
                                            @"SELECT UID_persona FROM View_UTENTI WHERE id_persona = @id_persona";
                                        var commandGetPersona = new SqlCommand(queryGetPersona, connection);
                                        commandGetPersona.Parameters.AddWithValue("@id_persona", id_persona);
                                        var find_uid_persona = commandGetPersona.ExecuteScalar();
                                        if (find_uid_persona == DBNull.Value)
                                        {
                                            find_uid_persona = Guid.NewGuid();
                                            var queryInsertPersona =
                                                @"INSERT INTO join_persona_AD (UID_persona, id_persona)
                                       VALUES
                                     (@UID_persona, @id_persona)";
                                            // Esegui la query di inserimento dei dati nella tabella ATTI_FIRME
                                            var commandInsertPersona = new SqlCommand(queryInsertPersona, connection);
                                            commandInsertPersona.Parameters.AddWithValue("@UID_persona",
                                                find_uid_persona);
                                            commandInsertPersona.Parameters.AddWithValue("@id_persona", id_persona);
                                            commandInsertPersona.ExecuteNonQuery(); // Esegui l'inserimento dei dati
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

                                        if (string.IsNullOrEmpty(data_ritiro_firma) ||
                                            data_ritiro_firma.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                            queryInsertFirmatario = queryInsertFirmatario
                                                .Replace("{FIELD_DATA_RITIRO_FIRMA}", "")
                                                .Replace("{PARAM_DATA_RITIRO_FIRMA}", "");
                                        else
                                            queryInsertFirmatario = queryInsertFirmatario
                                                .Replace("{FIELD_DATA_RITIRO_FIRMA}", ", Data_ritirofirma")
                                                .Replace("{PARAM_DATA_RITIRO_FIRMA}", ", @Data_ritirofirma");

                                        // Esegui la query di inserimento dei dati nella tabella ATTI_FIRME
                                        var commandInsertFirmatario = new SqlCommand(queryInsertFirmatario, connection);
                                        commandInsertFirmatario.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                        commandInsertFirmatario.Parameters.AddWithValue("@UID_persona",
                                            find_uid_persona);
                                        commandInsertFirmatario.Parameters.AddWithValue("@FirmaCert",
                                            CryptoHelper.EncryptString(nome_firmatario,
                                                AppsettingsConfiguration.MASTER_KEY));
                                        commandInsertFirmatario.Parameters.AddWithValue("@Data_firma",
                                            CryptoHelper.EncryptString(data_firma,
                                                AppsettingsConfiguration.MASTER_KEY));
                                        if (string.IsNullOrEmpty(data_ritiro_firma) ||
                                            data_ritiro_firma.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // Ignored
                                        }
                                        else
                                        {
                                            commandInsertFirmatario.Parameters.AddWithValue("@Data_ritirofirma",
                                                CryptoHelper.EncryptString(data_ritiro_firma,
                                                    AppsettingsConfiguration.MASTER_KEY));
                                        }

                                        var id_gruppo_firmatario =
                                            ParseNomeFirmatarioToGruppo(nome_firmatario, legislatura.id_legislatura);
                                        commandInsertFirmatario.Parameters.AddWithValue("@PrimoFirmatario",
                                            Convert.ToBoolean(primo_firmatario));
                                        commandInsertFirmatario.Parameters.AddWithValue("@id_gruppo",
                                            id_gruppo_firmatario);
                                        commandInsertFirmatario.Parameters.AddWithValue("@Timestamp",
                                            DateTime.Parse(data_firma));
                                        commandInsertFirmatario.ExecuteNonQuery(); // Esegui l'inserimento dei dati

                                        chkf++;

                                        if (Convert.ToBoolean(primo_firmatario))
                                        {
                                            proponenteId = (Guid)find_uid_persona;
                                            id_gruppo = id_gruppo_firmatario;
                                        }
                                    }
                                }

                                if (chkf == 0) throw new Exception("Nessuna firma trovata per l'atto.");

                                if (chkf > 0 && id_gruppo == 0) throw new Exception("Nessun proponente trovato.");

                                #endregion

                                #region RISPOSTE ASSOCIATE

                                // Itera attraverso le righe del foglio delle risposte
                                for (var rowRA = 2; rowRA <= rowCountRA; rowRA++)
                                {
                                    // Ottieni il valore della cella nella seconda colonna della riga corrente
                                    var valoreCella = cellsRisposteAssociate[rowRA, 2].Value;

                                    // Verifica se il valore della cella corrisponde al numeroAtto
                                    if (valoreCella != null && valoreCella.ToString() == nodeIdFromAlfresco)
                                    {
                                        var tipoOrgano = (int)TipoOrganoEnum.COMMISSIONE;
                                        var sub_query =
                                            @"(SELECT TOP(1) nome_organo FROM dbo.organi WHERE id_organo = @IdOrgano)";
                                        var tipoRispostaAssociata = cellsRisposteAssociate[rowRA, 10].Value;
                                        var dataRispostaAssociata = cellsRisposteAssociate[rowRA, 12].Value;
                                        if (Convert.ToString(dataRispostaAssociata).Equals("NULL"))
                                        {
                                            dataRispostaAssociata = null;
                                        }

                                        var dataTrasmissioneRispostaAssociata = cellsRisposteAssociate[rowRA, 13].Value;
                                        if (Convert.ToString(dataTrasmissioneRispostaAssociata).Equals("NULL"))
                                        {
                                            dataTrasmissioneRispostaAssociata = null;
                                        }

                                        var dataTrattazioneRispostaAssociata = cellsRisposteAssociate[rowRA, 14].Value;
                                        if (Convert.ToString(dataTrattazioneRispostaAssociata).Equals("NULL"))
                                        {
                                            dataTrattazioneRispostaAssociata = null;
                                        }

                                        var idCommissioneRispostaAssociata = cellsRisposteAssociate[rowRA, 16].Value;

                                        if (Convert.ToString(idCommissioneRispostaAssociata).Equals("NULL"))
                                        {
                                            var nodeIdRisposta =
                                                Convert.ToString(cellsRisposteAssociate[rowRA, 7].Value);
                                            for (var rowRG = 2; rowRG <= rowCountRG; rowRG++)
                                            {
                                                var valoreCellaRiferimento = cellsRisposteGiunta[rowRG, 2].Value;
                                                if (valoreCellaRiferimento != null &&
                                                    valoreCellaRiferimento.ToString() == nodeIdRisposta)
                                                {
                                                    idCommissioneRispostaAssociata =
                                                        cellsRisposteGiunta[rowRG, 6].Value;
                                                    tipoOrgano = (int)TipoOrganoEnum.GIUNTA;
                                                    sub_query =
                                                        @"(SELECT TOP (1) dbo.cariche.nome_carica
                                                            FROM dbo.join_persona_organo_carica AS jpoc INNER JOIN 
                                                                 dbo.cariche ON jpoc.id_carica = dbo.cariche.id_carica 
                                                            WHERE dbo.cariche.id_carica = @IdOrgano)";
                                                    break;
                                                }
                                            }
                                        }

                                        var queryInsertRispostaAssociata =
                                            @"INSERT INTO ATTI_RISPOSTE (UIDAtto, Tipo, TipoOrgano, Data, DataTrasmissione, DataTrattazione, IdOrgano, DescrizioneOrgano)
                                         VALUES"
                                            + $"(@UIDAtto, @Tipo, @TipoOrgano, @Data, @DataTrasmissione, @DataTrattazione, @IdOrgano, {sub_query})";

                                        var commandRispostaAssociata =
                                            new SqlCommand(queryInsertRispostaAssociata, connection);
                                        commandRispostaAssociata.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                        commandRispostaAssociata.Parameters.AddWithValue("@Tipo",
                                            ConvertToIntTipoRisposta(tipoRispostaAssociata.ToString()
                                                .Replace("risposta_", "").Replace("_", " ")));
                                        commandRispostaAssociata.Parameters.AddWithValue("@TipoOrgano", tipoOrgano);
                                        commandRispostaAssociata.Parameters.Add("@Data", SqlDbType.DateTime).Value =
                                            (object)dataRispostaAssociata ?? DBNull.Value;
                                        commandRispostaAssociata.Parameters.Add("@DataTrasmissione", SqlDbType.DateTime)
                                            .Value = (object)dataTrasmissioneRispostaAssociata ?? DBNull.Value;
                                        commandRispostaAssociata.Parameters.Add("@DataTrattazione", SqlDbType.DateTime)
                                            .Value = (object)dataTrattazioneRispostaAssociata ?? DBNull.Value;
                                        commandRispostaAssociata.Parameters.AddWithValue("@IdOrgano",
                                            Convert.ToInt16(idCommissioneRispostaAssociata));

                                        commandRispostaAssociata.ExecuteNonQuery();
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

                                        var commandMonitoraggioCommissione =
                                            new SqlCommand(queryInsertMonitoraggio, connection);
                                        commandMonitoraggioCommissione.Parameters.AddWithValue("@IdMonitoraggio",
                                            Guid.NewGuid());
                                        commandMonitoraggioCommissione.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                        commandMonitoraggioCommissione.Parameters.AddWithValue("@Tipo", tipoOrgano);
                                        commandMonitoraggioCommissione.Parameters.AddWithValue("@Nome", nomeMonitorato);
                                        commandMonitoraggioCommissione.Parameters.AddWithValue("@IdOrganoMonitorato",
                                            Convert.ToInt16(idMonitorato));

                                        commandMonitoraggioCommissione.ExecuteNonQuery();
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

                                        var commandMonitoraggioGiunta =
                                            new SqlCommand(queryInsertMonitoraggio, connection);
                                        commandMonitoraggioGiunta.Parameters.AddWithValue("@IdMonitoraggio",
                                            Guid.NewGuid());
                                        commandMonitoraggioGiunta.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                        commandMonitoraggioGiunta.Parameters.AddWithValue("@Tipo", tipoOrgano);
                                        commandMonitoraggioGiunta.Parameters.AddWithValue("@Nome", nomeMonitorato);
                                        commandMonitoraggioGiunta.Parameters.AddWithValue("@IdOrganoMonitorato",
                                            Convert.ToInt16(idMonitorato));

                                        commandMonitoraggioGiunta.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                //oggetto
                                var oggetto = Convert.ToString(cellsAtti[row, 13].Value);

                                //oggetto presentato
                                var oggettoPresentato = Convert.ToString(cellsAtti[row, 21].Value);

                                if (!string.IsNullOrEmpty(oggettoPresentato)
                                    && string.IsNullOrEmpty(oggetto))
                                {
                                    oggetto = oggettoPresentato;
                                }

                                //premesse
                                var premesse =
                                    "Atto presentato in modalità cartacea. Il testo originale, scansionato, è inserito in allegato.";

                                //tipo risposta richiesta
                                var tipoRispostaRichiestaAttoFromAlfresco = Convert.ToString(cellsAtti[row, 20].Value);
                                var tipoRispostaRichiestaAttoInt =
                                    ConvertToIntTipoRisposta(tipoRispostaRichiestaAttoFromAlfresco);

                                //data presentazione
                                var dataPresentazioneFromAlfresco = Convert.ToString(cellsAtti[row, 16].Value);
                                if (string.IsNullOrEmpty(dataPresentazioneFromAlfresco))
                                    throw new Exception("Data presentazione non valida");
                                var dataPresentazione = Convert.ToDateTime(dataPresentazioneFromAlfresco).ToUniversalTime();
                                var dataPresentazione_Cifrata = CryptoHelper.EncryptString(
                                    dataPresentazione.ToString("dd/MM/yyyy HH:mm:ss"),
                                    AppsettingsConfiguration.MASTER_KEY);

                                var area = Convert.ToString(cellsAtti[row, 23].Value);

                                var dataAnnunzio = Convert.ToString(cellsAtti[row, 17].Value);
                                var codiceMateria = Convert.ToString(cellsAtti[row, 18].Value);
                                var protocollo = Convert.ToString(cellsAtti[row, 14].Value);

                                var pubblicato = cellsAtti[row, 8].Value.ToString().Equals("1");
                                var sollecito = cellsAtti[row, 9].Value.ToString().Equals("1");

                                var tipoChiusuraIter = Convert.ToString(cellsAtti[row, 28].Value);
                                var dataChiusuraIter = Convert.ToString(cellsAtti[row, 30].Value);

                                var emendatoFromAlfresco = Convert.ToString(cellsAtti[row, 32].Value);
                                var emendato = !string.IsNullOrEmpty(emendatoFromAlfresco)
                                    ? emendatoFromAlfresco.Equals("1")
                                    : false;

                                var tipoVotazione = Convert.ToString(cellsAtti[row, 33].Value);
                                var dcrl = Convert.ToString(cellsAtti[row, 34].Value);
                                if (dcrl.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                {
                                    dcrl = string.Empty;
                                }

                                var dcr = Convert.ToString(cellsAtti[row, 44].Value);
                                if (dcr.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                {
                                    dcr = "0";
                                }

                                var dcrc = Convert.ToString(cellsAtti[row, 48].Value);
                                if (dcrc.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                {
                                    dcrc = "0";
                                }

                                var areaTematica = Convert.ToString(cellsAtti[row, 72].Value);
                                var altriSoggetti = Convert.ToString(cellsAtti[row, 71].Value);

                                var query = @"
                                INSERT INTO [ATTI_DASI] 
                                (UIDAtto, Tipo, TipoMOZ, NAtto, Etichetta, NAtto_search, Oggetto, Premesse, IDTipo_Risposta, DataPresentazione, IDStato, Legislatura, 
                                UIDPersonaCreazione, UIDPersonaPresentazione, idRuoloCreazione, UIDPersonaProponente, UIDPersonaPrimaFirma, 
                                UID_QRCode, id_gruppo, chkf, Timestamp, DataCreazione, OrdineVisualizzazione, AreaPolitica, Pubblicato, Sollecito, Protocollo, CodiceMateria{FIELD_DATA_ANNUNZIO}
{FIELD_TIPO_CHIUSURA_ITER}{FIELD_DATA_CHIUSURA_ITER}{FIELD_TIPO_VOTAZIONE_ITER}, Emendato, DCR, DCRC, DCRL, AreaTematica, AltriSoggetti, Proietta, Firma_su_invito, Eliminato) 
                                VALUES 
                                (@UIDAtto, @Tipo, @TipoMOZ, @NAtto, @Etichetta, @NAtto_search, @Oggetto, @Premesse, @IDTipo_Risposta, @DataPresentazione, @IDStato, @Legislatura, 
                                @UIDPersonaCreazione, @UIDPersonaPresentazione, @idRuoloCreazione, @UIDPersonaProponente, @UIDPersonaPrimaFirma, 
                                @UID_QRCode, @id_gruppo, @chkf, @Timestamp, GETDATE(), @OrdineVisualizzazione, @AreaPolitica, @Pubblicato, @Sollecito, @Protocollo, @CodiceMateria{PARAM_DATA_ANNUNZIO}
{PARAM_TIPO_CHIUSURA_ITER}{PARAM_DATA_CHIUSURA_ITER}{PARAM_TIPO_VOTAZIONE_ITER}, @Emendato, @DCR, @DCRC, @DCRL, @AreaTematica, @AltriSoggetti, 0, 0, 0)";

                                if (string.IsNullOrEmpty(dataAnnunzio))
                                    query = query
                                        .Replace("{FIELD_DATA_ANNUNZIO}", "")
                                        .Replace("{PARAM_DATA_ANNUNZIO}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_DATA_ANNUNZIO}", ", DataAnnunzio")
                                        .Replace("{PARAM_DATA_ANNUNZIO}", ", @DataAnnunzio");

                                if (string.IsNullOrEmpty(tipoChiusuraIter))
                                    query = query
                                        .Replace("{FIELD_TIPO_CHIUSURA_ITER}", "")
                                        .Replace("{PARAM_TIPO_CHIUSURA_ITER}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_TIPO_CHIUSURA_ITER}", ", TipoChiusuraIter")
                                        .Replace("{PARAM_TIPO_CHIUSURA_ITER}", ", @TipoChiusuraIter");

                                if (string.IsNullOrEmpty(dataChiusuraIter))
                                    query = query
                                        .Replace("{FIELD_DATA_CHIUSURA_ITER}", "")
                                        .Replace("{PARAM_DATA_CHIUSURA_ITER}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_DATA_CHIUSURA_ITER}", ", DataChiusuraIter")
                                        .Replace("{PARAM_DATA_CHIUSURA_ITER}", ", @DataChiusuraIter");

                                if (string.IsNullOrEmpty(tipoVotazione))
                                    query = query
                                        .Replace("{FIELD_TIPO_VOTAZIONE_ITER}", "")
                                        .Replace("{PARAM_TIPO_VOTAZIONE_ITER}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_TIPO_VOTAZIONE_ITER}", ", TipoVotazioneIter")
                                        .Replace("{PARAM_TIPO_VOTAZIONE_ITER}", ", @TipoVotazioneIter");

                                var command = new SqlCommand(query, connection);

                                // Assegna i valori dei parametri
                                command.Parameters.AddWithValue("@UIDAtto", attoImportato.UidAtto);
                                command.Parameters.AddWithValue("@Tipo", (int)tipoAttoEnum);
                                command.Parameters.AddWithValue("@TipoMOZ", tipoMozione);
                                command.Parameters.AddWithValue("@NAtto", etichettaAtto_Cifrata);
                                command.Parameters.AddWithValue("@Etichetta", etichettaAtto);
                                command.Parameters.AddWithValue("@NAtto_search", numeroAtto);
                                command.Parameters.AddWithValue("@Oggetto", oggetto);
                                command.Parameters.AddWithValue("@Premesse", premesse);
                                command.Parameters.AddWithValue("@IDTipo_Risposta", tipoRispostaRichiestaAttoInt);
                                command.Parameters.AddWithValue("@DataPresentazione", dataPresentazione_Cifrata);
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
                                command.Parameters.AddWithValue("@DCR", dcr);
                                command.Parameters.AddWithValue("@DCRC", dcrc);
                                command.Parameters.AddWithValue("@DCRL", dcrl);
                                command.Parameters.AddWithValue("@AreaTematica", areaTematica);
                                command.Parameters.AddWithValue("@AltriSoggetti", altriSoggetti);

                                if (string.IsNullOrEmpty(dataAnnunzio))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@DataAnnunzio", Convert.ToDateTime(dataAnnunzio));
                                }

                                if (string.IsNullOrEmpty(tipoChiusuraIter))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@TipoChiusuraIter",
                                        ParseDescr2Enum_Stato(tipoChiusuraIter));
                                }

                                if (string.IsNullOrEmpty(dataChiusuraIter))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@DataChiusuraIter",
                                        Convert.ToDateTime(dataChiusuraIter));
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

                                // Esegui la query di inserimento
                                var resQuery = command.ExecuteNonQuery();
                                if (resQuery == 1)
                                {
                                    Console.WriteLine($"[{row}/{rowCount}] {etichettaAtto}");
                                    attiImportati.Add(nodeIdFromAlfresco, attoImportato);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Errore durante l'elaborazione della riga. Dettagli dell'errore:");
                                sb.AppendLine(
                                    $"{foglio}, {row}, {legislaturaFromAlfresco}, {tipoAttoFromAlfresco}, {numeroAtto}, {e.Message}");
                            }
                        }

                        // Costruisci il nome del file usando il timestamp e il numero di riga
                        var fileName = $"dati_report.txt";

                        // Costruisci il percorso completo del file all'interno della cartella "errori"
                        var filePath = Path.Combine(errorFolderPath, fileName);

                        // Scrivi il messaggio dell'eccezione nel file
                        using (var sw = File.AppendText(filePath))
                        {
                            sw.WriteLine(sb.ToString());
                        }

                        Console.WriteLine($"Dettagli dell'errore salvati in {filePath}");

                        Console.WriteLine("Complete!");
                    }
                }

                using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                {
                    connection.Open();
                    if (attiImportati.Any())
                    {
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
                                    $@"INSERT INTO ATTI_ABBINAMENTI (Uid, Data, UIDAtto, UIDAttoAbbinato)
                                                    VALUES
                                                (@IdAbbinamento, GETDATE(), @UIDAtto, @UIDAttoAbbinato)";
                                var commandInsertAbbinamento =
                                    new SqlCommand(queryInsertAbbinamento, connection);
                                commandInsertAbbinamento.Parameters.AddWithValue("@IdAbbinamento", Guid.NewGuid());
                                commandInsertAbbinamento.Parameters.AddWithValue("@UIDAtto", attoPadre.UidAtto);
                                commandInsertAbbinamento.Parameters.AddWithValue("@UIDAttoAbbinato", attoAbbinato.UidAtto);
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
                                    $@"INSERT INTO ATTI_ABBINAMENTI (Uid, Data, UIDAtto, UIDAttoAbbinato)
                                                    VALUES
                                                (@IdAbbinamento, GETDATE(), @UIDAtto, @UIDAttoAbbinato)";
                                var commandInsertAbbinamentoGea =
                                    new SqlCommand(queryInsertAbbinamentoGea, connection);
                                commandInsertAbbinamentoGea.Parameters.AddWithValue("@IdAbbinamento", Guid.NewGuid());
                                commandInsertAbbinamentoGea.Parameters.AddWithValue("@UIDAtto", attoPadre.UidAtto);
                                commandInsertAbbinamentoGea.Parameters.AddWithValue("@UIDAttoAbbinato", abbinamentoGea.UIDAtto);
                                commandInsertAbbinamentoGea.ExecuteNonQuery();
                                count_inseriti++;
                            }
                            catch (Exception e)
                            {
                                count_errore++;
                                continue;
                            }
                        }

                        Console.WriteLine(
                            $"Risultato: {count_inseriti}, {count_nontrovati}, {count_errore}");
                    }
                }
            }
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

        private static int ParseNomeFirmatarioToGruppo(string input, int legislaturaId)
        {
            var pattern = @"\((.*?)\)"; // Cerca il testo all'interno delle parentesi tonde

            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                var codiceGruppoPolitico = match.Groups[1].Value.Trim();
                if (codiceGruppoPolitico.ToLower().Equals("u.d.c.")) codiceGruppoPolitico = "U.di.C.";

                var query = @"SELECT gp.id_gruppo
                FROM [gruppi_politici] gp
                INNER JOIN [join_gruppi_politici_legislature] jgp ON gp.id_gruppo = jgp.id_gruppo
                WHERE (gp.codice_gruppo = @CodiceGruppoPolitico OR gp.nome_gruppo = @CodiceGruppoPolitico) AND jgp.id_legislatura = @legislaturaId";

                using (var conn = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                {
                    conn.Open();

                    var command = new SqlCommand(query, conn);
                    command.Parameters.AddWithValue("@CodiceGruppoPolitico", codiceGruppoPolitico);
                    command.Parameters.AddWithValue("@legislaturaId", legislaturaId);

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var idGruppo = reader.GetInt32(0); // Leggi l'ID del gruppo politico
                        return idGruppo;
                    }

                    reader.Close();
                }
            }

            return 0;
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

        private static int ParseDescr2Enum_Stato(string statoDescr)
        {
            switch (statoDescr.ToLower())
            {
                case "comunicazione all'assemblea":
                {
                    return (int)StatiAttoEnum.CHIUSO;
                    //return (int)StatiAttoEnum.COMUNICAZIONE_ASSEMBLEA;
                }
                case "in trattazione":
                case "trattazione in assemblea":
                {
                    return (int)StatiAttoEnum.CHIUSO;
                    //return (int)StatiAttoEnum.TRATTAZIONE_IN_ASSEMBLEA;
                }
                case "decadenza per fine legislatura":
                {
                    return (int)StatiAttoEnum.CHIUSO;
                    //return (int)StatiAttoEnum.CHIUSO_DECADENZA_PER_FINE_LEGISLATURA;
                }
                case "ritiro":
                {
                    return (int)StatiAttoEnum.CHIUSO_RITIRATO;
                }
                case "presentato":
                case "depositato":
                {
                    return (int)StatiAttoEnum.PRESENTATO;
                }
                case "inammissibile":
                {
                    return (int)StatiAttoEnum.CHIUSO;
                    //return (int)StatiAttoEnum.CHIUSO_INAMMISSIBILE;
                }
                default:
                    return (int)StatiAttoEnum.CHIUSO;
            }
        }

        private static int ParseDescr2Enum_Area(string area)
        {
            switch (area.ToLower())
            {
                case "maggioranza":
                    return (int)AreaPoliticaIntEnum.Maggioranza;
                case "minoranza":
                    return (int)AreaPoliticaIntEnum.Minoranza;
                case "misto maggioranza/minoranza":
                    return (int)AreaPoliticaIntEnum.Misto;
                case "misto minoranza":
                    return (int)AreaPoliticaIntEnum.Misto_Minoranza;
                case "misto maggioranza":
                    return (int)AreaPoliticaIntEnum.Misto_Maggioranza;
                default:
                    return 0;
            }
        }

        private static LoginResponse AutenticazioneAPI()
        {
            BaseGateway.apiUrl = AppsettingsConfiguration.URL_API;
            var api = new ApiGateway();

            var task = Task.Run(async () => await api.Persone.Login(new LoginRequest
            {
                Username = AppsettingsConfiguration.USER_API,
                Password = AppsettingsConfiguration.PASSWORD_API
            }));

            return task.Result;
        }

        private static List<LegislaturaDto> GetLegislatureFromAPI(LoginResponse auth)
        {
            var api = new ApiGateway(auth.jwt);
            var task = Task.Run(async () => await api.Legislature.GetLegislature());

            return task.Result.ToList();
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
                case "Iter in assemblea + commissione":
                case "Iter in assemblea e commissione":
                case "risposta_iter_in_assemblea_e_commissione":
                    return (int)TipoRispostaEnum.ITER_IN_ASSEMBLEA_COMMISSIONE;
                case "Iter in assemblea":
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
                case nameof(TipoAttoEnum.RIS):
                    return TipoAttoEnum.RIS;
                default:
                    return TipoAttoEnum.ALTRO;
            }
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