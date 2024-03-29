USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[cariche]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cariche](
	[id_carica] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[nome_carica] [varchar](250) NOT NULL,
	[ordine] [int] NOT NULL,
	[tipologia] [varchar](20) NOT NULL,
	[presidente_gruppo] [bit] NULL,
	[id_tipo_carica] [tinyint] NULL,
 CONSTRAINT [PK_cariche] PRIMARY KEY CLUSTERED 
(
	[id_carica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
