using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OfficeOpenXml;
using PortaleRegione.Crypto;
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;

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

            var auth = AutenticazioneAPI();

            var legislatureFromApi = GetLegislatureFromAPI(auth);

            using (var package = new ExcelPackage(new FileInfo(percorsoXLS)))
            {
                var worksheetFirme = package.Workbook.Worksheets.First(w => w.Name.Equals(foglioFirme));
                var cellsFirme = worksheetFirme.Cells;
                var rowCountF = worksheetFirme.Dimension.Rows;
                
                var worksheetRisposteAssociate = package.Workbook.Worksheets.First(w => w.Name.Equals(foglioRispostaAssociata));
                var cellsRisposteAssociate = worksheetRisposteAssociate.Cells;
                var rowCountRA = worksheetRisposteAssociate.Dimension.Rows;
                
                var worksheetRisposteGiunta = package.Workbook.Worksheets.First(w => w.Name.Equals(foglioRispostaGiunta));
                var cellsRisposteGiunta = worksheetRisposteGiunta.Cells;
                var rowCountRG = worksheetRisposteGiunta.Dimension.Rows;

                foreach (var foglio in foglioAtti)
                {
                    var elaborationTicks = DateTime.Now.Ticks;
                    // Costruisci il percorso della cartella "errore"
                    var errorFolderPath =
                        Path.Combine(Environment.CurrentDirectory, $"errore_{elaborationTicks}");

                    // Crea la cartella "errori" se non esiste già
                    Directory.CreateDirectory(errorFolderPath);

                    using (var connection = new SqlConnection(AppsettingsConfiguration.CONNECTIONSTRING))
                    {
                        connection.Open();
                        var worksheetAtti = package.Workbook.Worksheets.First(w => w.Name.Equals(foglio));
                        var cellsAtti = worksheetAtti.Cells;

                        var rowCount = worksheetAtti.Dimension.Rows;

                        for (var row = 2; row <= rowCount; row++)
                            try
                            {
                                var uidAtto = Guid.NewGuid();
                                var proponenteId = auth.id;
                                var chkf = 0;
                                var id_gruppo = 0;
                                // nodeid alfresco 
                                var nodeIdFromAlfresco = Convert.ToString(cellsAtti[row, 2].Value);

                                //legislatura
                                var legislaturaFromAlfresco = Convert.ToString(cellsAtti[row, 47].Value);
                                if (string.IsNullOrEmpty(legislaturaFromAlfresco))
                                    throw new Exception("Legislatura non valida");

                                var legislatura =
                                    legislatureFromApi.First(i =>
                                        i.id_legislatura.Equals(Convert.ToInt16(legislaturaFromAlfresco)));

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
                                        commandInsertFirmatario.Parameters.AddWithValue("@UIDAtto", uidAtto);
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

                                if (id_gruppo == 0)
                                {
                                }

                                #endregion

                                #region RISPOSTE ASSOCIATE

                                // Itera attraverso le righe del foglio delle firme
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
                                            var nodeIdRisposta = Convert.ToString(cellsRisposteAssociate[rowRA, 7].Value);
                                            for (var rowRG = 2; rowRG <= rowCountRG; rowRG++)
                                            {
                                                var valoreCellaRiferimento = cellsRisposteGiunta[rowRG, 2].Value;
                                                if (valoreCellaRiferimento != null && valoreCellaRiferimento.ToString() == nodeIdRisposta)
                                                {
                                                    idCommissioneRispostaAssociata = cellsRisposteGiunta[rowRG, 6].Value;
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

                                        var queryInsertRispostaAssociata = @"INSERT INTO ATTI_RISPOSTE (UIDAtto, Tipo, TipoOrgano, Data, DataTrasmissione, DataTrattazione, IdOrgano, DescrizioneOrgano)
                                         VALUES"
                                        + $"(@UIDAtto, @Tipo, @TipoOrgano, @Data, @DataTrasmissione, @DataTrattazione, @IdOrgano, {sub_query})";

                                        var commandRispostaAssociata = new SqlCommand(queryInsertRispostaAssociata, connection);
                                        commandRispostaAssociata.Parameters.AddWithValue("@UIDAtto", uidAtto);
                                        commandRispostaAssociata.Parameters.AddWithValue("@Tipo", ConvertToIntTipoRisposta(tipoRispostaAssociata.ToString().Replace("risposta_", "").Replace("_", " ")));
                                        commandRispostaAssociata.Parameters.AddWithValue("@TipoOrgano", tipoOrgano);
                                        commandRispostaAssociata.Parameters.Add("@Data", SqlDbType.DateTime).Value = (object)dataRispostaAssociata ?? DBNull.Value;
                                        commandRispostaAssociata.Parameters.Add("@DataTrasmissione", SqlDbType.DateTime).Value = (object)dataTrasmissioneRispostaAssociata ?? DBNull.Value;
                                        commandRispostaAssociata.Parameters.Add("@DataTrattazione", SqlDbType.DateTime).Value = (object)dataTrattazioneRispostaAssociata ?? DBNull.Value;
                                        commandRispostaAssociata.Parameters.AddWithValue("@IdOrgano", Convert.ToInt16(idCommissioneRispostaAssociata));

                                        commandRispostaAssociata.ExecuteNonQuery();
                                    }
                                }

                                #endregion

                                //tipo
                                var tipoAttoFromAlfresco = Convert.ToString(cellsAtti[row, 4].Value);
                                var tipoAttoEnum = ConvertToEnumTipoAtto(tipoAttoFromAlfresco);

                                //tipo mozione
                                var tipoMozioneAttoFromAlfresco = Convert.ToString(cellsAtti[row, 22].Value);
                                var tipoMozione = ParseDescr2Enum_TipoMozione(tipoMozioneAttoFromAlfresco);

                                //numero
                                var numeroAtto = Convert.ToString(cellsAtti[row, 19].Value);

                                //etichette
                                var etichettaAtto = $"{tipoAttoEnum}_{numeroAtto}_{legislatura.num_legislatura}";
                                var etichettaAtto_Cifrata =
                                    CryptoHelper.EncryptString(etichettaAtto, AppsettingsConfiguration.MASTER_KEY);

                                //oggetto
                                var oggetto = Convert.ToString(cellsAtti[row, 13].Value);

                                //oggetto presentato
                                var oggettoPresentato = Convert.ToString(cellsAtti[row, 21].Value);

                                //premesse
                                var premesse = "Atto importato da Alfresco";

                                //tipo risposta richiesta
                                var tipoRispostaRichiestaAttoFromAlfresco = Convert.ToString(cellsAtti[row, 20].Value);
                                var tipoRispostaRichiestaAttoInt =
                                    ConvertToIntTipoRisposta(tipoRispostaRichiestaAttoFromAlfresco);

                                //data presentazione
                                var dataPresentazioneFromAlfresco = Convert.ToString(cellsAtti[row, 16].Value);
                                if (string.IsNullOrEmpty(dataPresentazioneFromAlfresco))
                                    throw new Exception("Data presentazione non valida");
                                var dataPresentazione = Convert.ToDateTime(dataPresentazioneFromAlfresco);
                                var dataPresentazione_Cifrata = CryptoHelper.EncryptString(
                                    dataPresentazione.ToString("dd/MM/yyyy HH:mm:ss"),
                                    AppsettingsConfiguration.MASTER_KEY);

                                var area = Convert.ToString(cellsAtti[row, 23].Value);

                                var dataAnnunzio = Convert.ToString(cellsAtti[row, 17].Value);
                                var codiceMateria = Convert.ToString(cellsAtti[row, 18].Value);
                                var protocollo = Convert.ToString(cellsAtti[row, 14].Value);

                                var pubblicato = cellsAtti[row, 8].Value.Equals("1");
                                var sollecito = cellsAtti[row, 9].Value.Equals("1");

                                var stato = Convert.ToString(cellsAtti[row, 10].Value);

                                var tipoChiusuraIter = Convert.ToString(cellsAtti[row, 28].Value);
                                var dataChiusuraIter = Convert.ToString(cellsAtti[row, 30].Value);
                                var noteChiusuraIter = Convert.ToString(cellsAtti[row, 29].Value);

                                var emendatoFromAlfresco = Convert.ToString(cellsAtti[row, 32].Value);
                                var emendato = !string.IsNullOrEmpty(emendatoFromAlfresco)
                                    ? emendatoFromAlfresco.Equals("1")
                                    : false;

                                var tipoVotazione = Convert.ToString(cellsAtti[row, 33].Value);

                                var areaTematica = Convert.ToString(cellsAtti[row, 72].Value);
                                var altriSoggetti = Convert.ToString(cellsAtti[row, 71].Value);

                                var query = @"IF NOT EXISTS (SELECT 1 FROM [ATTI_DASI] WHERE Etichetta = @Etichetta)
                            BEGIN
                                INSERT INTO [ATTI_DASI] 
                                (UIDAtto, Tipo, TipoMOZ, NAtto, Etichetta, NAtto_search, Oggetto{FIELD_OGGETTO_PRESENTATO}, Premesse, IDTipo_Risposta, DataPresentazione, IDStato, Legislatura, 
                                UIDPersonaCreazione, UIDPersonaPresentazione, idRuoloCreazione, UIDPersonaProponente, UIDPersonaPrimaFirma, 
                                UID_QRCode, id_gruppo, chkf, Timestamp, DataCreazione, OrdineVisualizzazione, AreaPolitica, Pubblicato, Sollecito, Protocollo, CodiceMateria{FIELD_DATA_ANNUNZIO}
{FIELD_TIPO_CHIUSURA_ITER}{FIELD_DATA_CHIUSURA_ITER}{FIELD_NOTE_CHIUSURA_ITER}{FIELD_TIPO_VOTAZIONE_ITER}, Emendato, AreaTematica, AltriSoggetti) 
                                VALUES 
                                (@UIDAtto, @Tipo, @TipoMOZ, @NAtto, @Etichetta, @NAtto_search, @Oggetto{PARAM_OGGETTO_PRESENTATO}, @Premesse, @IDTipo_Risposta, @DataPresentazione, @IDStato, @Legislatura, 
                                @UIDPersonaCreazione, @UIDPersonaPresentazione, @idRuoloCreazione, @UIDPersonaProponente, @UIDPersonaPrimaFirma, 
                                @UID_QRCode, @id_gruppo, @chkf, @Timestamp, GETDATE(), @OrdineVisualizzazione, @AreaPolitica, @Pubblicato, @Sollecito, @Protocollo, @CodiceMateria{PARAM_DATA_ANNUNZIO}
{PARAM_TIPO_CHIUSURA_ITER}{PARAM_DATA_CHIUSURA_ITER}{PARAM_NOTE_CHIUSURA_ITER}{PARAM_TIPO_VOTAZIONE_ITER}, @Emendato, @AreaTematica, @AltriSoggetti)
                            END";

                                if (string.IsNullOrEmpty(oggettoPresentato) ||
                                    oggettoPresentato.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    query = query
                                        .Replace("{FIELD_OGGETTO_PRESENTATO}", "")
                                        .Replace("{PARAM_OGGETTO_PRESENTATO}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_OGGETTO_PRESENTATO}", ", Oggetto_Presentato")
                                        .Replace("{PARAM_OGGETTO_PRESENTATO}", ", @Oggetto_Presentato");

                                if (string.IsNullOrEmpty(dataAnnunzio)
                                    || dataAnnunzio.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    query = query
                                        .Replace("{FIELD_DATA_ANNUNZIO}", "")
                                        .Replace("{PARAM_DATA_ANNUNZIO}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_DATA_ANNUNZIO}", ", DataAnnunzio")
                                        .Replace("{PARAM_DATA_ANNUNZIO}", ", @DataAnnunzio");

                                if (string.IsNullOrEmpty(tipoChiusuraIter)
                                    || tipoChiusuraIter.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    query = query
                                        .Replace("{FIELD_TIPO_CHIUSURA_ITER}", "")
                                        .Replace("{PARAM_TIPO_CHIUSURA_ITER}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_TIPO_CHIUSURA_ITER}", ", TipoChiusuraIter")
                                        .Replace("{PARAM_TIPO_CHIUSURA_ITER}", ", @TipoChiusuraIter");

                                if (string.IsNullOrEmpty(dataChiusuraIter)
                                    || dataChiusuraIter.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    query = query
                                        .Replace("{FIELD_DATA_CHIUSURA_ITER}", "")
                                        .Replace("{PARAM_DATA_CHIUSURA_ITER}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_DATA_CHIUSURA_ITER}", ", DataChiusuraIter")
                                        .Replace("{PARAM_DATA_CHIUSURA_ITER}", ", @DataChiusuraIter");

                                if (string.IsNullOrEmpty(noteChiusuraIter)
                                    || noteChiusuraIter.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    query = query
                                        .Replace("{FIELD_NOTE_CHIUSURA_ITER}", "")
                                        .Replace("{PARAM_NOTE_CHIUSURA_ITER}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_NOTE_CHIUSURA_ITER}", ", NoteChiusuraIter")
                                        .Replace("{PARAM_NOTE_CHIUSURA_ITER}", ", @NoteChiusuraIter");

                                if (string.IsNullOrEmpty(tipoVotazione)
                                    || tipoVotazione.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    query = query
                                        .Replace("{FIELD_TIPO_VOTAZIONE_ITER}", "")
                                        .Replace("{PARAM_TIPO_VOTAZIONE_ITER}", "");
                                else
                                    query = query
                                        .Replace("{FIELD_TIPO_VOTAZIONE_ITER}", ", TipoVotazioneIter")
                                        .Replace("{PARAM_TIPO_VOTAZIONE_ITER}", ", @TipoVotazioneIter");

                                var command = new SqlCommand(query, connection);

                                // Assegna i valori dei parametri
                                command.Parameters.AddWithValue("@UIDAtto", uidAtto);
                                command.Parameters.AddWithValue("@Tipo", (int)tipoAttoEnum);
                                command.Parameters.AddWithValue("@TipoMOZ", tipoMozione);
                                command.Parameters.AddWithValue("@NAtto", etichettaAtto_Cifrata);
                                command.Parameters.AddWithValue("@Etichetta", etichettaAtto);
                                command.Parameters.AddWithValue("@NAtto_search", numeroAtto);
                                command.Parameters.AddWithValue("@Oggetto", oggetto);
                                command.Parameters.AddWithValue("@Premesse", premesse);
                                command.Parameters.AddWithValue("@IDTipo_Risposta", tipoRispostaRichiestaAttoInt);
                                command.Parameters.AddWithValue("@DataPresentazione", dataPresentazione_Cifrata);
                                command.Parameters.AddWithValue("@IDStato", ParseDescr2Enum_Stato(stato));
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
                                command.Parameters.AddWithValue("@AreaTematica", areaTematica);
                                command.Parameters.AddWithValue("@AltriSoggetti", altriSoggetti);

                                if (string.IsNullOrEmpty(oggettoPresentato) ||
                                    oggettoPresentato.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@Oggetto_Presentato", oggettoPresentato);
                                }

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

                                if (string.IsNullOrEmpty(dataChiusuraIter) ||
                                    dataChiusuraIter.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@DataChiusuraIter",
                                        Convert.ToDateTime(dataChiusuraIter));
                                }

                                if (string.IsNullOrEmpty(noteChiusuraIter) ||
                                    noteChiusuraIter.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@NoteChiusuraIter", noteChiusuraIter);
                                }

                                if (string.IsNullOrEmpty(tipoVotazione)
                                    || tipoVotazione.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Ignored
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@TipoVotazioneIter",
                                        ParseDescr2Enum_TipoVotazioneIter(tipoVotazione));
                                }

                                // Esegui la query di inserimento
                                command.ExecuteNonQuery();

                                Console.WriteLine($"[{row}/{rowCount}] {etichettaAtto}");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Errore durante l'elaborazione della riga. Dettagli dell'errore:");

                                // Costruisci il nome del file usando il timestamp e il numero di riga
                                var fileName = $"{foglio}_{row}.txt";

                                // Costruisci il percorso completo del file all'interno della cartella "errori"
                                var filePath = Path.Combine(errorFolderPath, fileName);

                                // Scrivi il messaggio dell'eccezione nel file
                                File.WriteAllText(filePath, e.ToString());

                                Console.WriteLine($"Dettagli dell'errore salvati in {filePath}");
                            }

                        Console.WriteLine("Complete!");
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

        private static List<Tipi_AttoDto> GetTipiAttoFromAPI(LoginResponse auth)
        {
            var api = new ApiGateway(auth.jwt);

            var task = Task.Run(async () => await api.Atti.GetTipi());

            return task.Result.ToList();
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
                    return (int)TipoRispostaEnum.SCRITTA;
                case "orale":
                    return (int)TipoRispostaEnum.ORALE;
                case "in commissione":
                    return (int)TipoRispostaEnum.COMMISSIONE;
                case "Iter in assemblea + commissione":
                case "Iter in assemblea e commissione":
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
                default:
                    throw new ArgumentException("Tipo atto non riconosciuto");
            }
        }
    }
}