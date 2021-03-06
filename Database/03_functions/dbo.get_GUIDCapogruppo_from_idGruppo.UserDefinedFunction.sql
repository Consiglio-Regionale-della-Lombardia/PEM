USE [dbEmendamenti]
GO
/****** Object:  UserDefinedFunction [dbo].[get_GUIDCapogruppo_from_idGruppo]    Script Date: 12/09/2020 18:38:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_GUIDCapogruppo_from_idGruppo](@id_gruppo int)
RETURNS uniqueidentifier
AS 

BEGIN

	Declare @UID_persona uniqueidentifier;
 
select @UID_persona =
(
	SELECT   top(1) join_persona_AD.UID_persona
)
FROM            join_persona_gruppi_politici INNER JOIN
                         cariche ON join_persona_gruppi_politici.id_carica = cariche.id_carica INNER JOIN
                         join_persona_AD ON join_persona_gruppi_politici.id_persona = join_persona_AD.id_persona
WHERE        (cariche.presidente_gruppo = 1) AND (join_persona_gruppi_politici.id_gruppo = @id_gruppo) AND 
				((join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine >= GETDATE()) 
					OR
				(join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine IS NULL)
			 )	order by join_persona_gruppi_politici.data_inizio DESC	
			 	   
    RETURN @UID_persona;
END;
GO
