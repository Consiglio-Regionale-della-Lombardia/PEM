IF OBJECT_ID('TrigCOMMI') IS NOT NULL
    DROP TRIGGER [dbo].[TrigCOMMI];
GO

CREATE TRIGGER [dbo].[TrigCOMMI]
ON [dbo].[COMMI]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDUtenteModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[COMMI_Audit] (
        [DatAudit], [UteAudit],
        [UIDComma], [UIDAtto], [UIDArticolo], [Comma], [TestoComma],
        [Ordine], [UIDUtenteModifica], [DataModifica], [Eliminato]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDComma], d.[UIDAtto], d.[UIDArticolo], d.[Comma], d.[TestoComma],
        d.[Ordine], d.[UIDUtenteModifica], d.[DataModifica], d.[Eliminato]
    FROM deleted d;
END
GO