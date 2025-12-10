/****** Object:  Table [dbo].[COMMI]    Script Date: 30/10/2025 10:52:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[COMMI](
	[UIDComma] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UIDArticolo] [uniqueidentifier] NULL,
	[Comma] [varchar](50) NULL,
	[TestoComma] [varchar](max) NULL,
	[Ordine] [int] NULL,
	[UIDUtenteModifica] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
	[Eliminato] [bit] NOT NULL,
 CONSTRAINT [PK_COMMI] PRIMARY KEY CLUSTERED 
(
	[UIDComma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[COMMI] ADD  CONSTRAINT [DF_COMMI_UIDComma]  DEFAULT (newsequentialid()) FOR [UIDComma]
GO

ALTER TABLE [dbo].[COMMI] ADD  CONSTRAINT [DF_COMMI_Ordine]  DEFAULT ((0)) FOR [Ordine]
GO

ALTER TABLE [dbo].[COMMI] ADD  CONSTRAINT [DF_COMMI_Eliminato]  DEFAULT ((0)) FOR [Eliminato]
GO


