USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[STAMPE_INFO]    Script Date: 14/05/2021 11:51:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[STAMPE_INFO](
	[Id] [uniqueidentifier] NOT NULL,
	[UIDStampa] [uniqueidentifier] NOT NULL,
	[Message] [varchar](255) NULL,
	[Date] [datetime] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[STAMPE_INFO] ADD  CONSTRAINT [DF_STAMPE_INFO_Date]  DEFAULT (getdate()) FOR [Date]
GO

