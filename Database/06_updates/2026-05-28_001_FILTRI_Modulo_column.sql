/*
 * 2026-05-28 - 001 - FILTRI: nuova colonna Modulo per discriminare PEM/DASI
 *
 * La tabella FILTRI (filtri preferiti utente) era usata finora solo dal
 * modulo DASI. Con l'introduzione dei filtri preferiti anche per PEM
 * (richiesta utente, iterazione v2026.5.1) viene aggiunta una colonna
 * "Modulo" che discrimina il modulo applicativo a cui appartiene il
 * record. I valori riflettono l'enum ModuloEnum:
 *
 *     1 = PEM
 *     2 = DASI
 *
 * Il DEFAULT (2 = DASI) garantisce il backfill implicito dei record
 * esistenti, che sono tutti DASI per costruzione storica.
 *
 * Lo script e' idempotente: controlla l'esistenza della colonna prima
 * di aggiungerla.
 */

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH('dbo.FILTRI', 'Modulo') IS NULL
BEGIN
    ALTER TABLE [dbo].[FILTRI]
        ADD [Modulo] [tinyint] NOT NULL
        CONSTRAINT [DF_FILTRI_Modulo] DEFAULT ((2));
END
GO
