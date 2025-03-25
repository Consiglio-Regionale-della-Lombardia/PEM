/****** Object:  Table [dbo].[ATTI_MONITORAGGIO]    Script Date: 11/06/2024 09:28:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_MONITORAGGIO](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NULL,
	[TipoOrgano] [int] NULL,
	[DataCreazione] [datetime] NULL,
	[DescrizioneOrgano] [varchar](max) NULL,
	[IdOrgano] [int] NULL,
	[UIDDocumento] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Uid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

