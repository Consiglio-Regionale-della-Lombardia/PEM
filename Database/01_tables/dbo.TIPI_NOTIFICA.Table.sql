USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[TIPI_NOTIFICA]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TIPI_NOTIFICA](
	[IDTipo] [int] NOT NULL,
	[Tipo] [varchar](100) NULL
) ON [PRIMARY]
GO
