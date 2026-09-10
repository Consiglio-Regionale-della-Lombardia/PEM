CREATE TABLE [dbo].[REPORTS](
	[Id] [uniqueidentifier] NOT NULL,
	[UId_persona] [uniqueidentifier] NOT NULL,
	[DataCreazione] [datetime] NOT NULL,
	[Nome] [varchar](255) NOT NULL,
	[Filtri] [varchar](max) NOT NULL,
	[TipoCopertina] [varchar](50) NULL,
	[TipoVisualizzazione] [int] NOT NULL,
	[TipoVisualizzazione_Card_Template] [varchar](50) NULL,
	[FormatoEsportazione] [int] NOT NULL,
	[Colonne] [varchar](max) NOT NULL,
	[DettagliOrdinamento] [varchar](max) NULL,
	[FileSeparati] [bit] NOT NULL CONSTRAINT [DF_REPORTS_FileSeparati] DEFAULT ((0))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[REPORTS] ADD  CONSTRAINT [DF_REPORTS_TipoCopertina]  DEFAULT ((0)) FOR [TipoCopertina]
GO

