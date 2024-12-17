# Portale digitalizzazione atti (GeDASI)

Il progetto GeDASI è il risultato di diverse fasi di sviluppo che hanno portato alla costruzione di uno stack tecnologico, che comprende:
- il modulo PEM, per la presentazione degli Emendamenti ai progetti di legge, nell’ambiente quindi legato a una specifica seduta e attivato in concomitanza della stessa.
- il modulo DASI, per la presentazione e la gestione degli ATTI di indirizzo e sindacato ispettivo, non necessariamente legato a una specifica seduta del Consiglio.

Il portale è stato inizialmente studiato per digitalizzare la presentazione degli emendamenti/subemendamenti ai progetti di legge (modulo PEM) e, a seguito del riscontro positivo da parte degli utilizzatori, è stato esteso alla presentazione degli atti di indirizzo e sindacato ispettivo, realizzando il modulo DASI. In una terza fase il modulo DASI è stato esteso con una serie di funzionalità, dedicate al personale del Servizio Segreteria dell’Assemblea consiliare, che consento di gestire tutte le informazioni correlate agli atti di indirizzo e di sindacato ispettivo (ad esempio date di trattazione, note, ecc) e memorizzare i documenti relativi al fascicolo che compone la vita dell’atto (ad esempio le risposte fornite dalla Giunta in merito ad una interrogazione o interpellanza di un consigliere). A seguito di questa terza fase di sviluppo la piattaforma ha preso il nome di GeDASI.

A differenza degli emendamenti che si concludono con la votazione (approvato/respinto), gli atti di indirizzo e sindacato ispettivo hanno un iter complesso che si sviluppa in una serie di passaggi, da tracciare, e di documenti da generare e memorizzare. 

Per chiarezza espositiva tratteremo separatamente il modulo per la presentazione dematerializzata degli emendamenti (PEM) il modulo per la presentazione dematerializzata degli atti di indirizzo e di sindacato ispettivo (DASI) e il terzo modulo che consente la gestione di questi ultimi.

L’accesso al portale è rimasto comunque unico per tutti i moduli, le varie sezioni sono visibili solo ai profili autorizzati. Il nuovo sviluppo per la gestione degli atti di indirizzo e di sindacato ispettivo è accessibile solo al personale del Servizio Segreteria dell’Assemblea. Va puntualizzato che, per i due canali di accesso, la categoria di utenti non è coincidente: gli assessori, in quanto non sono consiglieri o sono sospesi dalla carica, non accederanno al canale DASI.

## Modulo PEM - Presentazione Emendamenti
  
Gli emendamenti sono proposte di modifica riferite ad uno specifico punto di un progetto di legge (titolo, articolo, comma, allegato, ecc.) prima che questo venga votato dall’assemblea legislativa regionale. I subemendamenti sono invece proposte di modifica riferite ad un emendamento precedentemente presentato. Il Portale PEM aiuta a de-materializzare e informatizzare le procedure del Consiglio Regionale per la presentazione di emendamenti/subemendamenti (EM/SUBEM).

## Modulo DASI – Digitalizzazione Atti di Sindacato ispettivo e d'Indirizzo

Gli atti di indirizzo e di sindacato ispettivo sono atti, tipici della tradizione parlamentare, attraverso i quali il Consiglio esercita la funzione di controllo sull’esecutivo e concorre alla determinazione dell’indirizzo politico (articolo 14, comma 1, dello Statuto d’autonomia).

Gli strumenti di indirizzo politico, denominati ATTI DI INDIRIZZO, previsti dal Regolamento generale sono mozioni, ordini del giorno e risoluzioni:

- La mozione (MOZ) è il tipico strumento assembleare, è uno strumento autonomo e, quanto ai contenuti, non subisce limiti espressi;
- L’ordine del giorno (ODG) è uno strumento accessorio ad altro atto, in genere un progetto di legge; è anche lo strumento tipico con cui l’Aula conclude i dibattiti su un determinato argomento;
- La risoluzione (RIS) è il frutto del dibattito e dell’elaborazione in una Commissione consiliare di un tema specifico a contenuto settoriale e non viene pertanto trattata attraverso questo strumento informatico.

La funzione di controllo invece, come definita a livello parlamentare, si estrinseca (anche) nell’attività di sindacato ispettivo (ATTI DI SINDACATO ISPETTIVO), e viene tradizionalmente esercitata attraverso gli strumenti tipici dell’interpellanza (ITL), dell’interrogazione (ITR) e dell’interrogazione a risposta immediata (IQT).

