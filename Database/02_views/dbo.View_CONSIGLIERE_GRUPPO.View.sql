USE [dbEmendamenti]
GO
/****** Object:  View [dbo].[View_CONSIGLIERE_GRUPPO]    Script Date: 13/05/2022 10:36:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_CONSIGLIERE_GRUPPO]
AS
SELECT DISTINCT P.id_persona, P.nome, P.cognome, GP.id_gruppo, GP.codice_gruppo, GP.nome_gruppo, GP.attivo, L.id_legislatura, L.num_legislatura
FROM         dbo.persona AS P INNER JOIN
                      dbo.join_persona_gruppi_politici AS P_GP ON P.id_persona = P_GP.id_persona INNER JOIN
                      dbo.gruppi_politici AS GP ON P_GP.id_gruppo = GP.id_gruppo INNER JOIN
                      dbo.legislature AS L ON P_GP.id_legislatura = L.id_legislatura
WHERE     (1 = 1) AND (L.id_legislatura = dbo.get_legislatura_attuale()) AND (GP.attivo = 1) AND (GP.deleted = 0) AND (P.deleted = 0) AND (P_GP.data_fine IS NULL)

GO
