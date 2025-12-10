CREATE TABLE [dbo].[FIRME_Audit] (
    [IdFIRME_Audit] INT NOT NULL IDENTITY(1,1),
    [DatAudit] DATETIME NOT NULL,
    [UteAudit] UNIQUEIDENTIFIER NULL,
    [UIDEM] UNIQUEIDENTIFIER NOT NULL,
    [UID_persona] UNIQUEIDENTIFIER NOT NULL,
    [FirmaCert] VARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL,
    [Data_firma] VARCHAR(255) COLLATE Latin1_General_CI_AI NOT NULL,
    [Data_ritirofirma] VARCHAR(255) COLLATE Latin1_General_CI_AI NULL,
    [id_AreaPolitica] INT NULL,
    [Timestamp] DATETIME NOT NULL,
    [ufficio] BIT NOT NULL,
    CONSTRAINT [PK_FIRME_Audit] PRIMARY KEY ([IdFIRME_Audit])
);