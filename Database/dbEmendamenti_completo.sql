/****** Object:  Database [dbPEMDASI]    Script Date: 10/18/2022 15:09:09 ******/
USE [nome_del_database]
GO
/****** Object:  Table [dbo].[NOTIFICHE_DESTINATARI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NOTIFICHE_DESTINATARI](
	[UID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDNotifica] [bigint] NOT NULL,
	[UIDPersona] [uniqueidentifier] NOT NULL,
	[Visto] [bit] NOT NULL,
	[DataVisto] [datetime] NULL,
	[Chiuso] [bit] NOT NULL,
	[DataChiusura] [datetime] NULL,
	[IdGruppo] [int] NULL,
 CONSTRAINT [PK_NOTIFICHE_DESTINATARI] PRIMARY KEY CLUSTERED 
(
	[UID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NOTIFICHE]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[NOTIFICHE](
	[UIDNotifica] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[DataCreazione] [datetime] NOT NULL,
	[UIDEM] [uniqueidentifier] NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[IDTipo] [int] NOT NULL,
	[Mittente] [uniqueidentifier] NOT NULL,
	[RuoloMittente] [int] NULL,
	[Messaggio] [varchar](max) NULL,
	[DataScadenza] [datetime] NULL,
	[SyncGUID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[IdGruppo] [int] NOT NULL,
	[Chiuso] [bit] NULL,
	[DataChiusura] [datetime] NULL,
	[BLOCCO_INVITI] [uniqueidentifier] NULL,
	[Valida] [bit] NOT NULL,
 CONSTRAINT [PK_NOTIFICHE] PRIMARY KEY CLUSTERED 
(
	[SyncGUID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[MISSIONI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MISSIONI](
	[UIDMissione] [uniqueidentifier] NOT NULL,
	[NMissione] [int] NOT NULL,
	[DAL] [date] NOT NULL,
	[AL] [date] NULL,
	[Desccrizione] [varchar](250) NOT NULL,
	[Ordine] [int] NOT NULL,
 CONSTRAINT [PK_MISSIONI_1] PRIMARY KEY CLUSTERED 
(
	[UIDMissione] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LETTERE]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LETTERE](
	[UIDLettera] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDComma] [uniqueidentifier] NULL,
	[Lettera] [varchar](5) NULL,
	[TestoLettera] [varchar](max) NULL,
	[Ordine] [int] NULL,
 CONSTRAINT [PK_LETTERE] PRIMARY KEY CLUSTERED 
(
	[UIDLettera] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[legislature]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[legislature](
	[id_legislatura] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[num_legislatura] [varchar](4) NOT NULL,
	[durata_legislatura_da] [datetime] NOT NULL,
	[durata_legislatura_a] [datetime] NULL,
	[attiva] [bit] NOT NULL,
	[id_causa_fine] [int] NULL,
 CONSTRAINT [PK_legislature] PRIMARY KEY CLUSTERED 
(
	[id_legislatura] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[join_persona_recapiti]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[join_persona_recapiti](
	[id_rec] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_persona] [int] NOT NULL,
	[recapito] [varchar](250) NOT NULL,
	[tipo_recapito] [char](2) NOT NULL,
 CONSTRAINT [PK_join_persona_recapiti] PRIMARY KEY CLUSTERED 
(
	[id_rec] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[join_persona_organo_carica]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[join_persona_organo_carica](
	[id_rec] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_organo] [int] NOT NULL,
	[id_persona] [int] NOT NULL,
	[id_legislatura] [int] NOT NULL,
	[id_carica] [int] NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[deleted] [bit] NOT NULL,
 CONSTRAINT [PK_join_persona_organo_carica_1] PRIMARY KEY CLUSTERED 
(
	[id_rec] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[join_persona_gruppi_politici]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[join_persona_gruppi_politici](
	[id_rec] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_gruppo] [int] NOT NULL,
	[id_persona] [int] NULL,
	[id_legislatura] [int] NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[deleted] [bit] NOT NULL,
	[id_carica] [int] NULL,
 CONSTRAINT [PK_join_persona_gruppi_politici] PRIMARY KEY CLUSTERED 
(
	[id_rec] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[join_persona_assessorati]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[join_persona_assessorati](
	[id_rec] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_legislatura] [int] NOT NULL,
	[id_persona] [int] NOT NULL,
	[nome_assessorato] [varchar](50) NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[deleted] [bit] NOT NULL,
 CONSTRAINT [PK_join_persona_assessorati] PRIMARY KEY CLUSTERED 
(
	[id_rec] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[join_persona_AD]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[JOIN_GRUPPO_AD]    Script Date: 10/18/2022 15:09:09 ******/
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[join_gruppi_politici_legislature]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[join_gruppi_politici_legislature](
	[id_rec] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_gruppo] [int] NOT NULL,
	[id_legislatura] [int] NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[deleted] [bit] NOT NULL,
 CONSTRAINT [PK_join_gruppi_politici_legislature] PRIMARY KEY CLUSTERED 
