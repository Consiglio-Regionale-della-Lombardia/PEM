/*
 * 2026-05-27 - 001 - ATTI_DASI: nuove colonne per integrazione EDMA
 *
 * Aggiunge alla tabella ATTI_DASI le colonne necessarie a tracciare la
 * protocollazione su EDMA. Solo ADD COLUMN: nessuna colonna esistente
 * viene rimossa o modificata.
 *
 *   EDMA_IdPratica            id EDMA del FascicoloPratica creato
 *   EDMA_NumeroPratica        numero pratica restituito da EDMA (per UI)
 *   EDMA_IdDocumento          id EDMA del DocumentoFile (PDF principale)
 *   EDMA_IdAllegatoGenerico   id EDMA del DocumentoFile figlio (allegato)
 *   EDMA_IdProtocollo         id della scheda di protocollo (modulo 9000)
 *   EDMA_Segnatura            segnatura formattata AOO.AAAA.0000000
 *   EDMA_TentativiInvio       contatore tentativi del job di retry
 *   EDMA_UltimoErrore         testo dell'ultimo errore EDMA
 *   EDMA_DataUltimoTentativo  timestamp dell'ultimo tentativo
 *
 * Le tre colonne pre-esistenti Inviato_Al_Protocollo, DataInvioAlProtocollo
 * e Protocollo restano invariate e continuano a essere usate dal flusso.
 *
 * Lo script e' idempotente: controlla l'esistenza delle colonne prima di
 * aggiungerle, quindi puo' essere rieseguito senza effetti collaterali.
 */

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_IdPratica') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_IdPratica] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_NumeroPratica') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_NumeroPratica] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_IdDocumento') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_IdDocumento] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_IdAllegatoGenerico') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_IdAllegatoGenerico] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_IdProtocollo') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_IdProtocollo] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_Segnatura') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_Segnatura] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_TentativiInvio') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI]
        ADD [EDMA_TentativiInvio] [int] NOT NULL
        CONSTRAINT [DF_ATTI_DASI_EDMA_TentativiInvio] DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_UltimoErrore') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_UltimoErrore] [nvarchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.ATTI_DASI', 'EDMA_DataUltimoTentativo') IS NULL
BEGIN
    ALTER TABLE [dbo].[ATTI_DASI] ADD [EDMA_DataUltimoTentativo] [datetime] NULL;
END
GO
