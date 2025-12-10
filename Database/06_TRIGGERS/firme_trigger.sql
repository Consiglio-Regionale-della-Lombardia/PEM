IF OBJECT_ID('TrigFIRME') IS NOT NULL
    DROP TRIGGER [dbo].[TrigFIRME];
GO

CREATE TRIGGER [dbo].[TrigFIRME]
ON [dbo].[FIRME]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Per le firme l'utente che modifica è sempre UID_persona (chi firma)
    SELECT TOP 1 @UteAudit = COALESCE(UID_persona, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[FIRME_Audit] (
        [DatAudit], [UteAudit],
        [UIDEM], [UID_persona], [FirmaCert], [Data_firma], [Data_ritirofirma],
        [id_AreaPolitica], [Timestamp], [ufficio]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDEM], d.[UID_persona], d.[FirmaCert], d.[Data_firma], d.[Data_ritirofirma],
        d.[id_AreaPolitica], d.[Timestamp], d.[ufficio]
    FROM deleted d;
END
GO