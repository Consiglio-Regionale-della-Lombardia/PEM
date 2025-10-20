IF OBJECT_ID('TrigLETTERE') IS NOT NULL
    DROP TRIGGER [dbo].[TrigLETTERE];
GO

CREATE TRIGGER [dbo].[TrigLETTERE]
ON [dbo].[LETTERE]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDUtenteModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[LETTERE_Audit] (
        [DatAudit], [UteAudit],
        [UIDLettera], [UIDComma], [Lettera], [TestoLettera],
        [Ordine], [UIDUtenteModifica], [DataModifica], [Eliminato]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDLettera], d.[UIDComma], d.[Lettera], d.[TestoLettera],
        d.[Ordine], d.[UIDUtenteModifica], d.[DataModifica], d.[Eliminato]
    FROM deleted d;
END
GO