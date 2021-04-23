USE [dbEmendamenti_test]
GO

/****** Object:  StoredProcedure [dbo].[UP_ATTO]    Script Date: 04/20/2021 14:32:27 ******/
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

/*Seleziono e memorizzo l'UID della seduta a cui appartiene l'ATTO*/
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

if (@Priorita <= 1) return;

/*Aggiorno l'ordine di votazione dell'ATTO sottraendo 1 a Priorita e quindi lo porto in alto di una posizione*/
UPDATE ATTI SET ATTI.Priorita=@Priorita-1 where ATTI.UIDAtto=@UIDAtto

/*Dopo l'aggiornamento avrò 2 ATTI con la stessa numerazione: 1 ATTO è quello che ho appena spostato mentre l'altro ATTO è quello che deve essere spostato in basso di una posizione*/
/*Quindi sposto l'altro ATTO in basso di una posizione sommando 1 a Priorita*/

UPDATE ATTI SET ATTI.Priorita=@Priorita where (ATTI.Priorita=@Priorita-1 AND ATTI.UIDAtto<>@UIDAtto) AND ATTI.UIDSeduta=@UIDSeduta	
	
END


GO

