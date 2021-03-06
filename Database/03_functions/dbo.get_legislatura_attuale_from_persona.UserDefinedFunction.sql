USE [dbEmendamenti]
GO
/****** Object:  UserDefinedFunction [dbo].[get_legislatura_attuale_from_persona]    Script Date: 12/09/2020 18:38:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_legislatura_attuale_from_persona](@id_persona int)
RETURNS bit
AS 

BEGIN

	Declare @n_incarichi_legislatura int;
	Declare @ris bit;
 
	select @n_incarichi_legislatura =
	(
		select COUNT(id_legislatura) 
	)
	from join_persona_organo_carica where id_legislatura in (select id_legislatura from legislature where attiva=1) and deleted=0
	and id_persona = @id_persona

	if @n_incarichi_legislatura >0 set @ris=1 ELSE set @ris=0
	
	RETURN @ris
END;
GO
