IF OBJECT_ID('TrigATTI') IS NOT NULL
    DROP TRIGGER [dbo].[TrigATTI];
GO

CREATE TRIGGER [dbo].[TrigATTI]
ON [dbo].[ATTI]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDPersonaModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[ATTI_Audit] (
        [DatAudit], [UteAudit],
        [UIDAtto], [NAtto], [IDTipoAtto], [Oggetto], [Note], [Path_Testo_Atto], [UIDSeduta],
        [Data_apertura], [Data_chiusura], [VIS_Mis_Prog], [UIDAssessoreRiferimento], [Notifica_deposito_differita],
        [OrdinePresentazione], [OrdineVotazione], [Priorita], [DataCreazione], [UIDPersonaCreazione],
        [DataModifica], [UIDPersonaModifica], [Eliminato], [LinkFascicoloPresentazione], [DataCreazionePresentazione],
        [LinkFascicoloVotazione], [DataCreazioneVotazione], [DataUltimaModificaEM], [BloccoEM], [BloccoODG],
        [Jolly], [Emendabile], [Fascicoli_Da_Aggiornare], [Legislatura], [Invio_Notifiche_Deposito_Solo_UOLA]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDAtto], d.[NAtto], d.[IDTipoAtto], d.[Oggetto], d.[Note], d.[Path_Testo_Atto], d.[UIDSeduta],
        d.[Data_apertura], d.[Data_chiusura], d.[VIS_Mis_Prog], d.[UIDAssessoreRiferimento], d.[Notifica_deposito_differita],
        d.[OrdinePresentazione], d.[OrdineVotazione], d.[Priorita], d.[DataCreazione], d.[UIDPersonaCreazione],
        d.[DataModifica], d.[UIDPersonaModifica], d.[Eliminato], d.[LinkFascicoloPresentazione], d.[DataCreazionePresentazione],
        d.[LinkFascicoloVotazione], d.[DataCreazioneVotazione], d.[DataUltimaModificaEM], d.[BloccoEM], d.[BloccoODG],
        d.[Jolly], d.[Emendabile], d.[Fascicoli_Da_Aggiornare], d.[Legislatura], d.[Invio_Notifiche_Deposito_Solo_UOLA]
    FROM deleted d;
END
GO