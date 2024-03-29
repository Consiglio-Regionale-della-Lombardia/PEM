USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[tbl_recapiti]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_recapiti](
	[id_recapito] [char](2) NOT NULL,
	[nome_recapito] [varchar](50) NOT NULL,
 CONSTRAINT [PK_tbl_recapiti] PRIMARY KEY CLUSTERED 
(
	[id_recapito] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
