USE [dbEmendamenti]
GO
/****** Object:  UserDefinedFunction [dbo].[get_CaricaGiunta_from_UIDpersona]    Script Date: 12/09/2020 18:38:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_CaricaGiunta_from_UIDpersona](@UID_persona uniqueidentifier)
RETURNS varchar(1000)
AS 

BEGIN

	Declare @Nome_CaricaGiunta varchar(1000);
	Declare @Nomi_CaricaGiunta varchar(1000);
 
DECLARE getemp_curs CURSOR FOR 
(
SELECT     cariche.nome_carica

FROM         cariche INNER JOIN
                      join_persona_organo_carica ON cariche.id_carica = join_persona_organo_carica.id_carica INNER JOIN
                      organi ON join_persona_organo_carica.id_organo = organi.id_organo INNER JOIN
                      join_persona_AD ON join_persona_organo_carica.id_persona = join_persona_AD.id_persona 
WHERE		
			UID_persona=@UID_persona AND
			organi.id_tipo_organo=4 AND
			organi.deleted = 0 AND
			join_persona_organo_carica.deleted = 0 AND
			not (nome_carica like '%non consigliere%') AND
			(
			  (join_persona_organo_carica.data_inizio <= GETDATE() AND join_persona_organo_carica.data_fine >= GETDATE()) OR (join_persona_organo_carica.data_inizio <= GETDATE() AND join_persona_organo_carica.data_fine IS NULL)
			)
)
ORDER BY cariche.ordine

OPEN getemp_curs
FETCH NEXT FROM getemp_curs into @Nome_CaricaGiunta
SET @Nomi_CaricaGiunta = @Nome_CaricaGiunta
FETCH NEXT FROM getemp_curs into @Nome_CaricaGiunta

WHILE @@FETCH_STATUS = 0 BEGIN
        SET @Nomi_CaricaGiunta = @Nomi_CaricaGiunta + ' e ' + @Nome_CaricaGiunta
 FETCH NEXT FROM getemp_curs into @Nome_CaricaGiunta
END

CLOSE getemp_curs
DEALLOCATE getemp_curs

RETURN @Nomi_CaricaGiunta;
END;
GO
