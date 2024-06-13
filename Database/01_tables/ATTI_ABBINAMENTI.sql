USE [dbEmendamenti_test]
GO

/****** Object:  Table [dbo].[ATTI_ABBINAMENTI]    Script Date: 11/06/2024 09:29:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_ABBINAMENTI](
	[Uid] [uniqueidentifier] NOT NULL,
	[Data] [datetime] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UIDAttoAbbinato] [uniqueidentifier] NULL,
	[OggettoAttoAbbinato] [varchar](max) NULL,
	[TipoAttoAbbinato] [varchar](20) NULL,
	[NumeroAttoAbbinato] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

