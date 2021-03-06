USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[RUOLI]    Script Date: 12/09/2020 18:38:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[RUOLI] ADD  CONSTRAINT [DF_RUOLI_Ruolo_di_Giunta]  DEFAULT ((0)) FOR [Ruolo_di_Giunta]
GO
