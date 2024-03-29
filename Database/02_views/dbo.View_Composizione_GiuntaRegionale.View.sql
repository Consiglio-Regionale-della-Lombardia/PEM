USE [dbEmendamenti]
GO
/****** Object:  View [dbo].[View_Composizione_GiuntaRegionale]    Script Date: 13/05/2022 10:36:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Composizione_GiuntaRegionale]
AS
SELECT DISTINCT dbo.persona.cognome, dbo.persona.nome, dbo.join_persona_recapiti.recapito AS email
FROM         dbo.join_persona_organo_carica INNER JOIN
                      dbo.persona ON dbo.join_persona_organo_carica.id_persona = dbo.persona.id_persona INNER JOIN
                      dbo.join_persona_recapiti ON dbo.join_persona_organo_carica.id_persona = dbo.join_persona_recapiti.id_persona INNER JOIN
                      dbo.organi ON dbo.join_persona_organo_carica.id_organo = dbo.organi.id_organo
WHERE     (dbo.join_persona_recapiti.tipo_recapito = 'EP') AND (dbo.persona.deleted = 0) AND (dbo.join_persona_organo_carica.deleted = 0) AND 
                      (dbo.join_persona_organo_carica.id_legislatura IN
                          (SELECT     id_legislatura
                            FROM          dbo.legislature
                            WHERE      (attiva = 1) AND (durata_legislatura_a IS NULL))) AND (dbo.join_persona_organo_carica.data_fine IS NULL) AND 
                      (dbo.join_persona_organo_carica.id_organo IN
                          (SELECT     id_organo
                            FROM          dbo.organi AS organi_1
                            WHERE      (id_tipo_organo = 4)))

GO
