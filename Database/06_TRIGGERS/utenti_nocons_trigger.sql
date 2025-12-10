IF OBJECT_ID('TrigUTENTI_NoCons') IS NOT NULL
    DROP TRIGGER [dbo].[TrigUTENTI_NoCons];
GO

CREATE TRIGGER [dbo].[TrigUTENTI_NoCons]
ON [dbo].[UTENTI_NoCons]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDUtenteModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[UTENTI_NoCons_Audit] (
        [DatAudit], [UteAudit],
        [UID_persona], [id_persona], [cognome], [nome], [email], [foto], [UserAD],
        [id_gruppo_politico_rif], [notifica_firma], [notifica_deposito], [RichiediModificaPWD],
        [Data_ultima_modifica_PWD], [pass_locale_crypt], [gruppi_autorizzazione], [attivo], [deleted],
        [UIDUtenteModifica], [DataModifica]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UID_persona], d.[id_persona], d.[cognome], d.[nome], d.[email], d.[foto], d.[UserAD],
        d.[id_gruppo_politico_rif], d.[notifica_firma], d.[notifica_deposito], d.[RichiediModificaPWD],
        d.[Data_ultima_modifica_PWD], d.[pass_locale_crypt], d.[gruppi_autorizzazione], d.[attivo], d.[deleted],
        d.[UIDUtenteModifica], d.[DataModifica]
    FROM deleted d;
END
GO