USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[Tags]    Script Date: 25/08/2022 17:50:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Tags](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tag] [varchar](150) NOT NULL
) ON [PRIMARY]
GO

