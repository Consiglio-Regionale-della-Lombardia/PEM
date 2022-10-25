USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[NOTIFICHE]    Script Date: 18/07/2022 18:55:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[NOTIFICHE](
	[UIDNotifica] [bigint] IDENTITY(1,1) NOT NULL,
	[DataCreazione] [datetime] NOT NULL,
	[UIDEM] [uniqueidentifier] NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[IDTipo] [int] NOT NULL,
	[Mittente] [uniqueidentifier] NOT NULL,
	[RuoloMittente] [int] NULL,
	[Messaggio] [varchar](max) NULL,
	[DataScadenza] [datetime] NULL,
	[SyncGUID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[IdGruppo] [int] NOT NULL,
	[Chiuso] [bit] NULL,
	[DataChiusura] [datetime] NULL,
	[BLOCCO_INVITI] [uniqueidentifier] NULL,
	[Valida] [bit] NOT NULL,
 CONSTRAINT [PK_NOTIFICHE] PRIMARY KEY CLUSTERED 
(
	[SyncGUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_DataCreazione]  DEFAULT (getdate()) FOR [DataCreazione]
GO

ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_SyncGUID]  DEFAULT (newsequentialid()) FOR [SyncGUID]
GO

ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_Valida]  DEFAULT ((1)) FOR [Valida]
GO

