USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[ARTICOLI]    Script Date: 12/09/2020 18:38:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ARTICOLI](
	[UIDArticolo] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[Articolo] [varchar](50) NOT NULL,
	[RubricaArticolo] [nvarchar](1000) NULL,
	[TestoArticolo] [varchar](max) NULL,
	[Ordine] [int] NOT NULL,
 CONSTRAINT [PK_Articoli] PRIMARY KEY CLUSTERED 
(
	[UIDArticolo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[ARTICOLI] ADD  CONSTRAINT [DF_Articoli_UIDArticolo]  DEFAULT (newsequentialid()) FOR [UIDArticolo]
GO
ALTER TABLE [dbo].[ARTICOLI] ADD  CONSTRAINT [DF_Articoli_Ordine]  DEFAULT ((1)) FOR [Ordine]
GO
