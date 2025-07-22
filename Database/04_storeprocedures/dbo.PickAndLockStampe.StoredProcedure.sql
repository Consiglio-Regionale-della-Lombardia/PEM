USE [dbGEDASI_PROD]
GO

/****** Object:  StoredProcedure [dbo].[PickAndLockStampe]    Script Date: 19/07/2025 11:35:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PickAndLockStampe]
    @Numero INT,
    @MaxTentativi INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Controlla se esistono già stampe in lavorazione
    IF EXISTS (
        SELECT 1 FROM STAMPE
        WHERE [Lock] = 1
          AND DataFineEsecuzione IS NULL
    )
    BEGIN
        -- Restituisci una tabella vuota (ma con le stesse colonne)
        SELECT 
            CAST(NULL AS INT) AS UIDStampa,
            CAST(NULL AS INT) AS UIDAtto,
            CAST(NULL AS VARCHAR(255)) AS [Da],
            CAST(NULL AS VARCHAR(255)) AS [A],
            CAST(NULL AS INT) AS UIDUtenteRichiesta,
            CAST(NULL AS DATETIME) AS DataRichiesta,
            CAST(NULL AS BIT) AS Invio,
            CAST(NULL AS DATETIME) AS DataInvio,
            CAST(NULL AS VARCHAR(500)) AS MessaggioErrore,
            CAST(NULL AS BIT) AS [Lock],
            CAST(NULL AS DATETIME) AS DataLock,
            CAST(NULL AS VARCHAR(255)) AS PathFile,
            CAST(NULL AS DATETIME) AS DataInizioEsecuzione,
            CAST(NULL AS DATETIME) AS DataFineEsecuzione,
            CAST(NULL AS INT) AS Tentativi,
            CAST(NULL AS INT) AS CurrentRole,
            CAST(NULL AS DATETIME) AS Scadenza,
            CAST(NULL AS INT) AS Ordine,
            CAST(NULL AS VARCHAR(MAX)) AS Query,
            CAST(NULL AS BIT) AS Notifica,
            CAST(NULL AS INT) AS UIDEM,
            CAST(NULL AS VARCHAR(50)) AS DASI,
            CAST(NULL AS INT) AS UIDFascicolo,
            CAST(NULL AS VARCHAR(100)) AS NumeroFascicolo
        WHERE 1 = 0;
        RETURN;
    END

    -- Seleziona le prime @Numero stampe sbloccate e sotto il numero massimo di tentativi
    ;WITH CTE AS (
        SELECT TOP (1) *
        FROM STAMPE WITH (ROWLOCK, READPAST, UPDLOCK)
        WHERE [Lock] = 0
          AND Tentativi < @MaxTentativi
          AND DataFineEsecuzione IS NULL
        ORDER BY CurrentRole DESC, DataRichiesta ASC
    )
    UPDATE CTE
    SET
        [Lock] = 1,
        DataLock = GETDATE(),
        DataInizioEsecuzione = GETDATE(),
        Tentativi = Tentativi + 1
    OUTPUT 
        inserted.UIDStampa,
        inserted.UIDAtto,
        inserted.[Da],
        inserted.[A],
        inserted.UIDUtenteRichiesta,
        inserted.DataRichiesta,
        inserted.Invio,
        inserted.DataInvio,
        inserted.MessaggioErrore,
        inserted.[Lock],
        inserted.DataLock,
        inserted.PathFile,
        inserted.DataInizioEsecuzione,
        inserted.DataFineEsecuzione,
        inserted.Tentativi,
        inserted.CurrentRole,
        inserted.Scadenza,
        inserted.Ordine,
        inserted.Query,
        inserted.Notifica,
        inserted.UIDEM,
        inserted.DASI,
        inserted.UIDFascicolo,
        inserted.NumeroFascicolo;
    
    -- Se non è stato aggiornato nulla, restituisci tabella vuota con le stesse colonne
    IF @@ROWCOUNT = 0
    BEGIN
        SELECT 
            CAST(NULL AS INT) AS UIDStampa,
            CAST(NULL AS INT) AS UIDAtto,
            CAST(NULL AS VARCHAR(255)) AS [Da],
            CAST(NULL AS VARCHAR(255)) AS [A],
            CAST(NULL AS INT) AS UIDUtenteRichiesta,
            CAST(NULL AS DATETIME) AS DataRichiesta,
            CAST(NULL AS BIT) AS Invio,
            CAST(NULL AS DATETIME) AS DataInvio,
            CAST(NULL AS VARCHAR(500)) AS MessaggioErrore,
            CAST(NULL AS BIT) AS [Lock],
            CAST(NULL AS DATETIME) AS DataLock,
            CAST(NULL AS VARCHAR(255)) AS PathFile,
            CAST(NULL AS DATETIME) AS DataInizioEsecuzione,
            CAST(NULL AS DATETIME) AS DataFineEsecuzione,
            CAST(NULL AS INT) AS Tentativi,
            CAST(NULL AS INT) AS CurrentRole,
            CAST(NULL AS DATETIME) AS Scadenza,
            CAST(NULL AS INT) AS Ordine,
            CAST(NULL AS VARCHAR(MAX)) AS Query,
            CAST(NULL AS BIT) AS Notifica,
            CAST(NULL AS INT) AS UIDEM,
            CAST(NULL AS VARCHAR(50)) AS DASI,
            CAST(NULL AS INT) AS UIDFascicolo,
            CAST(NULL AS VARCHAR(100)) AS NumeroFascicolo
        WHERE 1 = 0;
    END
END
GO

