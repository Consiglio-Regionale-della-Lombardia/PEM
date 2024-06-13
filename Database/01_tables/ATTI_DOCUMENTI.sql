USE [dbEmendamenti_test]
GO

/****** Object:  Table [dbo].[ATTI_DOCUMENTI]    Script Date: 11/06/2024 09:28:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_DOCUMENTI](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NULL,
	[Tipo] [int] NULL,
	[Data] [datetime] NULL,
	[Path] [varchar](max) NULL,
	[Titolo] [varchar](max) NULL,
	[Pubblica] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Uid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

