USE [dbEmendamenti]
GO

/****** Object:  View [dbo].[View_Commissioni_attive]    Script Date: 21/06/2022 10:55:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[View_Commissioni_attive]
AS
SELECT TOP (100) PERCENT id_organo, nome_organo, nome_organo_breve, ordinamento
FROM dbo.organi
WHERE (id_legislatura = dbo.get_legislatura_attuale()) AND (data_inizio <= GETDATE()) AND (data_fine >= GETDATE()) AND (deleted = 0) AND (id_tipo_organo = 2) AND
(id_categoria_organo IN (2, 10)) OR
(id_legislatura = dbo.get_legislatura_attuale()) AND (data_inizio <= GETDATE()) AND (data_fine IS NULL) AND (deleted = 0) AND (id_tipo_organo = 2) AND
(id_categoria_organo IN (2, 10))
ORDER BY ordinamento



GO

