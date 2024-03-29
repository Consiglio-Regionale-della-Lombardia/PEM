USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[ATTI_SOGGETTI_INTERROGATI]    Script Date: 21/06/2022 10:50:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_SOGGETTI_INTERROGATI](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[id_carica] [int] NOT NULL,
	[DataCreazione] [datetime] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_SOGGETTI_INTERROGATI] ADD  CONSTRAINT [DF_ATTI_SOGGETTI_INTERROGATI_DataCreazione]  DEFAULT (getdate()) FOR [DataCreazione]
GO

