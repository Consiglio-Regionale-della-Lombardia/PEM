USE [dbEmendamenti]
GO
/****** Object:  UserDefinedFunction [dbo].[get_SiglaGruppo_from_GUID]    Script Date: 12/09/2020 18:38:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_SiglaGruppo_from_GUID](@UID_gruppo uniqueidentifier)
RETURNS varchar(255)
AS 

BEGIN

	Declare @Nome_Gruppo varchar(255);
 
select @Nome_Gruppo =
(
	SELECT     gruppi_politici.codice_gruppo
)
FROM       gruppi_politici INNER  JOIN JOIN_GRUPPO_AD ON JOIN_GRUPPO_AD.id_gruppo = gruppi_politici.id_gruppo
WHERE 
JOIN_GRUPPO_AD.UID_Gruppo=@UID_gruppo
    RETURN @Nome_Gruppo;
END;
GO
