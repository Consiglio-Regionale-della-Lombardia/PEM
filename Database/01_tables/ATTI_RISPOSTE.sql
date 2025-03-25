/****** Object:  Table [dbo].[ATTI_RISPOSTE]    Script Date: 14/11/2024 11:38:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_RISPOSTE](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[TipoOrgano] [int] NOT NULL,
	[Tipo] [int] NOT NULL,
	[Data] [datetime] NULL,
	[DescrizioneOrgano] [nvarchar](255) NULL,
	[DataTrasmissione] [datetime] NULL,
	[DataTrattazione] [datetime] NULL,
	[IdOrgano] [int] NOT NULL,
	[UIDDocumento] [uniqueidentifier] NULL,
	[UIDRispostaAssociata] [uniqueidentifier] NULL,
	[DataRevoca] [datetime] NULL,
 CONSTRAINT [PK__ATTI_RIS__EBA6C886C13BAA3C] PRIMARY KEY CLUSTERED 
(
	[Uid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_RISPOSTE] ADD  CONSTRAINT [DF_ATTI_RISPOSTE_IdRisposta]  DEFAULT (newsequentialid()) FOR [Uid]
GO

