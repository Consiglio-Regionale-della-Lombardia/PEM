IF OBJECT_ID('TrigATTI_FIRME') IS NOT NULL
    DROP TRIGGER [dbo].[TrigATTI_FIRME];
GO

CREATE TRIGGER [dbo].[TrigATTI_FIRME]
ON [dbo].[ATTI_FIRME]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Per le firme l'utente che modifica è sempre UID_persona (chi firma)
    SELECT TOP 1 @UteAudit = COALESCE(UID_persona, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[ATTI_FIRME_Audit] (
        [DatAudit], [UteAudit],
        [UIDAtto], [UID_persona], [FirmaCert], [Data_firma], [Data_ritirofirma],
        [id_AreaPolitica], [Timestamp], [ufficio], [PrimoFirmatario], [id_gruppo],
        [Valida], [Capogruppo]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDAtto], d.[UID_persona], d.[FirmaCert], d.[Data_firma], d.[Data_ritirofirma],
        d.[id_AreaPolitica], d.[Timestamp], d.[ufficio], d.[PrimoFirmatario], d.[id_gruppo],
        d.[Valida], d.[Capogruppo]
    FROM deleted d;
END
GO