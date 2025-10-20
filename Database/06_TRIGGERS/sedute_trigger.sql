IF OBJECT_ID('TrigSEDUTE') IS NOT NULL
    DROP TRIGGER [dbo].[TrigSEDUTE];
GO

CREATE TRIGGER [dbo].[TrigSEDUTE]
ON [dbo].[SEDUTE]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DatAudit DATETIME = GETDATE();
    DECLARE @UteAudit UNIQUEIDENTIFIER;

    -- Recupera l'utente che ha effettuato la modifica dalla tabella deleted
    SELECT TOP 1 @UteAudit = COALESCE(UIDPersonaModifica, '00000000-0000-0000-0000-000000000000') FROM deleted;

    -- Inserisce il record nella tabella di audit
    INSERT INTO [dbo].[SEDUTE_Audit] (
        [DatAudit], [UteAudit],
        [UIDSeduta], [Data_seduta], [Data_apertura], [Data_effettiva_inizio], [Data_effettiva_fine],
        [IDOrgano], [Scadenza_presentazione], [DataScadenzaPresentazioneIQT], [DataScadenzaPresentazioneMOZ],
        [DataScadenzaPresentazioneMOZA], [DataScadenzaPresentazioneMOZU], [DataScadenzaPresentazioneODG],
        [id_legislatura], [Intervalli], [UIDPersonaCreazione], [DataCreazione], [UIDPersonaModifica],
        [DataModifica], [Eliminato], [Riservato_DASI], [Riservato_DASI_MOZ], [Riservato_DASI_IQT],
        [Blocco_MOZ_Abbinate], [Note]
    )
    SELECT
        @DatAudit, @UteAudit,
        d.[UIDSeduta], d.[Data_seduta], d.[Data_apertura], d.[Data_effettiva_inizio], d.[Data_effettiva_fine],
        d.[IDOrgano], d.[Scadenza_presentazione], d.[DataScadenzaPresentazioneIQT], d.[DataScadenzaPresentazioneMOZ],
        d.[DataScadenzaPresentazioneMOZA], d.[DataScadenzaPresentazioneMOZU], d.[DataScadenzaPresentazioneODG],
        d.[id_legislatura], d.[Intervalli], d.[UIDPersonaCreazione], d.[DataCreazione], d.[UIDPersonaModifica],
        d.[DataModifica], d.[Eliminato], d.[Riservato_DASI], d.[Riservato_DASI_MOZ], d.[Riservato_DASI_IQT],
        d.[Blocco_MOZ_Abbinate], d.[Note]
    FROM deleted d;
END
GO