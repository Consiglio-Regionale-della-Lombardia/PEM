/****** Object:  Table [dbo].[ATTI]    Script Date: 23/09/2025 10:27:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI](
	[UIDAtto] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[NAtto] [varchar](255) NOT NULL,
	[IDTipoAtto] [int] NOT NULL,
	[Oggetto] [varchar](500) NULL,
	[Note] [varchar](max) NULL,
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
	[BloccoEM] [bit] NOT NULL,
	[BloccoODG] [bit] NOT NULL,
	[Jolly] [bit] NOT NULL,
	[Emendabile] [bit] NOT NULL,
	[Fascicoli_Da_Aggiornare] [bit] NOT NULL,
	[Legislatura] [int] NULL,
	[Invio_Notifiche_Deposito_Solo_UOLA] [bit] NOT NULL,
 CONSTRAINT [PK_ATTI] PRIMARY KEY CLUSTERED 
(
	[UIDAtto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_UIDAtto]  DEFAULT (newsequentialid()) FOR [UIDAtto]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_IDTipoAtto]  DEFAULT ((1)) FOR [IDTipoAtto]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_VIS_Mis_Prog_1]  DEFAULT ((0)) FOR [VIS_Mis_Prog]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Notifica_deposito_differita]  DEFAULT ((1)) FOR [Notifica_deposito_differita]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_BloccoEM]  DEFAULT ((0)) FOR [BloccoEM]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_BloccoODG_1]  DEFAULT ((0)) FOR [BloccoODG]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Jolly_1]  DEFAULT ((0)) FOR [Jolly]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Emendabile_1]  DEFAULT ((0)) FOR [Emendabile]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Fascicoli_Da_Aggiornare_1]  DEFAULT ((0)) FOR [Fascicoli_Da_Aggiornare]
GO

ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Invio_Notifiche_Deposito_Solo_UOLA]  DEFAULT ((0)) FOR [Invio_Notifiche_Deposito_Solo_UOLA]
GO

ALTER TABLE [dbo].[ATTI]  WITH NOCHECK ADD  CONSTRAINT [FK_ATTI_SEDUTE] FOREIGN KEY([UIDSeduta])
REFERENCES [dbo].[SEDUTE] ([UIDSeduta])
NOT FOR REPLICATION 
GO

ALTER TABLE [dbo].[ATTI] CHECK CONSTRAINT [FK_ATTI_SEDUTE]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ATTI', @level2type=N'COLUMN',@level2name=N'Jolly'
GO


