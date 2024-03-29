USE [dbEmendamenti]
GO
/****** Object:  StoredProcedure [dbo].[ORDINA_EM_TRATTAZIONE]    Script Date: 13/05/2022 10:34:46 ******/
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
