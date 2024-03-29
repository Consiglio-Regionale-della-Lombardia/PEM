USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[STATI_EM]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[STATI_EM](
	[IDStato] [int] NOT NULL,
	[Stato] [nvarchar](50) NOT NULL,
	[icona] [nvarchar](50) NULL,
	[CssClass] [nvarchar](15) NULL,
	[Ordinamento] [int] NOT NULL,
 CONSTRAINT [PK_STATI_EM] PRIMARY KEY CLUSTERED 
(
	[IDStato] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[STATI_EM] ADD  CONSTRAINT [DF_STATI_EM_Ordinamento]  DEFAULT ((0)) FOR [Ordinamento]
GO
