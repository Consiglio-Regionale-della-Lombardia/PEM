USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[FIRME]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FIRME](
	[UIDEM] [uniqueidentifier] NOT NULL,
	[UID_persona] [uniqueidentifier] NOT NULL,
	[FirmaCert] [varchar](max) NULL,
	[Data_firma] [varchar](255) NOT NULL,
	[Data_ritirofirma] [varchar](255) NULL,
	[id_AreaPolitica] [int] NULL,
	[Timestamp] [datetime] NOT NULL,
	[ufficio] [bit] NOT NULL,
 CONSTRAINT [PK_FIRME] PRIMARY KEY CLUSTERED 
(
	[UIDEM] ASC,
	[UID_persona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[FIRME] ADD  CONSTRAINT [DF_FIRME_TimeStamp]  DEFAULT (getdate()) FOR [Timestamp]
GO
ALTER TABLE [dbo].[FIRME] ADD  CONSTRAINT [DF_FIRME_ufficio]  DEFAULT ((0)) FOR [ufficio]
GO
