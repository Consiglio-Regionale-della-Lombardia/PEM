/****** Object:  View [dbo].[View_cariche_assessori_in_carica]    Script Date: 27/11/2024 17:04:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_cariche_assessori_in_carica]
AS
SELECT DISTINCT TOP (100) PERCENT dbo.cariche.id_carica, dbo.cariche.nome_carica, ut.cognome + ' ' + ut.nome AS DisplayName, ut.UID_persona, dbo.cariche.ordine
FROM            dbo.join_persona_organo_carica AS jpoc INNER JOIN
                         dbo.persona ON jpoc.id_persona = dbo.persona.id_persona INNER JOIN
                         dbo.join_persona_AD AS p ON jpoc.id_persona = p.id_persona INNER JOIN
                         dbo.cariche ON jpoc.id_carica = dbo.cariche.id_carica LEFT OUTER JOIN
                         dbo.View_UTENTI AS ut ON p.UID_persona = ut.UID_persona
WHERE        (jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (ut.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                             (SELECT        id_organo
                               FROM            dbo.organi
                               WHERE        (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo = 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND (jpoc.data_fine >= GETDATE()) AND (NOT (dbo.cariche.id_tipo_carica IN (3))) OR
                         (jpoc.id_legislatura = dbo.get_legislatura_attuale()) AND (jpoc.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                             (SELECT        id_organo
                               FROM            dbo.organi AS organi_1
                               WHERE        (id_legislatura = dbo.get_legislatura_attuale()) AND (id_tipo_organo = 4) AND (deleted = 0))) AND (jpoc.data_inizio <= GETDATE()) AND (jpoc.data_fine IS NULL) AND (NOT (dbo.cariche.id_tipo_carica IN (3)))
ORDER BY dbo.cariche.ordine
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[29] 4[31] 2[13] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = -96
         Left = 0
      End
      Begin Tables = 
         Begin Table = "jpoc"
            Begin Extent = 
               Top = 31
               Left = 249
               Bottom = 215
               Right = 400
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "persona"
            Begin Extent = 
               Top = 7
               Left = 495
               Bottom = 115
               Right = 646
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "p"
            Begin Extent = 
               Top = 129
               Left = 483
               Bottom = 237
               Right = 668
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cariche"
            Begin Extent = 
               Top = 18
               Left = 8
               Bottom = 159
               Right = 179
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ut"
            Begin Extent = 
               Top = 116
               Left = 681
               Bottom = 224
               Right = 869
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 2970
         Width = 1950
         Width = 2790
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 117' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_cariche_assessori_in_carica'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'0
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_cariche_assessori_in_carica'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_cariche_assessori_in_carica'
GO

