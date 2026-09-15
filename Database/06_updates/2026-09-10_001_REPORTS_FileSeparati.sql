/*
 * 2026-09-10 - 001 - REPORTS: colonna FileSeparati - issue #1688
 *
 * Il flag "Genera un file separato per ogni atto (ZIP)" introdotto con la #1623
 * era collegato solo alla generazione al volo del report: non veniva scritto tra
 * i parametri del report salvato, quindi al richiamo non si ripresentava.
 *
 * Lo script aggiunge [FileSeparati] BIT NOT NULL DEFAULT 0 alla tabella REPORTS.
 * I report gia' salvati restano quindi sul comportamento storico (documento unico).
 *
 * Solo ADD COLUMN, nessuna colonna esistente viene toccata. Lo script e'
 * idempotente: puo' essere rieseguito senza effetti collaterali.
 */

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH('dbo.REPORTS', 'FileSeparati') IS NULL
BEGIN
    ALTER TABLE [dbo].[REPORTS]
        ADD [FileSeparati] [bit] NOT NULL
        CONSTRAINT [DF_REPORTS_FileSeparati] DEFAULT ((0));
END
GO
