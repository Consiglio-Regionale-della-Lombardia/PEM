CREATE TABLE [dbo].[ATTI_FIRME_Audit] (
    [IdATTI_FIRME_Audit] INT NOT NULL IDENTITY(1,1),
    [DatAudit] DATETIME NOT NULL,
    [UteAudit] UNIQUEIDENTIFIER NULL,
    [UIDAtto] UNIQUEIDENTIFIER NOT NULL,
    [UID_persona] UNIQUEIDENTIFIER NOT NULL,
    [FirmaCert] VARCHAR(MAX) COLLATE Latin1_General_CI_AI NOT NULL,
    [Data_firma] VARCHAR(255) COLLATE Latin1_General_CI_AI NULL,
    [Data_ritirofirma] VARCHAR(255) COLLATE Latin1_General_CI_AI NULL,
    [id_AreaPolitica] INT NULL,
    [Timestamp] DATETIME NOT NULL,
    [ufficio] BIT NOT NULL,
    [PrimoFirmatario] BIT NOT NULL,
    [id_gruppo] INT NOT NULL,
    [Valida] BIT NOT NULL,
    [Capogruppo] BIT NOT NULL,
    CONSTRAINT [PK_ATTI_FIRME_Audit] PRIMARY KEY ([IdATTI_FIRME_Audit])
);