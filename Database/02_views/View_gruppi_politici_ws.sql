CREATE VIEW [dbo].[View_gruppi_politici_ws]
AS
SELECT     TOP (100) PERCENT dbo.gruppi_politici.id_gruppo, dbo.gruppi_politici.nome_gruppo, dbo.gruppi_politici.codice_gruppo, dbo.gruppi_politici.data_inizio, 
                      dbo.gruppi_politici.data_fine, dbo.gruppi_politici.attivo, dbo.join_gruppi_politici_legislature.id_legislatura
FROM         dbo.join_gruppi_politici_legislature INNER JOIN
                      dbo.gruppi_politici ON dbo.join_gruppi_politici_legislature.id_gruppo = dbo.gruppi_politici.id_gruppo
WHERE     (dbo.gruppi_politici.deleted = 0) AND (dbo.join_gruppi_politici_legislature.deleted = 0)
ORDER BY dbo.join_gruppi_politici_legislature.id_legislatura

GO

