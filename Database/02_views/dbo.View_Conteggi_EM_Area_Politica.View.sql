USE [dbEmendamenti]
GO

/****** Object:  View [dbo].[View_Conteggi_EM_Area_Politica]    Script Date: 31/08/2022 09:56:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create view [dbo].[View_Conteggi_EM_Area_Politica] as(
select em.UIDAtto, em.AreaPolitica as IdArea, a.AreaPolitica, Count(*) as num_em
from EM em
inner join AREAPOLITICA a on a.Id=em.AreaPolitica
group by em.UIDAtto, em.AreaPolitica, a.AreaPolitica
)
GO

