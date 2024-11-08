/****** Object:  View [dbo].[View_cariche_assessori_per_legislatura]    Script Date: 16/04/2024 10:16:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_cariche_assessori_per_legislatura] AS
SELECT dbo.cariche.id_carica, dbo.cariche.nome_carica, ut.cognome + ' ' + ut.nome AS DisplayName, ut.UID_persona, dbo.cariche.ordine, jpoc.id_legislatura
FROM            dbo.join_persona_organo_carica AS jpoc INNER JOIN
                         dbo.persona ON jpoc.id_persona = dbo.persona.id_persona INNER JOIN
                         dbo.join_persona_AD AS p ON jpoc.id_persona = p.id_persona INNER JOIN
                         dbo.cariche ON jpoc.id_carica = dbo.cariche.id_carica LEFT OUTER JOIN
                         dbo.View_UTENTI AS ut ON p.UID_persona = ut.UID_persona
WHERE        (jpoc.deleted = 0) AND (ut.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                             (SELECT        id_organo
                               FROM            dbo.organi
                               WHERE        (id_tipo_organo = 4) AND (deleted = 0))) AND (NOT (dbo.cariche.id_tipo_carica IN (3, 10)))
GO

