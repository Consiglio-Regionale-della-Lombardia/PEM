USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[TIPI_EM]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TIPI_EM](
	[IDTipo_EM] [int] NOT NULL,
	[Tipo_EM] [nvarchar](25) NOT NULL,
	[Ordine] [int] NOT NULL,
 CONSTRAINT [PK_TIPI_EM] PRIMARY KEY CLUSTERED 
(
	[IDTipo_EM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id relativo alla tipologia di emendamento' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TIPI_EM', @level2type=N'COLUMN',@level2name=N'IDTipo_EM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tipo di emendamento (aggiuntivo, modificativo, ...)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TIPI_EM', @level2type=N'COLUMN',@level2name=N'Tipo_EM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ordine che verrà seguito dall''algoritmo di ordinamento della Griglia' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TIPI_EM', @level2type=N'COLUMN',@level2name=N'Ordine'
GO
