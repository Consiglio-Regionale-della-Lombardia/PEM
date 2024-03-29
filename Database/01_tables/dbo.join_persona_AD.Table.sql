USE [dbEmendamenti]
GO

/****** Object:  Table [dbo].[join_persona_AD]    Script Date: 22/06/2022 11:21:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[join_persona_AD](
	[UID_persona] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[id_persona] [int] NOT NULL,
	[UserAD] [nvarchar](50) NULL,
	[pass_locale_crypt] [varchar](max) NULL,
	[gruppi_autorizzazione] [varchar](max) NULL,
 CONSTRAINT [PK_join_persona_AD] PRIMARY KEY CLUSTERED 
(
	[UID_persona] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[join_persona_AD] ADD  CONSTRAINT [DF_join_persona_AD_UID_persona]  DEFAULT (newsequentialid()) FOR [UID_persona]
GO

