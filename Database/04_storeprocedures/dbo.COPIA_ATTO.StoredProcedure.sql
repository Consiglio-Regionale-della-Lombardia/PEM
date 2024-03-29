USE [dbEmendamenti]
GO
/****** Object:  StoredProcedure [dbo].[COPIA_ATTO]    Script Date: 13/05/2022 10:34:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[COPIA_ATTO]
   @UIDAtto uniqueidentifier,
   @UIDSeduta uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;


	DECLARE @newUIDAtto uniqueidentifier;

    SET @newUIDAtto = NewID();


	INSERT INTO ATTI([UIDAtto], [NAtto]
      ,[IDTipoAtto]
      ,[Oggetto]
      ,[Note]
      ,[Path_Testo_Atto]
      ,[UIDSeduta]
      ,[Data_apertura]
      ,[Data_chiusura]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[Priorita]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[Eliminato]) select @newUIDAtto, [NAtto]
      ,[IDTipoAtto]
      ,[Oggetto]
      ,[Note]
      ,[Path_Testo_Atto]
      ,@UIDSeduta
      ,[Data_apertura]
      ,[Data_chiusura]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[Priorita]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[Eliminato] FROM ATTI where UIDAtto=@UIDAtto;

	/* COPIO I RELATORI*/
	DECLARE @UIDPersona uniqueidentifier
    DECLARE cursor_relatori CURSOR FAST_FORWARD
	FOR SELECT ATTI_RELATORI.UIDPersona
		FROM   ATTI_RELATORI 
        WHERE  ATTI_RELATORI.UIDAtto = @UIDAtto

	OPEN cursor_relatori
	FETCH NEXT FROM cursor_relatori INTO @UIDPersona
	WHILE @@FETCH_STATUS = 0
	BEGIN
		/* Copio i relatori*/
		INSERT INTO ATTI_RELATORI(UIDAtto,UIDPersona) values (@newUIDAtto, @UIDPersona)

		FETCH NEXT FROM cursor_relatori INTO @UIDPersona
	END
	CLOSE cursor_relatori
	DEALLOCATE cursor_relatori


	/* COPIO GLI EM*/
	DECLARE @UIDEM uniqueidentifier
	DECLARE cursor_em CURSOR FAST_FORWARD
	FOR SELECT EM.UIDEM
		FROM   EM 
        WHERE  EM.UIDAtto = @UIDAtto

	OPEN cursor_em
	FETCH NEXT FROM cursor_em INTO @UIDEM
	WHILE @@FETCH_STATUS = 0
	BEGIN

	    DECLARE @newUIDEM uniqueidentifier;

		SET @newUIDEM = NewID();

		/* Copio le L'EM*/
		INSERT INTO EM([UIDEM]
      ,[Progressivo]
      ,[UIDAtto]
      ,[N_EM]
      ,[id_gruppo]
      ,[Rif_UIDEM]
      ,[N_SUBEM]
      ,[SubProgressivo]
      ,[UIDPersonaProponente]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[DataDeposito]
      ,[UIDPersonaPrimaFirma]
      ,[DataPrimaFirma]
      ,[UIDPersonaDeposito]
      ,[Proietta]
      ,[DataProietta]
      ,[UIDPersonaProietta]
      ,[DataRitiro]
      ,[UIDPersonaRitiro]
      ,[Hash]
      ,[IDTipo_EM]
      ,[IDParte]
      ,[NTitolo]
      ,[NCapo]
      ,[UIDArticolo]
      ,[UIDComma]
      ,[NLettera]
      ,[NNumero]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[TestoEM_originale]
      ,[EM_Certificato]
      ,[TestoREL_originale]
      ,[PATH_AllegatoGenerico]
      ,[PATH_AllegatoTecnico]
      ,[EffettiFinanziari]
      ,[NOTE_EM]
      ,[NOTE_Griglia]
      ,[IDStato]
      ,[TestoEM_Modificabile]
      ,[UID_QRCode]
      ,[AreaPolitica]
      ,[Eliminato]
      ,[UIDPersonaElimina]
      ,[DataElimina]
      ,[Timestamp]
      ,[Colore]
	  )select
	   @newUIDEM
      ,[Progressivo]
      ,@newUIDAtto
      ,[N_EM]
      ,[id_gruppo]
      ,[Rif_UIDEM]
      ,[N_SUBEM]
      ,[SubProgressivo]
      ,[UIDPersonaProponente]
      ,[DataCreazione]
      ,[UIDPersonaCreazione]
      ,[DataModifica]
      ,[UIDPersonaModifica]
      ,[DataDeposito]
      ,[UIDPersonaPrimaFirma]
      ,[DataPrimaFirma]
      ,[UIDPersonaDeposito]
      ,[Proietta]
      ,[DataProietta]
      ,[UIDPersonaProietta]
      ,[DataRitiro]
      ,[UIDPersonaRitiro]
      ,[Hash]
      ,[IDTipo_EM]
      ,[IDParte]
      ,[NTitolo]
      ,[NCapo]
      ,[UIDArticolo]
      ,[UIDComma]
      ,[NLettera]
      ,[NNumero]
      ,[OrdinePresentazione]
      ,[OrdineVotazione]
      ,[TestoEM_originale]
      ,[EM_Certificato]
      ,[TestoREL_originale]
      ,[PATH_AllegatoGenerico]
      ,[PATH_AllegatoTecnico]
      ,[EffettiFinanziari]
      ,[NOTE_EM]
      ,[NOTE_Griglia]
      ,[IDStato]
      ,[TestoEM_Modificabile]
      ,[UID_QRCode]
      ,[AreaPolitica]
      ,[Eliminato]
      ,[UIDPersonaElimina]
      ,[DataElimina]
      ,[Timestamp]
      ,[Colore] from EM where UIDEM=@UIDEM;


		INSERT INTO FIRME([UIDEM], [UID_persona]
      ,[FirmaCert]
      ,[Data_firma]
      ,[Data_ritirofirma]
      ,[Timestamp]) select @newUIDEM, [UID_persona]
      ,[FirmaCert]
      ,[Data_firma]
      ,[Data_ritirofirma]
      ,[Timestamp] from FIRME where UIDEM=@UIDEM;

		FETCH NEXT FROM cursor_em INTO @UIDEM
	END

	CLOSE cursor_em
	DEALLOCATE cursor_em


	/* COPIO GLI ARTICOLI*/
	DECLARE @UIDArticolo uniqueidentifier
	DECLARE cursor_articoli CURSOR FAST_FORWARD
	FOR SELECT ARTICOLI.UIDArticolo
		FROM   ARTICOLI 
        WHERE  ARTICOLI.UIDAtto = @UIDAtto

	OPEN cursor_articoli
	FETCH NEXT FROM cursor_articoli INTO @UIDArticolo
	WHILE @@FETCH_STATUS = 0
	BEGIN

	    DECLARE @newUIDArticolo uniqueidentifier;

		SET @newUIDArticolo = NewID();

		/* Copio gli articoli*/
		INSERT INTO ARTICOLI(UIDArticolo, UIDAtto ,Articolo, Ordine) select @newUIDArticolo, UIDAtto, Articolo, Ordine from ARTICOLI where UIDArticolo=@UIDArticolo;

		/*FACCIO l'UPDATE SUI NUOVI EM CON L'ARTICOLO APPENA CREATO*/
		UPDATE EM set UIDArticolo=@newUIDArticolo where UIDArticolo=@UIDArticolo AND UIDAtto=@newUIDAtto;
		
		
						/*COPIO I COMMI*/
						DECLARE @UIDcomma uniqueidentifier
						DECLARE cursor_commi CURSOR FAST_FORWARD
						FOR SELECT COMMI.UIDcomma
							FROM   COMMI 
							WHERE  COMMI.UIDArticolo = @UIDArticolo

						OPEN cursor_commi
						FETCH NEXT FROM cursor_commi INTO @UIDcomma
						WHILE @@FETCH_STATUS = 0
						BEGIN
							DECLARE @newUIDComma  uniqueidentifier;
		
							SET @newUIDComma = NewID();

							INSERT INTO COMMI(UIDComma, UIDAtto, UIDArticolo, Comma, Ordine) select @newUIDComma, @newUIDAtto, @newUIDArticolo, Comma, Ordine from COMMI where UIDComma=@UIDComma;
		
							/*FACCIO l'UPDATE SUI NUOVI EM CON IL NUOVO COMMA APPENA CREATO*/
							UPDATE EM set UIDComma=@newUIDComma where UIDComma=@UIDComma AND UIDAtto=@newUIDAtto;
							FETCH NEXT FROM cursor_commi INTO @UIDcomma
						END

						CLOSE cursor_commi
						DEALLOCATE cursor_commi

		FETCH NEXT FROM cursor_articoli INTO @UIDArticolo
	END

	CLOSE cursor_articoli
	DEALLOCATE cursor_articoli

END
GO
