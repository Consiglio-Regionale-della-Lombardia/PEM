/****** Object:  Table [dbo].[Sessioni]    Script Date: 01/10/2024 19:04:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Sessioni](
	[uidSessione] [uniqueidentifier] NOT NULL,
	[uidUtente] [uniqueidentifier] NULL,
	[DataIngresso] [datetime] NULL,
	[DataUscita] [datetime] NULL,
	[ChiusuraCorretta] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[uidSessione] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

