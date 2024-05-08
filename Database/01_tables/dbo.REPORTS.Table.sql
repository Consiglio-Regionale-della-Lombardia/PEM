USE [dbEmendamenti_test]
GO

/****** Object:  Table [dbo].[REPORTS]    Script Date: 07/05/2024 15:39:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[REPORTS](
	[Id] [uniqueidentifier] NOT NULL,
	[UId_persona] [uniqueidentifier] NOT NULL,
	[DataCreazione] [datetime] NOT NULL,
	[Nome] [varchar](100) NOT NULL,
	[Filtri] [varchar](max) NOT NULL,
	[TipoCopertina] [int] NOT NULL,
	[TipoVisualizzazione] [int] NOT NULL,
	[FormatoEsportazione] [int] NOT NULL,
	[Colonne] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[REPORTS] ADD  CONSTRAINT [DF_REPORTS_TipoCopertina]  DEFAULT ((0)) FOR [TipoCopertina]
GO

