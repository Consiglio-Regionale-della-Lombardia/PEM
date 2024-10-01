/****** Object:  Table [dbo].[ATTI_NOTE]    Script Date: 11/06/2024 09:28:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ATTI_NOTE](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UIDPersona] [uniqueidentifier] NOT NULL,
	[Tipo] [int] NOT NULL,
	[Data] [datetime] NOT NULL,
	[Nota] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_NOTE] ADD  CONSTRAINT [DF_ATTI_NOTE_IdNota]  DEFAULT (newid()) FOR [Uid]
GO

