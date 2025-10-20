IF OBJECT_ID('TrigATTI_RISPOSTE') IS NOT NULL
    DROP TRIGGER [dbo].[TrigATTI_RISPOSTE];
GO

CREATE TRIGGER [dbo].[TrigATTI_RISPOSTE]
ON [dbo].[ATTI_RISPOSTE]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDUtenteModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[ATTI_RISPOSTE_Audit] (
        [DatAudit], [UteAudit],
        [Uid], [UIDAtto], [TipoOrgano], [Tipo], [Data], [DescrizioneOrgano],
        [DataTrasmissione], [DataTrattazione], [IdOrgano], [UIDDocumento],
        [UIDRispostaAssociata], [DataRevoca], [UIDUtenteModifica], [DataModifica], [Eliminato]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[Uid], d.[UIDAtto], d.[TipoOrgano], d.[Tipo], d.[Data], d.[DescrizioneOrgano],
        d.[DataTrasmissione], d.[DataTrattazione], d.[IdOrgano], d.[UIDDocumento],
        d.[UIDRispostaAssociata], d.[DataRevoca], d.[UIDUtenteModifica], d.[DataModifica], d.[Eliminato]
    FROM deleted d;
END
GO