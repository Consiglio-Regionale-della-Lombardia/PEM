USE [dbEmendamenti_beta]
GO

/****** Object:  StoredProcedure [dbo].[DOWN_ATTO]    Script Date: 04/20/2021 14:32:00 ******/
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

