/****** Object:  Table [dbo].[STAMPE]    Script Date: 09/01/2025 16:54:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[STAMPE](
	[UIDStampa] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NULL,
	[Da] [int] NOT NULL,
	[A] [int] NOT NULL,
	[UIDUtenteRichiesta] [uniqueidentifier] NOT NULL,
	[DataRichiesta] [datetime] NOT NULL,
	[Invio] [bit] NOT NULL,
	[DataInvio] [datetime] NULL,
	[MessaggioErrore] [varchar](max) NULL,
	[Lock] [bit] NOT NULL,
	[DataLock] [datetime] NULL,
	[PathFile] [varchar](max) NULL,
	[DataInizioEsecuzione] [datetime] NULL,
	[DataFineEsecuzione] [datetime] NULL,
	[Tentativi] [int] NOT NULL,
	[CurrentRole] [int] NULL,
	[Scadenza] [datetime] NULL,
	[Ordine] [int] NULL,
	[Query] [varchar](max) NULL,
	[Notifica] [bit] NOT NULL,
	[UIDEM] [uniqueidentifier] NULL,
	[DASI] [bit] NOT NULL,
	[UIDFascicolo] [uniqueidentifier] NULL,
	[NumeroFascicolo] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_UIDStampa]  DEFAULT (newid()) FOR [UIDStampa]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_DataRichiesta]  DEFAULT (getdate()) FOR [DataRichiesta]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_Invio]  DEFAULT ((0)) FOR [Invio]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_Lock]  DEFAULT ((0)) FOR [Lock]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_Tentativi]  DEFAULT ((0)) FOR [Tentativi]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_NotificaDepositoEM_1]  DEFAULT ((0)) FOR [Notifica]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_DASI]  DEFAULT ((0)) FOR [DASI]
GO

ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_NumeroFascicolo]  DEFAULT ((0)) FOR [NumeroFascicolo]
GO


