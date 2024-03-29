USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[JOIN_GRUPPO_AD]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JOIN_GRUPPO_AD](
	[UID_Gruppo] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[id_gruppo] [int] NOT NULL,
	[GruppoAD] [nvarchar](50) NULL,
	[GiuntaRegionale] [bit] NOT NULL,
	[AbilitaEMPrivati] [bit] NOT NULL,
	[id_AreaPolitica] [int] NULL,
	[id_legislatura] [int] NOT NULL,
 CONSTRAINT [PK_join_gruppo_AD] PRIMARY KEY CLUSTERED 
(
	[UID_Gruppo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[JOIN_GRUPPO_AD] ADD  CONSTRAINT [DF_join_gruppo_AD_UID_Gruppo]  DEFAULT (newsequentialid()) FOR [UID_Gruppo]
GO
ALTER TABLE [dbo].[JOIN_GRUPPO_AD] ADD  CONSTRAINT [DF_join_gruppo_AD_GiuntaRegionale]  DEFAULT ((0)) FOR [GiuntaRegionale]
GO
ALTER TABLE [dbo].[JOIN_GRUPPO_AD] ADD  CONSTRAINT [DF_JOIN_GRUPPO_AD_AbilitaEMPrivati_1]  DEFAULT ((0)) FOR [AbilitaEMPrivati]
GO
