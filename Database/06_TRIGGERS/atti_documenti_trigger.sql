IF OBJECT_ID('TrigATTI_DOCUMENTI') IS NOT NULL
    DROP TRIGGER [dbo].[TrigATTI_DOCUMENTI];
GO

CREATE TRIGGER [dbo].[TrigATTI_DOCUMENTI]
ON [dbo].[ATTI_DOCUMENTI]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDUtenteModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[ATTI_DOCUMENTI_Audit] (
        [DatAudit], [UteAudit],
        [Uid], [UIDAtto], [Tipo], [Data], [Path], [Titolo], [Pubblica],
        [UIDUtenteModifica], [DataModifica], [Eliminato]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[Uid], d.[UIDAtto], d.[Tipo], d.[Data], d.[Path], d.[Titolo], d.[Pubblica],
        d.[UIDUtenteModifica], d.[DataModifica], d.[Eliminato]
    FROM deleted d;
END
GO