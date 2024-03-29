USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[organi]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[organi](
	[id_organo] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_legislatura] [int] NOT NULL,
	[nome_organo] [varchar](255) NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[deleted] [bit] NOT NULL,
	[ordinamento] [int] NULL,
	[id_tipo_organo] [int] NULL,
	[nome_organo_breve] [varchar](30) NULL,
	[id_categoria_organo] [int] NULL,
 CONSTRAINT [PK_organi] PRIMARY KEY CLUSTERED 
(
	[id_organo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
