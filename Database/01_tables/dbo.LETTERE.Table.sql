/****** Object:  Table [dbo].[LETTERE]    Script Date: 10/12/2025 11:09:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LETTERE](
	[UIDLettera] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDComma] [uniqueidentifier] NULL,
	[Lettera] [varchar](50) NULL,
	[TestoLettera] [varchar](max) NULL,
	[Ordine] [int] NULL,
	[UIDUtenteModifica] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
	[Eliminato] [bit] NOT NULL,
 CONSTRAINT [PK_LETTERE] PRIMARY KEY CLUSTERED 
(
	[UIDLettera] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[LETTERE] ADD  CONSTRAINT [DF_LETTERE_UIDLettera]  DEFAULT (newsequentialid()) FOR [UIDLettera]
GO

ALTER TABLE [dbo].[LETTERE] ADD  CONSTRAINT [DF_LETTERE_Ordine]  DEFAULT ((0)) FOR [Ordine]
GO

ALTER TABLE [dbo].[LETTERE] ADD  CONSTRAINT [DF_LETTERE_Eliminato]  DEFAULT ((0)) FOR [Eliminato]
GO


