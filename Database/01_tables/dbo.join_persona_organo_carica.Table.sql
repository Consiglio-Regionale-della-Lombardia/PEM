USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[join_persona_organo_carica]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[join_persona_organo_carica](
	[id_rec] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_organo] [int] NOT NULL,
	[id_persona] [int] NOT NULL,
	[id_legislatura] [int] NOT NULL,
	[id_carica] [int] NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[deleted] [bit] NOT NULL,
 CONSTRAINT [PK_join_persona_organo_carica_1] PRIMARY KEY CLUSTERED 
(
	[id_rec] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
