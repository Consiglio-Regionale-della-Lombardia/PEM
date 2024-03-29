USE [dbEmendamenti]
GO
/****** Object:  StoredProcedure [dbo].[SPOSTA_EM_TRATTAZIONE]    Script Date: 13/05/2022 10:34:46 ******/
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
