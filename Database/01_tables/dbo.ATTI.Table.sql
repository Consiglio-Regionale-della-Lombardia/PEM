USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[ATTI]    Script Date: 12/09/2020 18:38:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ATTI](
	[UIDAtto] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[NAtto] [varchar](50) NOT NULL,
	[IDTipoAtto] [int] NOT NULL,
	[Oggetto] [varchar](500) NULL,
	[Note] [varchar](255) NULL,
	[Path_Testo_Atto] [varchar](max) NULL,
	[UIDSeduta] [uniqueidentifier] NULL,
	[Data_apertura] [datetime] NULL,
	[Data_chiusura] [datetime] NULL,
	[VIS_Mis_Prog] [bit] NOT NULL,
	[UIDAssessoreRiferimento] [uniqueidentifier] NULL,
	[Notifica_deposito_differita] [bit] NOT NULL,
	[OrdinePresentazione] [bit] NULL,
	[OrdineVotazione] [bit] NULL,
	[Priorita] [int] NULL,
	[DataCreazione] [datetime] NULL,
	[UIDPersonaCreazione] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
	[UIDPersonaModifica] [uniqueidentifier] NULL,
	[Eliminato] [bit] NULL,
	[LinkFascicoloPresentazione] [varchar](max) NULL,
	[DataCreazionePresentazione] [datetime] NULL,
	[LinkFascicoloVotazione] [varchar](max) NULL,
	[DataCreazioneVotazione] [datetime] NULL,
	[DataUltimaModificaEM] [datetime] NULL,
 CONSTRAINT [PK_ATTI] PRIMARY KEY CLUSTERED 
(
	[UIDAtto] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[ATTI]  WITH NOCHECK ADD  CONSTRAINT [FK_ATTI_SEDUTE] FOREIGN KEY([UIDSeduta])
REFERENCES [dbo].[SEDUTE] ([UIDSeduta])
NOT FOR REPLICATION
GO
ALTER TABLE [dbo].[ATTI] CHECK CONSTRAINT [FK_ATTI_SEDUTE]
GO
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_UIDAtto]  DEFAULT (newsequentialid()) FOR [UIDAtto]
GO
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_IDTipoAtto]  DEFAULT ((1)) FOR [IDTipoAtto]
GO
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_VIS_Mis_Prog_1]  DEFAULT ((0)) FOR [VIS_Mis_Prog]
GO
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Notifica_deposito_differita]  DEFAULT ((1)) FOR [Notifica_deposito_differita]
GO