## Funzionalità di gestione avanzata

Nel suo complesso il GeDASI offre alcuni strumenti avanzati come la ricerca effettuata tramite la configurazione di filtri custom, la gestione massiva e puntuale degli atti. Il sistema è anche in grado di fornire una reportistica basata su una configurazione completamente personalizzata e memorizzabile di:

- criteri di ricerca
- tipo di documento (word, xls e pdf)
- tipo di visualizzazione
- template per le copertine e l'impaginazione
- dati esposti

# Note sul copyright
- Copyright: Consiglio regionale della Lombardia
- Stato del progetto: Beta
- Mantenimento in carico Namirial S.p.A. a socio unico - https://www.namirial.it/
- Per segnalare CVE e problemi di sicurezza scrivere a opensource@2Csolution.it (**ATTENZIONE:** utilizzare esclusivamente questo canale per segnalazioni relative alla sicurezza, non utilizzare lo strumento di issue tracking del repository)
 
# Contenuti

- [Introduzione](#introduzione)
- [Struttura del repository](#struttura-del-repository)
- [Struttura del progetto](#struttura-del-progetto)
- [Installazione](#installazione)
  - [Note sulla release](#note-sulla-release)
  - [Requisiti del sistema](#requisiti-del-sistema)
  - [Procedura di installazione](#procedura-di-installazione)
- [Licenza](#licenza)
  - [Autore / Copyright](#autore---copyright)
  - [Licenze dei componenti di terze parti](#licenze-dei-componenti-di-terze-parti)
  - [Dettagli della licenza](#dettagli-della-licenza)

# Introduzione

Questo repository contiene il codice sorgente e la documentazione del portale GeDASI. 
L'obiettivo del Portale è di informatizzare e rendere più efficiente e funzionale la procedura di deposito (numerazione e marcatura temporale) degli emendamenti ai progetti di legge e degli atti di indirizzo e di sindacato ispettivo, con un’applicazione multiutente con livelli di informatizzazione e automazione più o meno ampi, a seconda delle varie fasi del processo. L'applicazione, unica, è stata divisa a livello funzionale in due moduli denominati modulo PEM e modulo DASI.
 
Il Modulo PEM permette di:
- gestire da remoto la predisposizione dei testi degli EM/SUBEM, la firma e l’operazione di deposito;
- garantire autenticità del testo, intesa come integrità del testo, provenienza del documento dal suo autore e certezza temporale sulla sua presentazione;
- snellire le operazioni per la lavorazione degli EM/SUBEM (numerazione, marcatura, autenticazione, ecc…);
- ridurre i tempi necessari alla generazione di output finalizzati alla discussione in aula e alle fasi post-seduta, attraverso la loro generazione automatica;
- agevolare i lavori d'aula: visualizzare, gestire e modificare in Aula il testo degli emendamenti;
- ridurre l’utilizzo della carta e dei supporti per la stampa.

Il Modulo DASI permette di:
- gestire da remoto la predisposizione dei testi degli ATTI, la firma e l’operazione di deposito;
- garantire autenticità del testo, intesa come integrità del testo, provenienza del documento dal suo autore e certezza temporale sulla sua presentazione;
- snellire le operazioni per la lavorazione degli ATTI (numerazione, marcatura, autenticazione, ecc…);
- ridurre l’utilizzo della carta e dei supporti per la stampa.
- gestire massivamente gli atti
- gestire puntualmente i dati associati agli atti
- configurare la ricerca ricorrente degli atti
- customizzare la reportistica

Maggiori dettagli sulle funzionalità possono essere lette nella documentazione per l’utente finale:

- [Documentazione](/Documentazione/README.md)

# Struttura del repository

  All'interno della release, oltre al presente file README.md e al file di licenza LICENSE.md, sono presenti le seguenti cartelle:
  
  - Database: struttura di configurazione del database e sua documentazione
  
  - Documentazione: documentazione varia sull'installazione e sull'utilizzo del Portale GeDASI
  
  - Sorgenti API: sorgenti dei moduli API utilizzati dal Portale GeDASI
  
  - Sorgenti Client: sorgenti della parte client del Portale GeDASI
  
  - Sorgenti modulo di stampa asincrona: Sorgenti del modulo di stampa asincrona degli Emendamenti in formato pdf
  
Il repository corrente ha 3 branches:

- Branch master: contiene il codice e la documentazione di GeDASI nella versione attuale e su cui verranno effettuati gli aggiornamenti e le ulteriori modifiche evolutive

- Branch v2.2-beta: contiene il codice e la documentazione di GeDASI in versione 2.2 ovvero il modulo PEM con le ultime modifiche evolutive e il modulo DASI per la digitalizzazione della presentazione degli atti di sindacato ispettivo e d'indirizzo 

- Branch v2.0-stable: contiene il codice e la documentazione della versione PEM 2.0 ovvero il modulo PEM per la digitalizzazione della presentazione degli emendamenti senza le ultime modifiche evolutive effettuate

# Struttura del progetto

#### INTRODUZIONE

La versione pubblicata del software GeDASI è l’evoluzione di una prima versione di PEM (PEM v 1.0) e della sua successiva, PEM-DASI, sviluppata con tecnologia Microsoft ASP.net su Framework .NET 4.5.
La nuova versione è stata realizzata con l’obiettivo di migliorare e superare alcuni limiti del vecchio portale offrendo i seguenti vantaggi:
-	Eliminazione di librerie di terze parti coperte da licenza non opensource;
-	Separazione della parte client da quella server realizzando API dedicate che gestiscono tutta la logica applicativa di GeDASI e facilitano l'eventuale sviluppo di applicazioni mobile per dispositivi Apple e Android;
-	Aumento della modularità per consentire l’evoluzione del portale per la gestione di altre tipologie di ATTI (es. atti d’indirizzo e di sindacato ispettivo);
-	Miglioramento delle performance soprattutto nella parte di generazione delle stampe;
-	Miglioramento della sicurezza;
-	Introduzione di funzionalità che permetto di utilizzare la piattaforma in modalità stand-alone, gestendo in modalità nativa le funzioni di autenticazione, profilazione e di anagrafica.
 
La nuova versione è stata sviluppata utilizzando la tecnologia Microsoft MVC (model view controller) utilizzando C# come linguaggio di programmazione e il Framework .NET 4.7.2. 

Successivamente la piattaforma PEM v2.0, utilizzata per digitalizzare gli emendamenti/subemendamenti ai progetti di legge, è stata estesa per la digitalizzazione degli altri atti tipici delle assemblee regionali sviluppando il modulo DASI per la Digitalizzazione Atti di Sindacato ispettivo e d'Indirizzo PEM-DASI v2.2

La piattaforma GeDASI offre un nuovo incremento alle funzionalità del sistema, tra cui:
- filtraggio configurabile
- reportistica custom
- CRUD sul singolo atto
- gestione massiva atti
- gestione dei template per report lettere e copertine

### STRUTTURA DEL SISTEMA
Il portale GeDASI è stato progettato e sviluppato in modo da separare in maniera netta la parte server da quella client e allo stesso tempo fornire intefacce progrmmabili (API) di tipo web che possono essere richiamate ed utilizzate per altri scopi da altre applicazioni. 

![Struttura_sistema](/Documentazione/Screenshot/Struttura_sistema.jpg)
 
Le componenti principale del sistema risultano essere quelle in figura e descritte di seguito:
-	DATABASE:
Motore di database contenente i dati dell’applicazione, le funzioni e le procedure di basso livello. Si è scelto di utilizzate MS Sql Server come DBMS.
-	API:
È la parte core del sistema che contiene tutta la logica applicativa e di interfacciamento ad alto livello, tramite la modellazione di opportune classi, con la base dati di GeDASI. L’interfacciamento con la base dati è stato realizzato utilizzando un layer con EntityFramework 6
-	CLIENT:
È il modulo che interroga l’API e genera l’output finale (html, javascript, css) da inviare ai dispositivi client.
-	PROXYAD (WEBSERVICES DI AUTENTICAZIONE):
È un servizio web di tipo soap che si occupa di effettuare l’autenticazione e la profilazione degli utenti del sistema interfacciandosi con il repository Active Directory delle utenze di rete del CRL.
-	STAMPER (SCHEDULER DELLE STAMPE):
Al fine di migliorare le performance, le stampe in pdf generate da PEM avvengono principalmente in modalità asincrona utilizzando il modulo STAMPER che si occupa di generare i pdf e di inviarli via email al richiedente. Il modulo STAMPER è schedulato in modo da eseguire periodicamente il controllo della coda di stampa.

NOTA: 
Il progetto in produzione presso il Consiglio regionale della Lombardia utilizza un ulteriore webservices per la pubblicazione dei dai dati relativi agli emendamenti sul dataset dedicato all’interno del portale www.dati.lombardia.it. Questa funzionzionalità non è attiva nel sorgente pubblicato e tutte le chiamate al webservices sono state disabilitate attraverso l’impostazione della chiave presente nel web.config (AbilitaOpenData = 0)

### API
Come accennato il modulo API è la parte core della soluzione GeDASI e contiene tutta la logica applicativa e di interfacciamento alla base dati.
L’API dialoga pertanto sia con il modulo client, per l’invio di tutte le informazioni necessarie alla creazione delle pagine web finali, sia con il DBMS per la lettura e la memorizzazione dei dati.
Per l’interfacciamento con il database è stato sviluppato un layer con EntityFramework 6. Questo agevola l’utilizzo anche di altri provider nel caso non si voglia usare Microsoft SQL server. Per tutti i database supportati fare riferimento alla guida [Panoramica di Entity Framework 6 - EF6 | Microsoft Docs](https://docs.microsoft.com/it-it/ef/ef6/)

#### API – AUTENTICAZIONE
Le richieste all’API vengono soddisfatte solo se il richiedente risulta correttamente autenticato ed autorizzato ad accedere alla risorsa/funzionalità richiesta. Per l’autenticazione, l’API fornisce un endpoint (di tipo allow-anonimous) che permette il riconoscimento tramite username e password. In caso di corretta autenticazione viene fornito un token (JWT) che dovrà essere utilizzato per tutte le successive richieste all’API. Il token è una stringa crittografata di caratteri con una scadenza configurabile e contiene i dati dell’utente (ruoli e gruppi di appartenenza).
Per informazioni più dettagliate su JWT token si può consultare la seguente guida: [JSON Web Tokens - jwt.io](https://jwt.io/)

Per evitare il proliferare di utenze e password, in Consiglio regionale della Lombardia, si è scelto di utilizzare, come primo livello di accesso al portale, gli stessi user name e password utilizzati per l’accesso al dominio di rete interna. Per effettuare l’autenticazione viene utilizzato un webservice soap che si interfaccia con il repository delle utenze di rete.

Per rendere il portale GeDASI immediatamente riusabile, è possibile utilizzare delle credenziali (username e password) memorizzate sul database interno di GeDASI. Per attivare questo tipo di autenticazione è necessario impostare la chiave AutenticazioneAD = 0 nel web.config dell’applicazione.

#### API – STRUTTURA (SOTTO-PROGETTI)
Il modulo API è stato sviluppato secondo una logica di sotto-progetti per separare logicamente le diverse tipologie di operazioni.
Tale scelta è stata effettuata seguendo la logica del riuso in modo da consentire la sostituzione/rielaborazione di una singola componente facilitando l’integrazione con le tecnologie in uso presso le diverse amministrazioni. 
I sotto-progetti realizzati sono i seguenti:
-	API: amministra il routing, valida i token, gestisce le configurazioni
-	BAL: (Business Access Layer): amministra ed elabora i dati scambiati
-	DTO: (Data Transfer Object): contiene i modelli che mappano in classi gli oggetti di database (table e view)
-	DOMAIN: contiene i modelli del database
-	CONTRACTS: contiene le interfacce per accedere ai dati e consente/nega l’esecuzione di operazioni specifiche sui diversi oggetti dell’applicazione
-	PERSISTANCE: implementazione delle interfacce, e accesso ai dati chiamando il modulo Database
-	DATABASE: contesto che accede e modella il provider dati interfacciandosi direttamente con la base dati.
-	COMMON: contiene funzionalità comuni a tutte i sotto-progetti e le rende disponibili
-	LOGGER: gestisce i log dell’applicazione. log4net. I log sono stati gestiti utilizzando la libreria log4net ( [Apache log4net – Apache log4net: Home - Apache log4net](http://logging.apache.org/log4net/) )
-	EXPRESSION BUILDER: gestire i filtri (query di interrogazione). E’ stato realizzato personalizzando il progetto opensouce ExpressionBuilder ( [dbelmont/ExpressionBuilder: A library that provides a simple way to create lambda expressions to filter lists and database queries. (github.com)](https://github.com/dbelmont/ExpressionBuilder) )

La soluzione del progetto in Visual Studio risulta quella in figura:

![Soluzione_VisualStudio_API](/Documentazione/Screenshot/Soluzione_VisualStudio_API.jpg)

#### API - FUNZIONI
Il sotto-progetto API costituisce la parte principale della parte API in quanto contiene e rende disponibili tutte le funzioni necessarie per gestire i vari oggetti del progetto. Secondo la logica MVC sono stati sviluppati i seguenti controller:
-	SEDUTECONTROLLER: contiene tutte le operazioni per gestire le sedute
-	ATTICONTROLLER: Contiene tutte le operazioni per gestire gli atti (Gestione articoli/commi/lettere, salvataggio relatori, gestione dei fascicoli in ordine di votazione/presentazione, ecc.)
-	EMENDAMENTICONTROLLER: Contiene tutte le operazioni per gestire gli emendamenti (Gestione firme/inviti/depositi, visualizzazioni di preview, modifica metadati, gestione stati, ordinamenti e fascicolazione, ecc.)
-	DASICONTROLLER: contiene tutte le operazioni per gestire gli atti di indirizzo e di sindacato ispettivo.
-	PERSONECONTROLLER: contiene tutte le operazioni inerenti gli utenti del sistema e la gestione dei relativi ruoli e gruppi di appartenenza (swap ruoli/gruppi, visualizzazione utenti e ruoli del sistema, cambio pin, ecc.)
-	NOTIFICHECONTROLLER: contiene tutte le operazioni sulle notifiche
-	AUTENTICAZIONECONTROLLER: contiene tutte le operazioni di autenticazione al portale
-	ESPORTACONTROLLER: contiene le operazioni per le esportazioni di emendamenti (excel/word)
-	STAMPECONTROLLER: contiene tutte le operazioni per la gestione delle stampe
-	JOBCONTROLLER: contiene tutte le operazioni per i servizi esterni di stampa (effettua l’autenticazione utilizzando il ruolo SERVICE_JOB)
-	UTILCONTROLLER: contiene tutte le operazioni comuni dell’applicazione (es. invio mail)
-	ADMINCONTROLLER: contiene le funzioni per la gestione amministrativa del portale (definizione di utenti e password, impostazione dei ruoli, reset pin, configurazione gruppi politici, ecc.)

#### API – LIBRERIE
La parte API di GeDASI è stata realizzata utilizzando librerie opensource in modo da rendere tutta l'applicazione priva di strumenti coperti da licenze proprietarie e quindi con codice sorgente non modificabile. Per quanto riguarda la stampa in PDF degli Atti di Sindacato ispettivo e d'Indirizzo (Modulo DASI), per migliorare le performance, la qualità dell'output e soprattutto gestire le criticità dovute all'inserimento di testi complessi effettuando il "copia/incolla" da documenti Microsoft Word, la libreria opensource ITextSharp è stata sostituita con libreria **a pagamento** IronPDF (vedere il paragrafo "Licenze dei componenti di terze parti" per costi e maggiori informazioni).

Nel progetto è comunque disponibile la libreria opensource ITextSharp, ancora utilizzata per la stampa degli emendamenti, e  il codice per generare le stampe pdf utilizzando questa libreria, mettendo così a disposizione una versione completamente opensource e gratuita di PEM/DASI (il codice è tenuto aggiornato e compatibile con la libreria ITextSharp **fino** alla versione corrente della piattaforma - ver 2.2 - nelle versioni successive non si garantisce lo sviluppo di codice che utilizza ITextSharp)

### CLIENT
Il modulo client si occupa si generare le pagine web finali composte da html e librerie javscript e css. Le pagine vengono inviate ai web-browser per la visualizzazione. Il modulo CLIENT dialoga con il modulo API per la creazione delle pagine e la gestione dei diversi comandi e funzionalità del portale GeDASI. Come detto tutta la logica applicativa, la gestione dei permessi e l’interfacciamento con il database viene effettuato dal modulo API. Questo tipo di struttura separa in maniera netta l’interfaccia utente dalle logiche di business consentendo un’agevole sostituzione della parte client, ad esempio con un’App per dispositivi mobili Apple o Android.

#### CLIENT – STRUTTURA (SOTTO-PROGETTI)
Così come effettuato per il modulo API anche il modulo CLIENT è stato sviluppato secondo una logica di sotto-progetti per separare logicamente le diverse tipologie di operazioni per agevolare il riuso dell’applicazione permettendo la sostituzione/rielaborazione di singole componenti. 
I sotto-progetti realizzati nel modulo CLIENT sono i seguenti:
-	CLIENT: contiene le routine per generare l’interfaccia utente del portale
-	COMMON: contiene funzionalità comuni a tutte i sotto-progetti e le rende disponibili
-	DTO: (Data Transfer Object): contiene i modelli che mappano in classi gli oggetti di
-	GATEWAY: mette a disposizione le interfacce per la comunicazione tra API e CLIENT
-	LOGGER: gestisce i log dell’applicazione. log4net. I log sono stati gestiti utilizzando la libreria log4net

La soluzione del progetto in Visual Studio risulta quella in figura:

![Soluzione_VisualStudio_Client](/Documentazione/Screenshot/Soluzione_VisualStudio_Client.jpg)

#### CLIENT – FUNZIONI
Il sotto-progetto Client costituisce la parte principale della parte CLIENT. Secondo la logica MVC sono stati sviluppati i seguenti controller:
-	SEDUTECONTROLLER: gestisce tutte funzionalità per gestire le sedute interfacciandosi con SEDUTECONTROLLER del modulo API.
-	ATTICONTROLLER: gestisce tutte le funzionalità per gestire gli atti (gestione articoli/commi/lettere, relatori, fascicolazione, ecc.) interfacciandosi con ATTICONTROLLER del modulo API.
-	EMENDAMENTICONTROLLER: gestisce tutte le funzionalità per gestire gli emendamenti (gestione firme/inviti/depositi, preview, modifica metadati, gestione stati, ordinamenti/ fascicolazione, ecc.) interfacciandosi con EMENDAMENTICONTROLLER del modulo API.
-	DASICONTROLLER: gestisce tutte le funzionalità per gestire gli atti di sindacato ispettivo interfacciandosi con DASICONTROLLER dell'API.
-	PERSONECONTROLLER: gestisce tutte le funzionalità inerenti gli utenti del sistema e la gestione dei relativi ruoli e gruppi di appartenenza (swap ruoli/gruppi, visualizzazione utenti e ruoli del sistema, cambio pin, ecc.) interfacciandosi con PERSONECONTROLLER del modulo API.
-	NOTIFICHECONTROLLER: gestisce tutte le funzionalità sulle notifiche interfacciandosi con NOTIFICHECONTROLLER del modulo API.
-	AUTENTICAZIONECONTROLLER: gestisce tutte le operazioni di autenticazione al portale interfacciandosi con AUTENTICAZIONECONTROLLER del modulo API.
-	STAMPECONTROLLER: gestisce le stampe interfacciandosi con STAMPECONTROLLER del modulo API.
-	VIDEOTUTORIAL: Contiene i tutorial per l’utilizzo del PEM.

#### CLIENT – LIBRERIE
La parte di interfaccia utente di GeDASI è stata realizzata utilizzando le tecnologie attualmente più evolute che consentono la visualizzazione responsive dell’applicazione. Particolare attenzione è stata dedicata alla scelta di librerie opensource in modo da rendere tutta l'applicazione priva di strumenti coperti da licenze proprietarie e quindi con codice sorgente non modificabile. In quest'ottica si può affermare che la piattaforma GeDASI è pianmente in linea con la logica del riuso e consente la più ampia possibilità di personalizzazione. In particolare, sono stati utilizzati:
-	Materialize (stile) - [Documentation - Materialize (materializecss.com)](https://materializecss.com/)
-	jQuery (javascript) - [jQuery](https://jquery.com/)
-	Trumbowyg (editor testo) - [https://alex-d.github.io/Trumbowyg/](https://alex-d.github.io/Trumbowyg/))

#### TEMPLATE
Per rendere il portale GeDASI adattabile ad esigenze di layout differenti e personalizzabili, la visualizzazione e la stampa degli atti e dei fascicoli è stata sviluppata utilizzando dei templates html. Attraverso questi templates, contenuti nella cartella Template nel progetto API, è possibile personalizzare il layout degli emendamenti, degli atti e dei fascicoli sia nella versione html (per visualizzazione a video e per invio tramite email) sia nella versione pdf.

#### GESTIONE DELLE STAMPE
Come detto precedentemente, per una questione di performance, le stampe in pdf vengono generalmente effettuate in modalità asincrona.
Per tala gestione è stato sviluppato un servizio windows (GAMScheduler), un programma di interfaccia (Scheduer) che permette la configurazione di tale servizio e un job di stampa che si occupa di generare e inviare via email, le stampe pdf.

#### SERVIZIO (GAMScheduler service)
Servizio sviluppato in C# utilizzando la libreria Quartz.NET ([Quartz.NET (quartz-scheduler.net)](https://www.quartz-scheduler.net/))
Il servizio legge le configurazioni dallo schedulatore, carica tramite reflection la dll del job e la schedula passandogli i parametri letti sempre da configurazione nello scheduler.
Tutta la documentazione relativa alla libreria Quarts.NET è reperibile nel sito ufficiale [Quartz.NET Quick Start Guide | Quartz.NET (quartz-scheduler.net)](https://www.quartz-scheduler.net/documentation/quartz-3.x/quick-start.html#download-and-install)

#### SCHEDULER
Programma windows form sviluppato in C# che gestisce il servizio di windows GAMScheduler, la schedulazione e la configurazione dei job.
La schedulazione del job viene gestita tramite cronExpression (triggers_config.json).
La configurazione viene gestita tramite file jobs_config.json.

#### JOB - STAMPA
E’ un estensione dell’interfaccia IJob ereditata da Quartz ed esegue queste lavorazioni:
-	Autenticazione con utente di servizio all’api (ruolo SERVIZIO_JOB)
-	Elabora fino a 20 stampe
-	Ogni stampa ha un tot di atti da stampare
-	Scarica dall’api i template precompilati
-	Crea N task per creare i PDF degli emendamenti/atti di indirizzo e sindacato ispettivo
-	Manda la mail con il link al fascicolo
-	In caso di deposito differito, viene anche generato il pdf dell’atto appena depositato e inviato via mail

La soluzione del progetto in Visual Studio risulta quella in figura:

![Soluzione_VisualStudio_JobStampa](/Documentazione/Screenshot/Soluzione_VisualStudio_JobStampa.jpg)

#### WebService pubblico
Di seguito la documentazione del WebService pubblico:

![Strutturazione dei web services](/Documentazione/Strutturazione dei web services.pdf)

# Installazione

## Note sulla release

Il codice sorgente pubblicato è relativo alla piattaforma GeDASI nella release 2.2, che integra il modulo PEM e il modulo DASI ed è l'evoluzione di una prima versione di PEM sviluppata in asp.net.
La versione 2.0 separa la parte client dell’applicazione da quella server attraverso lo sviluppo di API dedicate e introduce miglioramenti nelle performance e nella gestione delle stampe pdf. L’introduzione delle API per la gestione dei dati e delle elaborazioni principali facilita lo sviluppo di App per dispositivi mobili (Apple e Android).

La piattaforma PEM/DASI è stata realizzata con librerie opensource gratuite e tutte le sue funzionalità sono state sviluppate utilizzando queste librerie.
Per quanto riguarda la stampa in PDF degli Atti di Sindacato ispettivo e d'Indirizzo (Modulo DASI), per migliorare le performance, la qualità dell'output e soprattutto gestire le criticità dovute all'inserimento di testi effettuando il "copia/incolla" da documenti Microsoft Word, è stata aggiunta **ed è utilizzata** dal Consiglio regionale della lombardia la libreria **a pagamento** IronPDF (vedere il paragrafo "Licenze dei componenti di terze parti" per costi e maggiori informazioni).

Nel progetto è comunque disponibile la libreria opensource ITextSharp e tutto il codice per generare le stampe pdf degli atti utilizzando questa libreria, mettendo così a disposizione una versione completamente opensource e gratuita di PEM/DASI (il codice è tenuto aggiornato e compatibile con la libreria ITextSharp **fino** alla versione corrente della piattaforma - ver 2.2 - nelle versioni successive non si garantisce lo sviluppo di codice che utilizza ITextSharp)


NOTA: Io modulo DASI è attualmente in fase di test e viene rilasciato in versione beta.

## Requisiti del sistema

Specifiche tecniche server consigliate:

- Sistema Operativo: Windows Server 2022 + Active Directory
- Web e Application server: IIS 10 + Entity Framework 6.0
- Database: Microsoft SQL server 2019

Specifiche tecniche client:
- Sistema Operativo: Microsoft windows 10 o superiore, Mac OsX
- Browser: Edge, FireFox, Chrome, Safari
- Dispositivi mobile (tablet/cellulari): iOS, Android - il portale è responsive ad esclusione di alcune parti.

## Procedura di installazione

L'installazione prevede la creazione del database tramite lo script fornito; la compilazione dei sorgenti Client e Api; la creazione di due application su IIS, una per la Api e una per il Client; la configurazione dei file web.config, sia dell'Api sia del Client, impostando i parametri di configurazione del proprio ambiente. Al termine, la compilazione, l'installazione e la schedulazione del modulo di stampa asincrona.

Per la procedura completa di installazione fare riferimento alla documentazione specifica:

- [Documentazione](/Documentazione/Installazione.md)
 

# Licenza

## Autore / Copyright

Portale GeDASI - Presentazione EMendamenti e Digitalizzazione Atti di Sindacato ispettivo e d'Indirizzo
2020-2022 (c) Consiglio Regionale dell Lombardia

Concesso in licenza [GNU Affero General Public Licence version 3](https://www.gnu.org/licenses/agpl-3.0.html) (SPDX: AGPL-3.0)

## Licenze dei componenti di terze parti

All'interno del codice del Portale GeDASI sono stati utilizzati i seguenti componenti di terze parti, nell'ambito delle relative licenze qui indicate:
  
- Log4net
 https://github.com/apache/logging-log4net
 con licenza [Apache-2.0 License](https://github.com/apache/logging-log4net/blob/master/LICENSE)
 
 
- ITextSharp
 https://github.com/itext/itextsharp
 con licenza [GNU Affero General Public License version 3](https://github.com/itext/itextsharp/blob/develop/LICENSE.md)
 
 - IronPDF
 https://ironpdf.com/licensing/
 con licenza commerciale EULA **non opensource** e **NON GRATUITA**
 
- Trumbowyg
 https://github.com/Alex-D/Trumbowyg
 con licenza [MIT License](https://github.com/Alex-D/Trumbowyg/blob/develop/LICENSE)
 
 
- Materialize 
 https://github.com/Dogfalo/materialize/tree/master
 con licenza [MIT License](https://github.com/Dogfalo/materialize/blob/v1-dev/LICENSE)
 
 
- Quartz.NET (quartz-scheduler.net)
 https://www.quartz-scheduler.net/
 con licenza [Apache 2.0 License](https://github.com/quartznet/quartznet/blob/master/license.txt)
 
 
- ExpressionBuilder (dbelmont/ExpressionBuilder)
 https://github.com/dbelmont/ExpressionBuilder
 con licenza [Apache 2.0 License](https://github.com/dbelmont/ExpressionBuilder/blob/master/LICENSE)
 
 
- Newtonsoft.json - MIT License
 https://www.nuget.org/packages/Newtonsoft.Json
 con licenza [MIT License](https://licenses.nuget.org/MIT)
 
 
- AutoMapper
 https://www.nuget.org/packages/AutoMapper/
 con licenza [MIT License](https://licenses.nuget.org/MIT)
 
 
- NPOI
 https://www.nuget.org/packages/NPOI/
 con licenza [Apache 2.0 License](https://www.nuget.org/packages/NPOI/2.5.2/License)



## Dettagli della licenza

La licenza per questo repository è [GNU Affero General Public Licence version 3](https://www.gnu.org/licenses/agpl-3.0.html) (SPDX: AGPL-3.0).
Non è possibile utilizzare l'opera salvo nel rispetto della Licenza.

È possibile ottenere una copia della Licenza al seguente indirizzo: https://opensource.org/licenses/AGPL-3.0

Salvo diversamente indicato dalla legge applicabile o concordato per iscritto, il software distribuito secondo i termini della Licenza è distribuito "TAL QUALE", SENZA GARANZIE O CONDIZIONI DI ALCUN TIPO, esplicite o implicite.
 
Si veda la Licenza per la lingua specifica che disciplina le autorizzazioni e le limitazioni secondo i termini della Licenza.
 
Si veda il file [LICENSE.md](LICENSE.md) all'interno del repository per i riferimenti completi.
 
Il logo della Regione Lombardia è di proprietà esclusiva di Regione Lombardia e per tanto non è rilasciato sotto licenza aperta.
 
