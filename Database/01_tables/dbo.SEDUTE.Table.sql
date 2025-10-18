/****** Object:  Table [dbo].[SEDUTE]    Script Date: 18/10/2025 16:08:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SEDUTE](
	[UIDSeduta] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Data_seduta] [datetime] NOT NULL,
	[Data_apertura] [datetime] NULL,
	[Data_effettiva_inizio] [datetime] NULL,
	[Data_effettiva_fine] [datetime] NULL,
	[IDOrgano] [int] NULL,
	[Scadenza_presentazione] [datetime] NULL,
	[DataScadenzaPresentazioneIQT] [datetime] NULL,
	[DataScadenzaPresentazioneMOZ] [datetime] NULL,
	[DataScadenzaPresentazioneMOZA] [datetime] NULL,
	[DataScadenzaPresentazioneMOZU] [datetime] NULL,
	[DataScadenzaPresentazioneODG] [datetime] NULL,
	[id_legislatura] [int] NULL,
	[Intervalli] [varchar](max) NULL,
	[UIDPersonaCreazione] [uniqueidentifier] NULL,
	[DataCreazione] [datetime] NULL,
	[UIDPersonaModifica] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
	[Eliminato] [bit] NULL,
	[Riservato_DASI] [bit] NOT NULL,
	[Riservato_DASI_MOZ] [bit] NOT NULL,
	[Riservato_DASI_IQT] [bit] NOT NULL,
	[Blocco_MOZ_Abbinate] [bit] NOT NULL,
	[Note] [varchar](150) NULL,
 CONSTRAINT [PK_SEDUTE] PRIMARY KEY CLUSTERED 
(
	[UIDSeduta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_UIDSeduta]  DEFAULT (newsequentialid()) FOR [UIDSeduta]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_IDOrgano]  DEFAULT ((1)) FOR [IDOrgano]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_Riservato_DASI_1]  DEFAULT ((0)) FOR [Riservato_DASI]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_Riservato_DASI_MOZ]  DEFAULT ((0)) FOR [Riservato_DASI_MOZ]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_Riservato_DASI_IQT]  DEFAULT ((0)) FOR [Riservato_DASI_IQT]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_Blocco_MOZ_Abbinate]  DEFAULT ((0)) FOR [Blocco_MOZ_Abbinate]
GO


