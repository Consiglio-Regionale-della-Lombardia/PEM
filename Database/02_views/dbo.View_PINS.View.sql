USE [dbEmendamenti]
GO
/****** Object:  View [dbo].[View_PINS]    Script Date: 13/05/2022 10:36:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_PINS]
AS
(SELECT     dbo.PINS.*
FROM         dbo.PINS)
UNION
(SELECT     dbo.PINS_NoCons.*
 FROM         dbo.PINS_NoCons)

GO
