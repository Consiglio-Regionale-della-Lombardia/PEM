# Portale PEM - Presentazione Emendamenti
# Struttura del database

Qui di seguito elenchiamo gli oggetti contenuti nel database del Portale PEM. 
Il legame tra questi oggetti è visibile nel diagramma entità-relazioni in calce al presente documento.

## Contenuti

- [Tabelle proprietarie](#tabelle_proprietarie)
- [Tabelle replicate](#tabelle_replicate)
- [Viste](#viste)
- [Funzioni](#funzioni)
- [Stored procedure](#stored_procedure)
- [Diagramma Entità-Relazioni](#diagramma-entità-relazioni)


## Tabelle proprietarie

#### ARTICOLI
Tabella per la gestione degli articoli contenuti in un ATTO (progetto di legge – PDL)

##### Campi
UIDArticolo: Guid

UIDAtto: Guid

Articolo: String

RubricaArticolo: String

TestoArticolo: String

Ordine: Int32 //campo utilizzato per ordinare gli articoli all’interno dell’atto

#### ATTI
Tabella per la gestione degli ATTI su cui presentare emendamenti (Progetti di legge)

##### Campi
UIDAtto: Guid

NAtto: String //Numero dell’atto

IDTipoAtto: Int32 //Tipo di atto. Nell’attuale versione vengono gestiti solo i Progetti di legge (PDL)

Oggetto: String

Note: String

Path_Testo_Atto: String //percorso per scaricare il testo completo dell’atto in formato pdf

UIDSeduta: Guid //riferimento alla seduta d’Aula in cui verrà trattato l’atto

Data_apertura: DateTime //Data a partire dalla quale sarà possibile inserire emendamenti

Data_chiusura: DateTime //Data di chiusura dell’atto che blocca l’inserimento/firma degli emendamenti

VIS_Mis_Prog: Boolean //Indica se l’atto è di tipo finanziario. Il format dell’emendamento darà la possibilità di selezionare Missioni e Programmi

UIDAssessoreRiferimento: Guid

Notifica_deposito_differita: Boolean //Se true, la mail contenente la ricevuta di consegna di un emendamento verrà generata in differita tramite il modulo Gestione Stampe

OrdinePresentazione: Boolean //abilita per tutti gli utenti la visibilità del fascicolo degli emendamenti in ordine di presentazione

OrdineVotazione: Boolean //abilita per tutti gli utenti la visibilità del fascicolo degli emendamenti in ordine di votazione

Priorità: Int32 //campo utilizzato per visualizzare gli atti in ordine di discussione, all’interno della seduta

DataCreazione: DateTime 

UIDPersonaCreazione: Guid

DataModifica: DateTime

UIDPersonaModifica: Guid

Eliminato: Boolean

LinkFascicoloVotazione: String

DataCreazioneVotazione: DateTime //memorizza la data/ora in cui è stato pubblicato il fascicolo in ordine di votazione

DataUltimaModificaEM: DateTime //memorizza la data/ora in cui è avvenuta l’ultima modifica su un emendamento (es. aggiunta o ritiro di una firma)

#### ATTI_RELATORI
Tabella che mette in relazione un atto a uno o più consiglieri individuati come relatori

##### Campi
UIDAtto: Guid

UIDPersona: Guid

sycReplica: Guid //campo di servizio utilizzato per replicare in real-time i dati tra db di produzione e db di backup

#### COMMI
Tabella per la gestione dei commi contenuti negli ARTICOLI di un ATTO (progetto di legge – PDL)

##### Campi
UIDComma: Guid

UIDAtto: Guid

UIDArticolo: Guid

Comma: String

TestoComma: String

Ordine: Int32 //campo utilizzato per visualizzare nell’ordine corretto i commi all’interno dell’articolo

#### EM
Tabella per la gestione degli EMENDAMENTI/SUBEMENDAMENTI

##### Campi
UIDEM: Guid //Identificativo univoco dell’em/subem

Progressivo: Int32 //ordine di inserimento di un emendamento riferito al singolo gruppo politico

UIDAtto: Guid

N_EM: String //Numero assegnato dal sistema all’emendamento al momento del deposito. Campo crittografato e immodificabile

Id_gruppo: Int32 //id del gruppo relativo al consigliere che presenta l’emendamento

Rif_UIDEM: Guid //riferimento a un emendamento nel caso il record inserito sia relativo a un subemendamento

N_SUBEM: String //Numero assegnato dal sistema a un subemendamento al momento del deposito. Campo crittografato e immodificabile

SubProgressivo: Int32 //ordine di inserimento di un subemendamento riferito al singolo gruppo politico

UIDPersonaProponente: Guid

UIDPersonaProponenteOLD: Guid //Memorizza il vecchio proponente per gestire la funzione “Fatto proprio”

DataCreazione: DateTime

UIDPersonaCreazione: Guid

idRuoloCreazione: Int32 //Ruolo della persona che ha inserito l’emendamento

DataModifica: DateTime

UIDPersonaModifica: Guid

DataDeposito: String //Data di deposito assegnata dal sistema al momento del deposito si un emendamento/subemendamento. Campo crittografato e immodificabile

UIDPersonaPrimaFirma: Guid //Identificativo del primo firmatario ovvero del proponente del em/subem

DataPrimaFirma: DateTime

UIDPersonaDeposito: Guid

Proietta: Boolean //se true, l’emendamento è attualmente proiettato tramite la funzionalità proietta

DataProietta: DateTime

UIDPersonaProietta: Guid

DataRitiro: DateTime

UIDPersonaRitiro: Guid

Hash: String //codice di controllo utilizzato per decrittografare l’emendamento/subemendamento depositato

IDTipo_EM: Int32 //Tipo di em: aggiuntivo, soppressivo, modificativo, ecc

IDParte: Int32 //Parte emendata: titolo, capo, articolo,…

NTitolo: String //Riferimento alla parte emendata

NCapo: String //Riferimento alla parte emendata

UIDArticolo: Guid //Riferimento alla parte emendata

UIDComma: Guid //Riferimento alla parte emendata

UIDLettera: Guid //Riferimento alla parte emendata

NLettera: String //Riferimento alla parte emendata. Le lettere nella prima versione di PEM erano gestite come campo testo. Nella versione successiva sono state codificate in modo di permetterne la selezione dopo aver selezionato un comma.

NNumero: String //Riferimento alla parte emendata

NMissione: Int32 //Riferimento alla parte emendata

NProgramma: Int32 //Riferimento alla parte emendata

NTitoloB: Int32 //Riferimento alla parte emendata

OrdinePresentazione: Int32 //memorizza la posizione all’interno del fascicolo in ordine di presentazione in cui posizionare l’emendamento/subem

OrdineVotazione: Int32 //memorizza la posizione all’interno del fascicolo in ordine di votazione in cui posizionare l’emendamento/subem

TestoEM_originale: String //Testo dell’em/subem in chiaro

EM_Certificato: String //Testo dell’em/subem crittografato e immodificabile

TestoREL_originale: String //Testo della relazione illustrativa associata ad un em/subem in chiaro

PATH_AllegatoGenerico: String

PATH_AllegatoTecnico: String

EffettiFinanziari: Int32

NOTE_EM: String

NOTE_Griglia: String //Note riservate all’ufficio segreteria dell’assemblea

IDStato: Int32

Firma_su_invito: Boolean //Indica se l’em/subem può essere firmato da altri consiglieri solo se invitati

TestoEM_Modificabile: String //Memorizza il testo dell’emendamento modificato durante la discussione in Aula

UID_QRCode: Guid //permette di accedere all’emendamento/subemendamento utilizzando il QRCode
 
AreaPolitica: Int32 //Area politica del gruppo che ha presentato l’em/subem: maggioranza, minoranza, misto, ecc

Eliminato: Boolean

UIDPersonaElimina: Guid

DataElimina: DateTime

chkf: String //campo utilizzato per verificare che il numero delle firme non sia stato alterato

chkem: Int32 //codice di controllo dell’emendamento 

Timestamp: DateTime //Data-ora di presentazione dell’em/subem

Colore: String //Colore dell’emendamento, utilizzato per gestire la funzione “Uguali tra loro”

#### FIRME
Tabella per la gestione delle FIRME degli EMENDAMENTI/SUBEMENDAMENTI

##### Campi
UIDEM: Guid

UID_persona: Guid

FirmaCert: String

Data_firma: String //Data assegnata dal sistema al momento della firma di un emendamento/subemendamento. Campo crittografato e immodificabile

Data_ritirofirma: String //Data assegnata dal sistema al momento del ritiro di una firma da emendamento/subemendamento. Campo crittografato e immodificabile

id_AreaPolitica: Int32 //area politica (maggioranza, minoranza, misto, ecc) a cui appartiene il consigliere firmatario

Timestamp: DateTime //data in cui è avvenuta l’operazione sulla firma

ufficio: Boolean //indica che l’em/subem è stato firmato dal Dirigente del Servizio Segreteria dell’Assemblea

#### LETTERE
Tabella per la gestione delle lettere contenute nei COMMI di un ATTO

##### Campi
UIDLettera: Guid

UIDComma: Guid

Lettera: String

TestoLettera: String

Ordine: Int32

#### MISSIONI
Tabella per la gestione delle MISSIONI di un ATTO

##### Campi
 UIDMissione: Guid
 
 NMissione: Int32
 
 DAL: DateTime
 
 AL: DateTime
 
 Descrizione: String
 
 Ordine: Int32

#### NOTIFICHE
Tabella per la gestione degli INVITI alla FIRMA di un EM/SUBEM

##### Campi
UIDNotifica: Int64

DataCreazione: DateTime

UIDEM: Guid

UIDAtto: Guid

IDTipo: Int32

Mittente: Guid

RuoloMittente: Int32

Messaggio: String

DataScadenza: DateTime

SyncGuid: Guid //campo Tecnico per la sincronizzazione in realtime del database di produzione con il database di backup

IdGruppo: Int32

Chiuso: Boolean

DataChiusura: DateTime

BLOCCO_INVITI: Guid //campo utilizzato per la gestione di blocchi di inviti: inviti che vengono accorpati in un unico messaggio email

#### NOTIFICHE_DESTINATARI
Tabella per mette in relazione un INVITO con l’elenco dei suoi DESTINATARI

##### Campi
UID: Guid

UIDNotifica: Int64

UIDPersona: Guid

Visto: Boolean

DataVisto: DateTime

Chiuso: Boolean

DataChiusura: DateTime

IdGruppo: Int32

#### PARTI_TESTO
Tabella di supporto per la memorizzazione della PARTE del progetto di legge da emendare (Titolo, Capo, Articolo, ecc)

##### Campi
IDParte: Int32

Parte: String

Ordine: Int32 //campo utilizzato dalla procedura di ordinamento automatico degli emendamenti per la creazione del fascicolo in ordine di votazione

Passo: Int32 //campo utilizzato dalla procedura di ordinamento automatico degli emendamenti per la creazione del fascicolo in ordine di votazione

#### PINS
Tabella che memorizza i PIN assegnati ai consiglieri/assessori

##### Campi
UIDPIN: Guid

UID_persona: Guid

PIN: String

Dal: DateTime

Al: DateTime

FIRMA_e_DEPOSITO: Boolean

RichiediModificaPIN: Boolean

#### PINS_NoCons
Tabella che memorizza i PIN assegnati al personale dei Gruppi politici e validi solo per il deposito (la tabella ha la stessa struttura della tabella PINS per poter eseguire una union e avere una vista unica di tutti i pin)

##### Campi
UIDPIN: Guid

UID_persona: Guid

PIN: String

Dal: DateTime

Al: DateTime

FIRMA_e_DEPOSITO: Boolean

RichiediModificaPIN: Boolean

#### RUOLI
Tabella di supporto per la memorizzazione dei ruoli da assegnare agli utenti dell’applicazione (Consigliere, Responsabile di segreteria politica, Assessore, Amministratore, ecc)

##### Campi
IDruolo: Int32

Ruolo: String

Priorita: Int32

ADGroup: String //Mappa il ruolo con il Gruppo Active Directory che gestisce i permessi nell’applicazione

Ruolo_di_Giunta: Boolean

#### RUOLI_UTENTE
Tabella che mette in relazione ogni UTENTE dell’applicazione a uno o più RUOLI

##### Campi
UID_ruolo_utente: Guid

UID_persona: Guid

IDruolo: Int32

#### SEDUTE
Tabella per la gestione delle SEDUTE d’Aula in cui vengono trattati i PDL e votati gli EM/SUBEM

##### Campi
UIDSeduta: Guid

Data_seduta: DateTime

Data_apertura: DateTime //Data-ora che indica il momento in cui la seduta è aperta per l’inserimento degli em/subem

Data_effettiva_inizio: DateTime

Data_effettiva_fine: DateTime

IDOrgano: Int32 //Campo non utilizzato. Previsto per poter estendere PEM alla gestione degli emendamenti presentati in commissione

Scadenza_presentazione: DateTime //Data-ora che memorizza il termine di presentazione degli emendamenti. Dopo questo orario gli emendamenti saranno marcati come “Presentato fuori termine”

id_legislatura: Int32

Intervalli: String //Campo di testo per memorizzare eventuali intervalli avvenuti durante una seduta

UIDPersonaModifica: Guid

DataModifica: DateTime

Eliminato: Boolean

#### STAMPE
Tabella per la gestione differita delle STAMPE (utilizzata dal modulo Gestione Stampe)

##### Campi
UIDStampa: Guid

UIDAtto: Guid

Da: Int32 //la stampa è richiesta dalla pagina…

A: Int32 // …alla pagina

UIDUtenteRichiesta: Guid

DataRichiesta: DateTime

Invio: Boolean //Se true, la stampa è stata inviata correttamente al richiedente

DataInvio: DateTime

MessaggioErrore: String

Lock: Boolean //Blocca il record per impedire che una nuova schedulazione del modulo di stampa generi 2 volte la medesima stampa

DataLock: DateTime

PathFile: String //link che permette di scaricare la stampa

DataInizioEsecuzione: DateTime

DataFineEsecuzione: DateTime

Tentativi: Int32 //memorizza il numero di tentativi effettuati dal modulo di stampa per generare la stampa pdf, in modo da poter gestire gli errori
 
CurrentRole: Int32 //Ruolo dell’utente che ha richiesto la stampa

Scadenza: DateTime //data di scadenza della stampa (utilizzato per pulire il filesystem che memorizza i pdf delle stampe)

Ordine: Int32

QueryEM: String //Clausula WHERE della query sql che seleziona tutti gli em/subem da inserire nella stampa

NotificaDepositoEM: Boolean //Se true, indica che questa stampa è relativa all’invio del messaggio di notifica in differita dell’avvenuto deposito di un em/subem al suo presentatore

#### STATI_EM
Tabella di supporto per la memorizzazione degli STATI degli EM/SUBEM (bozza, depositato, approvato, ecc)

##### Campi
IDStato: Int32

Stato: String

icona: String

CssClass: String //la formattazione dell’etichetta dello stato visualizzata nell’applicazione viene gestita utilizzando la classe css indicata in questo campo

Ordinamento: Int32 //Campo utilizzato per ordinare gli em/subem in base al loro attuale stato

#### TIPI_ATTO
Tabella di supporto per la memorizzazione dei tipi di ATTI emendabili (per ora solo PDL)

##### Campi
 IDTipoAtto: Int32
 
 TipoAtto: String

#### TIPI_EM
Tabella di supporto per la memorizzazione del tipo di  EMENDAMENTO (soppressivo, modificativo, aggiuntivo)

##### Campi
IDTipo_EM: Int32

Tipo_EM: String

Ordine: Int32 //Campo utilizzato per generare in automatico l’ordinamento di votazione degli em/subem (prima i soppressivi, poi i modificativi, ecc)

#### TIPI_NOTIFICA
Tabella di supporto per la memorizzazione il TIPO di NOTIFICA

##### Campi
IDTipo: Int32

Tipo: String

#### TITOLI_MISSIONI
Tabella di supporto per la memorizzazione dei TITOLI relativi alle Missioni

##### Campi
NTitoloB: Int32

Descrizione: String

#### UTENTI_NoCons
Tabella che memorizza tutti gli utenti del sistema che non sono consiglieri o assessori (gestiti tramite la replica del database AnagraficaConsiglieri)

##### Campi
UIDPersona: Guid

id_persona: Int32

cognome: String

nome: String

email: String

foto: String

UserAD: String

id_gruppo_politico_rif: Int32

notifica_firma: Boolean

notifica_deposito: Boolean

RichiediModificaPWD: Boolean

Data_ultima_modifica_PWD: DateTime

pass_locale_crypt: String

attivo: Boolean

deleted: Boolean

#### join_GRUPPO_AD
Tabella che associa ad un Gruppo Politico il relativo Gruppo ActiveDirectory utilizzato per la gestione dei permessi

##### Campi
UID_Gruppo: Guid

id_gruppo: Int32

GruppoAD: String

GiuntaRegionale: Boolean

AbilitaEMPrivati: Boolean

id_AreaPolitica: Int32

id_legislatura: Int32

#### join_persona_AD
Tabella associa ogni consigliere/assessore con il relativo account ActiveDirectory utilizzato per il login

##### Campi
UID_persona: Guid

id_persona: Int32

UserAD: String

RichiediModificaPWD: Boolean

Data_ultima_modifica_PWD: DateTime

pass_locale_crypt: String



## Tabelle replicate

Queste tabelle vengono replicate da un'altra base dati, per la gestione dei Consiglieri.
Indichiamo solamente la loro funzione, senza scendere nel dettaglio dei campi di ognuna.
 
#### cariche
Tabella di supporto contenente le cariche presenti in Consiglio regionale regionale della Lombardia
 
#### gruppi_politici
Tabella che memorizza tutte le formazioni politiche presenti nel Consiglio regionale della Lombardia

#### join_gruppi_politici_legislature
Tabella che mette in relazione un gruppo politico con la legislatura in cui è stato presente

#### join_persona_assessorati
Tabella che mette in relazione un assessore con il relativo/i assessorato/i

#### join_persona_gruppi_politici
Tabella che mette in relazione i consiglieri con i gruppi politici di appartenenza

#### join_persona_organo_carica
Tabella che mette in relazione i consiglieri con le cariche ricoperte all’interno di ogni legislatura in cui sono stati eletti

#### join_persona_recapiti
Tabella che associa a ogni consigliere/assessore l’indirizzo email a cui il sistema invia le notifiche

#### legislature
Tabella contenente l’anagrafica di tutte le legislature del Consiglio regionale della Lombardia

#### organi
Tabella contenente l’anagrafica di tutti gli organi presenti nelle diverse legislature del Consiglio regionale della Lombardia

#### persona
Tabella contenente l’anagrafica di tutti i consiglieri/assessori del Consiglio regionale della lombardia

#### tbl_recapiti
Tabella di supporto relativa al tipo di recapito gestito (email, telefono, cellulare, ecc)

#### tipo_organo
Tabella di supporto relativa al tipo di organo gestito


## Viste

#### View_assessori_in_carica
Vista che estrae l’elenco, con relativo GUID, dei componenti attuali della Giunta regionale

#### View_Composizione_GiuntaRegionale
Vista che estrae l’anagrafica dei componenti della Giunta Regionale (assessori/sottosegretari)

#### View_CONSIGLIERE_GRUPPO
Vista che visualizza i consiglieri della legislatura di riferimento con l’anagrafica del gruppo politico attuale

#### View_CONSIGLIERI_PEM
Vista che visualizza tutti i consiglieri della legislatura attiva

#### View_gruppi_politici_con_giunta
Vista tutti i gruppo politici della legislatura attiva più il gruppo fittizio “Giunta regionale”

#### View_PINS
Vista che unisce (esegue una union)  in un’unica “tabella” tutti i PIN assegnati (sia assegnati a consiglieri/assessori sia assegnati al personale delle segreterie politiche)

#### View_UTENTI
Vista che unisce (esegue una union) di tutti gli attori del sistema: consiglieri/assessori + altri utenti

#### View_consiglieri_in_carica
Vista che estrae l’anagrafica completa (GUID, id, nominativo, gruppo politico attuale, legislatura, ecc) dei consiglieri attualmente in carica


## Funzioni

#### consigliere_attivo

#### get_CaricaGiunta_from_UIDpersona

#### get_CodGruppo_from_ID

#### get_GUIDCapogruppo_from_idGruppo

#### get_GUIDgruppoAttuale_from_persona

#### get_IDgruppoAttuale_from_persona

#### get_legislatura_attuale

#### get_legislatura_attuale_from_persona

#### get_legislature_from_persona

#### get_NomeGruppo_from_GUID

#### get_PIN

#### get_ProgressivoEM

#### get_ProgressivoSUBEM

#### get_SiglaGruppo_from_GUID



## Stored procedure

#### COPIA_ATTO

#### DOWN_EM_TRATTAZIONE

#### ORDINA_EM_TRATTAZIONE

#### SPOSTA_EM_TRATTAZIONE

#### UP_EM_TRATTAZIONE



## Diagramma Entità-Relazioni

![EntityDesignerDiagram](/Database/EntityDesignerDiagram.jpg)

