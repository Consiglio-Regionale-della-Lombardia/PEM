/****** Object:  Table [dbo].[FILTRI]    Script Date: 16/09/2025 17:58:33 ******/
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
	[Preferito] [bit] NOT NULL,
	[Colonne] [varchar](max) NULL,
	[DettagliOrdinamento] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


