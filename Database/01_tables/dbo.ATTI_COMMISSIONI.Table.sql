USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[ATTI_COMMISSIONI]    Script Date: 21/06/2022 10:49:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_COMMISSIONI](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[id_organo] [int] NOT NULL,
	[DataCreazione] [datetime] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_COMMISSIONI] ADD  CONSTRAINT [DF_ATTI_COMMISSIONI_DataCreazione]  DEFAULT (getdate()) FOR [DataCreazione]
GO