(
	[id_rec] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[gruppi_politici]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[gruppi_politici](
	[id_gruppo] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[codice_gruppo] [varchar](50) NOT NULL,
	[nome_gruppo] [varchar](255) NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[attivo] [bit] NOT NULL,
	[id_causa_fine] [int] NULL,
	[deleted] [bit] NOT NULL,
	[TipoArea] [int] NOT NULL,
 CONSTRAINT [PK_gruppi_politici] PRIMARY KEY CLUSTERED 
(
	[id_gruppo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[FIRME]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EM]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EM](
	[UIDEM] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Progressivo] [int] NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[N_EM] [varchar](50) NULL,
	[id_gruppo] [int] NOT NULL,
	[Rif_UIDEM] [uniqueidentifier] NULL,
	[N_SUBEM] [varchar](50) NULL,
	[SubProgressivo] [int] NULL,
	[UIDPersonaProponente] [uniqueidentifier] NULL,
	[UIDPersonaProponenteOLD] [uniqueidentifier] NULL,
	[DataCreazione] [datetime] NULL,
	[UIDPersonaCreazione] [uniqueidentifier] NULL,
	[idRuoloCreazione] [int] NULL,
	[DataModifica] [datetime] NULL,
	[UIDPersonaModifica] [uniqueidentifier] NULL,
	[DataDeposito] [varchar](255) NULL,
	[UIDPersonaPrimaFirma] [uniqueidentifier] NULL,
	[DataPrimaFirma] [datetime] NULL,
	[UIDPersonaDeposito] [uniqueidentifier] NULL,
	[Proietta] [bit] NOT NULL,
	[DataProietta] [datetime] NULL,
	[UIDPersonaProietta] [uniqueidentifier] NULL,
	[DataRitiro] [datetime] NULL,
	[UIDPersonaRitiro] [uniqueidentifier] NULL,
	[Hash] [varchar](max) NULL,
	[IDTipo_EM] [int] NOT NULL,
	[IDParte] [int] NOT NULL,
	[NTitolo] [varchar](5) NULL,
	[NCapo] [varchar](5) NULL,
	[UIDArticolo] [uniqueidentifier] NULL,
	[UIDComma] [uniqueidentifier] NULL,
	[UIDLettera] [uniqueidentifier] NULL,
	[NLettera] [varchar](5) NULL,
	[UIDParte_LR] [uniqueidentifier] NULL,
	[NNumero] [varchar](5) NULL,
	[UIDMissione] [uniqueidentifier] NULL,
	[NMissione] [int] NULL,
	[NProgramma] [int] NULL,
	[NTitoloB] [int] NULL,
	[OrdinePresentazione] [int] NOT NULL,
	[OrdineVotazione] [int] NOT NULL,
	[TestoEM_originale] [varchar](max) NULL,
	[EM_Certificato] [varchar](max) NULL,
	[TestoREL_originale] [varchar](max) NULL,
	[PATH_AllegatoGenerico] [varchar](max) NULL,
	[PATH_AllegatoTecnico] [varchar](max) NULL,
	[EffettiFinanziari] [int] NULL,
	[NOTE_EM] [varchar](max) NULL,
	[NOTE_Griglia] [varchar](max) NULL,
	[TestoEM_Modificabile] [varchar](max) NULL,
	[IDStato] [int] NULL,
	[Firma_su_invito] [bit] NOT NULL,
	[UID_QRCode] [uniqueidentifier] NOT NULL,
	[AreaPolitica] [int] NULL,
	[Eliminato] [bit] NOT NULL,
	[UIDPersonaElimina] [uniqueidentifier] NULL,
	[DataElimina] [datetime] NULL,
	[chkf] [varchar](255) NULL,
	[chkem] [int] NULL,
	[Timestamp] [datetime] NULL,
	[Colore] [varchar](50) NULL,
	[Tags] [varchar](max) NULL,
 CONSTRAINT [PK_EM] PRIMARY KEY CLUSTERED 
(
	[UIDEM] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE UNIQUE NONCLUSTERED INDEX [chk_uniqe_nEM] ON [dbo].[EM] 
(
	[UIDAtto] ASC,
	[N_EM] ASC
)
WHERE ([N_EM] IS NOT NULL AND [UIDAtto] IS NOT NULL)
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TITOLI_MISSIONI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TITOLI_MISSIONI](
	[NTitoloB] [int] NOT NULL,
	[Descrizione] [nvarchar](150) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_organo]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tipo_organo](
	[id] [int] NOT NULL,
	[descrizione] [varchar](50) NULL,
 CONSTRAINT [PK_tipo_organo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TIPI_NOTIFICA]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TIPI_NOTIFICA](
	[IDTipo] [int] NOT NULL,
	[Tipo] [varchar](100) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TIPI_EM]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TIPI_EM](
	[IDTipo_EM] [int] NOT NULL,
	[Tipo_EM] [nvarchar](25) NOT NULL,
	[Ordine] [int] NOT NULL,
 CONSTRAINT [PK_TIPI_EM] PRIMARY KEY CLUSTERED 
(
	[IDTipo_EM] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id relativo alla tipologia di emendamento' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TIPI_EM', @level2type=N'COLUMN',@level2name=N'IDTipo_EM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tipo di emendamento (aggiuntivo, modificativo, ...)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TIPI_EM', @level2type=N'COLUMN',@level2name=N'Tipo_EM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ordine che verrà seguito dall''algoritmo di ordinamento della Griglia' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TIPI_EM', @level2type=N'COLUMN',@level2name=N'Ordine'
GO
/****** Object:  Table [dbo].[TIPI_ATTO]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TIPI_ATTO](
	[IDTipoAtto] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Tipo_Atto] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_TIPI_ATTO] PRIMARY KEY CLUSTERED 
(
	[IDTipoAtto] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_recapiti]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_recapiti](
	[id_recapito] [char](2) NOT NULL,
	[nome_recapito] [varchar](50) NOT NULL,
 CONSTRAINT [PK_tbl_recapiti] PRIMARY KEY CLUSTERED 
(
	[id_recapito] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Tags](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tag] [varchar](150) NOT NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[STATI_EM]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[STATI_EM](
	[IDStato] [int] NOT NULL,
	[Stato] [nvarchar](50) NOT NULL,
	[icona] [nvarchar](50) NULL,
	[CssClass] [nvarchar](15) NULL,
	[Ordinamento] [int] NOT NULL,
 CONSTRAINT [PK_STATI_EM] PRIMARY KEY CLUSTERED 
(
	[IDStato] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[STAMPE_INFO]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[STAMPE_INFO](
	[Id] [uniqueidentifier] NOT NULL,
	[UIDStampa] [uniqueidentifier] NOT NULL,
	[Message] [varchar](255) NULL,
	[Date] [datetime] NOT NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[STAMPE]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[STAMPE](
	[UIDStampa] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NULL,
	[Da] [int] NOT NULL,
	[A] [int] NOT NULL,
	[UIDUtenteRichiesta] [uniqueidentifier] NOT NULL,
	[DataRichiesta] [datetime] NOT NULL,
	[Invio] [bit] NOT NULL,
	[DataInvio] [datetime] NULL,
	[MessaggioErrore] [varchar](max) NULL,
	[Lock] [bit] NOT NULL,
	[DataLock] [datetime] NULL,
	[PathFile] [varchar](max) NULL,
	[DataInizioEsecuzione] [datetime] NULL,
	[DataFineEsecuzione] [datetime] NULL,
	[Tentativi] [int] NOT NULL,
	[CurrentRole] [int] NULL,
	[Scadenza] [datetime] NULL,
	[Ordine] [int] NULL,
	[Query] [varchar](max) NULL,
	[Notifica] [bit] NOT NULL,
	[UIDEM] [uniqueidentifier] NULL,
	[DASI] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ARTICOLI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ARTICOLI](
	[UIDArticolo] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[Articolo] [varchar](50) NOT NULL,
	[RubricaArticolo] [nvarchar](1000) NULL,
	[TestoArticolo] [varchar](max) NULL,
	[Ordine] [int] NOT NULL,
 CONSTRAINT [PK_Articoli] PRIMARY KEY CLUSTERED 
(
	[UIDArticolo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identificativo univoco dell''articolo' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ARTICOLI', @level2type=N'COLUMN',@level2name=N'UIDArticolo'
GO
/****** Object:  Table [dbo].[AREAPOLITICA]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AREAPOLITICA](
	[Id] [int] NULL,
	[AreaPolitica] [varchar](50) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identificativo dell''area politica (maggioranza, minoranza, misto, ...)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AREAPOLITICA', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'descrizione area politica (maggioranza, minoranza, misto, ...)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AREAPOLITICA', @level2type=N'COLUMN',@level2name=N'AreaPolitica'
GO
/****** Object:  Table [dbo].[UTENTI_NoCons]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UTENTI_NoCons](
	[UID_persona] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[id_persona] [int] IDENTITY(10000,1) NOT FOR REPLICATION NOT NULL,
	[cognome] [varchar](50) NOT NULL,
	[nome] [varchar](50) NULL,
	[email] [varchar](250) NULL,
	[foto] [varchar](250) NULL,
	[UserAD] [varchar](50) NULL,
	[id_gruppo_politico_rif] [int] NULL,
	[notifica_firma] [bit] NOT NULL,
	[notifica_deposito] [bit] NOT NULL,
	[RichiediModificaPWD] [bit] NOT NULL,
	[Data_ultima_modifica_PWD] [datetime] NULL,
	[pass_locale_crypt] [varchar](max) NULL,
	[gruppi_autorizzazione] [varchar](max) NULL,
	[attivo] [bit] NOT NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_UTENTI_NoCons_1] PRIMARY KEY CLUSTERED 
(
	[UID_persona] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[COMMI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[COMMI](
	[UIDComma] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UIDArticolo] [uniqueidentifier] NULL,
	[Comma] [varchar](50) NULL,
	[TestoComma] [varchar](max) NULL,
	[Ordine] [int] NULL,
 CONSTRAINT [PK_COMMI] PRIMARY KEY CLUSTERED 
(
	[UIDComma] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[cariche]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[cariche](
	[id_carica] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[nome_carica] [varchar](250) NOT NULL,
	[ordine] [int] NOT NULL,
	[tipologia] [varchar](20) NOT NULL,
	[presidente_gruppo] [bit] NULL,
	[id_tipo_carica] [tinyint] NULL,
 CONSTRAINT [PK_cariche] PRIMARY KEY CLUSTERED 
(
	[id_carica] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ATTI_SOGGETTI_INTERROGATI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ATTI_SOGGETTI_INTERROGATI](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[id_carica] [int] NOT NULL,
	[DataCreazione] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ATTI_RELATORI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ATTI_RELATORI](
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UIDPersona] [uniqueidentifier] NOT NULL,
	[sycReplica] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
 CONSTRAINT [PK_ATTI_RELATORI] PRIMARY KEY CLUSTERED 
(
	[sycReplica] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ATTI_FIRME]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ATTI_FIRME](
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[UID_persona] [uniqueidentifier] NOT NULL,
	[FirmaCert] [varchar](max) NOT NULL,
	[Data_firma] [varchar](255) NULL,
	[Data_ritirofirma] [varchar](255) NULL,
	[id_AreaPolitica] [int] NULL,
	[Timestamp] [datetime] NOT NULL,
	[ufficio] [bit] NOT NULL,
	[PrimoFirmatario] [bit] NOT NULL,
	[id_gruppo] [int] NOT NULL,
	[Valida] [bit] NOT NULL,
	[Capogruppo] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ATTI_DASI_CONTATORI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ATTI_DASI_CONTATORI](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Tipo] [int] NOT NULL,
	[Risposta] [int] NOT NULL,
	[Contatore] [int] NOT NULL,
	[Inizio] [int] NOT NULL,
	[Fine] [int] NULL,
 CONSTRAINT [PK_ATTI_DASI_CONTATORI] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ATTI_DASI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ATTI_DASI](
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[Tipo] [int] NOT NULL,
	[TipoMOZ] [int] NOT NULL,
	[Progressivo] [int] NULL,
	[Etichetta] [varchar](255) NULL,
	[NAtto] [varchar](max) NULL,
	[NAtto_search] [int] NOT NULL,
	[Oggetto] [varchar](max) NULL,
	[Oggetto_Modificato] [varchar](max) NULL,
	[Premesse] [varchar](max) NULL,
	[Premesse_Modificato] [varchar](max) NULL,
	[TipoRichiesta] [int] NOT NULL,
	[TipoRichiestaDestinatario] [int] NOT NULL,
	[Richiesta] [varchar](max) NULL,
	[Richiesta_Modificata] [varchar](max) NULL,
	[DataCreazione] [datetime] NOT NULL,
	[UIDPersonaCreazione] [uniqueidentifier] NOT NULL,
	[idRuoloCreazione] [int] NOT NULL,
	[DataModifica] [datetime] NULL,
	[UIDPersonaModifica] [uniqueidentifier] NULL,
	[DataPresentazione] [varchar](255) NULL,
	[DataPresentazione_MOZ] [varchar](255) NULL,
	[DataPresentazione_MOZ_URGENTE] [varchar](255) NULL,
	[DataPresentazione_MOZ_ABBINATA] [varchar](255) NULL,
	[UIDPersonaPresentazione] [uniqueidentifier] NULL,
	[DataRichiestaIscrizioneSeduta] [varchar](255) NULL,
	[UIDPersonaRichiestaIscrizione] [uniqueidentifier] NULL,
	[UIDPersonaProponente] [uniqueidentifier] NULL,
	[UIDPersonaPrimaFirma] [uniqueidentifier] NULL,
	[DataPrimaFirma] [datetime] NULL,
	[Proietta] [bit] NOT NULL,
	[DataProietta] [datetime] NULL,
	[UIDPersonaProietta] [uniqueidentifier] NULL,
	[DataRitiro] [datetime] NULL,
	[UIDPersonaRitiro] [uniqueidentifier] NULL,
	[Hash] [varchar](max) NULL,
	[IDTipo_Risposta] [int] NOT NULL,
	[OrdineVisualizzazione] [int] NOT NULL,
	[PATH_AllegatoGenerico] [varchar](max) NULL,
	[Note_Pubbliche] [varchar](max) NULL,
	[Note_Private] [varchar](max) NULL,
	[IDStato] [int] NOT NULL,
	[IDStato_Motivazione] [int] NOT NULL,
	[Firma_su_invito] [bit] NOT NULL,
	[UID_QRCode] [uniqueidentifier] NOT NULL,
	[AreaPolitica] [int] NOT NULL,
	[id_gruppo] [int] NOT NULL,
	[Eliminato] [bit] NOT NULL,
	[UIDPersonaElimina] [uniqueidentifier] NULL,
	[DataElimina] [datetime] NULL,
	[chkf] [varchar](255) NULL,
	[Timestamp] [datetime] NOT NULL,
	[Atto_Certificato] [varchar](max) NULL,
	[Legislatura] [int] NOT NULL,
	[UIDSeduta] [uniqueidentifier] NULL,
	[DataIscrizioneSeduta] [datetime] NULL,
	[UIDPersonaIscrizioneSeduta] [uniqueidentifier] NULL,
	[UID_MOZ_Abbinata] [uniqueidentifier] NULL,
	[UID_Atto_ODG] [uniqueidentifier] NULL,
	[Non_Passaggio_In_Esame] [bit] NOT NULL,
	[Inviato_Al_Protocollo] [bit] NOT NULL,
	[DataInvioAlProtocollo] [datetime] NULL,
 CONSTRAINT [PK_ATTI_DASI] PRIMARY KEY CLUSTERED 
(
	[UIDAtto] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ATTI_COMMISSIONI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ATTI_COMMISSIONI](
	[Uid] [uniqueidentifier] NOT NULL,
	[UIDAtto] [uniqueidentifier] NOT NULL,
	[id_organo] [int] NOT NULL,
	[DataCreazione] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SEDUTE]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SEDUTE](
	[UIDSeduta] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Data_seduta] [datetime] NOT NULL,
	[Data_apertura] [datetime] NULL,
	[Data_effettiva_inizio] [datetime] NULL,
	[Data_effettiva_fine] [datetime] NULL,
	[IDOrgano] [int] NULL,
	[Scadenza_presentazione] [datetime] NULL,
	[DataScadenzaPresentazioneIQT] [datetime] NULL,
	[DataScadenzaPresentazioneMOZ] [datetime] NULL,
	[DataScadenzaPresentazioneMOZA] [datetime] NULL,
	[DataScadenzaPresentazioneMOZU] [datetime] NULL,
	[DataScadenzaPresentazioneODG] [datetime] NULL,
	[id_legislatura] [int] NULL,
	[Intervalli] [varchar](max) NULL,
	[UIDPersonaCreazione] [uniqueidentifier] NULL,
	[DataCreazione] [datetime] NULL,
	[UIDPersonaModifica] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
	[Eliminato] [bit] NULL,
	[Riservato_DASI] [bit] NOT NULL,
 CONSTRAINT [PK_SEDUTE] PRIMARY KEY CLUSTERED 
(
	[UIDSeduta] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RUOLI_UTENTE]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RUOLI_UTENTE](
	[UID_ruolo_utente] [uniqueidentifier] NOT NULL,
	[UID_persona] [uniqueidentifier] NOT NULL,
	[IDruolo] [int] NOT NULL,
 CONSTRAINT [PK_RUOLI_UTENTE] PRIMARY KEY CLUSTERED 
(
	[UID_ruolo_utente] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RUOLI]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RUOLI](
	[IDruolo] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
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
/****** Object:  Table [dbo].[PINS_NoCons]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PINS_NoCons](
	[UIDPIN] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UID_persona] [uniqueidentifier] NOT NULL,
	[PIN] [nvarchar](255) NOT NULL,
	[Dal] [datetime] NOT NULL,
	[Al] [datetime] NULL,
	[FIRMA_e_DEPOSITO] [bit] NOT NULL,
	[RichiediModificaPIN] [bit] NOT NULL,
 CONSTRAINT [PK_PINS_NoCons] PRIMARY KEY CLUSTERED 
(
	[UIDPIN] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PINS]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PINS](
	[UIDPIN] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UID_persona] [uniqueidentifier] NOT NULL,
	[PIN] [varchar](255) NOT NULL,
	[Dal] [datetime] NOT NULL,
	[Al] [datetime] NULL,
	[FIRMA_e_DEPOSITO] [bit] NOT NULL,
	[RichiediModificaPIN] [bit] NOT NULL,
 CONSTRAINT [PK_PINS] PRIMARY KEY CLUSTERED 
(
	[UIDPIN] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[persona]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[persona](
	[id_persona] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[cognome] [varchar](50) NOT NULL,
	[nome] [varchar](50) NOT NULL,
	[data_nascita] [datetime] NULL,
	[foto] [varchar](255) NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_persona] PRIMARY KEY CLUSTERED 
(
	[id_persona] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PARTI_TESTO]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PARTI_TESTO](
	[IDParte] [int] NOT NULL,
	[Parte] [nvarchar](20) NOT NULL,
	[Ordine] [int] NOT NULL,
	[Passo] [int] NULL,
 CONSTRAINT [PK_PARTI_TESTO] PRIMARY KEY CLUSTERED 
(
	[IDParte] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id della tipologia di parte dell''atto emendabile' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PARTI_TESTO', @level2type=N'COLUMN',@level2name=N'IDParte'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Descrizione della parte di atto emendabile' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PARTI_TESTO', @level2type=N'COLUMN',@level2name=N'Parte'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ordine che verrà seguito dall''algoritmo di ordinamento della Griglia' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PARTI_TESTO', @level2type=N'COLUMN',@level2name=N'Ordine'
GO
/****** Object:  Table [dbo].[PARTE_LR]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PARTE_LR](
	[UIDParte_LR] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UIDLettera] [uniqueidentifier] NULL,
	[RifParte_LR] [varchar](250) NULL,
	[Parte_LR] [varchar](50) NULL,
	[TestoParte_LR] [varchar](max) NULL,
	[Ordine] [int] NOT NULL,
 CONSTRAINT [PK_PARTE_LR] PRIMARY KEY CLUSTERED 
(
	[UIDParte_LR] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[organi]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[organi](
	[id_organo] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[id_legislatura] [int] NOT NULL,
	[nome_organo] [varchar](255) NOT NULL,
	[data_inizio] [datetime] NOT NULL,
	[data_fine] [datetime] NULL,
	[deleted] [bit] NOT NULL,
	[ordinamento] [int] NULL,
	[id_tipo_organo] [int] NULL,
	[nome_organo_breve] [varchar](30) NULL,
	[id_categoria_organo] [int] NULL,
 CONSTRAINT [PK_organi] PRIMARY KEY CLUSTERED 
(
	[id_organo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[View_PINS]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_PINS]
AS
(SELECT     dbo.PINS.*
FROM         dbo.PINS)
UNION
(SELECT     dbo.PINS_NoCons.*
 FROM         dbo.PINS_NoCons)
GO
/****** Object:  View [dbo].[View_gruppi_politici_con_giunta]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_gruppi_politici_con_giunta]
AS
(SELECT        id_gruppo, nome_gruppo, codice_gruppo
FROM            dbo.gruppi_politici)
UNION
(SELECT        id_gruppo, 'GIUNTA REGIONALE' AS nome_gruppo, 'GIUNTA REGIONALE' AS codice_gruppo
 FROM            JOIN_GRUPPO_AD
 WHERE        GiuntaRegionale = 1)
GO
/****** Object:  View [dbo].[View_Conteggi_EM_Gruppi_Politici]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[View_Conteggi_EM_Gruppi_Politici] as(
select em.UIDAtto, em.id_gruppo, gp.nome_gruppo, Count(*) as num_em
from EM em
inner join gruppi_politici gp on em.id_gruppo = gp.id_gruppo
group by em.UIDAtto, em.id_gruppo, gp.nome_gruppo
)
GO
/****** Object:  View [dbo].[View_Conteggi_EM_Area_Politica]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[View_Conteggi_EM_Area_Politica] as(
select em.UIDAtto, em.AreaPolitica as IdArea, a.AreaPolitica, Count(*) as num_em
from EM em
inner join AREAPOLITICA a on a.Id=em.AreaPolitica
group by em.UIDAtto, em.AreaPolitica, a.AreaPolitica
)
GO
/****** Object:  View [dbo].[View_CONSIGLIERI_PEM]    Script Date: 10/18/2022 15:09:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_CONSIGLIERI_PEM]
   
AS
(SELECT     dbo.join_persona_AD.UID_persona, dbo.persona.id_persona, dbo.persona.cognome COLLATE database_default as Cognome, dbo.persona.nome COLLATE database_default as Nome, 
                      COALESCE (dbo.persona.foto, 'noface.png') COLLATE database_default AS foto
FROM         dbo.persona INNER JOIN
                      dbo.join_persona_AD ON dbo.join_persona_AD.id_persona = dbo.persona.id_persona)
UNION
(SELECT     dbo.UTENTI_NoCons.UID_persona, dbo.UTENTI_NoCons.id_persona, dbo.UTENTI_NoCons.cognome COLLATE database_default as Cognome, dbo.UTENTI_NoCons.nome COLLATE database_default as Nome, 
                      COALESCE (dbo.UTENTI_NoCons.foto, 'noface.png') COLLATE database_default AS foto
FROM         dbo.UTENTI_NoCons)
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[20] 2[19] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "persona"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 189
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "join_persona_AD"
            Begin Extent = 
               Top = 6
               Left = 227
               Bottom = 99
               Right = 378
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_CONSIGLIERI_PEM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_CONSIGLIERI_PEM'
GO
/****** Object:  StoredProcedure [dbo].[ORDINA_EM_TRATTAZIONE]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ORDINA_EM_TRATTAZIONE]
   @UIDAtto uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @Progressivo_Ordine int, @UIDEM uniqueidentifier;


	SET @Progressivo_Ordine = 1;
	/*SET @UIDAtto = '357DE0B0-358A-4722-8183-DD8B04978DDC' DA SOSTITUIRE CON UN PARAMETRO IN INPUT*/	

	/* Azzero il campo OrdineTrattazione per pulire l'ordinamento precedente */
	UPDATE EM set OrdineVotazione=0 where EM.IDStato<>0 and EM.DataElimina is null and EM.UIDAtto=@UIDAtto
	
	/* Faccio un semplice ordinamento con una Order by sui campi della tabella EM */
	DECLARE the_cursor CURSOR FAST_FORWARD	
	FOR SELECT EM.UIDEM
		FROM   EM LEFT OUTER JOIN
               PARTI_TESTO ON EM.IDParte = PARTI_TESTO.IDParte LEFT OUTER JOIN
               TIPI_EM ON EM.IDTipo_EM = TIPI_EM.IDTipo_EM LEFT OUTER JOIN
               ARTICOLI ON EM.UIDArticolo = ARTICOLI.UIDArticolo LEFT OUTER JOIN COMMI on EM.UIDComma = COMMI.UIDComma LEFT OUTER JOIN LETTERE on EM.UIDLettera = LETTERE.UIDLettera
        WHERE  EM.IDStato<>0 and EM.DataElimina is null and EM.UIDAtto=@UIDAtto
		ORDER BY PARTI_TESTO.Ordine, EM.NMissione, EM.NProgramma, EM.NTitoloB, EM.NTitolo, EM.NCapo, ARTICOLI.Ordine, COMMI.Ordine, LETTERE.Ordine, EM.NLettera, EM.NNumero, TIPI_EM.Ordine, EM.OrdinePresentazione, EM.[Timestamp] DESC

	OPEN the_cursor
	FETCH NEXT FROM the_cursor INTO @UIDEM

	WHILE @@FETCH_STATUS = 0
	BEGIN
		/* Modifico l'ordine di trattazione semplicemente aggiornandolo con il contatore progressivo Progressivo_ordine, in base all'order by fatta dalla query precedente*/
		UPDATE EM SET OrdineVotazione=@Progressivo_Ordine where UIDEM=@UIDEM
		SET @Progressivo_Ordine = @Progressivo_Ordine +1;
		FETCH NEXT FROM the_cursor INTO @UIDEM
	END

	CLOSE the_cursor
	DEALLOCATE the_cursor
END
GO
/****** Object:  Table [dbo].[ATTI]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ATTI](
	[UIDAtto] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[NAtto] [varchar](50) NOT NULL,
	[IDTipoAtto] [int] NOT NULL,
	[Oggetto] [varchar](500) NULL,
	[Note] [varchar](255) NULL,
	[Path_Testo_Atto] [varchar](max) NULL,
	[UIDSeduta] [uniqueidentifier] NULL,
	[Data_apertura] [datetime] NULL,
	[Data_chiusura] [datetime] NULL,
	[VIS_Mis_Prog] [bit] NOT NULL,
	[UIDAssessoreRiferimento] [uniqueidentifier] NULL,
	[Notifica_deposito_differita] [bit] NOT NULL,
	[OrdinePresentazione] [bit] NULL,
	[OrdineVotazione] [bit] NULL,
	[Priorita] [int] NULL,
	[DataCreazione] [datetime] NULL,
	[UIDPersonaCreazione] [uniqueidentifier] NULL,
	[DataModifica] [datetime] NULL,
	[UIDPersonaModifica] [uniqueidentifier] NULL,
	[Eliminato] [bit] NULL,
	[LinkFascicoloPresentazione] [varchar](max) NULL,
	[DataCreazionePresentazione] [datetime] NULL,
	[LinkFascicoloVotazione] [varchar](max) NULL,
	[DataCreazioneVotazione] [datetime] NULL,
	[DataUltimaModificaEM] [datetime] NULL,
	[BloccoODG] [bit] NOT NULL,
	[Jolly] [bit] NOT NULL,
	[Emendabile] [bit] NOT NULL,
	[Fascicoli_Da_Aggiornare] [bit] NOT NULL,
 CONSTRAINT [PK_ATTI] PRIMARY KEY CLUSTERED 
(
	[UIDAtto] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ATTI', @level2type=N'COLUMN',@level2name=N'Jolly'
GO
/****** Object:  View [dbo].[View_Composizione_GiuntaRegionale]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Composizione_GiuntaRegionale]
AS
SELECT DISTINCT dbo.persona.cognome, dbo.persona.nome, dbo.join_persona_recapiti.recapito AS email
FROM         dbo.join_persona_organo_carica INNER JOIN
                      dbo.persona ON dbo.join_persona_organo_carica.id_persona = dbo.persona.id_persona INNER JOIN
                      dbo.join_persona_recapiti ON dbo.join_persona_organo_carica.id_persona = dbo.join_persona_recapiti.id_persona INNER JOIN
                      dbo.organi ON dbo.join_persona_organo_carica.id_organo = dbo.organi.id_organo
WHERE     (dbo.join_persona_recapiti.tipo_recapito = 'EP') AND (dbo.persona.deleted = 0) AND (dbo.join_persona_organo_carica.deleted = 0) AND 
                      (dbo.join_persona_organo_carica.id_legislatura IN
                          (SELECT     id_legislatura
                            FROM          dbo.legislature
                            WHERE      (attiva = 1) AND (durata_legislatura_a IS NULL))) AND (dbo.join_persona_organo_carica.data_fine IS NULL) AND 
                      (dbo.join_persona_organo_carica.id_organo IN
                          (SELECT     id_organo
                            FROM          dbo.organi AS organi_1
                            WHERE      (id_tipo_organo = 4)))
GO
/****** Object:  UserDefinedFunction [dbo].[get_CaricaGiunta_from_UIDpersona]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_CaricaGiunta_from_UIDpersona](@UID_persona uniqueidentifier)
RETURNS varchar(1000)
AS 

BEGIN

	Declare @Nome_CaricaGiunta varchar(1000);
	Declare @Nomi_CaricaGiunta varchar(1000);
 
DECLARE getemp_curs CURSOR FOR 
(
SELECT     cariche.nome_carica

FROM         cariche INNER JOIN
                      join_persona_organo_carica ON cariche.id_carica = join_persona_organo_carica.id_carica INNER JOIN
                      organi ON join_persona_organo_carica.id_organo = organi.id_organo INNER JOIN
                      join_persona_AD ON join_persona_organo_carica.id_persona = join_persona_AD.id_persona 
WHERE		
			UID_persona=@UID_persona AND
			organi.id_tipo_organo=4 AND
			organi.deleted = 0 AND
			join_persona_organo_carica.deleted = 0 AND
			not (nome_carica like '%non consigliere%') AND
			(
			  (join_persona_organo_carica.data_inizio <= GETDATE() AND join_persona_organo_carica.data_fine >= GETDATE()) OR (join_persona_organo_carica.data_inizio <= GETDATE() AND join_persona_organo_carica.data_fine IS NULL)
			)
)
ORDER BY cariche.ordine

OPEN getemp_curs
FETCH NEXT FROM getemp_curs into @Nome_CaricaGiunta
SET @Nomi_CaricaGiunta = @Nome_CaricaGiunta
FETCH NEXT FROM getemp_curs into @Nome_CaricaGiunta

WHILE @@FETCH_STATUS = 0 BEGIN
        SET @Nomi_CaricaGiunta = @Nomi_CaricaGiunta + ' e ' + @Nome_CaricaGiunta
 FETCH NEXT FROM getemp_curs into @Nome_CaricaGiunta
END

CLOSE getemp_curs
DEALLOCATE getemp_curs

RETURN @Nomi_CaricaGiunta;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[consigliere_attivo]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[consigliere_attivo](@id_persona int)
RETURNS bit
AS 

BEGIN

	Declare @id_rec int;
	Declare @res bit;
 
select @id_rec =
(
	SELECT [id_rec]
)
  FROM [dbEmendamenti].[dbo].[join_persona_organo_carica]
  WHERE deleted=0 and id_persona=@id_persona and (id_carica=4 or id_carica=36 or id_carica=100) and ((GETDATE() between data_inizio and data_fine) or (data_inizio<=GETDATE() and data_fine is null))	   

if @id_rec is not null select @res=1 else select @res=0;

RETURN @res;

END;
GO
/****** Object:  StoredProcedure [dbo].[UP_EM_TRATTAZIONE]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UP_EM_TRATTAZIONE]
   @UIDEM uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @OrdineVotazione int;
    DECLARE @UIDAtto uniqueidentifier;

/*Seleziono e memorizzo l'UID dell'atto a cui appartiene l'EM*/
select @UIDAtto =
(
	SELECT EM.UIDAtto
)
  FROM [EM]
	WHERE EM.UIDEM=@UIDEM

/*Seleziono e memorizzo l'ordine di Votazione attuale dell'EM*/
select @OrdineVotazione =
(
	SELECT EM.OrdineVotazione
)
  FROM [EM]
	WHERE EM.UIDEM=@UIDEM

if (@OrdineVotazione <= 1) return;

/*Aggiorno l'ordine di votazione dell'EM sottraendo 1 a OrdineVotazione e quindi lo porto in alto di una posizione*/
UPDATE EM SET EM.OrdineVotazione=@OrdineVotazione-1 where EM.UIDEM=@UIDEM

/*Dopo l'aggiornamento avrò 2 EM con la stessa numerazione: 1 EM è quello che ho appena spostato mentre l'altro EM è quello che deve essere spostato in basso di una posizione*/
/*Quindi sposto l'altro EM in basso di una posizione sommando 1 a Ordine Votazione*/

UPDATE EM SET EM.OrdineVotazione=@OrdineVotazione where (EM.OrdineVotazione=@OrdineVotazione-1 AND EM.UIDEM<>@UIDEM) AND EM.UIDAtto=@UIDAtto	
	
END
GO
/****** Object:  StoredProcedure [dbo].[SPOSTA_EM_TRATTAZIONE]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SPOSTA_EM_TRATTAZIONE]
   @UIDEM uniqueidentifier,
   @Pos int
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @OrdineVotazione int;
    DECLARE @MaxOrdineVotazione int;
    DECLARE @UIDAtto uniqueidentifier;

	/*Seleziono e memorizzo l'UID dell'atto a cui appartiene l'EM*/
	select @UIDAtto =
	(
		SELECT EM.UIDAtto
	)
	  FROM [EM]
		WHERE EM.UIDEM=@UIDEM

	/*Seleziono e memorizzo l'attuale massimo ordine di Votazione per gli EM*/
	select @MaxOrdineVotazione =
	(
		SELECT MAX(EM.OrdineVotazione)
	)
	  FROM [EM]
		WHERE EM.UIDAtto=@UIDAtto
		
	if (@Pos>@MaxOrdineVotazione or @Pos<1 or @Pos=@OrdineVotazione) return;

	select @OrdineVotazione =
	(
		SELECT EM.OrdineVotazione
	)
	  FROM [EM]
		WHERE EM.UIDEM=@UIDEM

	/*Aggiorno l'ordine di votazione dell'EM portandolo nella posizione indicata*/
	UPDATE EM SET EM.OrdineVotazione=@Pos where EM.UIDEM=@UIDEM

	DECLARE @UIDEM1 uniqueidentifier;
	IF @Pos<@OrdineVotazione
	   BEGIN
		/* Sposto su gli EM che  */
		DECLARE the_cursor CURSOR FAST_FORWARD	
		FOR SELECT EM.UIDEM
			FROM   EM 
			WHERE  EM.UIDEM<>@UIDEM and EM.OrdineVotazione>=@Pos and EM.OrdineVotazione<=@OrdineVotazione and EM.UIDAtto=@UIDAtto 
			ORDER BY EM.OrdineVotazione

		OPEN the_cursor
		FETCH NEXT FROM the_cursor INTO @UIDEM1

		WHILE @@FETCH_STATUS = 0
		BEGIN
			/* Modifico l'ordine di trattazione semplicemente aggiornandolo con il contatore progressivo Progressivo_ordine, in base all'order by fatta dalla query precedente*/
			UPDATE EM SET OrdineVotazione=OrdineVotazione+1 where UIDEM=@UIDEM1
			FETCH NEXT FROM the_cursor INTO @UIDEM1
		END

		CLOSE the_cursor
		DEALLOCATE the_cursor
	   END
	ELSE
	   BEGIN
		/* Sposto su gli EM che  */
		DECLARE the_cursor CURSOR FAST_FORWARD	
		FOR SELECT EM.UIDEM
			FROM   EM 
			WHERE  EM.UIDEM<>@UIDEM and EM.OrdineVotazione<=@Pos and EM.OrdineVotazione>=@OrdineVotazione and EM.UIDAtto=@UIDAtto 
			ORDER BY EM.OrdineVotazione

		OPEN the_cursor
		FETCH NEXT FROM the_cursor INTO @UIDEM1

		WHILE @@FETCH_STATUS = 0
		BEGIN
			/* Modifico l'ordine di trattazione semplicemente aggiornandolo con il contatore progressivo Progressivo_ordine, in base all'order by fatta dalla query precedente*/
			UPDATE EM SET OrdineVotazione=OrdineVotazione-1 where UIDEM=@UIDEM1
			FETCH NEXT FROM the_cursor INTO @UIDEM1
		END

		CLOSE the_cursor
		DEALLOCATE the_cursor
	   END
	
END
GO
/****** Object:  StoredProcedure [dbo].[DOWN_EM_TRATTAZIONE]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DOWN_EM_TRATTAZIONE]
   @UIDEM uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @OrdineVotazione int;
    DECLARE @MaxOrdineVotazione int;
    DECLARE @UIDAtto uniqueidentifier;

/*Seleziono e memorizzo l'UID dell'atto a cui appartiene l'EM*/
select @UIDAtto =
(
	SELECT EM.UIDAtto
)
  FROM [EM]
	WHERE EM.UIDEM=@UIDEM

/*Seleziono e memorizzo l'attuale massimo ordine di Votazione per gli EM*/
select @MaxOrdineVotazione =
(
	SELECT COUNT(EM.OrdineVotazione)
)
  FROM [EM]
	WHERE EM.UIDAtto=@UIDAtto
	
/*Seleziono e memorizzo l'ordine di Votazione attuale dell'EM*/
select @OrdineVotazione =
(
	SELECT EM.OrdineVotazione
)
  FROM [EM]
	WHERE EM.UIDEM=@UIDEM
	
	if (@OrdineVotazione >= @MaxOrdineVotazione) return;

/*Aggiorno l'ordine di votazione dell'EM sottraendo 1 a OrdineVotazione e quindi lo porto in alto di una posizione*/
UPDATE EM SET EM.OrdineVotazione=@OrdineVotazione+1 where EM.UIDEM=@UIDEM

/*Dopo l'aggiornamento avrò 2 EM con la stessa numerazione: 1 EM è quello che ho appena spostato mentre l'altro EM è quello che deve essere spostato in basso di una posizione*/
/*Quindi sposto l'altro EM in basso di una posizione sommando 1 a Ordine Votazione*/

UPDATE EM SET EM.OrdineVotazione=@OrdineVotazione where (EM.OrdineVotazione=@OrdineVotazione+1 AND EM.UIDEM<>@UIDEM) AND EM.UIDAtto=@UIDAtto	
	
END
GO
/****** Object:  UserDefinedFunction [dbo].[get_SiglaGruppo_from_GUID]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_SiglaGruppo_from_GUID](@UID_gruppo uniqueidentifier)
RETURNS varchar(255)
AS 

BEGIN

	Declare @Nome_Gruppo varchar(255);
 
select @Nome_Gruppo =
(
	SELECT     gruppi_politici.codice_gruppo
)
FROM       gruppi_politici INNER  JOIN JOIN_GRUPPO_AD ON JOIN_GRUPPO_AD.id_gruppo = gruppi_politici.id_gruppo
WHERE 
JOIN_GRUPPO_AD.UID_Gruppo=@UID_gruppo
    RETURN @Nome_Gruppo;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_ProgressivoSUBEM]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_ProgressivoSUBEM](@UIDAtto uniqueidentifier, @id_gruppo int)
RETURNS int
AS
BEGIN

	Declare @progressivo int;
	set @progressivo = 1;
 
select @progressivo =
(
	SELECT COUNT(*)+1

FROM EM e
WHERE e.UIDAtto=@UIDAtto AND id_gruppo=@id_gruppo AND e.Progressivo is null
) 
    RETURN @progressivo;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_ProgressivoEM]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_ProgressivoEM](@UIDAtto uniqueidentifier, @id_gruppo int)
RETURNS int
AS
BEGIN

	Declare @progressivo int;
	set @progressivo = 1;
 
select @progressivo =
(
	SELECT COUNT(*)+1
FROM EM e
WHERE e.UIDAtto=@UIDAtto AND id_gruppo=@id_gruppo AND e.SubProgressivo is null
) 
    RETURN @progressivo;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_PIN]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[get_PIN](@Data datetime)
RETURNS varchar(255)
AS 

BEGIN

	Declare @PIN varchar(255);
 
select @PIN =
(
	SELECT     PIN
)
FROM       PINS
WHERE 
(
  (Dal <= @Data AND Al >= @Data) 
OR
  (Dal <= @Data AND al IS NULL)
)		   
    RETURN @PIN;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_NomeGruppo_from_GUID]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_NomeGruppo_from_GUID](@UID_gruppo uniqueidentifier)
RETURNS varchar(255)
AS 

BEGIN

	Declare @Nome_Gruppo varchar(255);
 
select @Nome_Gruppo =
(
	SELECT     gruppi_politici.nome_gruppo
)
FROM       gruppi_politici INNER  JOIN JOIN_GRUPPO_AD ON JOIN_GRUPPO_AD.id_gruppo = gruppi_politici.id_gruppo
WHERE 
JOIN_GRUPPO_AD.UID_Gruppo=@UID_gruppo
    RETURN @Nome_Gruppo;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_legislature_from_persona]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_legislature_from_persona](@id_persona int)
RETURNS varchar(1000)
AS 

BEGIN

	Declare @legislature varchar(1000);
 
select @legislature =
(
select LTRIM (STUFF((
               select distinct '-' + c.num_legislatura 
               from join_persona_organo_carica b
			   inner join legislature c
			   on b.id_legislatura = c.id_legislatura
               where id_persona = a.id_persona
        for xml path(''), type).value ('.' , 'varchar(max)' ), 1, 1, ''))
)
from persona a
where id_persona = @id_persona

		   
    RETURN '-' + @legislature + '-';
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_legislatura_attuale_from_persona]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_legislatura_attuale_from_persona](@id_persona int)
RETURNS bit
AS 

BEGIN

	Declare @n_incarichi_legislatura int;
	Declare @ris bit;
 
	select @n_incarichi_legislatura =
	(
		select COUNT(id_legislatura) 
	)
	from join_persona_organo_carica where id_legislatura in (select id_legislatura from legislature where attiva=1) and deleted=0
	and id_persona = @id_persona

	if @n_incarichi_legislatura >0 set @ris=1 ELSE set @ris=0
	
	RETURN @ris
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_legislatura_attuale]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_legislatura_attuale]()
RETURNS int
AS 

BEGIN

	Declare @idleg int;
 
	select @idleg =
	(
		SELECT id_legislatura from legislature where attiva=1
	) 
	RETURN @idleg
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_IDgruppoAttuale_from_persona]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_IDgruppoAttuale_from_persona](@UID_persona uniqueidentifier, @Giunta bit = 0)
RETURNS int
AS 

BEGIN

	Declare @id_Gruppo int;
 
if (@Giunta = 1)
  BEGIN
	select @id_Gruppo =
	(
		SELECT    id_gruppo
	)
	FROM   JOIN_GRUPPO_AD left join legislature on JOIN_GRUPPO_AD.id_legislatura = legislature.id_legislatura
	WHERE attiva = 1 AND GiuntaRegionale = 1
  END
IF (@Giunta = 0 or @Giunta is null)
  BEGIN
	select @id_Gruppo =
	(
		SELECT     join_persona_gruppi_politici.id_gruppo
	)
	FROM       (join_persona_gruppi_politici INNER JOIN join_persona_AD on join_persona_gruppi_politici.id_persona=join_persona_AD.id_persona)
	WHERE 
	UID_persona=@UID_persona AND    
	((join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine >= GETDATE()) 
	OR
	  (join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine IS NULL)
	)
  END
RETURN @id_gruppo;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_GUIDgruppoAttuale_from_persona]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_GUIDgruppoAttuale_from_persona](@UID_persona uniqueidentifier)
RETURNS uniqueidentifier
AS 

BEGIN

	Declare @UID_Gruppo uniqueidentifier;
 
select @UID_Gruppo =
(
	SELECT     JOIN_GRUPPO_AD.UID_Gruppo
)
FROM       (join_persona_gruppi_politici INNER JOIN join_persona_AD on join_persona_gruppi_politici.id_persona=join_persona_AD.id_persona) LEFT JOIN JOIN_GRUPPO_AD ON JOIN_GRUPPO_AD.id_gruppo = join_persona_gruppi_politici.id_gruppo
WHERE 
UID_persona=@UID_persona AND    
((join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine >= GETDATE()) 
OR
  (join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine IS NULL)
)		   
    RETURN @UID_Gruppo;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_GUIDCapogruppo_from_idGruppo]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_GUIDCapogruppo_from_idGruppo](@id_gruppo int)
RETURNS uniqueidentifier
AS 

BEGIN

	Declare @UID_persona uniqueidentifier;
 
select @UID_persona =
(
	SELECT   top(1) join_persona_AD.UID_persona
)
FROM            join_persona_gruppi_politici INNER JOIN
                         cariche ON join_persona_gruppi_politici.id_carica = cariche.id_carica INNER JOIN
                         join_persona_AD ON join_persona_gruppi_politici.id_persona = join_persona_AD.id_persona
WHERE        (cariche.presidente_gruppo = 1) AND (join_persona_gruppi_politici.id_gruppo = @id_gruppo) AND 
				((join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine >= GETDATE()) 
					OR
				(join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine IS NULL)
			 )	order by join_persona_gruppi_politici.data_inizio DESC	
			 	   
    RETURN @UID_persona;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[get_CodGruppo_from_ID]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_CodGruppo_from_ID] 
(
	-- Add the parameters for the function here
	@ID_gruppo int
)
RETURNS varchar(255)
AS
BEGIN

	Declare @Cod_Gruppo varchar(255);
 
select @Cod_Gruppo =
(
	SELECT     View_gruppi_politici_con_giunta.codice_gruppo
)
FROM       dbo.View_gruppi_politici_con_giunta
WHERE 
View_gruppi_politici_con_giunta.id_gruppo=@ID_gruppo
    RETURN @Cod_Gruppo;
END;
GO
/****** Object:  StoredProcedure [dbo].[DOWN_ATTO]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DOWN_ATTO]
   @UIDAtto uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @MaxPriorita int;
    DECLARE @Priorita int;
	DECLARE @UIDSeduta uniqueidentifier;
	DECLARE @Destinazione uniqueidentifier;
	
	select @UIDSeduta =
(
	SELECT ATTI.UIDSeduta
)
  FROM [ATTI]
	WHERE ATTI.UIDAtto=@UIDAtto

/*Seleziono e memorizzo l'attuale massimo ordine di priorità per gli ATTI*/
select @MaxPriorita =
(
	SELECT MAX(ATTI.Priorita)
)
  FROM [ATTI]
	WHERE ATTI.UIDSeduta=@UIDSeduta
	
/*Seleziono e memorizzo l'ordine di Priorità attuale dell'ATTO*/
select @Priorita =
(
	SELECT ATTI.Priorita
)
  FROM [ATTI]
	WHERE ATTI.UIDAtto=@UIDAtto
	
	if (@Priorita >= @MaxPriorita) return;

select @Destinazione =
(
	SELECT ATTI.UIDAtto
)
  FROM [ATTI]
	WHERE ATTI.Priorita=@Priorita + 1 AND ATTI.UIDSeduta = @UIDSeduta

/*Aggiorno l'ordine di votazione dell'ATTO sommando 1 a Priorita e quindi lo porto in basso di una posizione*/
UPDATE ATTI SET ATTI.Priorita=@Priorita+1 where ATTI.UIDAtto=@UIDAtto

/*Dopo l'aggiornamento avrò 2 ATTI con la stessa numerazione: 1 ATTO è quello che ho appena spostato mentre l'altro ATTO è quello che deve essere spostato in basso di una posizione*/
/*Quindi sposto l'altro ATTO in alto di una posizione sottraendo 1 a Ordine Votazione*/
UPDATE ATTI SET ATTI.Priorita=@Priorita where Atti.UIDAtto = @Destinazione	
	
END
GO
/****** Object:  StoredProcedure [dbo].[COPIA_ATTO]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[COPIA_ATTO]
   @UIDAtto uniqueidentifier,
   @UIDSeduta uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;


	DECLARE @newUIDAtto uniqueidentifier;

    SET @newUIDAtto = NewID();


	INSERT INTO ATTI([UIDAtto], [NAtto]
      ,[IDTipoAtto]
      ,[Oggetto]
      ,[Note]
      ,[Path_Testo_Atto]
      ,[UIDSeduta]
      ,[Data_apertura]
      ,[Data_chiusura]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[Priorita]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[Eliminato]) select @newUIDAtto, [NAtto]
      ,[IDTipoAtto]
      ,[Oggetto]
      ,[Note]
      ,[Path_Testo_Atto]
      ,@UIDSeduta
      ,[Data_apertura]
      ,[Data_chiusura]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[Priorita]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[Eliminato] FROM ATTI where UIDAtto=@UIDAtto;

	/* COPIO I RELATORI*/
	DECLARE @UIDPersona uniqueidentifier
    DECLARE cursor_relatori CURSOR FAST_FORWARD
	FOR SELECT ATTI_RELATORI.UIDPersona
		FROM   ATTI_RELATORI 
        WHERE  ATTI_RELATORI.UIDAtto = @UIDAtto

	OPEN cursor_relatori
	FETCH NEXT FROM cursor_relatori INTO @UIDPersona
	WHILE @@FETCH_STATUS = 0
	BEGIN
		/* Copio i relatori*/
		INSERT INTO ATTI_RELATORI(UIDAtto,UIDPersona) values (@newUIDAtto, @UIDPersona)

		FETCH NEXT FROM cursor_relatori INTO @UIDPersona
	END
	CLOSE cursor_relatori
	DEALLOCATE cursor_relatori


	/* COPIO GLI EM*/
	DECLARE @UIDEM uniqueidentifier
	DECLARE cursor_em CURSOR FAST_FORWARD
	FOR SELECT EM.UIDEM
		FROM   EM 
        WHERE  EM.UIDAtto = @UIDAtto

	OPEN cursor_em
	FETCH NEXT FROM cursor_em INTO @UIDEM
	WHILE @@FETCH_STATUS = 0
	BEGIN

	    DECLARE @newUIDEM uniqueidentifier;

		SET @newUIDEM = NewID();

		/* Copio le L'EM*/
		INSERT INTO EM([UIDEM]
      ,[Progressivo]
      ,[UIDAtto]
      ,[N_EM]
      ,[id_gruppo]
      ,[Rif_UIDEM]
      ,[N_SUBEM]
      ,[SubProgressivo]
      ,[UIDPersonaProponente]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[DataDeposito]
      ,[UIDPersonaPrimaFirma]
      ,[DataPrimaFirma]
      ,[UIDPersonaDeposito]
      ,[Proietta]
      ,[DataProietta]
      ,[UIDPersonaProietta]
      ,[DataRitiro]
      ,[UIDPersonaRitiro]
      ,[Hash]
      ,[IDTipo_EM]
      ,[IDParte]
      ,[NTitolo]
      ,[NCapo]
      ,[UIDArticolo]
      ,[UIDComma]
      ,[NLettera]
      ,[NNumero]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[TestoEM_originale]
      ,[EM_Certificato]
      ,[TestoREL_originale]
      ,[PATH_AllegatoGenerico]
      ,[PATH_AllegatoTecnico]
      ,[EffettiFinanziari]
      ,[NOTE_EM]
      ,[NOTE_Griglia]
      ,[IDStato]
      ,[TestoEM_Modificabile]
      ,[UID_QRCode]
      ,[AreaPolitica]
      ,[Eliminato]
      ,[UIDPersonaElimina]
      ,[DataElimina]
      ,[Timestamp]
      ,[Colore]
	  )select
	   @newUIDEM
      ,[Progressivo]
      ,@newUIDAtto
      ,[N_EM]
      ,[id_gruppo]
      ,[Rif_UIDEM]
      ,[N_SUBEM]
      ,[SubProgressivo]
      ,[UIDPersonaProponente]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[DataDeposito]
      ,[UIDPersonaPrimaFirma]
      ,[DataPrimaFirma]
      ,[UIDPersonaDeposito]
      ,[Proietta]
      ,[DataProietta]
      ,[UIDPersonaProietta]
      ,[DataRitiro]
      ,[UIDPersonaRitiro]
      ,[Hash]
      ,[IDTipo_EM]
      ,[IDParte]
      ,[NTitolo]
      ,[NCapo]
      ,[UIDArticolo]
      ,[UIDComma]
      ,[NLettera]
      ,[NNumero]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[TestoEM_originale]
      ,[EM_Certificato]
      ,[TestoREL_originale]
      ,[PATH_AllegatoGenerico]
      ,[PATH_AllegatoTecnico]
      ,[EffettiFinanziari]
      ,[NOTE_EM]
      ,[NOTE_Griglia]
      ,[IDStato]
      ,[TestoEM_Modificabile]
      ,[UID_QRCode]
      ,[AreaPolitica]
      ,[Eliminato]
      ,[UIDPersonaElimina]
      ,[DataElimina]
      ,[Timestamp]
      ,[Colore] from EM where UIDEM=@UIDEM;


		INSERT INTO FIRME([UIDEM], [UID_persona]
      ,[FirmaCert]
      ,[Data_firma]
      ,[Data_ritirofirma]
      ,[Timestamp]) select @newUIDEM, [UID_persona]
      ,[FirmaCert]
      ,[Data_firma]
      ,[Data_ritirofirma]
      ,[Timestamp] from FIRME where UIDEM=@UIDEM;

		FETCH NEXT FROM cursor_em INTO @UIDEM
	END

	CLOSE cursor_em
	DEALLOCATE cursor_em


	/* COPIO GLI ARTICOLI*/
	DECLARE @UIDArticolo uniqueidentifier
	DECLARE cursor_articoli CURSOR FAST_FORWARD
	FOR SELECT ARTICOLI.UIDArticolo
		FROM   ARTICOLI 
        WHERE  ARTICOLI.UIDAtto = @UIDAtto

	OPEN cursor_articoli
	FETCH NEXT FROM cursor_articoli INTO @UIDArticolo
	WHILE @@FETCH_STATUS = 0
	BEGIN

	    DECLARE @newUIDArticolo uniqueidentifier;

		SET @newUIDArticolo = NewID();

		/* Copio gli articoli*/
		INSERT INTO ARTICOLI(UIDArticolo, UIDAtto ,Articolo, Ordine) select @newUIDArticolo, UIDAtto, Articolo, Ordine from ARTICOLI where UIDArticolo=@UIDArticolo;

		/*FACCIO l'UPDATE SUI NUOVI EM CON L'ARTICOLO APPENA CREATO*/
		UPDATE EM set UIDArticolo=@newUIDArticolo where UIDArticolo=@UIDArticolo AND UIDAtto=@newUIDAtto;
		
		
						/*COPIO I COMMI*/
						DECLARE @UIDcomma uniqueidentifier
						DECLARE cursor_commi CURSOR FAST_FORWARD
						FOR SELECT COMMI.UIDcomma
							FROM   COMMI 
							WHERE  COMMI.UIDArticolo = @UIDArticolo

						OPEN cursor_commi
						FETCH NEXT FROM cursor_commi INTO @UIDcomma
						WHILE @@FETCH_STATUS = 0
						BEGIN
							DECLARE @newUIDComma  uniqueidentifier;
		
							SET @newUIDComma = NewID();

							INSERT INTO COMMI(UIDComma, UIDAtto, UIDArticolo, Comma, Ordine) select @newUIDComma, @newUIDAtto, @newUIDArticolo, Comma, Ordine from COMMI where UIDComma=@UIDComma;
		
							/*FACCIO l'UPDATE SUI NUOVI EM CON IL NUOVO COMMA APPENA CREATO*/
							UPDATE EM set UIDComma=@newUIDComma where UIDComma=@UIDComma AND UIDAtto=@newUIDAtto;
							FETCH NEXT FROM cursor_commi INTO @UIDcomma
						END

						CLOSE cursor_commi
						DEALLOCATE cursor_commi

		FETCH NEXT FROM cursor_articoli INTO @UIDArticolo
	END

	CLOSE cursor_articoli
	DEALLOCATE cursor_articoli

END
GO
/****** Object:  StoredProcedure [dbo].[UP_ATTO]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UP_ATTO]
   @UIDAtto uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;
    DECLARE @Priorita int;
	DECLARE @UIDSeduta uniqueidentifier;
	DECLARE @Destinazione uniqueidentifier;
	
	select @UIDSeduta =
(
	SELECT ATTI.UIDSeduta
)
  FROM [ATTI]
	WHERE ATTI.UIDAtto=@UIDAtto

/*Seleziono e memorizzo l'ordine di Priorità attuale dell'ATTO*/
select @Priorita =
(
	SELECT ATTI.Priorita
)
  FROM [ATTI]
	WHERE ATTI.UIDAtto=@UIDAtto
	
	if (@Priorita = 1) return;

select @Destinazione =
(
	SELECT ATTI.UIDAtto
)
  FROM [ATTI]
	WHERE ATTI.Priorita=@Priorita - 1 AND ATTI.UIDSeduta = @UIDSeduta

/*Aggiorno l'ordine di votazione dell'ATTO sommando 1 a Priorita e quindi lo porto in basso di una posizione*/
UPDATE ATTI SET ATTI.Priorita=@Priorita-1 where ATTI.UIDAtto=@UIDAtto

/*Dopo l'aggiornamento avrò 2 ATTI con la stessa numerazione: 1 ATTO è quello che ho appena spostato mentre l'altro ATTO è quello che deve essere spostato in basso di una posizione*/
/*Quindi sposto l'altro ATTO in alto di una posizione sottraendo 1 a Ordine Votazione*/
UPDATE ATTI SET ATTI.Priorita=@Priorita where Atti.UIDAtto = @Destinazione	
	
END
GO
/****** Object:  View [dbo].[View_CAPIGRUPPO]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_CAPIGRUPPO] AS
(SELECT     dbo.join_persona_AD.UID_persona, dbo.persona.id_persona AS id_persona, dbo.persona.cognome COLLATE database_default AS cognome, 
                      dbo.persona.nome COLLATE database_default AS nome, COALESCE (dbo.persona.foto, 
                      'noface.png') COLLATE database_default AS foto, dbo.join_persona_AD.UserAD COLLATE database_default AS userAD, 
                      dbo.get_legislature_from_persona(dbo.persona.id_persona) AS legislature, dbo.get_legislatura_attuale_from_persona(persona.id_persona) AS legislatura_attuale, 
                      dbo.get_NomeGruppo_from_GUID(dbo.get_GUIDgruppoAttuale_from_persona(dbo.join_persona_AD.UID_persona)) AS GruppoPolitico_attuale, NULL 
                      AS id_gruppo_politico_rif, cast(0 AS bit) AS notifica_firma, cast(0 AS bit) AS notifica_deposito, 0 AS No_Cons, dbo.join_persona_AD.pass_locale_crypt, dbo.join_persona_AD.gruppi_autorizzazione, dbo.consigliere_attivo(dbo.persona.id_persona) AS attivo, 
                      dbo.persona.deleted
FROM            join_persona_gruppi_politici INNER JOIN
                         cariche ON join_persona_gruppi_politici.id_carica = cariche.id_carica INNER JOIN
                         join_persona_AD ON join_persona_gruppi_politici.id_persona = join_persona_AD.id_persona INNER JOIN
                         persona ON persona.id_persona = join_persona_AD.id_persona
WHERE        (cariche.presidente_gruppo = 1) AND 
				((join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine >= GETDATE()) 
					OR
				(join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine IS NULL)
			 )
			 )
GO
/****** Object:  View [dbo].[View_CONSIGLIERE_GRUPPO]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_CONSIGLIERE_GRUPPO]
AS
SELECT DISTINCT P.id_persona, P.nome, P.cognome, GP.id_gruppo, GP.codice_gruppo, GP.nome_gruppo, GP.attivo, L.id_legislatura, L.num_legislatura
FROM         dbo.persona AS P INNER JOIN
                      dbo.join_persona_gruppi_politici AS P_GP ON P.id_persona = P_GP.id_persona INNER JOIN
                      dbo.gruppi_politici AS GP ON P_GP.id_gruppo = GP.id_gruppo INNER JOIN
                      dbo.legislature AS L ON P_GP.id_legislatura = L.id_legislatura
WHERE     (1 = 1) AND (L.id_legislatura = dbo.get_legislatura_attuale()) AND (GP.attivo = 1) AND (GP.deleted = 0) AND (P.deleted = 0) AND (P_GP.data_fine IS NULL)
GO
/****** Object:  View [dbo].[View_Commissioni_attive]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Commissioni_attive]
AS
SELECT TOP (100) PERCENT id_organo, nome_organo, nome_organo_breve, ordinamento
FROM dbo.organi
WHERE (id_legislatura = dbo.get_legislatura_attuale()) AND (data_inizio <= GETDATE()) AND (data_fine >= GETDATE()) AND (deleted = 0) AND (id_tipo_organo = 2) AND
(id_categoria_organo IN (2, 10)) OR
(id_legislatura = dbo.get_legislatura_attuale()) AND (data_inizio <= GETDATE()) AND (data_fine IS NULL) AND (deleted = 0) AND (id_tipo_organo = 2) AND
(id_categoria_organo IN (2, 10))
ORDER BY ordinamento
GO
/****** Object:  View [dbo].[View_UTENTI]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_UTENTI] AS
(SELECT     dbo.join_persona_AD.UID_persona, dbo.persona.id_persona AS id_persona, dbo.persona.cognome COLLATE database_default AS cognome, 
                      dbo.persona.nome COLLATE database_default AS nome, dbo.join_persona_recapiti.recapito COLLATE database_default AS email, COALESCE (dbo.persona.foto, 
                      'noface.png') COLLATE database_default AS foto, dbo.join_persona_AD.UserAD COLLATE database_default AS userAD, 
                      dbo.get_legislature_from_persona(dbo.persona.id_persona) AS legislature, dbo.get_legislatura_attuale_from_persona(persona.id_persona) AS legislatura_attuale, 
                      dbo.get_NomeGruppo_from_GUID(dbo.get_GUIDgruppoAttuale_from_persona(dbo.join_persona_AD.UID_persona)) AS GruppoPolitico_attuale, NULL 
                      AS id_gruppo_politico_rif, cast(0 AS bit) AS notifica_firma, cast(0 AS bit) AS notifica_deposito, 0 AS No_Cons, dbo.join_persona_AD.pass_locale_crypt, dbo.join_persona_AD.gruppi_autorizzazione, dbo.consigliere_attivo(dbo.persona.id_persona) AS attivo, 
                      dbo.persona.deleted
FROM         dbo.persona LEFT JOIN
                      dbo.join_persona_AD ON dbo.join_persona_AD.id_persona = dbo.persona.id_persona LEFT OUTER JOIN
                      dbo.join_persona_recapiti ON dbo.persona.id_persona = dbo.join_persona_recapiti.id_persona
WHERE     dbo.persona.deleted = 0)
UNION
(SELECT     dbo.UTENTI_NoCons.UID_persona, dbo.UTENTI_NoCons.id_persona, dbo.UTENTI_NoCons.cognome COLLATE database_default, 
                        dbo.UTENTI_NoCons.nome COLLATE database_default, dbo.UTENTI_NoCons.email COLLATE database_default, COALESCE (dbo.UTENTI_NoCons.foto, 
                        'noface.png') COLLATE database_default, dbo.UTENTI_NoCons.UserAD COLLATE database_default, NULL AS legislature, NULL AS legislatura_attuale, NULL 
                        AS GruppoPolitico_attuale, id_gruppo_politico_rif, notifica_firma, notifica_deposito, 1 AS No_Cons, dbo.UTENTI_NoCons.pass_locale_crypt, dbo.UTENTI_NoCons.gruppi_autorizzazione, attivo, dbo.UTENTI_NoCons.deleted
 FROM         dbo.UTENTI_NoCons)
GO
/****** Object:  View [dbo].[View_consiglieri_in_carica]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_consiglieri_in_carica]
AS
SELECT DISTINCT 
                      ut.cognome + ' ' + ut.nome + ' (' + COALESCE (dbo.get_NomeGruppo_from_GUID(dbo.get_GUIDgruppoAttuale_from_persona(p.UID_persona)), '--') + ')' AS DisplayName,
                       ut.UID_persona
FROM         dbo.join_persona_organo_carica AS jpoc INNER JOIN
                      dbo.persona ON jpoc.id_persona = dbo.persona.id_persona LEFT OUTER JOIN
                      dbo.join_persona_AD AS p ON jpoc.id_persona = p.id_persona LEFT OUTER JOIN
                      dbo.View_UTENTI AS ut ON p.UID_persona = ut.UID_persona
WHERE     (jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (ut.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                          (SELECT     id_organo
                            FROM          dbo.organi
                            WHERE      (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo <> 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND 
                      (jpoc.data_fine >= GETDATE()) OR
                      (jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                          (SELECT     id_organo
                            FROM          dbo.organi AS organi_1
                            WHERE      (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo <> 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND 
                      (jpoc.data_fine IS NULL)
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "jpoc"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 189
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "persona"
            Begin Extent = 
               Top = 6
               Left = 227
               Bottom = 114
               Right = 378
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "p"
            Begin Extent = 
               Top = 6
               Left = 416
               Bottom = 99
               Right = 567
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ut"
            Begin Extent = 
               Top = 6
               Left = 605
               Bottom = 114
               Right = 793
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_consiglieri_in_carica'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_consiglieri_in_carica'
GO
/****** Object:  View [dbo].[View_cariche_assessori_in_carica]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_cariche_assessori_in_carica]
AS
SELECT DISTINCT TOP (100) PERCENT dbo.cariche.id_carica, dbo.cariche.nome_carica, ut.cognome + ' ' + ut.nome AS DisplayName, ut.UID_persona, dbo.cariche.ordine
FROM dbo.join_persona_organo_carica AS jpoc INNER JOIN
dbo.persona ON jpoc.id_persona = dbo.persona.id_persona INNER JOIN
dbo.join_persona_AD AS p ON jpoc.id_persona = p.id_persona INNER JOIN
dbo.cariche ON jpoc.id_carica = dbo.cariche.id_carica LEFT OUTER JOIN
dbo.View_UTENTI AS ut ON p.UID_persona = ut.UID_persona
WHERE (jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (ut.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
(SELECT id_organo
FROM dbo.organi
WHERE (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo = 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND
(jpoc.data_fine >= GETDATE()) AND (NOT (dbo.cariche.id_tipo_carica IN (3, 10))) OR
(jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
(SELECT id_organo
FROM dbo.organi AS organi_1
WHERE (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo = 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND
(jpoc.data_fine IS NULL) AND (NOT (dbo.cariche.id_tipo_carica IN (3, 10)))
ORDER BY dbo.cariche.ordine
GO
/****** Object:  View [dbo].[View_assessori_in_carica]    Script Date: 10/18/2022 15:09:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_assessori_in_carica]
AS
SELECT DISTINCT ut.cognome + ' ' + ut.nome AS DisplayName, ut.UID_persona
FROM         dbo.join_persona_organo_carica AS jpoc INNER JOIN
                      dbo.persona ON jpoc.id_persona = dbo.persona.id_persona INNER JOIN
                      dbo.join_persona_AD AS p ON jpoc.id_persona = p.id_persona LEFT OUTER JOIN
                      dbo.View_UTENTI AS ut ON p.UID_persona = ut.UID_persona
WHERE     (jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (ut.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                          (SELECT     id_organo
                            FROM          dbo.organi
                            WHERE      (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo = 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND 
                      (jpoc.data_fine >= GETDATE()) OR
                      (jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                          (SELECT     id_organo
                            FROM          dbo.organi AS organi_1
                            WHERE      (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo = 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND 
                      (jpoc.data_fine IS NULL)
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "jpoc"
            Begin Extent = 
               Top = 247
               Left = 876
               Bottom = 355
               Right = 1027
            End
            DisplayFlags = 280
            TopColumn = 4
         End
         Begin Table = "persona"
            Begin Extent = 
               Top = 16
               Left = 624
               Bottom = 124
               Right = 775
            End
            DisplayFlags = 280
            TopColumn = 2
         End
         Begin Table = "p"
            Begin Extent = 
               Top = 179
               Left = 577
               Bottom = 277
               Right = 728
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ut"
            Begin Extent = 
               Top = 229
               Left = 234
               Bottom = 337
               Right = 422
            End
            DisplayFlags = 280
            TopColumn = 12
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 3060
         Width = 3585
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
E' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_assessori_in_carica'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'nd
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_assessori_in_carica'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_assessori_in_carica'
GO
/****** Object:  Default [DF_Table_1_UIDDestinatario]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[NOTIFICHE_DESTINATARI] ADD  CONSTRAINT [DF_Table_1_UIDDestinatario]  DEFAULT (newsequentialid()) FOR [UID]
GO
/****** Object:  Default [DF_NOTIFICHE_DESTINATARI_Visto]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[NOTIFICHE_DESTINATARI] ADD  CONSTRAINT [DF_NOTIFICHE_DESTINATARI_Visto]  DEFAULT ((0)) FOR [Visto]
GO
/****** Object:  Default [DF_NOTIFICHE_DESTINATARI_Chiuso]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[NOTIFICHE_DESTINATARI] ADD  CONSTRAINT [DF_NOTIFICHE_DESTINATARI_Chiuso]  DEFAULT ((0)) FOR [Chiuso]
GO
/****** Object:  Default [DF_NOTIFICHE_DataCreazione]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_DataCreazione]  DEFAULT (getdate()) FOR [DataCreazione]
GO
/****** Object:  Default [DF_NOTIFICHE_SyncGUID]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_SyncGUID]  DEFAULT (newsequentialid()) FOR [SyncGUID]
GO
/****** Object:  Default [DF_NOTIFICHE_Valida]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[NOTIFICHE] ADD  CONSTRAINT [DF_NOTIFICHE_Valida]  DEFAULT ((1)) FOR [Valida]
GO
/****** Object:  Default [DF_MISSIONI_UID_Missione]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[MISSIONI] ADD  CONSTRAINT [DF_MISSIONI_UID_Missione]  DEFAULT (newid()) FOR [UIDMissione]
GO
/****** Object:  Default [DF_LETTERE_UIDLettera]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[LETTERE] ADD  CONSTRAINT [DF_LETTERE_UIDLettera]  DEFAULT (newsequentialid()) FOR [UIDLettera]
GO
/****** Object:  Default [DF_LETTERE_Ordine]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[LETTERE] ADD  CONSTRAINT [DF_LETTERE_Ordine]  DEFAULT ((0)) FOR [Ordine]
GO
/****** Object:  Default [DF_join_persona_AD_UID_persona]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[join_persona_AD] ADD  CONSTRAINT [DF_join_persona_AD_UID_persona]  DEFAULT (newsequentialid()) FOR [UID_persona]
GO
/****** Object:  Default [DF_join_gruppo_AD_UID_Gruppo]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[JOIN_GRUPPO_AD] ADD  CONSTRAINT [DF_join_gruppo_AD_UID_Gruppo]  DEFAULT (newsequentialid()) FOR [UID_Gruppo]
GO
/****** Object:  Default [DF_join_gruppo_AD_GiuntaRegionale]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[JOIN_GRUPPO_AD] ADD  CONSTRAINT [DF_join_gruppo_AD_GiuntaRegionale]  DEFAULT ((0)) FOR [GiuntaRegionale]
GO
/****** Object:  Default [DF_JOIN_GRUPPO_AD_AbilitaEMPrivati_1]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[JOIN_GRUPPO_AD] ADD  CONSTRAINT [DF_JOIN_GRUPPO_AD_AbilitaEMPrivati_1]  DEFAULT ((0)) FOR [AbilitaEMPrivati]
GO
/****** Object:  Default [DF_gruppi_politici_TipoArea]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[gruppi_politici] ADD  CONSTRAINT [DF_gruppi_politici_TipoArea]  DEFAULT ((0)) FOR [TipoArea]
GO
/****** Object:  Default [DF_FIRME_TimeStamp]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[FIRME] ADD  CONSTRAINT [DF_FIRME_TimeStamp]  DEFAULT (getdate()) FOR [Timestamp]
GO
/****** Object:  Default [DF_FIRME_ufficio]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[FIRME] ADD  CONSTRAINT [DF_FIRME_ufficio]  DEFAULT ((0)) FOR [ufficio]
GO
/****** Object:  Default [DF_EM_UIDEM]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_UIDEM]  DEFAULT (newsequentialid()) FOR [UIDEM]
GO
/****** Object:  Default [DF_EM_Proietta]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_Proietta]  DEFAULT ((0)) FOR [Proietta]
GO
/****** Object:  Default [DF_EM_OrdinePresentazione]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_OrdinePresentazione]  DEFAULT ((1)) FOR [OrdinePresentazione]
GO
/****** Object:  Default [DF_EM_OrdineTrattazione]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_OrdineTrattazione]  DEFAULT ((1)) FOR [OrdineVotazione]
GO
/****** Object:  Default [DF_EM_Firma_su_invito]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_Firma_su_invito]  DEFAULT ((0)) FOR [Firma_su_invito]
GO
/****** Object:  Default [DF_EM_UID_QRCode]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_UID_QRCode]  DEFAULT (newid()) FOR [UID_QRCode]
GO
/****** Object:  Default [DF_EM_Eliminato]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[EM] ADD  CONSTRAINT [DF_EM_Eliminato]  DEFAULT ((0)) FOR [Eliminato]
GO
/****** Object:  Default [DF_STATI_EM_Ordinamento]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STATI_EM] ADD  CONSTRAINT [DF_STATI_EM_Ordinamento]  DEFAULT ((0)) FOR [Ordinamento]
GO
/****** Object:  Default [DF_STAMPE_INFO_Date]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE_INFO] ADD  CONSTRAINT [DF_STAMPE_INFO_Date]  DEFAULT (getdate()) FOR [Date]
GO
/****** Object:  Default [DF_STAMPE_UIDStampa]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_UIDStampa]  DEFAULT (newid()) FOR [UIDStampa]
GO
/****** Object:  Default [DF_STAMPE_DataRichiesta]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_DataRichiesta]  DEFAULT (getdate()) FOR [DataRichiesta]
GO
/****** Object:  Default [DF_STAMPE_Invio]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_Invio]  DEFAULT ((0)) FOR [Invio]
GO
/****** Object:  Default [DF_STAMPE_Lock]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_Lock]  DEFAULT ((0)) FOR [Lock]
GO
/****** Object:  Default [DF_STAMPE_Tentativi]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_Tentativi]  DEFAULT ((0)) FOR [Tentativi]
GO
/****** Object:  Default [DF_STAMPE_NotificaDepositoEM_1]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_NotificaDepositoEM_1]  DEFAULT ((0)) FOR [Notifica]
GO
/****** Object:  Default [DF_STAMPE_DASI]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[STAMPE] ADD  CONSTRAINT [DF_STAMPE_DASI]  DEFAULT ((0)) FOR [DASI]
GO
/****** Object:  Default [DF_Articoli_UIDArticolo]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ARTICOLI] ADD  CONSTRAINT [DF_Articoli_UIDArticolo]  DEFAULT (newsequentialid()) FOR [UIDArticolo]
GO
/****** Object:  Default [DF_Articoli_Ordine]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ARTICOLI] ADD  CONSTRAINT [DF_Articoli_Ordine]  DEFAULT ((1)) FOR [Ordine]
GO
/****** Object:  Default [DF_UTENTI_NoCons_UID_persona]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_UID_persona]  DEFAULT (newsequentialid()) FOR [UID_persona]
GO
/****** Object:  Default [DF_UTENTI_NoCons_notifica_firma]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_notifica_firma]  DEFAULT ((0)) FOR [notifica_firma]
GO
/****** Object:  Default [DF_UTENTI_NoCons_notifica_deposito]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_notifica_deposito]  DEFAULT ((0)) FOR [notifica_deposito]
GO
/****** Object:  Default [DF_UTENTI_NoCons_RichiediModificaPWD]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_RichiediModificaPWD]  DEFAULT ((1)) FOR [RichiediModificaPWD]
GO
/****** Object:  Default [DF_UTENTI_NoCons_attivo]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_attivo]  DEFAULT ((1)) FOR [attivo]
GO
/****** Object:  Default [DF_UTENTI_NoCons_deleted]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[UTENTI_NoCons] ADD  CONSTRAINT [DF_UTENTI_NoCons_deleted]  DEFAULT ((0)) FOR [deleted]
GO
/****** Object:  Default [DF_COMMI_UIDComma]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[COMMI] ADD  CONSTRAINT [DF_COMMI_UIDComma]  DEFAULT (newsequentialid()) FOR [UIDComma]
GO
/****** Object:  Default [DF_COMMI_Ordine]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[COMMI] ADD  CONSTRAINT [DF_COMMI_Ordine]  DEFAULT ((0)) FOR [Ordine]
GO
/****** Object:  Default [DF_ATTI_SOGGETTI_INTERROGATI_DataCreazione]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_SOGGETTI_INTERROGATI] ADD  CONSTRAINT [DF_ATTI_SOGGETTI_INTERROGATI_DataCreazione]  DEFAULT (getdate()) FOR [DataCreazione]
GO
/****** Object:  Default [DF_ATTI_RELATORI_sycReplica]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_RELATORI] ADD  CONSTRAINT [DF_ATTI_RELATORI_sycReplica]  DEFAULT (newsequentialid()) FOR [sycReplica]
GO
/****** Object:  Default [DF_ATTI_FIRME_ufficio]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_ufficio]  DEFAULT ((0)) FOR [ufficio]
GO
/****** Object:  Default [DF_ATTI_FIRME_PrimoFirmatario]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_PrimoFirmatario]  DEFAULT ((0)) FOR [PrimoFirmatario]
GO
/****** Object:  Default [DF_ATTI_FIRME_id_gruppo]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_id_gruppo]  DEFAULT ((0)) FOR [id_gruppo]
GO
/****** Object:  Default [DF_ATTI_FIRME_Valida]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_Valida]  DEFAULT ((1)) FOR [Valida]
GO
/****** Object:  Default [DF_ATTI_FIRME_Capogruppo]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_FIRME] ADD  CONSTRAINT [DF_ATTI_FIRME_Capogruppo]  DEFAULT ((0)) FOR [Capogruppo]
GO
/****** Object:  Default [DF_ATTI_DASI_TipoMOZ]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_TipoMOZ]  DEFAULT ((0)) FOR [TipoMOZ]
GO
/****** Object:  Default [DF_ATTI_DASI_NAtto_search]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_NAtto_search]  DEFAULT ((0)) FOR [NAtto_search]
GO
/****** Object:  Default [DF_ATTI_DASI_TipoRichiesta]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_TipoRichiesta]  DEFAULT ((0)) FOR [TipoRichiesta]
GO
/****** Object:  Default [DF_ATTI_DASI_TipoRichiestaDestinatario]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_TipoRichiestaDestinatario]  DEFAULT ((0)) FOR [TipoRichiestaDestinatario]
GO
/****** Object:  Default [DF_ATTI_DASI_IDStato_Motivazione]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_IDStato_Motivazione]  DEFAULT ((0)) FOR [IDStato_Motivazione]
GO
/****** Object:  Default [DF_ATTI_DASI_Non_Passaggio_In_Esame]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Non_Passaggio_In_Esame]  DEFAULT ((0)) FOR [Non_Passaggio_In_Esame]
GO
/****** Object:  Default [DF_ATTI_DASI_Inviato_Al_Protocollo]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Inviato_Al_Protocollo]  DEFAULT ((0)) FOR [Inviato_Al_Protocollo]
GO
/****** Object:  Default [DF_ATTI_COMMISSIONI_DataCreazione]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[ATTI_COMMISSIONI] ADD  CONSTRAINT [DF_ATTI_COMMISSIONI_DataCreazione]  DEFAULT (getdate()) FOR [DataCreazione]
GO
/****** Object:  Default [DF_SEDUTE_UIDSeduta]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_UIDSeduta]  DEFAULT (newsequentialid()) FOR [UIDSeduta]
GO
/****** Object:  Default [DF_SEDUTE_IDOrgano]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_IDOrgano]  DEFAULT ((1)) FOR [IDOrgano]
GO
/****** Object:  Default [DF_SEDUTE_Riservato_DASI]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[SEDUTE] ADD  CONSTRAINT [DF_SEDUTE_Riservato_DASI]  DEFAULT ((0)) FOR [Riservato_DASI]
GO
/****** Object:  Default [DF_RUOLI_Ruolo_di_Giunta]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[RUOLI] ADD  CONSTRAINT [DF_RUOLI_Ruolo_di_Giunta]  DEFAULT ((0)) FOR [Ruolo_di_Giunta]
GO
/****** Object:  Default [DF_PINS_NoCons_UIDPIN]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS_NoCons] ADD  CONSTRAINT [DF_PINS_NoCons_UIDPIN]  DEFAULT (newsequentialid()) FOR [UIDPIN]
GO
/****** Object:  Default [DF_PINS_NoCons_Dal]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS_NoCons] ADD  CONSTRAINT [DF_PINS_NoCons_Dal]  DEFAULT (getdate()) FOR [Dal]
GO
/****** Object:  Default [DF_PINS_FIRMA_e_DEPOSITO_]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS_NoCons] ADD  CONSTRAINT [DF_PINS_FIRMA_e_DEPOSITO_]  DEFAULT ((0)) FOR [FIRMA_e_DEPOSITO]
GO
/****** Object:  Default [DF_PINS_NoCons_RichiediModificaPIN]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS_NoCons] ADD  CONSTRAINT [DF_PINS_NoCons_RichiediModificaPIN]  DEFAULT ((0)) FOR [RichiediModificaPIN]
GO
/****** Object:  Default [DF_PINS_UIDPIN]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS] ADD  CONSTRAINT [DF_PINS_UIDPIN]  DEFAULT (newsequentialid()) FOR [UIDPIN]
GO
/****** Object:  Default [DF_PINS_Dal]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS] ADD  CONSTRAINT [DF_PINS_Dal]  DEFAULT (getdate()) FOR [Dal]
GO
/****** Object:  Default [DF_PINS_FIRMA_e_DEPOSITO]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS] ADD  CONSTRAINT [DF_PINS_FIRMA_e_DEPOSITO]  DEFAULT ((1)) FOR [FIRMA_e_DEPOSITO]
GO
/****** Object:  Default [DF_PINS_RichiediModificaPIN]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PINS] ADD  CONSTRAINT [DF_PINS_RichiediModificaPIN]  DEFAULT ((0)) FOR [RichiediModificaPIN]
GO
/****** Object:  Default [DF_PARTI_TESTO_Ordine_1]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PARTI_TESTO] ADD  CONSTRAINT [DF_PARTI_TESTO_Ordine_1]  DEFAULT ((0)) FOR [Ordine]
GO
/****** Object:  Default [DF_PARTE_LR_UIDParte_LR]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PARTE_LR] ADD  CONSTRAINT [DF_PARTE_LR_UIDParte_LR]  DEFAULT (newsequentialid()) FOR [UIDParte_LR]
GO
/****** Object:  Default [DF_PARTE_LR_Ordine]    Script Date: 10/18/2022 15:09:09 ******/
ALTER TABLE [dbo].[PARTE_LR] ADD  CONSTRAINT [DF_PARTE_LR_Ordine]  DEFAULT ((0)) FOR [Ordine]
GO
/****** Object:  Default [DF_ATTI_UIDAtto]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_UIDAtto]  DEFAULT (newsequentialid()) FOR [UIDAtto]
GO
/****** Object:  Default [DF_ATTI_IDTipoAtto]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_IDTipoAtto]  DEFAULT ((1)) FOR [IDTipoAtto]
GO
/****** Object:  Default [DF_ATTI_VIS_Mis_Prog_1]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_VIS_Mis_Prog_1]  DEFAULT ((0)) FOR [VIS_Mis_Prog]
GO
/****** Object:  Default [DF_ATTI_Notifica_deposito_differita]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Notifica_deposito_differita]  DEFAULT ((1)) FOR [Notifica_deposito_differita]
GO
/****** Object:  Default [DF_ATTI_BloccoODG]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_BloccoODG]  DEFAULT ((0)) FOR [BloccoODG]
GO
/****** Object:  Default [DF_ATTI_Jolly]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Jolly]  DEFAULT ((0)) FOR [Jolly]
GO
/****** Object:  Default [DF_ATTI_Emendabile]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Emendabile]  DEFAULT ((0)) FOR [Emendabile]
GO
/****** Object:  Default [DF_ATTI_Fascicoli_Da_Aggiornare]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI] ADD  CONSTRAINT [DF_ATTI_Fascicoli_Da_Aggiornare]  DEFAULT ((0)) FOR [Fascicoli_Da_Aggiornare]
GO
/****** Object:  ForeignKey [FK_ATTI_SEDUTE]    Script Date: 10/18/2022 15:09:10 ******/
ALTER TABLE [dbo].[ATTI]  WITH NOCHECK ADD  CONSTRAINT [FK_ATTI_SEDUTE] FOREIGN KEY([UIDSeduta])
REFERENCES [dbo].[SEDUTE] ([UIDSeduta])
NOT FOR REPLICATION
GO
ALTER TABLE [dbo].[ATTI] CHECK CONSTRAINT [FK_ATTI_SEDUTE]
GO

