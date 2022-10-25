USE [dbEmendamenti]
GO

/****** Object:  View [dbo].[View_Conteggi_EM_Gruppi_Politici]    Script Date: 31/08/2022 09:56:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE view [dbo].[View_Conteggi_EM_Gruppi_Politici] as(
select em.UIDAtto, em.id_gruppo, gp.nome_gruppo, Count(*) as num_em
from EM em
inner join gruppi_politici gp on em.id_gruppo = gp.id_gruppo
group by em.UIDAtto, em.id_gruppo, gp.nome_gruppo
)
GO

