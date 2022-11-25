USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[gruppi_politici]    Script Date: 27/08/2022 10:42:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[gruppi_politici](
	[id_gruppo] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[codice_gruppo] [varchar](50) NOT NULL,
	[nome_gruppo] [varchar](255) NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[attivo] [bit] NOT NULL,
	[id_causa_fine] [int] NULL,
	[deleted] [bit] NOT NULL,
 CONSTRAINT [PK_gruppi_politici] PRIMARY KEY CLUSTERED 
(
	[id_gruppo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[gruppi_politici] ADD  CONSTRAINT [DF_gruppi_politici_TipoArea]  DEFAULT ((0)) FOR [TipoArea]
GO

