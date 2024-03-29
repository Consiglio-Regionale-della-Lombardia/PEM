USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[RUOLI]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RUOLI](
	[IDruolo] [int] IDENTITY(1,1) NOT NULL,
	[Ruolo] [varchar](50) NOT NULL,
	[Priorita] [int] NULL,
	[ADGroup] [varchar](100) NULL,
	[Ruolo_di_Giunta] [bit] NOT NULL,
 CONSTRAINT [PK_tbl_ruoli] PRIMARY KEY CLUSTERED 
(
	[IDruolo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[RUOLI] ADD  CONSTRAINT [DF_RUOLI_Ruolo_di_Giunta]  DEFAULT ((0)) FOR [Ruolo_di_Giunta]
GO
