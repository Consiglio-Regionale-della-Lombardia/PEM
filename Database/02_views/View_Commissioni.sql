CREATE VIEW [dbo].[View_Commissioni]
AS
SELECT     TOP (100) PERCENT id_organo, nome_organo, nome_organo_breve, ordinamento
FROM         dbo.organi
WHERE     (id_tipo_organo = 2) AND 
                      (id_categoria_organo IN (2, 10))
ORDER BY ordinamento
GO


