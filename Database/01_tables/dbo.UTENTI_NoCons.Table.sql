/****** Object:  Table [dbo].[UTENTI_NoCons]    Script Date: 20/10/2025 12:19:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UTENTI_NoCons](
	[UID_persona] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[id_persona] [int] IDENTITY(10000,1) NOT FOR REPLICATION NOT NULL,
	[cognome] [varchar](50) NOT NULL,
	[nome] [varchar](50) NULL,
	[email] [varchar](250) NULL,
	[foto] [varchar](250) NULL,
	[UserAD] [varchar](50) NULL,
	[id_gruppo_politico_rif] [int] NULL,
	[notifica_firma] [bit] NOT NULL,
	[notifica_deposito] [bit] NOT NULL,
	[RichiediModificaPWD] [bit] NOT NULL,
	[Data_ultima_modifica_PWD] [datetime] NULL,
	[pass_locale_crypt] [varchar](max) NULL,
	[gruppi_autorizzazione] [varchar](max) NULL,
	[attivo] [bit] NOT NULL,
	[deleted] [bit] NOT NULL,
	[UIDUtenteModifica] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
 CONSTRAINT [PK_UTENTI_NoCons_1] PRIMARY KEY CLUSTERED 
(
	[UID_persona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_UID_persona]  DEFAULT (newsequentialid()) FOR [UID_persona]
GO

ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_notifica_firma]  DEFAULT ((0)) FOR [notifica_firma]
GO

ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_notifica_deposito]  DEFAULT ((0)) FOR [notifica_deposito]
GO

ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_RichiediModificaPWD]  DEFAULT ((1)) FOR [RichiediModificaPWD]
GO

ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_attivo]  DEFAULT ((1)) FOR [attivo]
GO

ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_deleted]  DEFAULT ((0)) FOR [deleted]
GO


