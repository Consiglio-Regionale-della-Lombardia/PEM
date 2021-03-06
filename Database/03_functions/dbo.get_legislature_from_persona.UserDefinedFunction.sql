USE [dbEmendamenti]
GO
/****** Object:  UserDefinedFunction [dbo].[get_legislature_from_persona]    Script Date: 12/09/2020 18:38:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_legislature_from_persona](@id_persona int)
RETURNS varchar(1000)
AS 

BEGIN

	Declare @legislature varchar(1000);
 
select @legislature =
(
select LTRIM (STUFF((
               select distinct '-' + c.num_legislatura 
               from join_persona_organo_carica b
			   inner join legislature c
			   on b.id_legislatura = c.id_legislatura
               where id_persona = a.id_persona
        for xml path(''), type).value ('.' , 'varchar(max)' ), 1, 1, ''))
)
from persona a
where id_persona = @id_persona

		   
    RETURN '-' + @legislature + '-';
END;
GO
