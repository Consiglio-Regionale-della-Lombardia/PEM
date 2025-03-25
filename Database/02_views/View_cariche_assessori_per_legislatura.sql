/****** Object:  View [dbo].[View_cariche_assessori_per_legislatura]    Script Date: 27/11/2024 17:04:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_cariche_assessori_per_legislatura]
AS
SELECT        dbo.cariche.id_carica, dbo.cariche.nome_carica, ut.cognome + ' ' + ut.nome AS DisplayName, ut.UID_persona, dbo.cariche.ordine, jpoc.id_legislatura
FROM            dbo.join_persona_organo_carica AS jpoc INNER JOIN
                         dbo.persona ON jpoc.id_persona = dbo.persona.id_persona INNER JOIN
                         dbo.join_persona_AD AS p ON jpoc.id_persona = p.id_persona INNER JOIN
                         dbo.cariche ON jpoc.id_carica = dbo.cariche.id_carica LEFT OUTER JOIN
                         dbo.View_UTENTI AS ut ON p.UID_persona = ut.UID_persona
WHERE        (jpoc.deleted = 0) AND (ut.deleted = 0) AND (dbo.persona.deleted = 0) AND (jpoc.id_organo IN
                             (SELECT        id_organo
                               FROM            dbo.organi
                               WHERE        (id_tipo_organo = 4) AND (deleted = 0))) AND (NOT (dbo.cariche.id_tipo_carica IN (3)))
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
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
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "jpoc"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 228
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "persona"
            Begin Extent = 
               Top = 6
               Left = 266
               Bottom = 136
               Right = 456
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "p"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 268
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cariche"
            Begin Extent = 
               Top = 270
               Left = 38
               Bottom = 400
               Right = 228
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ut"
            Begin Extent = 
               Top = 402
               Left = 38
               Bottom = 532
               Right = 248
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
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
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
  ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_cariche_assessori_per_legislatura'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_cariche_assessori_per_legislatura'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_cariche_assessori_per_legislatura'
GO

