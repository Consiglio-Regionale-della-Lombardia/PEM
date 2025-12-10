/****** Object:  Table [dbo].[ATTI_DOCUMENTI]    Script Date: 30/10/2025 10:51:57 ******/
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
	[UIDUtenteModifica] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
	[Eliminato] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Uid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_DOCUMENTI] ADD  CONSTRAINT [DF_ATTI_DOCUMENTI_Eliminato]  DEFAULT ((0)) FOR [Eliminato]
GO


