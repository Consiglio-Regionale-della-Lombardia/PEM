USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[ATTI_FIRME]    Script Date: 21/06/2022 10:50:05 ******/
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
	[id_gruppo] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_ufficio]  DEFAULT ((0)) FOR [ufficio]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_PrimoFirmatario]  DEFAULT ((0)) FOR [PrimoFirmatario]
GO

ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_id_gruppo]  DEFAULT ((0)) FOR [id_gruppo]
GO
