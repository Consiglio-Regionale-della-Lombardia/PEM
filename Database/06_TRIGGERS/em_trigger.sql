IF OBJECT_ID('TrigEM') IS NOT NULL
    DROP TRIGGER [dbo].[TrigEM];
GO

CREATE TRIGGER [dbo].[TrigEM]
ON [dbo].[EM]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDPersonaModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[EM_Audit] (
        [DatAudit], [UteAudit],
        [UIDEM], [Progressivo], [UIDAtto], [N_EM], [id_gruppo], [Rif_UIDEM], [N_SUBEM], [SubProgressivo],
        [UIDPersonaProponente], [UIDPersonaProponenteOLD], [DataCreazione], [UIDPersonaCreazione], [idRuoloCreazione],
        [DataModifica], [UIDPersonaModifica], [DataDeposito], [UIDPersonaPrimaFirma], [DataPrimaFirma],
        [UIDPersonaDeposito], [Proietta], [DataProietta], [UIDPersonaProietta], [DataRitiro], [UIDPersonaRitiro],
        [Hash], [IDTipo_EM], [IDParte], [NTitolo], [NCapo], [UIDArticolo], [UIDComma], [UIDLettera],
        [NLettera], [UIDParte_LR], [NNumero], [UIDMissione], [NMissione], [NProgramma], [NTitoloB],
        [OrdinePresentazione], [OrdineVotazione], [TestoEM_originale], [EM_Certificato], [TestoREL_originale],
        [PATH_AllegatoGenerico], [PATH_AllegatoTecnico], [EffettiFinanziari], [NOTE_EM], [NOTE_Griglia],
        [TestoEM_Modificabile], [IDStato], [Firma_su_invito], [UID_QRCode], [AreaPolitica], [Eliminato],
        [UIDPersonaElimina], [DataElimina], [chkf], [chkem], [Timestamp], [Colore], [Tags],
        [VersioneStampa], [DataUltimaStampa], [PathStampa], [StampaValida]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDEM], d.[Progressivo], d.[UIDAtto], d.[N_EM], d.[id_gruppo], d.[Rif_UIDEM], d.[N_SUBEM], d.[SubProgressivo],
        d.[UIDPersonaProponente], d.[UIDPersonaProponenteOLD], d.[DataCreazione], d.[UIDPersonaCreazione], d.[idRuoloCreazione],
        d.[DataModifica], d.[UIDPersonaModifica], d.[DataDeposito], d.[UIDPersonaPrimaFirma], d.[DataPrimaFirma],
        d.[UIDPersonaDeposito], d.[Proietta], d.[DataProietta], d.[UIDPersonaProietta], d.[DataRitiro], d.[UIDPersonaRitiro],
        d.[Hash], d.[IDTipo_EM], d.[IDParte], d.[NTitolo], d.[NCapo], d.[UIDArticolo], d.[UIDComma], d.[UIDLettera],
        d.[NLettera], d.[UIDParte_LR], d.[NNumero], d.[UIDMissione], d.[NMissione], d.[NProgramma], d.[NTitoloB],
        d.[OrdinePresentazione], d.[OrdineVotazione], d.[TestoEM_originale], d.[EM_Certificato], d.[TestoREL_originale],
        d.[PATH_AllegatoGenerico], d.[PATH_AllegatoTecnico], d.[EffettiFinanziari], d.[NOTE_EM], d.[NOTE_Griglia],
        d.[TestoEM_Modificabile], d.[IDStato], d.[Firma_su_invito], d.[UID_QRCode], d.[AreaPolitica], d.[Eliminato],
        d.[UIDPersonaElimina], d.[DataElimina], d.[chkf], d.[chkem], d.[Timestamp], d.[Colore], d.[Tags],
        d.[VersioneStampa], d.[DataUltimaStampa], d.[PathStampa], d.[StampaValida]
    FROM deleted d;
END
GO