/****** Object:  View [dbo].[View_consiglieri]    Script Date: 11/06/2024 09:26:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[View_consiglieri] AS
SELECT DISTINCT ut.cognome + ' ' + ut.nome + ' (' + COALESCE (dbo.get_NomeGruppo_from_GUID(dbo.get_GUIDgruppoAttuale_from_persona(p.UID_persona)), '--') + ')' AS DisplayName, ut.UID_persona, ut.id_persona, jpoc.id_legislatura
FROM            dbo.join_persona_organo_carica AS jpoc INNER JOIN
                         dbo.persona ON jpoc.id_persona = dbo.persona.id_persona LEFT OUTER JOIN
                         dbo.join_persona_AD AS p ON jpoc.id_persona = p.id_persona LEFT OUTER JOIN
                         dbo.View_UTENTI AS ut ON p.UID_persona = ut.UID_persona
WHERE        (jpoc.deleted = 0) AND (ut.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                             (SELECT        id_organo
                               FROM            dbo.organi
                               WHERE       (id_tipo_organo <> 4) AND (deleted = 0))) OR
                         (jpoc.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                             (SELECT        id_organo
                               FROM            dbo.organi AS organi_1
                               WHERE        (id_tipo_organo <> 4) AND (deleted = 0)))
GO

