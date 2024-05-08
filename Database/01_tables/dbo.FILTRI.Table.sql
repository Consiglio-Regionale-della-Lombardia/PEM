USE [dbEmendamenti_test]
GO

/****** Object:  Table [dbo].[FILTRI]    Script Date: 07/05/2024 15:39:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FILTRI](
	[Id] [uniqueidentifier] NOT NULL,
	[UId_persona] [uniqueidentifier] NOT NULL,
	[DataCreazione] [datetime] NOT NULL,
	[Nome] [varchar](100) NOT NULL,
	[Filtri] [varchar](max) NULL,
	[Preferito] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

