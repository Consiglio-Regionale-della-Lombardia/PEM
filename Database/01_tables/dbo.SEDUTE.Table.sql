USE [dbEmendamenti_beta]
GO

/****** Object:  Table [dbo].[SEDUTE]    Script Date: 29/09/2022 01:34:23 ******/
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
 CONSTRAINT [PK_SEDUTE] PRIMARY KEY CLUSTERED 
(
	[UIDSeduta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_UIDSeduta]  DEFAULT (newsequentialid()) FOR [UIDSeduta]
GO

ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_IDOrgano]  DEFAULT ((1)) FOR [IDOrgano]
GO

