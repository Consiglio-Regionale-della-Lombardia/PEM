USE [dbEmendamenti]
GO
/****** Object:  Table [dbo].[NOTIFICHE]    Script Date: 12/09/2020 18:38:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[NOTIFICHE](
	[UIDNotifica] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[DataCreazione] [datetime] NOT NULL,
	[UIDEM] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[IDTipo] [int] NOT NULL,
	[Mittente] [uniqueidentifier] NOT NULL,
	[RuoloMittente] [int] NULL,
	[Messaggio] [varchar](max) NULL,
	[DataScadenza] [datetime] NULL,
	[SyncGUID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[IdGruppo] [int] NULL,
	[Chiuso] [bit] NULL,
	[DataChiusura] [datetime] NULL,
	[BLOCCO_INVITI] [uniqueidentifier] NULL,
 CONSTRAINT [PK_NOTIFICHE] PRIMARY KEY CLUSTERED 
(
	[SyncGUID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[NOTIFICHE]  WITH NOCHECK ADD  CONSTRAINT [repl_identity_range_DA57B90C_3A22_4AB0_884B_DFF3C7A9FFA0] CHECK NOT FOR REPLICATION (([UIDNotifica]>(225361296) AND [UIDNotifica]<=(225371296) OR [UIDNotifica]>(225371296) AND [UIDNotifica]<=(225381296)))
GO
ALTER TABLE [dbo].[NOTIFICHE] CHECK CONSTRAINT [repl_identity_range_DA57B90C_3A22_4AB0_884B_DFF3C7A9FFA0]
GO
ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_DataCreazione]  DEFAULT (getdate()) FOR [DataCreazione]
GO
ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_SyncGUID]  DEFAULT (newsequentialid()) FOR [SyncGUID]
GO
