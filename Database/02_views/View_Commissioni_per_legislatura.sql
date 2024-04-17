USE [dbEmendamenti_test]
GO

/****** Object:  View [dbo].[View_Commissioni_per_legislatura]    Script Date: 16/04/2024 10:16:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_Commissioni_per_legislatura] AS
SELECT         id_organo, nome_organo, nome_organo_breve, ordinamento, id_legislatura
FROM            dbo.organi
WHERE        (deleted = 0) AND (id_tipo_organo = 2) AND (id_categoria_organo IN (2, 10))
GO

