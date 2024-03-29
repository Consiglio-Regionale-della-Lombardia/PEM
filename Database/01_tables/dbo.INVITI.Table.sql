USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[INVITI]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[INVITI](
	[UID_Invitante] [uniqueidentifier] NOT NULL,
	[UID_Invitato] [uniqueidentifier] NOT NULL,
	[UID_EM] [uniqueidentifier] NOT NULL,
	[Data_Invito] [datetime] NULL,
	[Data_Visto] [datetime] NULL,
 CONSTRAINT [PK_INVITI] PRIMARY KEY CLUSTERED 
(
	[UID_Invitante] ASC,
	[UID_Invitato] ASC,
	[UID_EM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
