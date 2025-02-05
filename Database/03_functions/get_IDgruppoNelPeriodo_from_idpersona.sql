
/****** Object:  UserDefinedFunction [dbo].[get_IDgruppoNelPeriodo_from_idpersona]    Script Date: 01/31/2025 12:43:21 ******/
SET ANSI_NULLS ON
GO
 
SET QUOTED_IDENTIFIER ON
GO
 
 
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<**Restituisce l'id del gruppo politico a cui apparteneva il firmatario il giorno in cui ha apposto la firma su un atto. Il parametro Giunta indica che il firmatario è un Assessore/Sottosegretario (che non firmano mai atti DASI)**>
-- =============================================
CREATE FUNCTION [dbo].[get_IDgruppoNelPeriodo_from_idpersona](@id_persona int, @giorno as date, @Giunta bit = 0)
RETURNS int
AS
 
BEGIN
 
	Declare @id_Gruppo int;
if (@Giunta = 1)
  BEGIN
	select @id_Gruppo =
	(
		SELECT    id_gruppo
	)
	FROM   JOIN_GRUPPO_AD left join legislature on JOIN_GRUPPO_AD.id_legislatura = legislature.id_legislatura
	WHERE attiva = 1 AND GiuntaRegionale = 1
  END
IF (@Giunta = 0 or @Giunta is null)
  BEGIN
	select @id_Gruppo =
	(
		SELECT     join_persona_gruppi_politici.id_gruppo
	)
	FROM       join_persona_gruppi_politici
	WHERE 
	id_persona=@id_persona AND    
	((join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= @giorno AND join_persona_gruppi_politici.data_fine >= @giorno) 
	OR
	  (join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= @giorno AND join_persona_gruppi_politici.data_fine IS NULL)
	)
  END
RETURN @id_gruppo;
END;
