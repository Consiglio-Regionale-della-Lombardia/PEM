/*
 * 2026-11-02 - 001 - ATTI_DASI: data di presentazione delle bozze - issue #1712
 *
 * Dal 2022 al 2024 l'entita' ATTI_DASI dava a Timestamp (la data di
 * presentazione) il valore iniziale 01/01/1970: ogni bozza nasceva con quella
 * data e solo il deposito la sostituiva con quella vera. Le bozze mai
 * presentate di quel periodo la conservano ancora, e la griglia di segreteria
 * la mostrava come /Date(-3600000)/.
 *
 * Lo script riporta a NULL la data di queste bozze, eliminate comprese, come
 * per tutte le bozze create dopo. Tocca solo dati: puo' girare prima o dopo
 * il deploy, e rieseguito non trova piu' righe.
 *
 * Il trigger TrigATTI_DASI copia in ATTI_DASI_Audit ogni bozza corretta con
 * la data di prima.
 */

SET NOCOUNT ON;

UPDATE [dbo].[ATTI_DASI]
SET [Timestamp] = NULL
WHERE [Timestamp] = '19700101'
  AND [IDStato] IN (1, 2); -- BOZZA_RISERVATA, BOZZA

PRINT CONCAT('Bozze corrette: ', @@ROWCOUNT);
GO
