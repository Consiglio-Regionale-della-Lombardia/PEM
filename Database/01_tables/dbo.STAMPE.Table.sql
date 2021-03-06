USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[STAMPE]    Script Date: 12/09/2020 18:38:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[STAMPE](
	[UIDStampa] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
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
	[QueryEM] [varchar](max) NULL,
	[NotificaDepositoEM] [bit] NOT NULL,
	[UIDEM] [uniqueidentifier] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
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
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_NotificaDepositoEM_1]  DEFAULT ((0)) FOR [NotificaDepositoEM]
GO
