USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[persona]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[persona](
	[id_persona] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[cognome] [varchar](50) NOT NULL,
	[nome] [varchar](50) NOT NULL,
	[data_nascita] [datetime] NULL,
	[foto] [varchar](255) NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_persona] PRIMARY KEY CLUSTERED 
(
	[id_persona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
