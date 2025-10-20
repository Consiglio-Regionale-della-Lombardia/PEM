IF OBJECT_ID('TrigARTICOLI') IS NOT NULL
    DROP TRIGGER [dbo].[TrigARTICOLI];
GO

CREATE TRIGGER [dbo].[TrigARTICOLI]
ON [dbo].[ARTICOLI]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDUtenteModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[ARTICOLI_Audit] (
        [DatAudit], [UteAudit],
        [UIDArticolo], [UIDAtto], [Articolo], [RubricaArticolo], [TestoArticolo],
        [Ordine], [UIDUtenteModifica], [DataModifica], [Eliminato]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDArticolo], d.[UIDAtto], d.[Articolo], d.[RubricaArticolo], d.[TestoArticolo],
        d.[Ordine], d.[UIDUtenteModifica], d.[DataModifica], d.[Eliminato]
    FROM deleted d;
END
GO