USE [dbEmendamenti]
GO

/****** Object:  View [dbo].[View_UTENTI]    Script Date: 22/06/2022 11:22:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[View_UTENTI] AS
(SELECT     dbo.join_persona_AD.UID_persona, dbo.persona.id_persona AS id_persona, dbo.persona.cognome COLLATE database_default AS cognome, 
                      dbo.persona.nome COLLATE database_default AS nome, dbo.join_persona_recapiti.recapito COLLATE database_default AS email, COALESCE (dbo.persona.foto, 
                      'noface.png') COLLATE database_default AS foto, dbo.join_persona_AD.UserAD COLLATE database_default AS userAD, 
                      dbo.get_legislature_from_persona(dbo.persona.id_persona) AS legislature, dbo.get_legislatura_attuale_from_persona(persona.id_persona) AS legislatura_attuale, 
                      dbo.get_NomeGruppo_from_GUID(dbo.get_GUIDgruppoAttuale_from_persona(dbo.join_persona_AD.UID_persona)) AS GruppoPolitico_attuale, NULL 
                      AS id_gruppo_politico_rif, cast(0 AS bit) AS notifica_firma, cast(0 AS bit) AS notifica_deposito, 0 AS No_Cons, dbo.join_persona_AD.pass_locale_crypt, dbo.join_persona_AD.gruppi_autorizzazione, dbo.consigliere_attivo(dbo.persona.id_persona) AS attivo, 
                      dbo.persona.deleted
FROM         dbo.persona LEFT JOIN
                      dbo.join_persona_AD ON dbo.join_persona_AD.id_persona = dbo.persona.id_persona LEFT OUTER JOIN
                      dbo.join_persona_recapiti ON dbo.persona.id_persona = dbo.join_persona_recapiti.id_persona
WHERE     dbo.persona.deleted = 0)
UNION
(SELECT     dbo.UTENTI_NoCons.UID_persona, dbo.UTENTI_NoCons.id_persona, dbo.UTENTI_NoCons.cognome COLLATE database_default, 
                        dbo.UTENTI_NoCons.nome COLLATE database_default, dbo.UTENTI_NoCons.email COLLATE database_default, COALESCE (dbo.UTENTI_NoCons.foto, 
                        'noface.png') COLLATE database_default, dbo.UTENTI_NoCons.UserAD COLLATE database_default, NULL AS legislature, NULL AS legislatura_attuale, NULL 
                        AS GruppoPolitico_attuale, id_gruppo_politico_rif, notifica_firma, notifica_deposito, 1 AS No_Cons, dbo.UTENTI_NoCons.pass_locale_crypt, dbo.UTENTI_NoCons.gruppi_autorizzazione, attivo, dbo.UTENTI_NoCons.deleted
 FROM         dbo.UTENTI_NoCons)

GO

