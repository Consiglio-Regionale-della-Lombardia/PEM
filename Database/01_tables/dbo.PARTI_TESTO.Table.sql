USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[PARTI_TESTO]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PARTI_TESTO](
	[IDParte] [int] NOT NULL,
	[Parte] [nvarchar](20) NOT NULL,
	[Ordine] [int] NOT NULL,
	[Passo] [int] NULL,
 CONSTRAINT [PK_PARTI_TESTO] PRIMARY KEY CLUSTERED 
(
	[IDParte] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PARTI_TESTO] ADD  CONSTRAINT [DF_PARTI_TESTO_Ordine_1]  DEFAULT ((0)) FOR [Ordine]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id della tipologia di parte dell''atto emendabile' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PARTI_TESTO', @level2type=N'COLUMN',@level2name=N'IDParte'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Descrizione della parte di atto emendabile' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PARTI_TESTO', @level2type=N'COLUMN',@level2name=N'Parte'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ordine che verrà seguito dall''algoritmo di ordinamento della Griglia' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PARTI_TESTO', @level2type=N'COLUMN',@level2name=N'Ordine'
GO
