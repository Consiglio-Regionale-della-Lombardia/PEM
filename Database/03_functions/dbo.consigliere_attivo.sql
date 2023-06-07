USE [dbEmendamenti]
GO

/****** Object:  UserDefinedFunction [dbo].[consigliere_attivo]    Script Date: 06/01/2023 14:44:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[consigliere_attivo](@id_persona int)
RETURNS bit
AS 

BEGIN

	Declare @id_rec int;
	Declare @res bit;
 
select @id_rec =
(
	SELECT [id_rec]
)
  FROM [dbo].[join_persona_organo_carica]
  WHERE deleted=0 and id_persona=@id_persona and id_carica in (select id_carica from [cariche] where tipologia='ORGANI') and ((GETDATE() between data_inizio and data_fine) or (data_inizio<=GETDATE() and data_fine is null))	   

if @id_rec is not null select @res=1 else select @res=0;

RETURN @res;

END;


GO

