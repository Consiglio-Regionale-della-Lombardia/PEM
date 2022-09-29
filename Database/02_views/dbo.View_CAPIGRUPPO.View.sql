USE [dbDASI]
GO

/****** Object:  View [dbo].[View_CAPIGRUPPO]    Script Date: 29/09/2022 05:16:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[View_CAPIGRUPPO] AS
(SELECT     dbo.join_persona_AD.UID_persona, dbo.persona.id_persona AS id_persona, dbo.persona.cognome COLLATE database_default AS cognome, 
                      dbo.persona.nome COLLATE database_default AS nome, COALESCE (dbo.persona.foto, 
                      'noface.png') COLLATE database_default AS foto, dbo.join_persona_AD.UserAD COLLATE database_default AS userAD, 
                      dbo.get_legislature_from_persona(dbo.persona.id_persona) AS legislature, dbo.get_legislatura_attuale_from_persona(persona.id_persona) AS legislatura_attuale, 
                      dbo.get_NomeGruppo_from_GUID(dbo.get_GUIDgruppoAttuale_from_persona(dbo.join_persona_AD.UID_persona)) AS GruppoPolitico_attuale, NULL 
                      AS id_gruppo_politico_rif, cast(0 AS bit) AS notifica_firma, cast(0 AS bit) AS notifica_deposito, 0 AS No_Cons, dbo.join_persona_AD.pass_locale_crypt, dbo.join_persona_AD.gruppi_autorizzazione, dbo.consigliere_attivo(dbo.persona.id_persona) AS attivo, 
                      dbo.persona.deleted
FROM            join_persona_gruppi_politici INNER JOIN
                         cariche ON join_persona_gruppi_politici.id_carica = cariche.id_carica INNER JOIN
                         join_persona_AD ON join_persona_gruppi_politici.id_persona = join_persona_AD.id_persona INNER JOIN
                         persona ON persona.id_persona = join_persona_AD.id_persona
WHERE        (cariche.presidente_gruppo = 1) AND 
				((join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine >= GETDATE()) 
					OR
				(join_persona_gruppi_politici.deleted = 0 AND join_persona_gruppi_politici.data_inizio <= GETDATE() AND join_persona_gruppi_politici.data_fine IS NULL)
			 )
			 )	


GO

