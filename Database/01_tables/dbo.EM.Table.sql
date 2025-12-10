/****** Object:  Table [dbo].[EM]    Script Date: 30/10/2025 10:54:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EM](
	[UIDEM] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Progressivo] [int] NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[N_EM] [varchar](50) NULL,
	[id_gruppo] [int] NOT NULL,
	[Rif_UIDEM] [uniqueidentifier] NULL,
	[N_SUBEM] [varchar](50) NULL,
	[SubProgressivo] [int] NULL,
	[UIDPersonaProponente] [uniqueidentifier] NULL,
	[UIDPersonaProponenteOLD] [uniqueidentifier] NULL,
	[DataCreazione] [datetime] NULL,
	[UIDPersonaCreazione] [uniqueidentifier] NULL,
	[idRuoloCreazione] [int] NULL,
	[DataModifica] [datetime] NULL,
	[UIDPersonaModifica] [uniqueidentifier] NULL,
	[DataDeposito] [varchar](255) NULL,
	[UIDPersonaPrimaFirma] [uniqueidentifier] NULL,
	[DataPrimaFirma] [datetime] NULL,
	[UIDPersonaDeposito] [uniqueidentifier] NULL,
	[Proietta] [bit] NOT NULL,
	[DataProietta] [datetime] NULL,
	[UIDPersonaProietta] [uniqueidentifier] NULL,
	[DataRitiro] [datetime] NULL,
	[UIDPersonaRitiro] [uniqueidentifier] NULL,
	[Hash] [varchar](max) NULL,
	[IDTipo_EM] [int] NOT NULL,
	[IDParte] [int] NOT NULL,
	[NTitolo] [varchar](5) NULL,
	[NCapo] [varchar](5) NULL,
	[UIDArticolo] [uniqueidentifier] NULL,
	[UIDComma] [uniqueidentifier] NULL,
	[UIDLettera] [uniqueidentifier] NULL,
	[NLettera] [varchar](5) NULL,
	[UIDParte_LR] [uniqueidentifier] NULL,
	[NNumero] [varchar](5) NULL,
	[UIDMissione] [uniqueidentifier] NULL,
	[NMissione] [int] NULL,
	[NProgramma] [int] NULL,
	[NTitoloB] [int] NULL,
	[OrdinePresentazione] [int] NOT NULL,
	[OrdineVotazione] [int] NOT NULL,
	[TestoEM_originale] [varchar](max) NULL,
	[EM_Certificato] [varchar](max) NULL,
	[TestoREL_originale] [varchar](max) NULL,
	[PATH_AllegatoGenerico] [varchar](max) NULL,
	[PATH_AllegatoTecnico] [varchar](max) NULL,
	[EffettiFinanziari] [int] NULL,
	[NOTE_EM] [varchar](max) NULL,
	[NOTE_Griglia] [varchar](max) NULL,
	[TestoEM_Modificabile] [varchar](max) NULL,
	[IDStato] [int] NULL,
	[Firma_su_invito] [bit] NOT NULL,
	[UID_QRCode] [uniqueidentifier] NOT NULL,
	[AreaPolitica] [int] NULL,
	[Eliminato] [bit] NOT NULL,
	[UIDPersonaElimina] [uniqueidentifier] NULL,
	[DataElimina] [datetime] NULL,
	[chkf] [varchar](255) NULL,
	[chkem] [int] NULL,
	[Timestamp] [datetime] NULL,
	[Colore] [varchar](50) NULL,
	[Tags] [varchar](max) NULL,
	[SubEM]  AS (case when [N_SUBEM] IS NULL then (0) else (1) end),
	[VersioneStampa] [int] NOT NULL,
	[DataUltimaStampa] [datetime] NULL,
	[PathStampa] [varchar](max) NULL,
	[StampaValida] [bit] NOT NULL,
 CONSTRAINT [PK_EM] PRIMARY KEY CLUSTERED 
(
	[UIDEM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_UIDEM]  DEFAULT (newsequentialid()) FOR [UIDEM]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_Proietta]  DEFAULT ((0)) FOR [Proietta]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_OrdinePresentazione]  DEFAULT ((1)) FOR [OrdinePresentazione]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_OrdineTrattazione]  DEFAULT ((1)) FOR [OrdineVotazione]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_Firma_su_invito]  DEFAULT ((0)) FOR [Firma_su_invito]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_UID_QRCode]  DEFAULT (newid()) FOR [UID_QRCode]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_Eliminato]  DEFAULT ((0)) FOR [Eliminato]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_VersioneStampa]  DEFAULT ((0)) FOR [VersioneStampa]
GO

ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_StampaValida]  DEFAULT ((0)) FOR [StampaValida]
GO


