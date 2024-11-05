USE [dbEmendamenti_test]
GO

/****** Object:  Table [dbo].[ATTI_DASI]    Script Date: 05/11/2024 10:08:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
	[Oggetto_Privacy] [varchar](max) NULL,
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
	[Firma_su_invito] [bit] NOT NULL,
	[UID_QRCode] [uniqueidentifier] NOT NULL,
	[AreaPolitica] [int] NOT NULL,
	[id_gruppo] [int] NOT NULL,
	[Eliminato] [bit] NOT NULL,
	[UIDPersonaElimina] [uniqueidentifier] NULL,
	[DataElimina] [datetime] NULL,
	[chkf] [varchar](255) NULL,
	[Timestamp] [datetime] NULL,
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
	[CapogruppoNeiTermini] [bit] NOT NULL,
	[MOZU_Capigruppo] [bit] NOT NULL,
	[FirmeCartacee] [varchar](max) NULL,
	[Pubblicato] [bit] NOT NULL,
	[Sollecito] [bit] NOT NULL,
	[IDTipo_Risposta_Effettiva] [int] NULL,
	[Protocollo] [varchar](150) NULL,
	[CodiceMateria] [varchar](150) NULL,
	[DataAnnunzio] [datetime] NULL,
	[TipoChiusuraIter] [int] NULL,
	[DataChiusuraIter] [datetime] NULL,
	[NoteChiusuraIter] [varchar](max) NULL,
	[Emendato] [bit] NOT NULL,
	[TipoVotazioneIter] [int] NULL,
	[AreaTematica] [varchar](max) NULL,
	[AltriSoggetti] [varchar](max) NULL,
	[DCR] [int] NULL,
	[DCCR] [int] NULL,
	[DCRL] [varchar](50) NULL,
	[BURL] [varchar](50) NULL,
	[Privacy_Dati_Personali_Giudiziari] [bit] NOT NULL,
	[Privacy_Divieto_Pubblicazione_Salute] [bit] NOT NULL,
	[Privacy_Divieto_Pubblicazione_Vita_Sessuale] [bit] NOT NULL,
	[Privacy_Divieto_Pubblicazione] [bit] NOT NULL,
	[Privacy_Dati_Personali_Sensibili] [bit] NOT NULL,
	[Privacy_Divieto_Pubblicazione_Altri] [bit] NOT NULL,
	[Privacy_Dati_Personali_Semplici] [bit] NOT NULL,
	[Privacy] [bit] NOT NULL,
	[DataComunicazioneAssemblea] [datetime] NULL,
	[ImpegniScadenze] [varchar](max) NULL,
	[StatoAttuazione] [varchar](max) NULL,
	[CompetenzaMonitoraggio] [varchar](max) NULL,
	[MonitoraggioConcluso] [bit] NOT NULL,
	[DataTrasmissioneMonitoraggio] [datetime] NULL,
	[IterMultiplo] [bit] NOT NULL,
	[UIDPersonaRelatore1] [uniqueidentifier] NULL,
	[UIDPersonaRelatore2] [uniqueidentifier] NULL,
	[UIDPersonaRelatoreMinoranza] [uniqueidentifier] NULL,
 CONSTRAINT [PK_ATTI_DASI] PRIMARY KEY CLUSTERED 
(
	[UIDAtto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_TipoMOZ]  DEFAULT ((0)) FOR [TipoMOZ]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_NAtto_search]  DEFAULT ((0)) FOR [NAtto_search]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_TipoRichiesta]  DEFAULT ((0)) FOR [TipoRichiesta]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_TipoRichiestaDestinatario]  DEFAULT ((0)) FOR [TipoRichiestaDestinatario]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Proietta]  DEFAULT ((0)) FOR [Proietta]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Firma_su_invito]  DEFAULT ((0)) FOR [Firma_su_invito]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Eliminato]  DEFAULT ((0)) FOR [Eliminato]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Non_Passaggio_In_Esame]  DEFAULT ((0)) FOR [Non_Passaggio_In_Esame]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Inviato_Al_Protocollo]  DEFAULT ((0)) FOR [Inviato_Al_Protocollo]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_CapogruppoNeiTermini]  DEFAULT ((0)) FOR [CapogruppoNeiTermini]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_MOZU_Capigruppo]  DEFAULT ((0)) FOR [MOZU_Capigruppo]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Pubblicato]  DEFAULT ((0)) FOR [Pubblicato]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Sollecito]  DEFAULT ((0)) FOR [Sollecito]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Emendato]  DEFAULT ((0)) FOR [Emendato]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy_Dati_Personali_Giudiziari]  DEFAULT ((0)) FOR [Privacy_Dati_Personali_Giudiziari]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy_Divieto_Pubblicazione_Salute]  DEFAULT ((0)) FOR [Privacy_Divieto_Pubblicazione_Salute]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy_Divieto_Pubblicazione_Vita_Sessuale]  DEFAULT ((0)) FOR [Privacy_Divieto_Pubblicazione_Vita_Sessuale]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy_Divieto_Pubblicazione]  DEFAULT ((0)) FOR [Privacy_Divieto_Pubblicazione]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy_Dati_Personali_Sensibili]  DEFAULT ((0)) FOR [Privacy_Dati_Personali_Sensibili]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy_Divieto_Pubblicazione_Altri]  DEFAULT ((0)) FOR [Privacy_Divieto_Pubblicazione_Altri]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy_Dati_Personali_Semplici]  DEFAULT ((0)) FOR [Privacy_Dati_Personali_Semplici]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_Privacy]  DEFAULT ((0)) FOR [Privacy]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_MonitoraggioConcluso]  DEFAULT ((0)) FOR [MonitoraggioConcluso]
GO

ALTER TABLE [dbo].[ATTI_DASI] ADD  CONSTRAINT [DF_ATTI_DASI_IterMultiplo]  DEFAULT ((0)) FOR [IterMultiplo]
GO

