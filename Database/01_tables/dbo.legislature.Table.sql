USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[legislature]    Script Date: 12/09/2020 18:38:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[legislature](
	[id_legislatura] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[num_legislatura] [varchar](4) NOT NULL,
	[durata_legislatura_da] [datetime] NOT NULL,
	[durata_legislatura_a] [datetime] NULL,
	[attiva] [bit] NOT NULL,
	[id_causa_fine] [int] NULL,
 CONSTRAINT [PK_legislature] PRIMARY KEY CLUSTERED 
(
	[id_legislatura] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
