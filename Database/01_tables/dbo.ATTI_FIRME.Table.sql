USE [dbDASI]
GO

/****** Object:  Table [dbo].[ATTI_FIRME]    Script Date: 29/09/2022 02:49:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_FIRME](
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UID_persona] [uniqueidentifier] NOT NULL,
	[FirmaCert] [varchar](max) NOT NULL,
	[Data_firma] [varchar](255) NULL,
	[Data_ritirofirma] [varchar](255) NULL,
	[id_AreaPolitica] [int] NULL,
	[Timestamp] [datetime] NOT NULL,
	[ufficio] [bit] NOT NULL,
	[PrimoFirmatario] [bit] NOT NULL,
	[id_gruppo] [int] NOT NULL,
	[Valida] [bit] NOT NULL,
	[Capogruppo] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_ufficio]  DEFAULT ((0)) FOR [ufficio]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_PrimoFirmatario]  DEFAULT ((0)) FOR [PrimoFirmatario]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_id_gruppo]  DEFAULT ((0)) FOR [id_gruppo]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_Valida]  DEFAULT ((1)) FOR [Valida]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_Capogruppo]  DEFAULT ((0)) FOR [Capogruppo]
GO

