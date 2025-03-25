/****** Object:  Table [dbo].[ATTI_PROPONENTI]    Script Date: 07/11/2024 13:51:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_PROPONENTI](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UidPersona] [uniqueidentifier] NULL,
	[IdOrgano] [int] NULL,
	[DescrizioneOrgano] [varchar](255) NULL
) ON [PRIMARY]
GO


