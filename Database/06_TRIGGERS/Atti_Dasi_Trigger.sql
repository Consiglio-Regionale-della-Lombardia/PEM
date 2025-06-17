IF OBJECT_ID('TrigATTI_DASI') IS NOT NULL
    DROP TRIGGER [dbo].[TrigATTI_DASI];
GO

CREATE TRIGGER [dbo].[TrigATTI_DASI]
ON [dbo].[ATTI_DASI]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    SELECT TOP 1 @UteAudit = COALESCE(UIDPersonaModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    INSERT INTO [dbo].[ATTI_DASI_Audit] (
         [DatAudit], [UteAudit],
         [UIDAtto], [Tipo], [TipoMOZ], [Progressivo], [Etichetta], [NAtto], [NAtto_search],
         [Oggetto], [Oggetto_Modificato], [Oggetto_Approvato], [Premesse], [Premesse_Modificato],
         [TipoRichiesta], [TipoRichiestaDestinatario], [Richiesta], [Richiesta_Modificata],
         [DataCreazione], [UIDPersonaCreazione], [idRuoloCreazione], [DataModifica], [UIDPersonaModifica],
         [DataPresentazione], [DataPresentazione_MOZ], [DataPresentazione_MOZ_URGENTE], [DataPresentazione_MOZ_ABBINATA],
         [UIDPersonaPresentazione], [DataRichiestaIscrizioneSeduta], [UIDPersonaRichiestaIscrizione],
         [UIDPersonaProponente], [UIDPersonaPrimaFirma], [DataPrimaFirma], [Proietta], [DataProietta],
         [UIDPersonaProietta], [DataRitiro], [UIDPersonaRitiro], [Hash], [IDTipo_Risposta], [OrdineVisualizzazione],
         [PATH_AllegatoGenerico], [Note_Pubbliche], [Note_Private], [IDStato], [Firma_su_invito], [UID_QRCode],
         [AreaPolitica], [id_gruppo], [Eliminato], [UIDPersonaElimina], [DataElimina], [chkf], [Timestamp],
         [Atto_Certificato], [Legislatura], [UIDSeduta], [DataIscrizioneSeduta], [UIDPersonaIscrizioneSeduta],
         [UID_MOZ_Abbinata], [UID_Atto_ODG], [Non_Passaggio_In_Esame], [Inviato_Al_Protocollo], [DataInvioAlProtocollo],
         [CapogruppoNeiTermini], [MOZU_Capigruppo], [FirmeCartacee], [DataAnnunzio], [Protocollo], [CodiceMateria],
         [Pubblicato], [Sollecito], [IDTipo_Risposta_Effettiva], [TipoChiusuraIter], [DataChiusuraIter], [NoteChiusuraIter],
         [Emendato], [TipoVotazioneIter], [AreaTematica], [AltriSoggetti], [DCR], [DCCR], [DCRL], [BURL],
         [Privacy_Dati_Personali_Giudiziari], [Privacy_Divieto_Pubblicazione_Salute], [Privacy_Divieto_Pubblicazione_Vita_Sessuale],
         [Privacy_Divieto_Pubblicazione], [Privacy_Dati_Personali_Sensibili], [Privacy_Divieto_Pubblicazione_Altri],
         [Privacy_Dati_Personali_Semplici], [Privacy], [DataComunicazioneAssemblea], [ImpegniScadenze], [StatoAttuazione],
         [CompetenzaMonitoraggio], [MonitoraggioConcluso], [DataTrasmissioneMonitoraggio], [IterMultiplo], [UIDPersonaRelatore1],
         [UIDPersonaRelatore2], [UIDPersonaRelatoreMinoranza], [TipoChiusuraIterCommissione], [DataChiusuraIterCommissione],
         [TipoVotazioneIterCommissione], [RisultatoVotazioneIterCommissione], [DataSedutaRisposta], [DataComunicazioneAssembleaRisposta],
         [DataProposta], [DataTrasmissione], [FlussoRespingi], [UIDPersonaFlussoRespingi], [DataFlussoRespingi], [Ritardo]
    )
    SELECT
         @DatAudit, @UteAudit,
         d.[UIDAtto], d.[Tipo], d.[TipoMOZ], d.[Progressivo], d.[Etichetta], d.[NAtto], d.[NAtto_search],
         d.[Oggetto], d.[Oggetto_Modificato], d.[Oggetto_Approvato], d.[Premesse], d.[Premesse_Modificato],
         d.[TipoRichiesta], d.[TipoRichiestaDestinatario], d.[Richiesta], d.[Richiesta_Modificata],
         d.[DataCreazione], d.[UIDPersonaCreazione], d.[idRuoloCreazione], d.[DataModifica], d.[UIDPersonaModifica],
         d.[DataPresentazione], d.[DataPresentazione_MOZ], d.[DataPresentazione_MOZ_URGENTE], d.[DataPresentazione_MOZ_ABBINATA],
         d.[UIDPersonaPresentazione], d.[DataRichiestaIscrizioneSeduta], d.[UIDPersonaRichiestaIscrizione],
         d.[UIDPersonaProponente], d.[UIDPersonaPrimaFirma], d.[DataPrimaFirma], d.[Proietta], d.[DataProietta],
         d.[UIDPersonaProietta], d.[DataRitiro], d.[UIDPersonaRitiro], d.[Hash], d.[IDTipo_Risposta], d.[OrdineVisualizzazione],
         d.[PATH_AllegatoGenerico], d.[Note_Pubbliche], d.[Note_Private], d.[IDStato], d.[Firma_su_invito], d.[UID_QRCode],
         d.[AreaPolitica], d.[id_gruppo], d.[Eliminato], d.[UIDPersonaElimina], d.[DataElimina], d.[chkf], d.[Timestamp],
         d.[Atto_Certificato], d.[Legislatura], d.[UIDSeduta], d.[DataIscrizioneSeduta], d.[UIDPersonaIscrizioneSeduta],
         d.[UID_MOZ_Abbinata], d.[UID_Atto_ODG], d.[Non_Passaggio_In_Esame], d.[Inviato_Al_Protocollo], d.[DataInvioAlProtocollo],
         d.[CapogruppoNeiTermini], d.[MOZU_Capigruppo], d.[FirmeCartacee], d.[DataAnnunzio], d.[Protocollo], d.[CodiceMateria],
         d.[Pubblicato], d.[Sollecito], d.[IDTipo_Risposta_Effettiva], d.[TipoChiusuraIter], d.[DataChiusuraIter], d.[NoteChiusuraIter],
         d.[Emendato], d.[TipoVotazioneIter], d.[AreaTematica], d.[AltriSoggetti], d.[DCR], d.[DCCR], d.[DCRL], d.[BURL],
         d.[Privacy_Dati_Personali_Giudiziari], d.[Privacy_Divieto_Pubblicazione_Salute], d.[Privacy_Divieto_Pubblicazione_Vita_Sessuale],
         d.[Privacy_Divieto_Pubblicazione], d.[Privacy_Dati_Personali_Sensibili], d.[Privacy_Divieto_Pubblicazione_Altri],
         d.[Privacy_Dati_Personali_Semplici], d.[Privacy], d.[DataComunicazioneAssemblea], d.[ImpegniScadenze], d.[StatoAttuazione],
         d.[CompetenzaMonitoraggio], d.[MonitoraggioConcluso], d.[DataTrasmissioneMonitoraggio], d.[IterMultiplo], d.[UIDPersonaRelatore1],
         d.[UIDPersonaRelatore2], d.[UIDPersonaRelatoreMinoranza], d.[TipoChiusuraIterCommissione], d.[DataChiusuraIterCommissione],
         d.[TipoVotazioneIterCommissione], d.[RisultatoVotazioneIterCommissione], d.[DataSedutaRisposta], d.[DataComunicazioneAssembleaRisposta],
         d.[DataProposta], d.[DataTrasmissione], d.[FlussoRespingi], d.[UIDPersonaFlussoRespingi], d.[DataFlussoRespingi], d.[Ritardo]
    FROM deleted d;
END
GO
